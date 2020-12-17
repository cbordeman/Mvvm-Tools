using System.Windows;
using System.Windows.Controls;

namespace MvvmTools.Controls
{
    public class ComboBoxEx : ComboBox
    {
        #region Data
        #endregion Data

        #region Events
        
        #endregion Events

        #region Ctor

        static ComboBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxEx), new FrameworkPropertyMetadata(typeof(ComboBoxEx)));
        }
        
        #endregion Ctor

        #region Dependency Properties

        #region LivePreviewItem
        public object LivePreviewItem
        {
            get { return GetValue(LivePreviewItemProperty); }
            set { SetValue(LivePreviewItemProperty, value); }
        }
        public static readonly DependencyProperty LivePreviewItemProperty =
            DependencyProperty.Register(nameof(LivePreviewItem), typeof(object), typeof(ComboBoxEx), new PropertyMetadata(null));
        #endregion LivePreviewItem

        #region ShowError
        public bool ShowError
        {
            get { return (bool)GetValue(ShowErrorProperty); }
            set { SetValue(ShowErrorProperty, value); }
        }
        public static readonly DependencyProperty ShowErrorProperty =
            DependencyProperty.Register(nameof(ShowError), typeof(bool), typeof(ComboBoxEx), new PropertyMetadata(false));
        #endregion ShowError

        #region Watermark
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(ComboBoxEx), new PropertyMetadata(null));
        #endregion Watermark
        
        #endregion Dependency Properties

        #region Virtuals
        
        #endregion Virtuals

        #region Private Helpers
        
        #endregion Private Helpers
    }
}