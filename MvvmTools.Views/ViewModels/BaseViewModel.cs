using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class BaseViewModel : BindableBase
    {
        [Inject]
        public IKernel Kernel { get; set; }
    }
}
