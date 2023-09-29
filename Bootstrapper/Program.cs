using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace Bootstrapper
{
    class Program
    {
        class App : Application
        {
            private readonly SocketServer _socketServer;
            private readonly BootstrapperSettings _bootstrapperSettings;

            public App(SocketServer socketServer, BootstrapperSettings bootstrapperSettings)
            {
                _socketServer = socketServer;
                _bootstrapperSettings = bootstrapperSettings;
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                var mainWindow = new MainWindow(_socketServer, _bootstrapperSettings);
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
            var bootstrapperSettingsFilePath = Path.Combine(currentFolder, "bootstrapperSettings.json");
            var bootstrapperSettings = JsonConvert.DeserializeObject<BootstrapperSettings>(File.ReadAllText(bootstrapperSettingsFilePath));

            LaunchUI(LaunchServer(bootstrapperSettings), bootstrapperSettings);
        }

        private static void LaunchUI(SocketServer launchServer, BootstrapperSettings bootstrapperSettings)
        {
            Application app = new App(launchServer, bootstrapperSettings);

            app.Run();
        }

        private static SocketServer LaunchServer(BootstrapperSettings bootstrapperSettings)
        {
            var socketServer = new SocketServer(bootstrapperSettings.Port, IPAddress.Parse(bootstrapperSettings.ListenAddress));
            socketServer.Start();
            return socketServer;
        }
    }
}
