using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MvvmTools.Utilities;

namespace MvvmTools.Views
{
    /// <summary>
    /// Interaction logic for SelectFileWindow.xaml
    /// </summary>
    public partial class SelectFileDialog
    {
        public SelectFileDialog()
        {
            InitializeComponent();
        }

        private void MyListView_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.MyListView.Focus();
            this.MyListView.SelectedItem = this.MyListView.Items[0];
            // Have to do this because the ListView doesn't fully select the first item, user would
            // otherwise have to press down twice to get the selection to move to the second item.
            KeyboardUtilities.PressKey(this.MyListView, Key.Down);
        }

        private void OkButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void MyListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OkButton.IsEnabled = MyListView.SelectedItem != null;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
