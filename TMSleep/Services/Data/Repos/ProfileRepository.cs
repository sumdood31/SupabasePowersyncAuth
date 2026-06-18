/// <summary>
/// All reads come from the local PowerSync SQLite DB (instant, offline-capable).
/// All writes go to local SQLite first; PowerSync queues and uploads them to Supabase.
/// </summary>

using PowerSync;
namespace TMSleep.Services;

public class ProfileRepository : IRepository<Profile>
{
    private readonly DatabaseService _db;

    public ProfileRepository(DatabaseService db)
    {
        _db = db;
    }

    public Task<Profile[]> GetAllAsync() =>
        _db.Db.GetAll<Profile>("SELECT * FROM profiles ORDER BY full_name ASC");

    public Task<Profile?> GetByIdAsync(string id) =>
        _db.Db.GetOptional<Profile>(
            "SELECT * FROM profiles WHERE id = ? LIMIT 1", [id]);

    public Task<Profile?> GetCurrentUserProfileAsync() =>
        _db.Db.GetOptional<Profile>(
            "SELECT * FROM profiles WHERE id = ? LIMIT 1",
            [_db.Connector.UserId]);

    /// Creates a new profile row for the current user.
    /// Call once after the user signs up, not on every login.
    public Task InsertAsync(Profile entity) =>
        _db.Db.Execute(
            @"INSERT INTO profiles (id, full_name, created_at, updated_at)
              VALUES (uuid(), ?, ?, datetime('now'), datetime('now'))",
            [entity.Id, entity.FullName]);

    public Task UpdateAsync(Profile entity) =>
        _db.Db.Execute(
            @"UPDATE profiles
              SET full_name  = ?,
                  updated_at = datetime('now')
              WHERE id = ?",
            [entity.FullName, entity.Id]);

    public Task DeleteAsync(string id) =>
        _db.Db.Execute("DELETE FROM profiles WHERE id = ?", [id]);

   
    /// Returns an async-enumerable that yields a new list whenever the
    /// profile table changes. Use with 'await foreach' in a Blazor component.
    /// Remember to cancel via CancellationToken when the component disposes.
    public IAsyncEnumerable<IList<Profile>> WatchAllAsync(CancellationToken ct) =>
        _db.Db.Watch<Profile>(
            "SELECT * FROM profiles ORDER BY full_name ASC",
            parameters: null,
            new PowerSync.Common.Client.SQLWatchOptions { Signal = ct });
}
