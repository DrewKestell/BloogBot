using BloogBot.Game;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BloogBot.UI
{
    public partial class MainWindow : Window
    {
        private static MainViewModel mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = (MainViewModel)DataContext;
        }
    }
}
