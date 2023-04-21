using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using MvvmTools.ViewModels;
using MvvmTools.Views;
using MessageBox = System.Windows.MessageBox;

namespace MvvmTools.Services
{
    public interface IDialogService
    {
        bool ShowDialog(BaseDialogViewModel vm);
        Task ShowMessage(string title, string message);
        Task<AskResult> Ask(string title, string message, AskButton buttons);
    }

    public class MessageOverrideActions
    {
        public Action AddAction;
        public Action RemoveAction;

        public MessageOverrideActions(Action addAction, Action removeAction)
        {
            AddAction = addAction;
            RemoveAction = removeAction;
        }
    }

    public enum AskButton
    {
        Ok = 0,
        OkCancel = 1,
        YesNoCancel = 3,
        YesNo = 4,
    }

    public enum AskResult
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        Yes = 6,
        No = 7,
    }

    /// <summary>
    /// This dialog service is fully abstracted.  
    /// </summary>
    public class DialogService : IDialogService
    {
        public IViewFactory ViewFactory { get; set; }
        
        private readonly Dictionary<BaseDialogViewModel, DialogWindow> _dialogs = new Dictionary<BaseDialogViewModel, DialogWindow>();

        public DialogService(IViewFactory viewFactory)
        {
            ViewFactory = viewFactory;
        }
        
        public Task ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
            return Task.FromResult<object>(null);
        }

        public Task<AskResult> Ask(string title, string message, AskButton buttons)
        {
            var b = (MessageBoxButton) Enum.Parse(typeof (MessageBoxButton), buttons.ToString());
            var result = MessageBox.Show(message, title, b);
            return Task.FromResult((AskResult)Enum.Parse(typeof(AskResult), result.ToString()));
        }
        
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

            MoveSizingFromViewToDialog(view, dialog);

            dialog.Content = view;

            _dialogs.Add(vm, dialog);

            // BaseDialogViewModel can read its own properties such as vm.DialogResult
            // or it can just read the bool returned by dialog.ShowDialog().
            var result = dialog.ShowDialog().GetValueOrDefault();
            if (_dialogs.ContainsKey(vm))
                _dialogs.Remove(vm);

            vm.PropertyChanged -= VmOnPropertyChanged;

            if (!result)
                vm.DialogResult = false;

            return result;
        }

        private static void MoveSizingFromViewToDialog(FrameworkElement view, DialogWindow dialog)
        {
            // If Width, MaxWidth, Height, or MaxHeight are specified on the view, we transfer
            // those to the dialog and clear them from the view.  The initial width and height,
            // if specified on the view, are used as minimums on the dialog.
            //
            // If width or height isn't specified, we set the dialog's SizeToContent so that
            // the dialog will size to the content in that dimension.
            //
            // After the dialog window loads, actual width/height are set as the minimums on 
            // each dimension that wasn't specified by the developer.  This bit is done in 
            // DialogWindow.xaml.cs (code behind).

            // Width
            if (!double.IsNaN(view.Width))
            {
                dialog.Width = view.Width;
                dialog.MinWidth = view.Width;
                view.ClearValue(FrameworkElement.WidthProperty);
            }
            if (!double.IsInfinity(view.MinWidth))
            {
                view.ClearValue(FrameworkElement.MinWidthProperty);
            }
            if (!double.IsInfinity((view.MaxWidth)))
            {
                dialog.MaxWidth = view.MaxWidth;
                view.ClearValue(FrameworkElement.MaxWidthProperty);
            }

            // Height
            if (!double.IsNaN(view.Height))
            {
                dialog.Height = view.Height;
                dialog.MinHeight = view.Height;
                view.ClearValue(FrameworkElement.HeightProperty);
            }
            if (!double.IsInfinity(view.MinHeight))
            {
                view.ClearValue(FrameworkElement.MinHeightProperty);
            }
            if (!double.IsInfinity(view.MaxHeight))
            {
                dialog.MaxHeight = view.MaxHeight;
                view.ClearValue(FrameworkElement.MaxHeightProperty);
            }
            
            // Let dialog size to content on things that aren't specified.
            if (!double.IsNaN(dialog.Width) && !double.IsNaN(dialog.Height))
                dialog.SizeToContent = SizeToContent.Manual;
            else if (!double.IsNaN(dialog.Width))
                dialog.SizeToContent = SizeToContent.Height;
            else if (!double.IsNaN(dialog.Height))
                dialog.SizeToContent = SizeToContent.Width;
            else
                dialog.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(BaseDialogViewModel.DialogResult))
            {
                var vm = (BaseDialogViewModel) sender;
                var dialog = _dialogs[vm];
                //_dialogs.Remove(vm);
                dialog.DialogResult = vm.DialogResult;
                //dialog.Close();
            }
        }
    }
}
