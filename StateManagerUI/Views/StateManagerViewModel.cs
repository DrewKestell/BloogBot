using Microsoft.Extensions.Logging;
using StateManagerUI.Handlers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace StateManagerUI.Views
{
    public sealed class StateManagerViewModel : INotifyPropertyChanged
    {
        public StateManagerViewModel() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
