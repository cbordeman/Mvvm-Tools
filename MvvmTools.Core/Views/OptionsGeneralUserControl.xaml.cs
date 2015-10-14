using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for OptionsUserControl.xaml
    /// </summary>
    public partial class OptionsGeneralUserControl
    {
        public OptionsGeneralUserControl()
        {
            InitializeComponent();
        }
        
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var vm = (OptionsViewModel)DataContext;
                vm?.ExecuteEditViewSuffixCommand();
            }
        }
    }
}
