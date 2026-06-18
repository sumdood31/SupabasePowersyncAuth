/// <summary>
/// All reads come from the local PowerSync SQLite DB (instant, offline-capable).
/// All writes go to local SQLite first; PowerSync queues and uploads them to Supabase.
/// </summary>

namespace TMSleep.Services;

public class ScheduleRepository : IRepository<Schedule>
{
    private readonly DatabaseService _db;

    public ScheduleRepository(DatabaseService db)
    {
        _db = db;
    }

    public Task<Schedule[]> GetAllAsync() =>
        _db.Db.GetAll<Schedule>(
            "SELECT * FROM schedules ORDER BY start_time ASC");

    public Task<Schedule[]> GetByProfileIdAsync(string profileId) =>
        _db.Db.GetAll<Schedule>(
            "SELECT * FROM schedules WHERE user_id = ? ORDER BY start_time ASC",
            [profileId]);

    public Task<Schedule[]> GetActiveAsync(string profileId) =>
        _db.Db.GetAll<Schedule>(
            "SELECT * FROM schedules WHERE user_id = ? AND enabled = 1 ORDER BY start_time ASC",
            [profileId]);

    public Task<Schedule?> GetByIdAsync(string id) =>
        _db.Db.GetOptional<Schedule>(
            "SELECT * FROM schedules WHERE id = ? LIMIT 1", [id]);

    public Task InsertAsync(Schedule entity) =>
        _db.Db.Execute(
            @"INSERT INTO schedules
                (id, user_id, title, description, type, start_time, end_time, enabled, created_at)
              VALUES
                (uuid(), ?, ?, ?, ?, ?, ?, ?, datetime('now'))",
            [entity.UserId, entity.Title, entity.Description, entity.Type,
             entity.StartTime, entity.EndTime, entity.Enabled]);

    public Task UpdateAsync(Schedule entity) =>
        _db.Db.Execute(
            @"UPDATE schedules
              SET title       = ?,
                  description = ?,
                  start_time  = ?,
                  end_time    = ?,
                  enabled   = ?,
                  updated_at  = datetime('now')
              WHERE id = ?",
            [
                entity.Title,
                entity.Description,
                entity.StartTime,
                entity.EndTime,
                entity.Enabled,
                entity.Id
            ]);

    public Task DeleteAsync(string id) =>
        _db.Db.Execute("DELETE FROM schedules WHERE id = ?", [id]);

    public Task SetActiveAsync(string id, int isActive) =>
        _db.Db.Execute(
            "UPDATE schedules SET enabled = ? WHERE id = ?",
            [isActive, id]);

    /// Yields a fresh list of schedule rows for a profile whenever the
    /// schedule table changes. Use with 'await foreach' in a Blazor component.
    public IAsyncEnumerable<IList<Schedule>> WatchByProfileAsync(
        string userId, CancellationToken ct) =>
        _db.Db.Watch<Schedule>(
            "SELECT * FROM schedules WHERE user_id = ? ORDER BY start_time ASC",
            parameters: [userId],
            new PowerSync.Common.Client.SQLWatchOptions { Signal = ct });
}

