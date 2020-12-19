using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MvvmTools.Views
{
    /// <summary>
    /// Interaction logic for HeaderUserControl.xaml
    /// </summary>
    public partial class HeaderUserControl : UserControl
    {
        public HeaderUserControl()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/cbordeman/Mvvm-Tools/issues");
        }
    }
}
