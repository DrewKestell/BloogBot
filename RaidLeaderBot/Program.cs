using System;
using System.Net;
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

            public App(CommandSockerServer socketServer)
            {
                _socketServer = socketServer;

                _pathfindingSocketServer = new NavigationSocketServer(RaidLeaderBotSettings.Instance.PathfindingPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
                _pathfindingSocketServer.Start();

                _databaseSocketServer = new DatabaseSocketServer(RaidLeaderBotSettings.Instance.DatabasePort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
                _databaseSocketServer.Start();
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                MainWindow mainWindow = new MainWindow(_socketServer);
                Current.MainWindow = mainWindow;
                mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
                mainWindow.Show();

                base.OnStartup(e);
            }
        }

        [STAThread]
        static void Main()
        {
            LaunchUI(LaunchServer());
        }

        private static void LaunchUI(CommandSockerServer launchServer)
        {
            Application app = new App(launchServer);

            app.Run();
        }

        private static CommandSockerServer LaunchServer()
        {
            CommandSockerServer socketServer = new CommandSockerServer(RaidLeaderBotSettings.Instance.CommandPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
            socketServer.Start();
            return socketServer;
        }
    }
}
