using System.Windows.Input;

namespace WoWStateManagerUI
{
    internal class CommandHandler(Action action, bool canExecute) : ICommand
    {
        private readonly Action action = action;
        private readonly bool canExecute = canExecute;

        public void Execute(object parameter) => action();

        public bool CanExecute(object parameter) => canExecute;

        // this needs to be here to satisfy the compiler even though we never use it
#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067
    }
}
