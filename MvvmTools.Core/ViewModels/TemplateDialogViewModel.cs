using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class TemplateDialogViewModel : BaseDialogViewModel, IDataErrorInfo
    {
        #region Data

        private TemplateDialogViewModel _unmodifiedValue;
        private bool _isAdd;
        private IEnumerable<string> _existingNames;
        
        // This is recalculated after the user completes an operation that modifies, adds,
        // or deletes any field.  This is much more efficient than doing this calculation 
        // every time OkCommand.RaiseCanExecuteChanged() is called.
        private bool _fieldsChanged;

        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties

        #region DialogService
        [Inject]
        public IDialogService DialogService { get; set; }
        #endregion DialogService

        #region IsInternal
        private bool _isInternal;
        public bool IsInternal
        {
            get { return _isInternal; }
            set { SetProperty(ref _isInternal, value); }
        }
        #endregion IsInternal

        #region Platforms
        private CheckListUserControlViewModel<Platform> _platforms;
        public CheckListUserControlViewModel<Platform> Platforms
        {
            get { return _platforms; }
            set { SetProperty(ref _platforms, value); }
        }
        #endregion Platforms

        #region FormFactors
        private CheckListUserControlViewModel<FormFactor> _formFactors;
        public CheckListUserControlViewModel<FormFactor> FormFactors
        {
            get { return _formFactors; }
            set { SetProperty(ref _formFactors, value); }
        }
        #endregion FormFactors

        #region Framework
        private string _framework = string.Empty;
        public string Framework
        {
            get { return _framework; }
            set { SetProperty(ref _framework, value); }
        }
        #endregion Framework
        
        #region Name
        private string _name = string.Empty;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Tags
        private string _tags = string.Empty;
        public string Tags
        {
            get { return _tags; }
            set
            {
                SetProperty(ref _tags, value);
            }
        }
        #endregion Tags

        #region Description
        private string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        #endregion Description

        #region Fields
        private ListCollectionView _fields;
        public ListCollectionView Fields
        {
            get { return _fields; }
            set { SetProperty(ref _fields, value); }
        }
        #endregion Fields

        #region View
        private T4UserControlViewModel _view;
        public T4UserControlViewModel View
        {
            get { return _view; }
            set { SetProperty(ref _view, value); }
        }
        #endregion View

        #region ViewModelCSharp
        private string _viewModelCSharp = string.Empty;
        public string ViewModelCSharp
        {
            get { return _viewModelCSharp; }
            set { SetProperty(ref _viewModelCSharp, value); }
        }
        #endregion ViewModelCSharp

        #region ViewModelVisualBasic
        private string _viewModelVisualBasic = string.Empty;
        public string ViewModelVisualBasic
        {
            get { return _viewModelVisualBasic; }
            set { SetProperty(ref _viewModelVisualBasic, value); }
        }
        #endregion ViewModelVisualBasic
        
        #region CodeBehindCSharp
        private string _codeBehindCSharp = string.Empty;
        public string CodeBehindCSharp
        {
            get { return _codeBehindCSharp; }
            set { SetProperty(ref _codeBehindCSharp, value); }
        }
        #endregion CodeBehindCSharp

        #region CodeBehindVisualBasic
        private string _codeBehindVisualBasic = string.Empty;
        public string CodeBehindVisualBasic
        {
            get { return _codeBehindVisualBasic; }
            set { SetProperty(ref _codeBehindVisualBasic, value); }
        }
        #endregion CodeBehindVisualBasic

        #endregion Properties

        #region Commands

        #region OkCommand
        DelegateCommand _okCommand;
        public DelegateCommand OkCommand => _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand, CanOkCommand));
        public bool CanOkCommand() => Error == null && (_isAdd || !IsUnchanged());
        public void ExecuteOkCommand()
        {
            if (string.IsNullOrEmpty(Error))
            {
                _unmodifiedValue = null;
                DialogResult = true;
            }
        }

        #endregion Commands
        
        #region AddFieldCommand
        DelegateCommand _addFieldCommand;
        public DelegateCommand AddFieldCommand => _addFieldCommand ?? (_addFieldCommand = new DelegateCommand(ExecuteAddFieldCommand, CanAddFieldCommand));
        public bool CanAddFieldCommand() => !IsInternal;
        public void ExecuteAddFieldCommand()
        {
            var vm = Kernel.Get<FieldDialogViewModel>();
            var source = (ObservableCollection<FieldDialogViewModel>) Fields.SourceCollection;
            vm.Add(IsInternal, source.Select(t => t.Name));
            if (DialogService.ShowDialog(vm))
            {
                source.Add(vm);
                Fields.MoveCurrentTo(vm);
                _fieldsChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        
        #region EditFieldCommand
        DelegateCommand _editFieldCommand;
        public DelegateCommand EditFieldCommand => _editFieldCommand ?? (_editFieldCommand = new DelegateCommand(ExecuteEditFieldCommand, CanEditFieldCommand));
        public bool CanEditFieldCommand() => Fields.CurrentItem != null && !IsInternal;
        public void ExecuteEditFieldCommand()
        {
            var vm = (FieldDialogViewModel) Fields.CurrentItem;

            var copy = FieldDialogViewModel.CreateFrom(Kernel, vm);

            var source = (ObservableCollection<FieldDialogViewModel>)Fields.SourceCollection;
            copy.Edit(IsInternal, source.Select(t => t.Name));
            if (DialogService.ShowDialog(copy))
            {
                // Success, copy fields back into our instance, save, and refresh frameworks (filter combobox).
                vm.CopyFrom(copy);
                Fields.MoveCurrentTo(vm);
                _fieldsChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion
        
        #region DeleteFieldCommand
        DelegateCommand _deleteFieldCommand;
        public DelegateCommand DeleteFieldCommand => _deleteFieldCommand ?? (_deleteFieldCommand = new DelegateCommand(ExecuteDeleteFieldCommand, CanDeleteFieldCommand));
        public bool CanDeleteFieldCommand() => Fields.CurrentItem != null && !IsInternal;
        public async void ExecuteDeleteFieldCommand()
        {
            var vm = (FieldDialogViewModel) Fields.CurrentItem;
            if ((await DialogService.Ask("Delete Field?", $"Delete field \"{vm.Name}?\"", AskButton.OKCancel)) == AskResult.OK)
            {
                if ((await DialogService.Ask("Are you sure?", $"Are you sure you want to DELETE field \"{vm.Name}?\"", AskButton.OKCancel)) == AskResult.OK)
                {
                    var source = (ObservableCollection<FieldDialogViewModel>)Fields.SourceCollection;
                    source.Remove(vm);
                    Fields.MoveCurrentTo(vm);
                    _fieldsChanged = true;
                    OkCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion
        
        #region CopyFieldCommand
        DelegateCommand _copyFieldCommand;
        public DelegateCommand CopyFieldCommand => _copyFieldCommand ?? (_copyFieldCommand = new DelegateCommand(ExecuteCopyFieldCommand, CanCopyFieldCommand));
        public bool CanCopyFieldCommand() => Fields.CurrentItem != null;
        public void ExecuteCopyFieldCommand()
        {
            var vm = (FieldDialogViewModel)Fields.CurrentItem;
            var source = (ObservableCollection<FieldDialogViewModel>)Fields.SourceCollection;
            vm.Add(IsInternal, source.Select(t => t.Name));
            if (DialogService.ShowDialog(vm))
            {
                Fields.MoveCurrentTo(vm);
                _fieldsChanged = true;
                OkCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #endregion Commands

        #region Public Methods

        public void Add(IEnumerable<string> existingNames)
        {
            _isAdd = true;
            Title = "New Template";

            _existingNames = existingNames;
        }

        public void Edit(IEnumerable<string> existingNames)
        {
            _isAdd = false;
            Title = $"Editing \"{Name}\"";
            
            _existingNames = existingNames.Where(t => !string.Equals(t, Name, StringComparison.OrdinalIgnoreCase)).ToList();

            // Save unmodified properties.
            _unmodifiedValue = Kernel.Get<TemplateDialogViewModel>();
            _unmodifiedValue.CopyFrom(this);
        }

        public static TemplateDialogViewModel CreateFrom(IKernel kernel)
        {
            var vm = kernel.Get<TemplateDialogViewModel>();
            vm.InitEmpty();
            return vm;
        }

        public static TemplateDialogViewModel CreateFrom(IKernel kernel, TemplateDialogViewModel template)
        {
            var vm = kernel.Get<TemplateDialogViewModel>();
            vm.CopyFrom(template);
            return vm;
        }

        public static TemplateDialogViewModel CreateFrom(IKernel kernel, Template template)
        {
            var vm = kernel.Get<TemplateDialogViewModel>();
            vm.InitFrom(template);
            return vm;
        }

        private void InitEmpty()
        {
            Platforms = new CheckListUserControlViewModel<Platform>(Enum.GetValues(typeof(Platform)).Cast<Platform>().OrderBy(p => p.ToString().ToLower()).Select(p => new CheckedItemViewModel<Platform>(p, false)), "All Platforms");
            FormFactors = new CheckListUserControlViewModel<FormFactor>(Enum.GetValues(typeof(FormFactor)).Cast<FormFactor>().OrderBy(ff => ff.ToString().ToLower()).Select(ff => new CheckedItemViewModel<FormFactor>(ff, false)), "All Form Factors");

            Fields = new ListCollectionView(new ObservableCollection<FieldDialogViewModel>());

            View = new T4UserControlViewModel(null, string.Empty);
        }
        
        private void InitFrom(Template template)
        {
            IsInternal = template.IsInternal;

            Platforms = new CheckListUserControlViewModel<Platform>(Enum.GetValues(typeof(Platform)).Cast<Platform>().OrderBy(p => p.ToString().ToLower()).Select(p => new CheckedItemViewModel<Platform>(p, template.Platforms.Contains(p))), "All Platforms");

            FormFactors = new CheckListUserControlViewModel<FormFactor>(Enum.GetValues(typeof(FormFactor)).Cast<FormFactor>().OrderBy(ff => ff.ToString().ToLower()).Select(ff => new CheckedItemViewModel<FormFactor>(ff, template.FormFactors.Contains(ff))), "All Form Factors");

            Framework = template.Framework ?? string.Empty;
            Name = template.Name ?? string.Empty;
            Description = template.Description ?? string.Empty;

            // Deep copy fields.
            var fieldVms = new ObservableCollection<FieldDialogViewModel>();
            foreach (var f in template.Fields)
            {
                var newFieldVm = Kernel.Get<FieldDialogViewModel>();
                newFieldVm.CopyFrom(f);
                fieldVms.Add(newFieldVm);
            }
            Fields = new ListCollectionView(fieldVms);
            
            View = new T4UserControlViewModel(null, template.View ?? string.Empty);

            ViewModelCSharp = template.ViewModelCSharp ?? string.Empty;
            CodeBehindCSharp = template.CodeBehindCSharp ?? string.Empty;

            ViewModelVisualBasic = template.ViewModelVisualBasic ?? string.Empty;
            CodeBehindVisualBasic = template.CodeBehindVisualBasic ?? string.Empty;
        }

        public void CopyFrom(TemplateDialogViewModel template)
        {
            IsInternal = template.IsInternal;

            Platforms = new CheckListUserControlViewModel<Platform>(template.Platforms.Items.Select(p => new CheckedItemViewModel<Platform>(p.Value, p.IsChecked)), "All Platforms ");
            FormFactors = new CheckListUserControlViewModel<FormFactor>(template.FormFactors.Items.Select(ff => new CheckedItemViewModel<FormFactor>(ff.Value, ff.IsChecked)), "All Form Factors");

            Framework = template.Framework;
            Name = template.Name;
            Description = template.Description;

            // Deep copy fields.
            Fields = new ListCollectionView(new ObservableCollection<FieldDialogViewModel>((ObservableCollection<FieldDialogViewModel>)template.Fields.SourceCollection));

            View = template.View;

            ViewModelCSharp = template.ViewModelCSharp;
            CodeBehindCSharp = template.CodeBehindCSharp;

            ViewModelVisualBasic = template.ViewModelVisualBasic;
            CodeBehindVisualBasic = template.CodeBehindVisualBasic;
        }


        #endregion Public Methods

        #region Virtuals

        protected override void TakePropertyChanged(string propertyName)
        {
            OkCommand.RaiseCanExecuteChanged();
        }
        
        #endregion Virtuals

        #region Private Helpers

        #endregion Private Helpers

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Required";

                        if (Name.StartsWith(" "))
                            return "Can't start with a space";

                        if (Name.EndsWith(" "))
                            return "Can't end with a space";

                        if (_existingNames != null &&
                            _existingNames.Any(s => string.Equals(s, Name, StringComparison.OrdinalIgnoreCase)))
                            return "Duplicated name";
                        
                        return null;

                    case nameof(Description):
                        if (string.IsNullOrWhiteSpace(Description))
                            return "Required";

                        return null;
                }
                return null;
            }
        }

        public string Error
        {
            get
            {
                if (this[nameof(Name)] != null ||
                    this[nameof(Description)] != null)
                    return "Error";
                
                return null;
            }
        }

        private bool IsUnchanged()
        {
            if (Platforms == null || FormFactors == null || _unmodifiedValue == null)
                return true;

            return string.Equals(Name, _unmodifiedValue.Name, StringComparison.Ordinal) &&
                   string.Equals(Description, _unmodifiedValue.Description, StringComparison.Ordinal) &&
                   string.Equals(Framework, _unmodifiedValue.Framework, StringComparison.Ordinal) &&
                   string.Equals(Tags, _unmodifiedValue.Tags, StringComparison.Ordinal) &&
                   string.Equals(View.Buffer, _unmodifiedValue.View.Buffer, StringComparison.Ordinal) &&
                   string.Equals(CodeBehindCSharp, _unmodifiedValue.CodeBehindCSharp, StringComparison.Ordinal) &&
                   string.Equals(ViewModelCSharp, _unmodifiedValue.ViewModelCSharp, StringComparison.Ordinal) &&
                   string.Equals(CodeBehindVisualBasic, _unmodifiedValue.CodeBehindVisualBasic, StringComparison.Ordinal) &&
                   string.Equals(ViewModelVisualBasic, _unmodifiedValue.ViewModelVisualBasic, StringComparison.Ordinal) &&

                   !_fieldsChanged &&

                   Platforms.CheckedItemsCommaSeparated == _unmodifiedValue.Platforms.CheckedItemsCommaSeparated &&
                   FormFactors.CheckedItemsCommaSeparated == _unmodifiedValue.FormFactors.CheckedItemsCommaSeparated;
        }

        #endregion IDataErrorInfo
    }
}
