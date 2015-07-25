using MvvmTools.Core.Utilities;

namespace MvvmTools.Core.ViewModels
{
    public abstract class BaseDialogViewModel : BaseViewModel
    {
        #region Properties

        #region Title
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        #endregion Title

        #region DialogResult
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set
            {
                if (SetProperty(ref _dialogResult, value) && !value.GetValueOrDefault() && !_inCancel)
                    Cancel();
            }
        }
        #endregion DialogResult

        #endregion Properties

        #region Commands
        
        #region CancelCommand
        DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new DelegateCommand(ExecuteCancelCommand, CanCancelCommand));
        public bool CanCancelCommand() => true;
        public void ExecuteCancelCommand()
        {
            Cancel();
        }
        #endregion

        #endregion Commands

        #region Virtuals

        private bool _inCancel;

        public virtual void Cancel()
        {
            _inCancel = true;
            DialogResult = false;
            _inCancel = false;
        }

        #endregion Virtuals
    }
}