using System;
using System.Collections.Generic;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class T4UserControlViewModel : BaseViewModel
    {
        #region Data

        private string _initialBuffer;

        #endregion Data

        #region Ctor and Init

        public T4UserControlViewModel()
        {
            
        }

        #endregion Ctor and Init

        #region Properties

        #region TemplateService
        [Inject]
        public ITemplateService TemplateService { get; set; }
        #endregion TemplateService

        #region ShowErrors
        private bool _showErrors = false;
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

        #region Buffer
        private string _buffer;
        public string Buffer
        {
            get { return _buffer; }
            set
            {
                if (SetProperty(ref _buffer, value))
                {
                    NotifyPropertyChanged(nameof(IsModified));
                    Transform();
                }
            }
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
        private List<string> _errors;
        public List<string> Errors
        {
            get { return _errors; }
            set
            {
                if (SetProperty(ref _errors, value))
                    ShowErrors = Errors?.Count > 0;
            }
        }
        #endregion Errors

        #region IsModified
        public bool IsModified => _initialBuffer != Buffer;
        #endregion IsModified

        #region PredefinedFields
        private List<InsertFieldViewModel> _predefinedFields;
        public List<InsertFieldViewModel> PredefinedFields
        {
            get { return _predefinedFields; }
            set { SetProperty(ref _predefinedFields, value); }
        }
        #endregion PredefinedFields

        #region CustomFields
        private List<InsertFieldViewModel> _customFields;
        public List<InsertFieldViewModel> CustomFields
        {
            get { return _customFields; }
            set { SetProperty(ref _customFields, value); }
        }
        #endregion CustomFields

        #endregion Properties

        #region Commands
        
        #endregion Commands

        #region Public Methods

        public static T4UserControlViewModel Create(IKernel kernel, string isEnabledText, string buffer)
        {
            var rval = kernel.Get<T4UserControlViewModel>();
            rval.Init(isEnabledText, buffer);
            return rval;
        }

        public void Init(string isEnabledText, string buffer)
        {
            _isEnabledText = isEnabledText;
            _initialBuffer = buffer;
            Buffer = buffer;

            PredefinedFields = new List<InsertFieldViewModel>
            {
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field1", Description = "Description 1"}, "Value 1"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field2", Description = "Description 2"}, "Value 2"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field3", Description = "Description 3"}, "Value 3"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field4", Description = "Description 4"}, "Value 4"),
            };

            CustomFields = new List<InsertFieldViewModel>
            {
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field1", Description = "Description 1"}, "Value 1"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field2", Description = "Description 2"}, "Value 2"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field3", Description = "Description 3"}, "Value 3"),
                InsertFieldViewModel.Create(Kernel, new FieldDialogViewModel { Name = "Field4", Description = "Description 4"}, "Value 4"),
            };
        }

        #endregion Public Methods

        #region Virtuals

        #endregion Virtuals

        #region Private Helpers

        private void Transform()
        {
            try
            {
                string preview;
                Errors = TemplateService.Transform(Buffer, out preview);
                Preview = preview;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Private Helpers
    }
}
