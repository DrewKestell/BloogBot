using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace RaidLeaderBot
{
    class Program
    {
        class App : Application
        {
            private readonly CommandSockerServer _socketServer;
            private readonly NavigationSocketServer _pathfindingSocketServer;
            private readonly DatabaseSocketServer _databaseSocketServer;
            private readonly RaidLeaderBotSettings _raidLeaderBotSettings;

            public App(CommandSockerServer socketServer, RaidLeaderBotSettings raidLeaderBotSettings)
            {
                _socketServer = socketServer; 
                _raidLeaderBotSettings = raidLeaderBotSettings;

                _pathfindingSocketServer = new NavigationSocketServer(raidLeaderBotSettings.PathfindingPort, IPAddress.Parse(raidLeaderBotSettings.ListenAddress));
                _pathfindingSocketServer.Start();

                _databaseSocketServer = new DatabaseSocketServer(raidLeaderBotSettings.DatabasePort, IPAddress.Parse(raidLeaderBotSettings.ListenAddress));
                _databaseSocketServer.Start();
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                var mainWindow = new MainWindow(_socketServer, _raidLeaderBotSettings);
                Current.MainWindow = mainWindow;
                mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
                mainWindow.Show();

                base.OnStartup(e);
            }
        }
        
        [STAThread]
        static void Main()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var raidLeaderBotSettingsFilePath = Path.Combine(currentFolder, "Settings\\raidLeaderBotSettings.json");
            var raidLeaderBotSettings = JsonConvert.DeserializeObject<RaidLeaderBotSettings>(File.ReadAllText(raidLeaderBotSettingsFilePath));

            LaunchUI(LaunchServer(raidLeaderBotSettings), raidLeaderBotSettings);
        }

        private static void LaunchUI(CommandSockerServer launchServer, RaidLeaderBotSettings raidLeaderBotSettings)
        {
            Application app = new App(launchServer, raidLeaderBotSettings);

            app.Run();
        }

        private static CommandSockerServer LaunchServer(RaidLeaderBotSettings raidLeaderBotSettings)
        {
            var socketServer = new CommandSockerServer(raidLeaderBotSettings.CommandPort, IPAddress.Parse(raidLeaderBotSettings.ListenAddress));
            socketServer.Start();
            return socketServer;
        }
    }
}
