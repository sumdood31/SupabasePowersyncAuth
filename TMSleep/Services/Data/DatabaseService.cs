using Microsoft.Extensions.Logging;
using PowerSync.Common.Client;
using PowerSync.Common.MDSQLite;
using PowerSync.Maui.SQLite;
using Supabase;

namespace TMSleep.Services;

public class DatabaseService
{
    public PowerSyncDatabase Db { get; }
    public SupabaseConnector Connector { get; }

    public DatabaseService(SupabaseConnector connector)
    {
        Connector = connector;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TMSleep.db");
        var factory = new MAUISQLiteDBOpenFactory(new MDSQLiteOpenFactoryOptions { DbFilename = dbPath });

        ILoggerFactory loggerFactory = LoggerFactory.Create(b => {
            b.AddDebug();
            b.SetMinimumLevel(LogLevel.Information);
        });

        Db = new PowerSyncDatabase(new PowerSyncDatabaseOptions
        {
            Database = factory,
            Schema = AppSchema.PowerSyncSchema,
            Logger = loggerFactory.CreateLogger("PowerSync"),
        });

        Connectivity.Current.ConnectivityChanged += OnNetworkConnectivityChanged;

    }

    private async void OnNetworkConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {

        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            try
            {
                Connector.Client.Auth.Online = true;
                // Verify if we have a valid logged-in session locally
                if (Connector.Client.Auth.CurrentSession != null)
                {
                    Console.WriteLine("[Network Monitor] Session active. Waking up PowerSync stream...");
                   
                    // Force PowerSync to reconnect. 
                    // This wakes up the sync engine, flushes the UploadData queue, and pulls down fresh changes!
                    await Db.Connect(Connector);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Network Monitor] Failed to wake up sync engine: {ex.Message}");
            }
        }
        else
        {
            Connector.Client.Auth.Online = false;
            //make sure to tell powersync not to try and push changes to supabase
            await Db.Disconnect();

            Console.WriteLine("[Network Monitor] Internet disconnected. Syncing paused.");
        }
    }

    public async Task<bool> AppStartupSequenceAsync()
    {
        // Wait for Supabase client setup to finish internally
        await Connector.InitializationTask;
        
        // Bring up local SQLite structure
        await Db.Init();

        //Tell the app it's current network status
        if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
        {  Connector.Client.Auth.Online = true; }
        else
        { Connector.Client.Auth.Online = false; }

        bool isLoggedIn = await Task.Run(async () =>
        {
            bool restored = await Connector.TryAutoLogin();

            if (restored)
            {
                try
                {
                    if (Connector.Client.Auth.Online)
                    {
                        //tell powersync it can push changes to supabase
                        await Db.Connect(Connector);

                    }
                    Console.WriteLine("PowerSync live sync stream connected successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PowerSync Offline Boot Warning]: {ex.Message}");
                }
            }
            return restored;
        });



        return isLoggedIn;
    }


    // specifically for the manual Login UI
    public async Task LoginAndSyncAsync(string email, string password)
    {
        if (Connector.Client.Auth.Online)
        {
            await Connector.LoginAsync(email, password);
            await Db.Connect(Connector);
        }
        else
        {
            throw new Exception("Must have access to the internet to login");
        }
    }
}