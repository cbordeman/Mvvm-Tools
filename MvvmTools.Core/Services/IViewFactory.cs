using System;
using System.Windows;
using JetBrains.Annotations;
using MvvmTools.Core.ViewModels;
using Ninject;

namespace MvvmTools.Core.Services
{
    public interface IViewFactory
    {
        FrameworkElement GetView(BaseViewModel vm);
    }

    public class ViewFactory : IViewFactory
    {
        [Inject]
        public IKernel Kernel { get; set; }

        public FrameworkElement GetView([NotNull] BaseViewModel vm)
        {
            // View type is the vierw model type in the corresponding namespace, less the ViewModel suffix.
            // For example, X.Y.ViewModels.MainDialogViewModel => X.Y.Views.MainDialog.

            var vmType = vm.GetType().FullName;
            var vType = vmType.Replace(".ViewModels.", ".Views.");
            vType = vType.Substring(0, vType.Length - ("ViewModel".Length));
            
            var type = Type.GetType(vType);
            var view = Kernel.Get(type) as FrameworkElement;
            
            if (view == null)
                throw new InvalidOperationException($"In ViewFactory.GetView(), couldn't locate view for view model parameter {vm.GetType()}.");

            //view.DataContext = vm;

            return view;
        }
    }
}
