using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BloogBot.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            Debugger.Launch();
#endif

            // Force software rendering so WPF does not create a second in-process Direct3D9 device
            // on its render thread. D3D9 hooks/overlays (such as OBS graphics-hook32.dll) assume a
            // single device per process and can release WoW's single-threaded D3D9 resources from
            // WPF's render thread, deadlocking the graphics driver and freezing WoW.
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            WardenDisabler.Initialize();

            var mainWindow = new MainWindow();
            Current.MainWindow = mainWindow;
            mainWindow.Closed += (sender, args) => { Environment.Exit(0); };
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
