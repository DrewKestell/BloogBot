using RaidMemberBot.AI;
using System;
using System.Windows;
using System.Windows.Threading;

namespace RaidMemberBot.UI
{
    public partial class App : Application
    {
        private BotRunner _botRunner;
        protected override void OnStartup(StartupEventArgs e)
        {
            _botRunner = new BotRunner();

            base.OnStartup(e);
        }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
