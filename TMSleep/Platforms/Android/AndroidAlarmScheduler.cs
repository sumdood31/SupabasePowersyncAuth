using Android.App;
using Android.Content;
using Android.OS;
using TMSleep.Services;
using Application = Android.App.Application;

namespace TMSleep.Platforms.Android;

public class AndroidAlarmScheduler : IAlarmScheduler
{
    private Context Context => Application.Context;

    public void ScheduleAlarm(Schedule alarm)
    {
        var alarmManager = (AlarmManager)Context.GetSystemService(Context.AlarmService);

        // Create an "Intent" - this is the message sent when the alarm goes off
        var intent = new Intent(Context, typeof(AlarmBroadcastReceiver));
        intent.PutExtra("ALARM_ID", alarm.Id);
        intent.PutExtra("ALARM_TITLE", alarm.Title);

        // A PendingIntent is a "ticket" given to the Android OS to run later
        var pendingIntent = PendingIntent.GetBroadcast(
            Context,
            alarm.Id.GetHashCode(), // Unique ID for this alarm
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

        // Calculate the time in milliseconds
        long triggerTime = CalculateTimeInMillis(alarm.StartTime);

        // Schedule the Exact Alarm (Requires the permission we just set up!)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S && alarmManager.CanScheduleExactAlarms())
        {
            alarmManager.SetExactAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                triggerTime,
                pendingIntent);
        }
        else
        {
            alarmManager.SetAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                triggerTime,
                pendingIntent);
        }
    }

    public void CancelAlarm(string alarmId)
    {
        var intent = new Intent(Context, typeof(AlarmBroadcastReceiver));
        var pendingIntent = PendingIntent.GetBroadcast(
            Context,
            alarmId.GetHashCode(),
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.NoCreate);

        if (pendingIntent != null)
        {
            var alarmManager = (AlarmManager)Context.GetSystemService(Context.AlarmService);
            alarmManager.Cancel(pendingIntent);
        }
    }

    private long CalculateTimeInMillis(string startTime)
    {
        // Simple logic to convert "HH:mm" to the next occurrence in UTC milliseconds
        var parts = startTime.Split(':');
        var time = DateTime.Today.AddHours(int.Parse(parts[0])).AddMinutes(int.Parse(parts[1]));
        if (time < DateTime.Now) time = time.AddDays(1);

        return new DateTimeOffset(time).ToUnixTimeMilliseconds();
    }
}