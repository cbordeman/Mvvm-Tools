using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using MvvmTools.Core.Utilities;

namespace MvvmTools.Core.ViewModels
{

    public class StringDialogViewModel : BaseDialogViewModel, IDataErrorInfo
    {
        #region Data

        private string _unmodifiedValue;
        private bool _isAdd;
        private IEnumerable<string> _existingValues;
        private Regex _regexValidator;
        private string _regexErrorMessage;

        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties

        #region Label
        private string _label;
        public string Label
        {
            get { return _label; }
            set { SetProperty(ref _label, value); }
        }
        #endregion Label

        #region Value
        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (SetProperty(ref _value, value))
                {
                    OkCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion Value

        #endregion Properties

        #region Commands
        
        #region OkCommand
        DelegateCommand _okCommand;
        public DelegateCommand OkCommand => _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand, CanOkCommand));
        public bool CanOkCommand() => Error == null;
        public void ExecuteOkCommand()
        {
            if (string.IsNullOrEmpty(Error))
                DialogResult = true;
        }
        #endregion

        #endregion Commands

        #region Virtuals



        #endregion Virtuals

        #region Private Helpers and Event Handlers



        #endregion Private Helpers and Event Handlers

        #region Public Methods

        public void Add(string title, string label, IEnumerable<string> existingValues = null, Regex regexValidator = null, string regexErrorMessage = null)
        {
            _isAdd = true;
            Title = title;
            Label = label;
            Value = null;
            _existingValues = existingValues;
            _regexValidator = regexValidator;
            _regexErrorMessage = regexErrorMessage;
        }

        public void Edit(string title, string label, string value, IEnumerable<string> existingValues = null, Regex regexValidator = null, string regexErrorMessage = null)
        {
            _isAdd = false;
            Title = title;
            Label = label;
            Value = value;
            _unmodifiedValue = value;
            _existingValues = existingValues?.Where(s => !string.Equals(s, value));
            _regexValidator = regexValidator;
            _regexErrorMessage = regexErrorMessage;
        }

        #endregion Public Methods

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (columnName == "Value")
                {
                    if (string.IsNullOrWhiteSpace(Value))
                        return "Can't be empty.";

                    if (_existingValues != null && _existingValues.Any(s => string.Equals(s, Value, StringComparison.OrdinalIgnoreCase)))
                        return "Already in list.";

                    if (!_isAdd)
                    {
                        // In edit, must change the value.
                        if (string.Equals(Value, _unmodifiedValue, StringComparison.OrdinalIgnoreCase))
                            return "Unchanged.";
                    }

                    if (_regexValidator != null)
                    {
                        if (!_regexValidator.IsMatch(Value))
                            return _regexErrorMessage ?? "Invalid value.";
                    }

                    return null;
                }
                return null;
            }
        }

        public string Error => this["Value"];

        #endregion IDataErrorInfo
    }
}
