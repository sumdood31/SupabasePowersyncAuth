/// <summary>
/// Minimal contract every repository implements.
/// T is the PowerSync local model type (e.g. Profile, Schedule, EVERY POWERYSYNC TABLE MUST/SHOULD INHERITED FROM THIS INTERFACE).
/// </summary>

namespace TMSleep.Services;

public interface IRepository<T> where T : class
{
    Task<T[]> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}

