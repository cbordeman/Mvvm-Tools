using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable ExplicitCallerInfoArgument

namespace MvvmTools.Core.ViewModels
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        //{
        //    add
        //    {
        //        ReactiveUI.WeakEventManager<BindableBase, PropertyChangedEventHandler, PropertyChangedEventArgs>.AddHandler(this, value);
        //    }
        //    remove
        //    {
        //        ReactiveUI.WeakEventManager<BindableBase, PropertyChangedEventHandler, PropertyChangedEventArgs>.RemoveHandler(this, value);
        //    }
        //}

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            // Important modification from the usual SetProperty() implementation: we always
            // raise the PropertyChanged event, though we still return false if the value
            // didn't change.  This was needed to fix where some bindings weren't updating
            // simply becuase the view model was a singleton and was being reused.
            // We also implement INotifyPropertyChainging.

            NotifyPropertyChanging(propertyName);

            var orig = storage;
            storage = value;
            NotifyPropertyChanged(propertyName);

            return !Equals(orig, value);
        }
        
        /// <summary>
        /// Override to consume own PropertyChanged events without having to subscribe.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void TakePropertyChanged(string propertyName)
        {
        }

        /// <summary>
        /// Override to consume own PropertyChanging events without having to subscribe.
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void TakePropertyChanging(string propertyName)
        {
        }
        
        protected void NotifyPropertyChanged(string propertyName)
        {
            //ReactiveUI.WeakEventManager<BindableBase, PropertyChangedEventHandler, PropertyChangedEventArgs>.DeliverEvent(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            TakePropertyChanged(propertyName);
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void NotifyPropertyChanging([CallerMemberName] string propertyName = null)
        {
            //ReactiveUI.WeakEventManager<BindableBase, PropertyChangingEventHandler, PropertyChangingEventArgs>.DeliverEvent(this, new PropertyChangingEventArgs(propertyName));
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
            TakePropertyChanging(propertyName);
        }

        public event PropertyChangingEventHandler PropertyChanging;
        //{
        //    add
        //    {
        //        ReactiveUI.WeakEventManager<BindableBase, PropertyChangingEventHandler, PropertyChangingEventArgs>.AddHandler(this, value);
        //    }
        //    remove
        //    {
        //        ReactiveUI.WeakEventManager<BindableBase, PropertyChangingEventHandler, PropertyChangingEventArgs>.RemoveHandler(this, value);
        //    }
        //}
    }
}
