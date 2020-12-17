using System.Windows.Input;
using MvvmTools.ViewModels;

namespace MvvmTools.Views
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
