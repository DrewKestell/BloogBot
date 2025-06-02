using System.Windows.Input;

namespace StateManagerUI.Handlers
{
    internal class CommandHandler(Action action, bool canExecute) : ICommand
    {
        private readonly Action action = action;
        private readonly bool canExecute = canExecute;

        public void Execute(object parameter) => action();

        public bool CanExecute(object parameter) => canExecute;

        public event EventHandler? CanExecuteChanged;
    }
}
