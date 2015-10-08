using System.Windows;
using System.Windows.Controls;

namespace MvvmTools.Core.Controls
{
    public class ComboBoxEx : ComboBox
    {
        public ComboBoxEx()
        {
            DefaultStyleKey = typeof (ComboBoxEx);
        }

        #region ShowError
        public bool ShowError
        {
            get { return (bool)GetValue(ShowErrorProperty); }
            set { SetValue(ShowErrorProperty, value); }
        }
        public static readonly DependencyProperty ShowErrorProperty =
            DependencyProperty.Register("ShowError", typeof(bool), typeof(ComboBoxEx), new PropertyMetadata(false));
        #endregion ShowError
    }
}
