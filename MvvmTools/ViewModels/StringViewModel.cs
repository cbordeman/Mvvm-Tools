using Unity;

namespace MvvmTools.ViewModels
{
    public class StringViewModel : BaseViewModel
    {
        public static StringViewModel CreateFromString(IUnityContainer container, string s)
        {
            var cVm = container.Resolve<StringViewModel>();
            cVm._value = s;
            return cVm;

        }

        #region Value
        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }
        #endregion Value

        public StringViewModel(IUnityContainer container) : base(container)
        {
        }
    }
    
}
