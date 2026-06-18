using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;                // Table, PrimaryKey, Column
using Supabase.Postgrest.Models;

namespace TMSleep.Services
{
    // ─────────────────────────────────────────────
    //  Supabase models!
    // ─────────────────────────────────────────────
    [Table("profiles")]
    public class SupabaseProfile : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = "";

        //[Column("user_id")]
        //public string UserId { get; set; } = "";

        [Column("full_name")]
        public string FullName { get; set; } = "";

        //[Column("email")]
        //public string Email { get; set; } = "";

        [Column("created_at")]
        public string CreatedAt { get; set; } = "";

        [Column("updated_at")]
        public string UpdatedAt { get; set; } = "";
    }

    [Table("schedules")]
    public class SupabaseSchedule : BaseModel
    {
        [PrimaryKey("id", false)]
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [Column("user_id")]
        [JsonProperty("user_id")]
        public string UserId { get; set; } = "";

        [Column("title")]
        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [Column("type")]
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [Column("description")]
        [JsonProperty("description")]
        public string? Description { get; set; } = "";

        //start_time and end_time are Postgres time without time zone, not full timestamps, so they'll arrive as strings like "07:30:00" — TimeSpan.Parse() will handle those in the UI.

        // Postgres 'time without time zone' comes across as a string e.g. "07:30:00"
        [Column("start_time")]
        [JsonProperty("start_time")]
        public string StartTime { get; set; } = "";

        [Column("end_time")]
        [JsonProperty("end_time")]
        public string? EndTime { get; set; } = "";

        // Postgres integer[] arrives as a JSON array string — store as string,
        // parse it in the UI when you need the individual day values.
        // days_of_week is a Postgres integer[] array.Neither SQLite nor the Supabase C# client handles native arrays, so it serializes to a JSON string like [1,2,3,4,5]. When you need the actual day values in the UI, parse it with JsonConvert.DeserializeObject<List<int>>(item.DaysOfWeek).
        [Column("days_of_week")]
        [JsonProperty("days_of_week")]
        public long DaysOfWeek { get; set; }

        // boolean in Postgres → long in SQLite (0/1)
        [Column("enabled")]
        [JsonProperty("enabled")]
        public long Enabled { get; set; }

        // uuid FK — nullable in Postgres so default to empty string
        [Column("sound_id")]
        [JsonProperty("sound_id")]
        public string? SoundId { get; set; } = "";

        [Column("volume_max")]
        [JsonProperty("volume_max")]
        public double VolumeMax { get; set; }

        // integer in Postgres → long in SQLite
        [Column("volume_ramp_duration")]
        [JsonProperty("volume_ramp_duration")]
        public long VolumeRampDuration { get; set; }

        [Column("reminder_offset_mins")]
        [JsonProperty("reminder_offset_mins")]
        public long ReminderOffsetMins { get; set; }

        // uuid FK — nullable
        [Column("group_id")]
        [JsonProperty("group_id")]
        public string? GroupId { get; set; } = "";

        [Column("created_at")]
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; } = "";
    }
}
