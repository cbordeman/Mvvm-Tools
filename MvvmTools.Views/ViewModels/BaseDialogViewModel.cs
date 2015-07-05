using Microsoft.Internal.VisualStudio.PlatformUI;

namespace MvvmTools.Core.ViewModels
{
    public class BaseDialogViewModel : BaseViewModel
    {
        #region Title
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        #endregion Title

        #region DialogResult
        private bool _dialogResult;
        public bool DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(ref _dialogResult, value); }
        }
        #endregion DialogResult
    }
}
