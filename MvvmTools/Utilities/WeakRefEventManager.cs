using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MvvmTools.Utilities
{
    public static class WeakRefEventManager
    {
        #region Methods

        internal static void AddHandlersToRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers == null) return;

            foreach (var handlerRef in handlers)
            {
                var handler = handlerRef.Target as EventHandler;
                if (handler != null)
                    CommandManager.RequerySuggested += handler;
            }
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler)
        {
            AddWeakReferenceHandler(ref handlers, handler, -1);
        }

        internal static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)
        {
            if (handlers == null)
                handlers = (defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());

            handlers.Add(new WeakReference(handler));
        }

        internal static void CallWeakReferenceHandlers(List<WeakReference> handlers)
        {
            if (handlers == null) return;

            // Take a snapshot of the handlers before we call out to them since the handlers
            // could cause the array to be modified while we are reading it.

            EventHandler[] callees = new EventHandler[handlers.Count];
            var count = 0;

            for (var i = handlers.Count - 1; i >= 0; i--)
            {
                var reference = handlers[i];
                var handler = reference.Target as EventHandler;
                if (handler == null)
                {
                    // Clean up old handlers that have been collected
                    handlers.RemoveAt(i);
                }
                else
                {
                    callees[count] = handler;
                    count++;
                }
            }

            // Call the handlers that we snapshotted
            for (var i = 0; i < count; i++)
            {
                EventHandler handler = callees[i];
                handler(null, EventArgs.Empty);
            }
        }

        internal static void RemoveHandlersFromRequerySuggested(List<WeakReference> handlers)
        {
            if (handlers == null) return;

            foreach (var handlerRef in handlers)
            {
                var handler = handlerRef.Target as EventHandler;
                if (handler != null)
                {
                    CommandManager.RequerySuggested -= handler;
                }
            }
        }

        internal static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
        {
            if (handlers == null) return;

            for (var i = handlers.Count - 1; i >= 0; i--)
            {
                var reference = handlers[i];
                var existingHandler = reference.Target as EventHandler;
                if ((existingHandler == null) || (existingHandler == handler))
                {
                    // Clean up old handlers that have been collected
                    // in addition to the handler that is to be removed.
                    handlers.RemoveAt(i);
                }
            }
        }

        #endregion
    }
}
