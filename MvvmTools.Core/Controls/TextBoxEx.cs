using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmTools.Core.Controls
{
    public class TextBoxEx : TextBox
    {
        #region Data

        private object[] _items;

        private BindingEvaluator _displayMemberPathBindingEvaluator;

        private ScrollViewer _scrollViewer;
        private Popup _popup;
        private Selector _selector;

        private CancellationTokenSource _cts;
        private readonly DispatcherTimer _timer;

        #endregion Data

        #region Ctor

        public TextBoxEx()
        {
            DefaultStyleKey = typeof (TextBoxEx);
            
             _timer = new DispatcherTimer
             {
                 Interval = new TimeSpan(1000),
             };
            _timer.Tick += TimerOnTick;

            var pd = DependencyPropertyDescriptor.FromProperty(IsKeyboardFocusWithinProperty, typeof (TextBoxEx));
            pd.AddValueChanged(this, IsKeyboardFocusWithinChanged);
        }
        
        private void IsKeyboardFocusWithinChanged(object obj, EventArgs e)
        {
            
        }

        #endregion Ctor

        #region Virtuals

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var clearButton = Template.FindName("ClearButton", this) as Button;
            if (clearButton != null)
                clearButton.Click += ClearButtonOnClick;
            _scrollViewer = Template.FindName("PART_ContentHost", this) as ScrollViewer;

            _popup = Template.FindName("PART_Popup", this) as Popup;
            //if (_popup != null)
            //{
            //    _popup.StaysOpen = false;
            //}

            _selector = Template.FindName("PART_Selector", this) as Selector;

            SelectClosest();
        }

        private void SelectClosest()
        {
            
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (!IsKeyboardFocusWithin)
            {
                if (_popup != null)
                    _popup.IsOpen = false;
            }
        }
        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (SuggestionsProvider == null || _popup == null || _selector == null)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    if (!_popup.IsOpen)
                        return;

                    if (_selector.SelectedIndex != -1)
                    {
                        Text = GetDisplayText(_selector.SelectedValue);
                        SelectionStart = CaretIndex = Text.Length;
                        SelectionLength = 0;
                    }

                    e.Handled = true;
                    return;

                case Key.Up:
                    if (!_popup.IsOpen)
                        OpenPopup();
                    else
                        SelectPrevious();
                    e.Handled = true;
                    break;

                case Key.Down:
                    if (!_popup.IsOpen)
                        OpenPopup();
                    else
                        SelectNext();
                    e.Handled = true;
                    break;
            }
        }

        private void SelectNext()
        {
            if (!HasItems)
                return;

            if (_selector.SelectedIndex == -1)
                _selector.SelectedIndex = 0;

            if (_selector.SelectedIndex < _selector.Items.Count - 1)
                _selector.SelectedIndex++;
        }

        private void SelectPrevious()
        {
            if (!HasItems)
                return;

            if (_selector.SelectedIndex == -1)
                _selector.SelectedIndex = _selector.Items.Count - 1;

            if (_selector.SelectedIndex > 0)
                _selector.SelectedIndex--;
        }

        private void OpenPopup()
        {
            if (!_popup.IsOpen)
            {
                _popup.IsOpen = true;
            }
        }

        private async void TimerOnTick(object sender, EventArgs eventArgs)
        {
            _timer.Stop();

            if (SuggestionsProvider == null)
                return;

            // If active request, cancel it.
            _cts?.Cancel();

            // Start a request.
            _cts = new CancellationTokenSource();

            _items = await SuggestionsProvider.GetSuggestions(Text, _cts.Token);
            HasItems = _items != null && _items.Length > 0;

            if (_cts == null || _cts.IsCancellationRequested)
            {
                _cts = null;
                return;
            }
            _cts = null;
            
            // No reason to search if we don't have any values.
            if (_items == null)
                return;
            _selector.ItemsSource = _items;

            // Do search and changes here.
            string match = null;
            int matchPos = -1;
            foreach (var obj in _items)
            {
                var str = GetDisplayText(obj);
                matchPos = str.IndexOf(Text, StringComparison.OrdinalIgnoreCase);
                if (matchPos != -1)
                {
                    match = str;
                    break;
                }
            }

            // No match.  Leave them alone.
            if (string.IsNullOrEmpty(match))
                return;

            //TextChanged -= OnTextChanged;

            if (IsKeyboardFocusWithin)
            {
                // If Text == SelectedValue, don't open because the value was just selected.
                if (Text == GetDisplayText(_selector.SelectedValue))
                    _popup.IsOpen = false;
                else
                {
                    OpenPopup();
                    if (_selector.SelectedIndex == -1)
                        _selector.SelectedIndex = 0;
                }
            }
            else
                _popup.IsOpen = false;

            //Text = match;
            //CaretIndex = textLength;
            //SelectionStart = matchPos;
            //SelectionLength = textLength;
            //TextChanged += OnTextChanged;
        }

        private string GetDisplayText(object dataItem)
        {
            if (_displayMemberPathBindingEvaluator == null)
                _displayMemberPathBindingEvaluator = new BindingEvaluator(new Binding(DisplayMemberPath));
            if (dataItem == null)
                return string.Empty;
            if (string.IsNullOrEmpty(DisplayMemberPath))
                return dataItem.ToString();
            return _displayMemberPathBindingEvaluator.Evaluate(dataItem);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SuggestionsProvider == null || _popup == null) return;

            // No reason to search if there's nothing there.
            if (string.IsNullOrEmpty(Text))
            {
                _popup.IsOpen = false;
                return;
            }

            _timer.Stop();
            _timer.Start();
        }

        #endregion Virtuals

        #region Properties

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

        #region MaxDropDownHeight
        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }
        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(TextBoxEx), new PropertyMetadata(double.NaN));
        #endregion MaxDropDownHeight

        #region ItemTemplate
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TextBoxEx), new PropertyMetadata(null));
        #endregion ItemTemplate

        #region DisplayMemberPath
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(TextBoxEx), new PropertyMetadata(null));
        #endregion DisplayMemberPath

        #region ItemTemplateSelector
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            DependencyProperty.Register(nameof(ItemTemplateSelector), typeof(DataTemplateSelector), typeof(TextBoxEx), new PropertyMetadata(null));
        #endregion ItemTemplateSelector

        #region SuggestionsProvider
        public ISuggestionsProvider SuggestionsProvider
        {
            get { return (ISuggestionsProvider)GetValue(SuggestionsProviderProperty); }
            set { SetValue(SuggestionsProviderProperty, value); }
        }
        public static readonly DependencyProperty SuggestionsProviderProperty =
            DependencyProperty.Register(nameof(SuggestionsProvider), typeof(ISuggestionsProvider), typeof(TextBoxEx), new PropertyMetadata(null, SuggestionProviderChanged));
        private static void SuggestionProviderChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var tb = dependencyObject as TextBoxEx;
            if (tb == null) return;
            if (tb.SuggestionsProvider == null)
                tb.TearDownAutoComplete();
            else
                tb.SetupAutoComplete();
        }
        #endregion SuggestionsProvider

        #region HasItems
        private static readonly DependencyPropertyKey HasItemsKey = DependencyProperty.RegisterReadOnly(
            nameof(HasItems), typeof(bool), typeof(TextBoxEx),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty HasItemsProperty = HasItemsKey.DependencyProperty;
        public bool HasItems
        {
            get { return (bool)GetValue(HasItemsProperty); }
            protected set { SetValue(HasItemsKey, value); }
        }
        #endregion HasItems

        #endregion Properties

        #region Private Methods

        private void SetupAutoComplete()
        {
            TextChanged += OnTextChanged;
            //PreviewKeyDown += OnPreviewKeyDown;
        }

        private void TearDownAutoComplete()
        {
            TextChanged -= OnTextChanged;
            //PreviewKeyDown -= OnPreviewKeyDown;
        }

        private void ClearButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Text = string.Empty;
            _scrollViewer?.Focus();
        }

        #endregion Private Methods
    }
}
