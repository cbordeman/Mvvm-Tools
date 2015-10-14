using System.Windows;
using System.Windows.Input;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for TemplateDialog.xaml
    /// </summary>
    public partial class TemplateDialog
    {
        public TemplateDialog()
        {
            InitializeComponent();
        }

        private void NameBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            NameBox.Focus();
        }

        private void FieldItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var vm = (TemplateDialogViewModel)DataContext;
                vm?.ExecuteEditFieldCommand();
                FieldsDataGrid.Focus();
            }
        }
    }
}
