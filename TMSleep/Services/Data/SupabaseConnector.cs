//using Android.Content.PM;
//using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using PowerSync;
using PowerSync.Common.Client;
using PowerSync.Common.Client.Connection;
using PowerSync.Common.DB.Crud;
using Supabase;                                     // Supabase.Client, SupabaseOptions
using Supabase.Gotrue;                              // Session
using System;
using System.Threading.Tasks;
using TMSleep.Services;
using static Supabase.Gotrue.Constants;
//using static Android.Graphics.ColorSpace;

// ─────────────────────────────────────────────
//  PowerSync backend connector
// ─────────────────────────────────────────────
public class SupabaseConnector : IPowerSyncBackendConnector
{
    private const string SupabaseUrl = "[YOUR SUPABASE URL]";
    private const string SupabaseAnonKey = "[YOUR SUPABASE ANON KEY";
    private const string PowerSyncUrl = "[YOUR POWERSYNC URL]";
    private readonly Supabase.Client _supabase; 
    private TaskCompletionSource<bool> _initTcs = new();
    private SupabaseOfflineSession _supabaseSessionHandler = new SupabaseOfflineSession();

    // Allows other classes to block until auth setup is complete
    public Task InitializationTask => _initTcs.Task;
    //User this for powersync queries in for the UI
    public string UserId { get; private set; } = "";
    // Provide a way for the Blazor AuthenticationStateProvider to know who is logged in
    public Supabase.Client Client => _supabase;

    public SupabaseConnector()
    {
        _supabase = new Supabase.Client(SupabaseUrl, SupabaseAnonKey, new SupabaseOptions
        {
            AutoConnectRealtime = false,
            AutoRefreshToken = true, 
            SessionHandler = _supabaseSessionHandler
        });
        
        // Ensure initialization finishes before doing auth operations
        _ = InitializeClientAsync();
    }

    private async Task InitializeClientAsync()
    {
        await _supabase.InitializeAsync();

        _initTcs.SetResult(true);
    }

    public async Task Logout()
    {
        if (_supabase.Auth.CurrentSession != null)
        {
            if (_supabase.Auth.Online)
            {
                await _supabase.Auth.SignOut();
            }
            else
            {   //if user tried to logout when offline supabase's SignOut will try and hit the server causing http error
                //this cuase's the destruction of local session and notifies UI of logout state change.
                _supabase.Auth.NotifyAuthStateChange(AuthState.SignedOut);
            }

        }
    }

    //Used for the user auth form
    public async Task LoginAsync(string email, string password)
    {
        await _initTcs.Task; // Ensure SDK is ready

        var response = await _supabase.Auth.SignInWithPassword(email, password);
        if (response?.User == null || response.AccessToken == null)
            throw new Exception("Supabase login failed. Check your credentials.");

        UserId = response.User.Id ?? "";

    }

    //used to try and reload session if the app is closed and opened again and loads previous session if offline
    public async Task<bool> TryAutoLogin()
    {
        try
        {
            await _initTcs.Task; // Ensure SDK is ready

            if (_supabase.Auth.Online)
            {
                //The SetSession and RefreshSession require a internet connection.
                //NOTE: the offline LoadSession will never work with RefreashSession, even if the user in Online, don't know why
                var savedSession = _supabaseSessionHandler.SetOnlineSession();
                if (savedSession == null) { return false; }
                await _supabase.Auth.SetSession(savedSession.AccessToken, savedSession.RefreshToken);

                if (savedSession.ExpiresAt() <= DateTime.UtcNow)
                {
                    Console.WriteLine("Session is expired locally. Refreshing token...");

                    //await _supabase.Auth.RetrieveSessionAsync();
                    await _supabase.Auth.RefreshSession();

                }
            }
            else
            {
                //LoadSession was the only way i could get active session if the launchs the app offline.
                //The JWT default expire is 1 hour and can't be changed unless you have the Pro plain
                //SO for this to work the user has to open the app in offline mode wihtin an hour of the last refreashed token
                _supabase.Auth.LoadSession();

                if (_supabase.Auth.CurrentSession == null) return false;
                if (_supabase.Auth.CurrentSession.ExpiresAt() <= DateTime.UtcNow)
                {
                    //if it's expire, there is nothing we can do about it
                    await Logout();
                }

            }

            UserId = _supabase.Auth.CurrentUser?.Id ?? "";
            return !string.IsNullOrEmpty(UserId);
        }
            catch (Exception ex)
        {
            await Logout();
            Console.WriteLine($"Auto-login failed: {ex.Message}");
            return false;
        }

        
    }

