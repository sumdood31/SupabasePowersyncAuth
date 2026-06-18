using Android.App;
using Android.Content;
using Android.OS;

namespace TMSleep.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class AlarmBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        var alarmId = intent.GetStringExtra("ALARM_ID");
        var alarmTitle = intent.GetStringExtra("ALARM_TITLE");

        // We create an intent to start our Foreground Service
        var serviceIntent = new Intent(context, typeof(AlarmService));
        serviceIntent.PutExtra("ALARM_ID", alarmId);
        serviceIntent.PutExtra("ALARM_TITLE", alarmTitle);

        // On Android 8.0 (API 26) and above, we must use StartForegroundService
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            context.StartForegroundService(serviceIntent);
        }
        else
        {
            context.StartService(serviceIntent);
        }
    }
}
