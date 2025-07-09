using BackgroundBotRunner;
using DecisionEngineService;
using PromptHandlingService;

namespace StateManager
{
    public class Program
    {
        private static IConfiguration _configuration;

        public static void Main(string[] args)
        {
            // Build configuration
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Launch PathfindingService if not already running
            if (!IsPathfindingServiceRunning())
            {
                PathfindingService.Program.LaunchServiceFromCommandLine();
            }

            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        private static bool IsPathfindingServiceRunning()
        {
            try
            {
                // Read PathfindingService connection info from appsettings.json
                var ipAddress = _configuration["PathfindingService:IpAddress"];
                var port = int.Parse(_configuration["PathfindingService:Port"]);

                using var client = new System.Net.Sockets.TcpClient();
                var result = client.BeginConnect(ipAddress, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(200));
                if (success)
                {
                    client.EndConnect(result);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
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
                    services.AddHostedService<DecisionEngineWorker>();
                    services.AddHostedService<PromptHandlingServiceWorker>();
                    services.AddTransient<BackgroundBotWorker>();
                });
    }
}
