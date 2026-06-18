//using Java.Net;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using PowerSync.Common.Client;
using PowerSync.Common.MDSQLite;
using PowerSync.Maui.SQLite;
using Supabase;
using TMSleep.Services;

namespace TMSleep
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
           
            var connector = new SupabaseConnector();
            builder.Services.AddSingleton<SupabaseConnector>(connector);
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthStateProvider>();

            // ── Repositories ──────────────────────────────────────────────
            // Singleton so they share the same DatabaseService instance and
            // don't re-open the SQLite file on every page navigation.
            builder.Services.AddSingleton<ProfileRepository>();
            builder.Services.AddSingleton<ScheduleRepository>();

            // ── Pages ─────────────────────────────────────────────────────
            // Transient: a fresh instance each time the page is navigated to.
            // (Add your pages here as you create them.)
            builder.Services.AddTransient<MainPage>();
            // builder.Services.AddTransient<MainViewModel>();


#if ANDROID
            builder.Services.AddSingleton<IPermissionService, TMSleep.Platforms.Android.AndroidPermissionService>();
            builder.Services.AddSingleton<IAlarmScheduler, TMSleep.Platforms.Android.AndroidAlarmScheduler>();
#endif

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
