using System.Collections.ObjectModel;
using MvvmTools.Core.Services;

namespace MvvmTools.Core.ViewModels
{

    public class T4UserControlViewModel : BaseViewModel
    {
        #region Data



        #endregion Data

        #region Ctor and Init

        public T4UserControlViewModel(string isEnabledText, string buffer)
        {
            _isEnabledText = isEnabledText;
            _buffer = buffer;
        }

        #endregion Ctor and Init

        #region Properties

        #region ShowErrors
        private bool _showErrors;
        public bool ShowErrors
        {
            get { return _showErrors; }
            set { SetProperty(ref _showErrors, value); }
        }
        #endregion ShowErrors

        #region IsEnabledText
        private string _isEnabledText;
        public string IsEnabledText
        {
            get { return _isEnabledText; }
            set { SetProperty(ref _isEnabledText, value); }
        }
        #endregion IsEnabledText

        #region IsEnabled
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        #endregion IsEnabled

        #region Name
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Commands

        #region Buffer
        private string _buffer;
        public string Buffer
        {
            get { return _buffer; }
            set { SetProperty(ref _buffer, value); }
        }
        #endregion Buffer

        #region Preview
        private string _preview;
        public string Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }
        #endregion Preview

        #region Errors
        private ObservableCollection<ParseError> _errors;
        public ObservableCollection<ParseError> Errors
        {
            get { return _errors; }
            set { SetProperty(ref _errors, value); }
        }
        #endregion Errors

        #endregion Commands

        #endregion Properties

        #region Virtuals



        #endregion Virtuals

        #region Private Helpers



        #endregion Private Helpers
    }
}
