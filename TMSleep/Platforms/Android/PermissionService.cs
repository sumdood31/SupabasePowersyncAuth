//using global::Android.App;
//using global::Android.Content;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using TMSleep.Services;
using Application = Android.App.Application;

namespace TMSleep.Platforms.Android;

public class AndroidPermissionService : IPermissionService
{
    private Context Context => Application.Context;

    public async Task<PermissionStatusData> GetAllPermissionsStatus()
    {
        return new PermissionStatusData(
            NotificationsGranted: await CheckNotificationPermission(),
            BatteryOptimizationIgnored: IsBatteryOptimizationIgnored(),
            ExactAlarmGranted: CanScheduleExactAlarms(),
            OverlayGranted: Settings.CanDrawOverlays(Context)
        );
    }

    private async Task<bool> CheckNotificationPermission()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.PostNotifications>();
        return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
    }

    private bool IsBatteryOptimizationIgnored()
    {
        var pm = (PowerManager)Context.GetSystemService(Context.PowerService);
        return pm.IsIgnoringBatteryOptimizations(Context.PackageName);
    }

    private bool CanScheduleExactAlarms()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S) // Android 12+
        {
            var alarmManager = (AlarmManager)Context.GetSystemService(Context.AlarmService);
            return alarmManager.CanScheduleExactAlarms();
        }
        return true;
    }

    public async Task<bool> RequestNotificationPermission()
    {
        var status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.PostNotifications>();
        return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
    }

    public Task RequestBatteryOptimizationExemption()
    {
        var intent = new Intent(Settings.ActionRequestIgnoreBatteryOptimizations);
        intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
        intent.AddFlags(ActivityFlags.NewTask);
        Context.StartActivity(intent);
        return Task.CompletedTask;
    }

    public Task RequestExactAlarmPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            var intent = new Intent(Settings.ActionRequestScheduleExactAlarm);
            intent.AddFlags(ActivityFlags.NewTask);
            Context.StartActivity(intent);
        }
        return Task.CompletedTask;
    }

    public Task RequestOverlayPermission()
    {
        var intent = new Intent(Settings.ActionManageOverlayPermission);
        intent.SetData(global::Android.Net.Uri.Parse($"package:{Context.PackageName}"));
        intent.AddFlags(ActivityFlags.NewTask);
        Context.StartActivity(intent);
        return Task.CompletedTask;
    }
}