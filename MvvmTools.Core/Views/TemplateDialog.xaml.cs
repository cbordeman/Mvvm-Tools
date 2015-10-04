using System;
using System.Windows;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for TemplateDialog.xaml
    /// </summary>
    public partial class TemplateDialog
    {
        public TemplateDialog()
        {
            InitializeComponent();
        }

        private void NameBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            NameBox.Focus();
        }
    }
}
