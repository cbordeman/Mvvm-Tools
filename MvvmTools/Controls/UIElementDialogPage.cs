// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Utilities;
using ComboBox = System.Windows.Controls.ComboBox;
using Forms = System.Windows.Forms;
using IWin32Window = System.Windows.Forms.IWin32Window;
using TextBox = System.Windows.Controls.TextBox;

namespace MvvmTools.Controls
{
    /// <summary>
    /// Event args used by <see cref="UIElementDialogPage.DialogKeyPendingEvent"/>.
    /// </summary>
    public class DialogKeyEventArgs : RoutedEventArgs
    {
        internal DialogKeyEventArgs(RoutedEvent evt, Key key)
          : base(evt)
        {
            Key = key;
        }

        /// <summary>
        /// Gets the key being pressed within the UIElementDialogPage.
        /// </summary>
        public Key Key
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Class which is used to seamlessly host WPF content inside a native dialog
    /// running an IsDialogMessage-style message loop.  UIElementDialogPage enables
    /// tabbing into and out of the WPF child HWND, and enables keyboard navigation
    /// within the WPF child HWND.
    /// </summary>
    public abstract class UIElementDialogPage : DialogPage
    {
        /// <summary>
        /// Routed event used to determine whether or not key input in the dialog should be handled by the dialog or by
        /// the content of this page.  If this event is marked as handled, the keypress should be handled by the content,
        /// and DLGC_WANTALLKEYS will be returned from WM_GETDLGCODE.  If the event is not handled, then only arrow keys,
        /// tabbing, and character input will be handled within this dialog page.
        /// </summary>
        public static readonly RoutedEvent DialogKeyPendingEvent = EventManager.RegisterRoutedEvent("DialogKeyPending", RoutingStrategy.Bubble, typeof(EventHandler<DialogKeyEventArgs>), typeof(UIElementDialogPage));

        private ElementHost m_elementHost;

        static UIElementDialogPage()
        {
            // Common controls that require centralized handling should have handlers here.
            EventManager.RegisterClassHandler(typeof(TextBoxEx), DialogKeyPendingEvent, (EventHandler<DialogKeyEventArgs>)HandleTextBoxDialogKey);
            EventManager.RegisterClassHandler(typeof(TextBox), DialogKeyPendingEvent, (EventHandler<DialogKeyEventArgs>)HandleTextBoxDialogKey);

            EventManager.RegisterClassHandler(typeof(ComboBox), DialogKeyPendingEvent, (EventHandler<DialogKeyEventArgs>)HandleComboBoxDialogKey);
            EventManager.RegisterClassHandler(typeof(DatePicker), DialogKeyPendingEvent, (EventHandler<DialogKeyEventArgs>)HandleDatePickerDialogKey);
        }

        /// <summary>
        /// Returns the handle to the UI control hosted in the ToolsOption page.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                if (m_elementHost != null) return m_elementHost;

                m_elementHost = new DialogPageElementHost {Dock = Forms.DockStyle.Fill};

                var child = Child;
                if (child != null)
                {
                    // The child is the root of a visual tree, so it has no parent from whom to 
                    // inherit its TextFormattingMode; set it appropriately.
                    // NOTE: We're setting this value on an element we didn't create; we should consider
                    // creating a wrapping ContentPresenter to nest the external Visual in.
                    TextOptions.SetTextFormattingMode(child, TextFormattingMode.Display);

                    HookChildHwndSource(child);
                    m_elementHost.Child = child;
                }

                return m_elementHost;
            }
        }

        /// <summary>
        /// Gets the WPF child element to be hosted inside the dialog page.
        /// </summary>
        protected abstract UIElement Child
        {
            get;
        }

        /// <summary>
        /// Observes for HwndSource changes on the given UIElement,
        /// and adds and removes an HwndSource hook when the HwndSource
        /// changes.
        /// </summary>
        /// <param name="child">The UIElement to observe.</param>
        static void HookChildHwndSource(UIElement child)
        {
            // The delegate reference is stored on the UIElement, and the lifetime
            // of the child is equal to the lifetime of this UIElementDialogPage,
            // so we are not leaking memory by not calling RemoveSourceChangedHandler.
            PresentationSource.AddSourceChangedHandler(child, OnSourceChanged);
        }

        static void OnSourceChanged(object sender, SourceChangedEventArgs e)
        {
            var oldSource = e.OldSource as HwndSource;
            var newSource = e.NewSource as HwndSource;
            oldSource?.RemoveHook(SourceHook);
            newSource?.AddHook(SourceHook);
        }

