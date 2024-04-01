using RaidMemberBot.AI;
using RaidMemberBot.Mem.AntiWarden;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace RaidMemberBot
{
    public partial class App : Application
    {
        private BotRunner _botRunner;
        protected override void OnStartup(StartupEventArgs e)
        {
            WardenDisabler.Initialize();
            _botRunner = new BotRunner();

            base.OnStartup(e);
        }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var name = args.Name.Split(',')[0];
            var assembly = Assembly.Load(name) ?? currentAssembly;
            return assembly;
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
