using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MvvmTools.Core.Utilities
{
    /// <summary>
    /// Use a DelegateCommand to implement an ICommand by simply passing CanExecute-/Execute-delegates.
    /// </summary>
    /// <remarks>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd458928.aspx">Prism DelegateCommand<T></see>
    /// <seealso cref="http://www.wpftutorial.net/DelegateCommand.html">How to implement a reusable ICommand</seealso>
    /// 
    /// If you are using the MVVM (Model-Scene-SceneModel) pattern, one of the most used mechanism to bind actions 
    /// to the view are commands. To provide a command, you have to implement the ICommand interface.
    /// 
    /// The idea of this pattern build an universal command, that takes two delegates: 
    /// One that is called, when ICommand.Execute(object param) is invoked,
    /// and one that evaluates the state of the command when ICommand.CanExecute(object param) is called.
    /// 
    /// In addition to this, we need a method, that triggers the CanExecuteChanged event. 
    /// This causes the UI element to reevaluate the CanExecute() of the command.
    /// </remarks>
    public class DelegateCommand : ICommand // where T : class
    {
        #region Fields

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        private List<WeakReference> _canExecuteChangedHandlers;
        private bool _isAutomaticRequeryDisabled;

        #endregion

        #region Properties

        /// <summary>
        ///     Property to enable or disable CommandManager's automatic requery on this command
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return _isAutomaticRequeryDisabled;
            }
            set
            {
                if (_isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        WeakRefEventManager.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);
                    }
                    else
                    {
                        WeakRefEventManager.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);
                    }
                    _isAutomaticRequeryDisabled = value;
                }
            }
        }

        #endregion

        #region ICommand implementation

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                WeakRefEventManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                WeakRefEventManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
            }
        }

        #endregion

        #region Constructors

        public DelegateCommand(Action execute)
            : this(execute, null, false)
        { }

        public DelegateCommand(Action execute, Func<bool> canExecute)
            : this(execute, canExecute, false)
        { }

        public DelegateCommand(Action execute, Func<bool> canExecute, bool isAutomaticRequeryDisabled)
        {
            _execute = execute;
            _canExecute = canExecute;
            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Methods

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakRefEventManager.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
        }

        #endregion
    }

    /// <summary>
    /// Use a DelegateCommand to implement an ICommand by simply passing CanExecute-/Execute-delegates.
    /// </summary>
    /// <remarks>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd458928.aspx">Prism DelegateCommand<T></see>
    /// <seealso cref="http://www.wpftutorial.net/DelegateCommand.html">How to implement a reusable ICommand</seealso>
    /// 
    /// If you are using the MVVM (Model-Scene-SceneModel) pattern, one of the most used mechanism to bind actions 
    /// to the view are commands. To provide a command, you have to implement the ICommand interface.
    /// 
    /// The idea of this pattern build an universal command, that takes two delegates: 
    /// One that is called, when ICommand.Execute(object param) is invoked,
    /// and one that evaluates the state of the command when ICommand.CanExecute(object param) is called.
    /// 
    /// In addition to this, we need a method, that triggers the CanExecuteChanged event. 
    /// This causes the UI element to reevaluate the CanExecute() of the command.
    /// </remarks>
    public class DelegateCommand<T> : ICommand // where T : class
    {
        #region Fields

        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;
        private List<WeakReference> _canExecuteChangedHandlers;
        private bool _isAutomaticRequeryDisabled;

        #endregion

        #region Properties

        /// <summary>
        ///     Property to enable or disable CommandManager's automatic requery on this command
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return _isAutomaticRequeryDisabled;
            }
            set
            {
                if (_isAutomaticRequeryDisabled != value)
                {
                    if (value)
                    {
                        WeakRefEventManager.RemoveHandlersFromRequerySuggested(_canExecuteChangedHandlers);
                    }
                    else
                    {
                        WeakRefEventManager.AddHandlersToRequerySuggested(_canExecuteChangedHandlers);
                    }
                    _isAutomaticRequeryDisabled = value;
                }
            }
        }

        #endregion

        #region ICommand implementation

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        /// <summary>
        ///     ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested += value;
                }
                WeakRefEventManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                if (!_isAutomaticRequeryDisabled)
                {
                    CommandManager.RequerySuggested -= value;
                }
                WeakRefEventManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
            }
        }

        #endregion

        #region Constructors

        public DelegateCommand(Action<T> execute)
            : this(execute, null, false)
        { }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
            : this(execute, canExecute, false)
        { }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute, bool isAutomaticRequeryDisabled)
        {
            _execute = execute;
            _canExecute = canExecute;
            _isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }

        #endregion

        #region Methods

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        ///     Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakRefEventManager.CallWeakReferenceHandlers(_canExecuteChangedHandlers);
        }

        #endregion
    }

}
