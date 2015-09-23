using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for OptionsTemplateMaintenanceUserControl.xaml
    /// </summary>
    public partial class OptionsTemplateMaintenanceUserControl
    {
        private const uint DLGC_WANTARROWS = 0x0001;
        private const uint DLGC_WANTTAB = 0x0002;
        private const uint DLGC_WANTALLKEYS = 0x0004;
        private const uint DLGC_HASSETSEL = 0x0008;
        private const uint DLGC_WANTCHARS = 0x0080;
        private const uint WM_GETDLGCODE = 0x0087;

        public OptionsTemplateMaintenanceUserControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var s = HwndSource.FromVisual(this) as HwndSource;
            if (s != null)
                s.AddHook(ChildHwndSourceHook);
        }

        static IntPtr ChildHwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETDLGCODE)
            {
                handled = true;
                return new IntPtr(DLGC_WANTALLKEYS | DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL | DLGC_WANTTAB);
            }
            return IntPtr.Zero;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var vm = (OptionsUserControlViewModel)DataContext;
                vm?.ExecuteEditViewSuffixCommand();
            }
        }
    }
}
