using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.PlatformUI;
using MvvmTools.Core.ViewModels;
using Ninject;

namespace MvvmTools.Core.Services
{
    public interface IDialogService
    {
        bool ShowDialog(BaseDialogViewModel vm);
    }

    public class DialogService : IDialogService
    {
        [Inject]
        public IViewFactory ViewFactory { get; set; }

        public bool ShowDialog(BaseDialogViewModel vm)
        {
            var dialog = new Views.DialogWindow
            {
                DataContext = vm
            };

            // When the vm sets its DialogResult, that should set the DialogResult 
            // of the DialogWindow.
            vm.PropertyChanged += VmOnPropertyChanged;

            var view = ViewFactory.GetView(vm);
            dialog.Content = view;

            // BaseDialogViewModel can read its own properties such as vm.DialogResult
            // or it can just read the bool returned by dialog.ShowDialog().
            var result = dialog.ShowDialog().GetValueOrDefault();

            vm.PropertyChanged -= VmOnPropertyChanged;

            return result;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var dialog = (DialogWindow) sender;
            if (args.PropertyName == "DialogResult")
            {
                var vm = (BaseDialogViewModel)dialog.DataContext;
                dialog.DialogResult = vm.DialogResult;
            }
        }
    }
}
