using System.Collections.Generic;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class StringViewModel : BaseViewModel
    {
        public static StringViewModel CreateFromString(IKernel kernel, string s)
        {
            var cVm = kernel.Get<StringViewModel>();
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
    }
    
}
