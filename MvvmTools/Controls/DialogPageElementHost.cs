using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using MvvmTools.Utilities;

namespace MvvmTools.Controls
{
    /// <summary>
    /// Subclass of ElementHost designed to work around focus problems with ElementHost.
    /// </summary>
    class DialogPageElementHost : ElementHost
    { 
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_SETFOCUS)
            {
                IntPtr oldHandle = m.WParam;

                // Get the handle to the child WPF element that we are hosting
                // After that get the next and previous items that would fall before 
                // and after the WPF control in the tools->options page tabbing order
                var source = PresentationSource.FromVisual(Child) as HwndSource;
                if (source != null && oldHandle != IntPtr.Zero)
                {
                    IntPtr nextTabElement = GetNextFocusElement(source.Handle, forward: true);
                    IntPtr previousTabElement = GetNextFocusElement(source.Handle, forward: false);

                    UIElement rootElement = source.RootVisual as UIElement;

                    // If we tabbed back from the next element then set focus to the last item
                    if (rootElement != null && nextTabElement == oldHandle)
                    {
                        rootElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
                    }

                    // If we tabbed in from the previous element then set focus to the first item
                    else if (rootElement != null && previousTabElement == oldHandle)
                    {
                        rootElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                    }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Set up an IKeyboardInputSite that understands how to tab outside the WPF content.
            // (see the notes on DialogKeyboardInputSite for more detail).
            // NOTE: This should be done after calling base.OnHandleCreated, which is where
            // ElementHost sets up its own IKeyboardInputSite.
            HwndSource source = PresentationSource.FromVisual(Child) as HwndSource;
            if (source != null)
            {
                ((IKeyboardInputSink)source).KeyboardInputSite = new DialogKeyboardInputSite(source);
            }
        }

        // From a given handle get the next focus element either forward or backward
        internal static IntPtr GetNextFocusElement(IntPtr handle, bool forward)
        {
            IntPtr hDlg = NativeMethods.GetAncestor(handle, NativeMethods.GA_ROOT);
            if (hDlg != IntPtr.Zero)
            {
                // Find the next dialog item in the parent dialog (searching in the correct direction)
                // This can return IntPtr.Zero if there are no more items in that direction
                return NativeMethods.GetNextDlgTabItem(hDlg, handle, !forward);
            }

            return IntPtr.Zero;
        }
    }

    /// <summary>
    /// The default IKeyboardInputSite that ElementHost uses relies on being hosted
    /// in a pure Windows Forms window for tabbing outside the ElementHost's WPF content.
    /// However, this DialogPageElementHost is hosted inside a Win32 dialog, and should
    /// rely on the Win32 navigation logic directly.  This replaces the default
    /// IKeyboardInputSite with one that has specialized handling for OnNoMoreTabStops.
    /// </summary>
    class DialogKeyboardInputSite : IKeyboardInputSite
    {
        HwndSource _source;
        public DialogKeyboardInputSite(HwndSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets the IKeyboardInputSink associated with this site.
        /// </summary>
        public IKeyboardInputSink Sink
        {
            get
            {
                return _source;
            }
        }

        public void Unregister()
        {
            // We have nothing to unregister, so do nothing.
        }

        public bool OnNoMoreTabStops(TraversalRequest request)
        {
            // First, determine if we are tabbing forward or backwards
            // outside of our content.
            bool forward = true;
            if (request != null)
            {
                switch (request.FocusNavigationDirection)
                {
                    case FocusNavigationDirection.Next:
                    case FocusNavigationDirection.Right:
                    case FocusNavigationDirection.Down:
                        forward = true;
                        break;

                    case FocusNavigationDirection.Previous:
                    case FocusNavigationDirection.Left:
                    case FocusNavigationDirection.Up:
                        forward = false;
                        break;
                }
            }

            // Based on the direction, tab forward or backwards in our parent dialog.
            IntPtr nextHandle = DialogPageElementHost.GetNextFocusElement(_source.Handle, forward);
            if (nextHandle != IntPtr.Zero)
            {
                // If we were able to find another control, send focus to it and inform
                // WPF that we moved focus outside the HwndSource.
                NativeMethods.SetFocus(nextHandle);
                return true;
            }

            // If we couldn't find a dialog item to focus, inform WPF that it should
            // continue cycling inside its own tab order.
            return false;
        }
    }
}
