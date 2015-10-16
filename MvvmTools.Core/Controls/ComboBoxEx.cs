using System.Windows;
using System.Windows.Controls;

namespace MvvmTools.Core.Controls
{
    public class ComboBoxEx : ComboBox
    {
        static ComboBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxEx), new FrameworkPropertyMetadata(typeof(ComboBoxEx)));
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

        #region Watermark
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(ComboBoxEx), new PropertyMetadata(null));
        #endregion Watermark

    }
}
