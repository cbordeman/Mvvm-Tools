namespace MvvmTools.ViewModels
{
    public class CheckedItemViewModel<T> : BaseViewModel
    {
        public CheckedItemViewModel(T value, bool isChecked) : base(null)
        {
            _isChecked = isChecked;
            Value = value;
        }

        public T Value { get; set; }

        #region IsChecked
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
        #endregion IsChecked
    }
}
