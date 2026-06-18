namespace TMSleep.Services;

public interface IPermissionService
{
    Task<PermissionStatusData> GetAllPermissionsStatus();
    Task<bool> RequestNotificationPermission();
    Task RequestBatteryOptimizationExemption();
    Task RequestExactAlarmPermission();
    Task RequestOverlayPermission();
}

public record PermissionStatusData(
    bool NotificationsGranted,
    bool BatteryOptimizationIgnored,
    bool ExactAlarmGranted,
    bool OverlayGranted
);