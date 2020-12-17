using Unity;

namespace MvvmTools.ViewModels
{
    public class BaseViewModel : BindableBase
    {
        protected IUnityContainer Container { get; }

        public BaseViewModel(IUnityContainer container)
        {
            Container = container;
        }
    }
}
