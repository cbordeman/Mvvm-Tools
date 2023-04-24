using System.Threading.Tasks;
using MvvmTools.Services;
using MvvmTools.Utilities;
using Unity;

namespace MvvmTools.ViewModels
{
    public abstract class BaseDialogViewModel : BaseViewModel
    {
        #region Properties

        public IDialogService DialogService { get; set; }

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

        public BaseDialogViewModel(IDialogService dialogService,
            IUnityContainer container) : base(container)
        {
            DialogService = dialogService;
        }
        
        #region Protected Methods

        protected async Task<bool> ConfirmDiscard()
        {
            return await DialogService.Ask("Discard Changes?", "Cancel dialog?  You will lose any changes.",
                        AskButton.OKCancel) == AskResult.OK;
        }

        #endregion Protected Methods

        #region Virtuals

        private bool _inCancel;

        public virtual void Cancel()
        {
            _inCancel = true;
            DialogResult = false;
            _inCancel = false;
        }

        /// <summary>
        /// Return true to cancel close.
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> OnClosing()
        {
            return Task.FromResult(false);
        }

        #endregion Virtuals
    }
}