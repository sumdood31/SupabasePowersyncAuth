

namespace TMSleep.Services;

public interface IAlarmScheduler
{
    void ScheduleAlarm(Schedule alarm);
    void CancelAlarm(string alarmId);
}