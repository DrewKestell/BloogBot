using System;
using System.Windows;

namespace RaidLeaderBot
{
    class Program
    {
        class App : Application
        {
            public App()
            {
            }

            protected override void OnStartup(StartupEventArgs e)
            {
                RaidActivityManagerWindow mainWindow = new RaidActivityManagerWindow();
                Current.MainWindow = mainWindow;
                mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
                mainWindow.Show();

                base.OnStartup(e);
            }
        }

        [STAThread]
        static void Main()
        {
            Application app = new App();

            app.Run();
        }
    }
}
