using System.Windows;

namespace MvvmTools.Views
{
    /// <summary>
    /// Interaction logic for EditStringUserControl.xaml
    /// </summary>
    public partial class StringDialog
    {
        public StringDialog()
        {
            InitializeComponent();
        }

        private void ValueBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            ValueBox.Focus();
        }
    }
}
