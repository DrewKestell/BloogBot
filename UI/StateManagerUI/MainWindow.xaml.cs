using StateManagerUI.Views;
using System.Windows;

namespace StateManagerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StateManagerViewModel _stateManagerViewModel;
        public MainWindow()
        {
            _stateManagerViewModel = new StateManagerViewModel();
            DataContext = _stateManagerViewModel;
        }
    }
}