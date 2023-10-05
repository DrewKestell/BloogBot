using System.Windows;

namespace RaidMemberBot.UI
{
    public partial class MainWindow : Window
    {
        private static RaidMemberViewModel mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = (RaidMemberViewModel)DataContext;
        }
    }
}
