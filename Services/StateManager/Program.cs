using BackgroundBotRunner;
using DecisionEngineService;
using PathfindingService;
using PromptHandlingService;

namespace StateManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    builder.AddEnvironmentVariables();

                    if (args != null)
                        builder.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<StateManagerWorker>();
                    services.AddHostedService<PathfindingServiceWorker>();
                    services.AddHostedService<DecisionEngineWorker>();
                    services.AddHostedService<PromptHandlingServiceWorker>();
                    services.AddTransient<BackgroundBotWorker>();
                });
    }
}
