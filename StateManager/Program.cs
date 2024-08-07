using ActivityManager;
using StateManager;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<StateManagerServiceWorker>();
                services.AddHostedService(provider => provider.GetRequiredService<ActivityManagerServiceWorker>());
            });
}
