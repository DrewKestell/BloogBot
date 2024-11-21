using StateManagerUI.Views;
using System.Windows;
using System.Windows.Controls;

namespace StateManagerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StateManagerViewModel _worldStatePresetViewModel;
        public MainWindow()
        {
            _worldStatePresetViewModel = new StateManagerViewModel();
            DataContext = _worldStatePresetViewModel;
        }

        private void ActivityMember_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GroupBox senderControl = (GroupBox)sender;

        }
        private void Activity_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GroupBox senderControl = (GroupBox)sender;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

        }

        private void InstanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}