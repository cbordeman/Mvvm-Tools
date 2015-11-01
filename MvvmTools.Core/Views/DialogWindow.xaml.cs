using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MvvmTools.Core.Utilities;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class DialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();

            SourceInitialized += OnSourceInitialized;
        }

        private void OnSourceInitialized(object sender, EventArgs eventArgs)
        {
            _windowHandle = new WindowInteropHelper(this).Handle;

            //disable minimize button
            DisableMinimizeButton();
        }

        private IntPtr _windowHandle;

        private const int GWL_STYLE = -16;

        private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
        private const int WS_MINIMIZEBOX = 0x20000; //minimize button

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        protected void DisableMinimizeButton()
        {
            if (_windowHandle == IntPtr.Zero)
                throw new InvalidOperationException("The window has not yet been completely initialized");

            SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MINIMIZEBOX);
        }

        //private bool _pauseKeyCapture;

        private void DialogWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Escape)
            //{
            //    e.Handled = true;

            //    var myLock = new AsyncLock();
            //    using (await myLock.LockAsync())
            //        DialogResult = false;
            //}
        }

        private void DialogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            switch (SizeToContent)
            {
                case SizeToContent.Manual:
                    break;
                case SizeToContent.Width:
                    MinWidth = ActualWidth;
                    break;
                case SizeToContent.Height:
                    MinHeight = ActualHeight;
                    break;
                case SizeToContent.WidthAndHeight:
                    MinWidth = ActualWidth;
                    MinHeight = ActualHeight;
                    break;
            }
        }

        private async void DialogWindow_OnClosing(object sender, CancelEventArgs e)
        {
            var vm = DataContext as BaseDialogViewModel;
            if (vm == null) return;

            var doNotClose = await vm.OnClosing();
            e.Cancel = doNotClose;
        }
    }
}
