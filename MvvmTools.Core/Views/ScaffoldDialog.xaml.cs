using System.Windows;
using System.Windows.Controls;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for ScaffoldDialog.xaml
    /// </summary>
    public partial class ScaffoldDialog
    {
        public ScaffoldDialog()
        {
            InitializeComponent();
        }

        private void NameTextBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus();
        }
    }
}
