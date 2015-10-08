using System.Windows;
using System.Windows.Input;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for FieldDialog.xaml
    /// </summary>
    public partial class FieldDialog
    {
        public FieldDialog()
        {
            InitializeComponent();
        }

        private void NameBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            NameBox.Focus();
        }

        private void StringItem_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var vm = (FieldDialogViewModel) DataContext;
                vm?.ExecuteEditChoiceCommand();
                ChoicesDataGrid.Focus();
            }
        }
    }
}
