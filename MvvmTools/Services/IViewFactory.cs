using System;
using System.Windows;
using JetBrains.Annotations;
using MvvmTools.ViewModels;
using Unity;

namespace MvvmTools.Services
{
    public interface IViewFactory
    {
        FrameworkElement GetView(BaseViewModel vm);
    }

    public class ViewFactory : IViewFactory
    {
        public IUnityContainer Container { get; }

        public ViewFactory(IUnityContainer container)
        {
            Container = container;
        }
        
        public FrameworkElement GetView([NotNull] BaseViewModel vm)
        {
            // View type is the vierw model type in the corresponding namespace, less the ViewModel suffix.
            // For example, X.Y.ViewModels.MainDialogViewModel => X.Y.Views.MainDialog.

            var vmType = vm.GetType().FullName;
            var vType = vmType.Replace(".ViewModels.", ".Views.");
            vType = vType.Substring(0, vType.Length - ("ViewModel".Length));
            
            var type = Type.GetType(vType);
            var view = Container.Resolve(type) as FrameworkElement;
            
            if (view == null)
                throw new InvalidOperationException($"In ViewFactory.GetView(), couldn't locate view for view model parameter {vm.GetType()}.");

            //view.DataContext = vm;

            return view;
        }
    }
}
