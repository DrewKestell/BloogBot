using System.Windows;

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