        static IntPtr SourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle WM_GETDLGCODE in order to allow for arrow and tab navigation inside the dialog page.
            // By returning this code, Windows will pass arrow and tab keys to our HWND instead of handling
            // them for its own default tab and directional navigation.
            switch (msg)
            {
                case NativeMethods.WM_GETDLGCODE:
                    var dlgCode = NativeMethods.DLGC_WANTARROWS | NativeMethods.DLGC_WANTTAB | NativeMethods.DLGC_WANTCHARS;

                    // Ask the currently-focused element if it wants to handle all keys or not.  The DialogKeyPendingEvent
                    // is a routed event starting with the focused control.  If any control in the route handles
                    // this message, then we'll add DLGC_WANTALLKEYS to request that this pending message
                    // be delivered to our content instead of the default dialog procedure.
                    var currentElement = Keyboard.FocusedElement;
                    if (currentElement != null)
                    {
                        var args = new DialogKeyEventArgs(DialogKeyPendingEvent, KeyInterop.KeyFromVirtualKey(wParam.ToInt32()));
                        currentElement.RaiseEvent(args);

                        if (args.Handled)
                        {
                            dlgCode |= NativeMethods.DLGC_WANTALLKEYS;
                        }
                    }

                    handled = true;
                    return new IntPtr(dlgCode);
            }

            return IntPtr.Zero;
        }

        private static void HandleTextBoxDialogKey(object sender, DialogKeyEventArgs e)
        {
            // Eat Enter and Escape rather than allowing the default button
            // or cancel button to be invoked.
            if ((e.Key == Key.Enter || e.Key == Key.Escape))
                e.Handled = true;
        }

        private static void HandleComboBoxDialogKey(object sender, DialogKeyEventArgs e)
        {
            // If the ComboBox is dropped down and Enter or Escape are pressed, we should
            // cancel or commit the selection change rather than allowing the default button
            // or cancel button to be invoked.
            var comboBox = (ComboBox)sender;
            if ((e.Key == Key.Enter || e.Key == Key.Escape) && comboBox.IsDropDownOpen)
            {
                e.Handled = true;
            }
        }

        private static void HandleDatePickerDialogKey(object sender, DialogKeyEventArgs e)
        {
            // If the DatePicker is dropped down and Enter or Escape are pressed, we should
            // cancel or commit the selection change rather than allowing the default button
            // or cancel button to be invoked.
            var datePicker = (DatePicker)sender;
            if ((e.Key == Key.Enter || e.Key == Key.Escape) && datePicker.IsDropDownOpen)
            {
                e.Handled = true;
            }
        }

        protected void MoveFocusToNext()
        {
            Child?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        /// <summary>
        /// Subclass of ElementHost designed to work around focus problems with ElementHost.
        /// </summary>
        class DialogPageElementHost : ElementHost
        {
            protected override void WndProc(ref Forms.Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == NativeMethods.WM_SETFOCUS)
                {
                    var oldHandle = m.WParam;

                    // Get the handle to the child WPF element that we are hosting
                    // After that get the next and previous items that would fall before 
                    // and after the WPF control in the tools->options page tabbing order
                    var source = PresentationSource.FromVisual(Child) as HwndSource;
                    if (source != null && oldHandle != IntPtr.Zero)
                    {
                        var nextTabElement = GetNextFocusElement(source.Handle, forward: true);
                        var previousTabElement = GetNextFocusElement(source.Handle, forward: false);

                        var rootElement = source.RootVisual as UIElement;

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
                var source = PresentationSource.FromVisual(Child) as HwndSource;
                if (source != null)
                {
                    ((IKeyboardInputSink)source).KeyboardInputSite = new DialogKeyboardInputSite(source);
                }
            }

            // From a given handle get the next focus element either forward or backward
            internal static IntPtr GetNextFocusElement(IntPtr handle, bool forward)
            {
                var hDlg = NativeMethods.GetAncestor(handle, NativeMethods.GA_ROOT);
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
            readonly HwndSource _source;

            public DialogKeyboardInputSite(HwndSource source)
            {
                _source = source;
            }

            /// <summary>
            /// Gets the IKeyboardInputSink associated with this site.
            /// </summary>
            public IKeyboardInputSink Sink => _source;

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
                            break;

                        case FocusNavigationDirection.Previous:
                        case FocusNavigationDirection.Left:
                        case FocusNavigationDirection.Up:
                            forward = false;
                            break;
                    }
                }

                // Based on the direction, tab forward or backwards in our parent dialog.
                var nextHandle = DialogPageElementHost.GetNextFocusElement(_source.Handle, forward);
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
}
