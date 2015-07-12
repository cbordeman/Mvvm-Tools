namespace MvvmTools.Core.Views
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

        private void ValueBox_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ValueBox.Focus();
        }
    }
}
