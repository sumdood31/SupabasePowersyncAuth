using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace TMSleep.Services
{
    public class SupabaseOfflineSession : IGotrueSessionPersistence<Session>
    {
        private const string SessionStorageKey = "supabase_TMSleep_session";

        //THIS CLASS WAS THE ONLY WAY I COULD GET OFFLINE AUTH WORKING CORRECTLY

        // Saves the current session to local secure storage.
        public void SaveSession(Session session)
        {
            try
            {
                if (session == null) return;
                var json = JsonConvert.SerializeObject(session); //
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await SecureStorage.Default.SetAsync(SessionStorageKey, json);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save session: {ex.Message}");
            }
        }

        // Deletes the local session when a user explicitly logs out.
        public void DestroySession()
        {
            try
            {
                SecureStorage.Default.Remove(SessionStorageKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to destroy session: {ex.Message}");
            }
        }

        // Retrieves the session from storage. 
        // Note: The interface method is synchronous, so we retrieve the Task Result synchronously.
        public Session? LoadSession()
        {
            try
            {
                // SecureStorage is asynchronous, but Gotrue demands a synchronous return here
                var jsonTask = SecureStorage.Default.GetAsync(SessionStorageKey);
                var json = jsonTask.Result;

                if (string.IsNullOrEmpty(json)) return null;

                return JsonConvert.DeserializeObject<Session>(json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load session: {ex.Message}");
                return null;
            }
        }

        public Session? SetOnlineSession()
        {
            try { 
            var json = SecureStorage.Default.GetAsync(SessionStorageKey);
            var sessionJson = json.Result;
            if (string.IsNullOrEmpty(sessionJson)) return null;

                return JsonConvert.DeserializeObject<Session>(sessionJson);
 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load session: {ex.Message}");
                return null;
            }
        }
    }
}
