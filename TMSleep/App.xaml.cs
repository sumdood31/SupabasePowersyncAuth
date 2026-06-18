namespace TMSleep
{
    public partial class App : Application
    {
       // private readonly Services.DatabaseService _dbService;

        public App()
        {
            InitializeComponent();
            //_dbService = dbService;
        }

        protected override async void OnStart()
        {
            //base.OnStart();

            //try
            //{
            //    // Force the app to completely finish loading storage and tokens
            //    bool userWasRestored = await _dbService.AppStartupSequenceAsync();

            //    // The session is now safely loaded into the Supabase C# Client SDK memory.
            //    // When Blazor kicks off, the AuthStateProvider will see the user instantly!

            //    if (!userWasRestored)
            //    {
            //        Console.WriteLine("No session found. Blazor routing engine will handle login redirection.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"[Critical Startup Failure]: {ex.Message}");
            //}
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "TMSleep" };
        }
    }
}
