namespace MvvmTools.Core.ViewModels
{
    public class StringViewModel : BaseViewModel
    {
        public StringViewModel()
        {
            
        }

        public StringViewModel(string value)
        {
            _value = value;
        }

        #region Value
        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        #endregion Value
    }
}