    /// Called automatically by PowerSync. It must fetch a FRESH token.
    public async Task<PowerSyncCredentials?> FetchCredentials()
    {
        await _initTcs.Task;

        var session = _supabase.Auth.CurrentSession;
        if (session == null) return null;

        try
        {
            if (session.ExpiresAt() <= DateTime.UtcNow.AddSeconds(10))
            {
                Console.WriteLine("[PowerSync Auth] Token expired or expiring soon. Refreshing session...");
                if (_supabase.Auth.Online)
                {
                    session = await _supabase.Auth.RefreshSession();
                    if (session == null) { await Logout(); }
                }
                else
                {
                    // Device is offline and token is expired.
                    // Throw an exception so PowerSync safely enters a "waiting for connectivity" state.
                    throw new Exception("Cannot refresh Supabase token while offline.");
                }
            }

            var freshToken = session?.AccessToken;
            if (freshToken == null) return null;

            return new PowerSyncCredentials(PowerSyncUrl, freshToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PowerSync Auth] Failed to refresh credentials: {ex.Message}");
            return null;
        }
    }

    public async Task UploadData(IPowerSyncDatabase database)
    {
        var transaction = await database.GetNextCrudTransaction();
        if (transaction == null) return;
        try
        {
            foreach (var op in transaction.Crud)
            {
                var table = op.Table.ToLowerInvariant().Trim();
                switch (op.Op)
                {
                    case UpdateType.PUT: await HandlePut(table, op); break;
                    case UpdateType.PATCH: await HandlePatch(table, op); break;
                    case UpdateType.DELETE: await HandleDelete(table, op); break;
                    default: throw new InvalidOperationException($"Unknown op: {op.Op}");
                }
            }
            await transaction.Complete();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SupabaseConnector] UploadData error: {ex.Message}");

            // Check if this is a serialization/application data logic bug.
            // bad JSON mapping, missing columns, or bad fields, 
            // we must complete the transaction so a bad record doesn't block the queue forever.
            if (ex is Newtonsoft.Json.JsonException || ex is ArgumentException || ex is NullReferenceException)
            {
                Console.WriteLine("[SupabaseConnector] Structural data error detected. Skipping corrupted record.");
                await transaction.Complete();
                return;
            }

            // For ANY other error (Connection reset, Socket exceptions, HTTP drops, timeouts, Supabase faults), 
            // we MUST rethrow it. Do NOT call transaction.Complete().
            // This forces PowerSync to keep your changes safe in local storage and keep retrying natively.
            throw;
        }
    }

    private async Task HandlePut(string table, CrudEntry op)
    {
        if (table == "profiles")
        {
            var model = JsonConvert.DeserializeObject<SupabaseProfile>(JsonConvert.SerializeObject(op.OpData)) ?? new SupabaseProfile();
            model.Id = op.Id;
            await _supabase.From<SupabaseProfile>().Upsert(model);
        }
        else if (table == "schedules")
        {
            var model = JsonConvert.DeserializeObject<SupabaseSchedule>(JsonConvert.SerializeObject(op.OpData)) ?? new SupabaseSchedule();
            model.Id = op.Id;
            await _supabase.From<SupabaseSchedule>().Upsert(model);
        }
    }

    private async Task HandlePatch(string table, CrudEntry op)
    {
        if (op.OpData == null || op.OpData.Count == 0) return;
        if (table == "profiles")
        {
            var query = _supabase.From<SupabaseProfile>().Where(x => x.Id == op.Id);
            foreach (var kvp in op.OpData) query = SupabasePatchHelper.ApplySet(query, kvp.Key, kvp.Value!);
            await query.Update();
        }
        else if (table == "schedules")
        {
            var query = _supabase.From<SupabaseSchedule>().Where(x => x.Id == op.Id);
            foreach (var kvp in op.OpData) query = SupabasePatchHelper.ApplySet(query, kvp.Key, kvp.Value!);
            await query.Update();
        }
    }

    private async Task HandleDelete(string table, CrudEntry op)
    {
        if (table == "profiles")
        {
            await _supabase.From<SupabaseProfile>().Where(x => x.Id == op.Id).Delete();
        }
        else if (table == "schedules")
        {
            await _supabase.From<SupabaseSchedule>().Where(x => x.Id == op.Id).Delete();
        }
    }
}