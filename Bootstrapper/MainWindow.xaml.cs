
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Bootstrapper
{
    public partial class MainWindow : Window
    {
        private readonly SocketServer _socketServer;
        
        public MainWindow(SocketServer socketServer)
        {
            _socketServer = socketServer;
            InitializeComponent();
            _socketServer.InstanceUpdateObservable.Subscribe(OnInstanceUpdate);
        }

        private void OnInstanceUpdate(InstanceUpdate update)
        {
            Dispatcher.Invoke(() =>
            {
                ProofTextBlock.Text = update.CurrentPosition;
            });
        }
    }
}
