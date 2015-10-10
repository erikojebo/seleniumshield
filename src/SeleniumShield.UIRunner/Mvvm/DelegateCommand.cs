using System;
using System.Windows.Input;

namespace SeleniumShield.UIRunner.Mvvm
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public DelegateCommand(Action<object> execute)
            : this(execute, null) { }

        public DelegateCommand(Action execute) 
            : this(x => execute()) { }

        public DelegateCommand(Action execute, Func<bool> canExecute)
            : this(x => execute(), x => canExecute()) { }

        public DelegateCommand(Action<object> execute,
                               Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}