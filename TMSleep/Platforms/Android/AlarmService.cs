using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Microsoft.Maui.Controls;

namespace TMSleep.Platforms.Android;

[Service(ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
public class AlarmService : Service
{
    private MediaPlayer? _mediaPlayer;
    private System.Timers.Timer? _volumeTimer;
    private float _currentVolume = 0.0f;
    private const string CHANNEL_ID = "alarm_service_channel";

    public override IBinder? OnBind(Intent intent) => null;

    public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
    {
        string title = intent.GetStringExtra("ALARM_TITLE") ?? "Meditation Time";

        CreateNotificationChannel();

        // Create the notification that makes this a "Foreground" service
        var notification = new Notification.Builder(this, CHANNEL_ID)
            .SetContentTitle(title)
            .SetContentText("Your scheduled meditation is playing...")
            .SetSmallIcon(global::Android.Resource.Drawable.IcLockIdleAlarm)
            .SetOngoing(true) // User cannot swipe it away
            .Build();

        // Start as foreground (Requirement for background audio)
        StartForeground(1001, notification);

        PlayAlarmSound();

        return StartCommandResult.Sticky;
    }

    private void PlayAlarmSound()
    {
        // For now, we use a system default alarm sound. 
        // Later we will hook this up to your 'sounds' table.
        var uri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);

        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.SetDataSource(this, uri);
        _mediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()
            .SetUsage(AudioUsageKind.Alarm)
            .SetContentType(AudioContentType.Music)
            .Build());

        _mediaPlayer.Looping = true;
        _mediaPlayer.SetVolume(0, 0); // Start at zero volume
        _mediaPlayer.Prepare();
        _mediaPlayer.Start();

        StartVolumeRamp();
    }

    private void StartVolumeRamp()
    {
        // Gradual Volume Logic: Increase volume every 2 seconds
        _volumeTimer = new System.Timers.Timer(2000);
        _volumeTimer.Elapsed += (s, e) =>
        {
            if (_currentVolume < 1.0f)
            {
                _currentVolume += 0.05f; // Increase by 5%
                _mediaPlayer?.SetVolume(_currentVolume, _currentVolume);
            }
            else
            {
                _volumeTimer.Stop();
            }
        };
        _volumeTimer.Start();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(CHANNEL_ID, "Alarm Service", NotificationImportance.High);
            var manager = (NotificationManager)GetSystemService(NotificationService);
            manager.CreateNotificationChannel(channel);
        }
    }

    public override void OnDestroy()
    {
        _volumeTimer?.Stop();
        _mediaPlayer?.Stop();
        _mediaPlayer?.Release();
        base.OnDestroy();
    }
}