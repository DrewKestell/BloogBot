using System;
using System.Windows;
using System.Windows.Threading;

namespace BloogBot.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var context = (MainViewModel)DataContext;
            context.InitializeObjectManager();

            // make sure the output window stays scrolled to the bottom
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += ((sender, e) =>
            {
                if (Console.VerticalOffset == Console.ScrollableHeight)
                    Console.ScrollToEnd();
            });
            timer.Start();
        }
    }
}
