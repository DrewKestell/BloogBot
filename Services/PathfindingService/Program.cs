using System.Diagnostics;

namespace PathfindingService
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
                    services.AddHostedService<PathfindingServiceWorker>();
                });

        /// <summary>
        /// Launches the PathfindingService as an external process
        /// </summary>
        public static void LaunchServiceFromCommandLine()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "PathfindingService.dll",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                Process.Start(processInfo);
                Console.WriteLine("PathfindingService has been launched externally.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to launch PathfindingService: {ex.Message}");
            }
        }
    }
}
