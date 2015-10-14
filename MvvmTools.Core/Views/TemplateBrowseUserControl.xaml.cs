using System.Windows;
using System.Windows.Input;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for TemplatesUserControl.xaml
    /// </summary>
    public partial class TemplateBrowseUserControl
    {
        public TemplateBrowseUserControl()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var vm = (TemplateBrowseUserControlViewModel)DataContext;
                vm?.ExecuteEditTemplateCommand();
                TemplatesDataGrid.Focus();
            }
        }

        private void TemplatesDataGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        #region ShowSelectButton
        public bool ShowSelectButton
        {
            get { return (bool)GetValue(ShowSelectButtonProperty); }
            set { SetValue(ShowSelectButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowSelectButtonProperty =
            DependencyProperty.Register("ShowSelectButton", typeof(bool), typeof(TemplateBrowseUserControl), new PropertyMetadata(false));
        #endregion

        #region SelectCommand
        public ICommand SelectCommand
        {
            get { return (ICommand)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(TemplateBrowseUserControl), new PropertyMetadata(null));
        #endregion SelectCommand
    }
}
