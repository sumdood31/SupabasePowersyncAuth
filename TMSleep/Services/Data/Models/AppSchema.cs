using PowerSync.Common.DB.Schema;
using PowerSync.Common.DB.Schema.Attributes;

//POWERSYNC TABLES

[Table("sounds")]
public class Sound
{
    [Column("id")]
    public string Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("origin")]
    public string Origin { get; set; }

    [Column("remote_url")]
    public string RemoteUrl { get; set; }

    [Column("local_path")]
    public string LocalPath { get; set; }

    [Column("category")]
    public string Category { get; set; }
}

[Table("profiles")]
public class Profile
{
    [Column("id")]
    public string Id { get; set; }

    [Column("username")]
    public string Username { get; set; }
    [Column("fullname")]
    public string FullName { get; set; }

    [Column("preferences")]
    public string Preferences { get; set; }

    [Column("updated_at")]
    public string UpdatedAt { get; set; }
}

[Table("schedules")]
public class Schedule
{
    [Column("id")]
    public string Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; }

    [Column("title")]
    public string Title { get; set; }
    [Column("description")]
    public string? Description { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("start_time")]
    public string StartTime { get; set; }
    [Column("end_time")]
    public string? EndTime { get; set; }

    [Column("days_of_week")]
    public long DaysOfWeek { get; set; }

    [Column("enabled")]
    public long Enabled { get; set; }

    [Column("sound_id")]
    public string? SoundId { get; set; }

    [Column("volume_max")]
    public double VolumeMax { get; set; }

    [Column("volume_ramp_duration")]
    public long VolumeRampDuration { get; set; }

    [Column("reminder_offset_mins")]
    public long ReminderOffsetMins { get; set; }

    [Column("group_id")]
    public string? GroupId { get; set; }

    [Column("created_at")]
    public string CreatedAt { get; set; }
}

// ─────────────────────────────────────────────
//  The schema passed to PowerSyncDatabase.
//  Make sure to add any new class/tables to the PowerSyncSchema
// ─────────────────────────────────────────────
public class AppSchema
{
    public static Schema PowerSyncSchema = new Schema(typeof(Sound), typeof(Profile), typeof(Schedule));
}


//// Check if Monday is set
//bool isMondayOn = DaysOfWeek.Has(item.DaysOfWeek, DaysOfWeek.Monday);

//// Toggle Wednesday
//item.DaysOfWeek = DaysOfWeek.Has(item.DaysOfWeek, DaysOfWeek.Wednesday)
//    ? DaysOfWeek.Unset(item.DaysOfWeek, DaysOfWeek.Wednesday)
//    : DaysOfWeek.Set(item.DaysOfWeek, DaysOfWeek.Wednesday);


//@foreach(var(label, bit) in new[] {
//    ("M", DaysOfWeek.Monday),  ("T", DaysOfWeek.Tuesday),
//    ("W", DaysOfWeek.Wednesday),("T", DaysOfWeek.Thursday),
//    ("F", DaysOfWeek.Friday),  ("S", DaysOfWeek.Saturday),
//    ("S", DaysOfWeek.Sunday) })
//{
//    < button class= "day-btn @(DaysOfWeek.Has(_form.DaysOfWeek, bit) ? "active" : "")"
//            @onclick="() => _form.DaysOfWeek = DaysOfWeek.Has(_form.DaysOfWeek, bit)
//                ? DaysOfWeek.Unset(_form.DaysOfWeek, bit)
//                : DaysOfWeek.Set(_form.DaysOfWeek, bit)">
//        @label
//    </ button >
//}

public static class DaysOfWeek
{
    public const long Monday = 1;
    public const long Tuesday = 2;
    public const long Wednesday = 4;
    public const long Thursday = 8;
    public const long Friday = 16;
    public const long Saturday = 32;
    public const long Sunday = 64;

    public const long Weekdays = 31;
    public const long Weekend = 96;
    public const long All = 127;

    public static bool Has(long mask, long day) => (mask & day) != 0;
    public static long Set(long mask, long day) => mask | day;
    public static long Unset(long mask, long day) => mask & ~day;
}