using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmTools.Core.Controls
{
    public class TextBoxEx : TextBox
    {
        private ScrollViewer _scrollViewer;

        public TextBoxEx()
        {
            DefaultStyleKey = typeof (TextBoxEx);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var clearButton = Template.FindName("ClearButton", this) as Button;
            if (clearButton != null)
                clearButton.Click += ClearButtonOnClick;
            _scrollViewer = Template.FindName("PART_ContentHost", this) as ScrollViewer;
        }

        private void ClearButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Text = string.Empty;
            _scrollViewer?.Focus();
            //if (SearchCommand != null)
            //{
            //    if (SearchCommand.CanExecute(SearchCommandParameter))
            //        SearchCommand.Execute(SearchCommandParameter);
            //}
        }

        #region ShowError
        public bool ShowError
        {
            get { return (bool)GetValue(ShowErrorProperty); }
            set { SetValue(ShowErrorProperty, value); }
        }
        public static readonly DependencyProperty ShowErrorProperty =
            DependencyProperty.Register("ShowError", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false));
        #endregion ShowError

        #region Watermark
        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(TextBoxEx), new PropertyMetadata(null));
        #endregion Watermark

        #region ShowClearButton
        public bool ShowClearButton
        {
            get { return (bool)GetValue(ShowClearButtonProperty); }
            set { SetValue(ShowClearButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowClearButtonProperty =
            DependencyProperty.Register("ShowClearButton", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false));
        #endregion ShowClearButton

        //#region ShowSearchButton
        //public bool ShowSearchButton
        //{
        //    get { return (bool)GetValue(ShowSearchButtonProperty); }
        //    set { SetValue(ShowSearchButtonProperty, value); }
        //}
        //public static readonly DependencyProperty ShowSearchButtonProperty =
        //    DependencyProperty.Register("ShowSearchButton", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false));
        //#endregion ShowSearchButton

        //#region SearchCommand
        //public ICommand SearchCommand
        //{
        //    get { return (ICommand)GetValue(SearchCommandProperty); }
        //    set { SetValue(SearchCommandProperty, value); }
        //}
        //public static readonly DependencyProperty SearchCommandProperty =
        //    DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(TextBoxEx), new PropertyMetadata(null));
        //#endregion SearchCommand

        //#region SearchCommandParameter
        //public object SearchCommandParameter
        //{
        //    get { return GetValue(SearchCommandParameterProperty); }
        //    set { SetValue(SearchCommandParameterProperty, value); }
        //}
        //public static readonly DependencyProperty SearchCommandParameterProperty =
        //    DependencyProperty.Register("SearchCommandParameter", typeof(object), typeof(TextBoxEx), new PropertyMetadata(null));
        //#endregion SearchCommandParameter
    }
}
