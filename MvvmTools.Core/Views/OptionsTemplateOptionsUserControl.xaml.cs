using System.Diagnostics;
using System.Windows;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for OptionsTemplateOptionsUserControl.xaml
    /// </summary>
    public partial class OptionsTemplateOptionsUserControl
    {
        public OptionsTemplateOptionsUserControl()
        {
            InitializeComponent();
        }
        
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("mailto://mvvmtools@outlook.com");
        }
    }
}
