using System.Collections.Generic;
using System.ComponentModel;
using MvvmTools.Core.ViewModels;
using MvvmTools.Core.Views;
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

        private readonly Dictionary<BaseDialogViewModel, DialogWindow> _dialogs = new Dictionary<BaseDialogViewModel, DialogWindow>();

        public bool ShowDialog(BaseDialogViewModel vm)
        {
            var dialog = new DialogWindow
            {
                DataContext = vm
            };

            // When the vm sets its DialogResult, that should set the DialogResult 
            // of the DialogWindow.
            vm.PropertyChanged += VmOnPropertyChanged;

            var view = ViewFactory.GetView(vm);
            dialog.Content = view;

            _dialogs.Add(vm, dialog);

            // BaseDialogViewModel can read its own properties such as vm.DialogResult
            // or it can just read the bool returned by dialog.ShowDialog().
            var result = dialog.ShowDialog().GetValueOrDefault();
            if (_dialogs.ContainsKey(vm))
                _dialogs.Remove(vm);

            vm.PropertyChanged -= VmOnPropertyChanged;

            if (!result)
                vm.DialogResult = result;

            return result;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "DialogResult")
            {
                var vm = (BaseDialogViewModel) sender;
                var dialog = _dialogs[vm];
                _dialogs.Remove(vm);
                dialog.DialogResult = vm.DialogResult;
            }
        }
    }
}
