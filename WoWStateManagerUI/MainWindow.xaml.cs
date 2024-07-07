using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WoWStateManagerUI.Views;

namespace WoWStateManagerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WoWStateManagerViewModel _worldStatePresetViewModel;
        public MainWindow()
        {
            _worldStatePresetViewModel = new WoWStateManagerViewModel();
            DataContext = _worldStatePresetViewModel;
        }

        private void ActivityMember_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GroupBox senderControl = (GroupBox)sender;
            _worldStatePresetViewModel.SelectedActivityMemberIndex = _worldStatePresetViewModel.SelectedActivityMemberViewModels.IndexOf((ActivityMemberViewModel)senderControl.DataContext);
        }
        private void Activity_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GroupBox senderControl = (GroupBox)sender;
            _worldStatePresetViewModel.SelectedActivityIndex = _worldStatePresetViewModel.ActivityViewModels.IndexOf((ActivityViewModel)senderControl.DataContext);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            _worldStatePresetViewModel.EditActivityMember(textBox.Name.Replace("TextBox", "Name"), textBox.Text);
        }

        private void InstanceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _worldStatePresetViewModel.EditActivity();
        }
    }
}