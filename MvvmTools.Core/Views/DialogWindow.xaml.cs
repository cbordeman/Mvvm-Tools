using System.Windows.Input;

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
        }

        private void DialogWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                DialogResult = false;
        }
    }
}
