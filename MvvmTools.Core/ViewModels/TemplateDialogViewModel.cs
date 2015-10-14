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

        #region BottomError
        private string _bottomError;
        public string BottomError
        {
            get { return _bottomError; }
            set { SetProperty(ref _bottomError, value); }
        }
        #endregion BottomError

        #region DialogService
        [Inject]
        public IDialogService DialogService { get; set; }
        #endregion DialogService

        public Template TemplateModel { get; set; }

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
        private T4UserControlViewModel _viewModelCSharp;
        public T4UserControlViewModel ViewModelCSharp
        {
            get { return _viewModelCSharp; }
            set { SetProperty(ref _viewModelCSharp, value); }
        }
        #endregion ViewModelCSharp

        #region ViewModelVisualBasic
        private T4UserControlViewModel _viewModelVisualBasic;
        public T4UserControlViewModel ViewModelVisualBasic
        {
            get { return _viewModelVisualBasic; }
            set { SetProperty(ref _viewModelVisualBasic, value); }
        }
        #endregion ViewModelVisualBasic
        
        #region CodeBehindCSharp
        private T4UserControlViewModel _codeBehindCSharp;
        public T4UserControlViewModel CodeBehindCSharp
        {
            get { return _codeBehindCSharp; }
            set { SetProperty(ref _codeBehindCSharp, value); }
        }
        #endregion CodeBehindCSharp

        #region CodeBehindVisualBasic
        private T4UserControlViewModel _codeBehindVisualBasic;
        public T4UserControlViewModel CodeBehindVisualBasic
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
        #endregion OkCommand

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

            _fieldsChanged = false;

            _existingNames = existingNames;
        }

        public void Edit(IEnumerable<string> existingNames)
        {
            _isAdd = false;
            Title = $"Editing \"{Name}\"";
            
            _existingNames = existingNames.Where(t => !string.Equals(t, Name, StringComparison.OrdinalIgnoreCase)).ToList();

            _fieldsChanged = false;

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

            View = T4UserControlViewModel.Create(Kernel, null, string.Empty);
            View.PropertyChanged += T4OnPropertyChanged;

            ViewModelCSharp = T4UserControlViewModel.Create(Kernel, null, string.Empty);
            ViewModelCSharp.PropertyChanged += T4OnPropertyChanged;
            CodeBehindCSharp = T4UserControlViewModel.Create(Kernel, null, string.Empty);
            CodeBehindCSharp.PropertyChanged += T4OnPropertyChanged;

            ViewModelVisualBasic = T4UserControlViewModel.Create(Kernel, null, string.Empty);
            ViewModelVisualBasic.PropertyChanged += T4OnPropertyChanged;
            CodeBehindVisualBasic = T4UserControlViewModel.Create(Kernel, null, string.Empty);
            CodeBehindVisualBasic.PropertyChanged += T4OnPropertyChanged;
        }

        private void T4OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(T4UserControlViewModel.IsModified))
            {
                //var vm = (T4UserControlViewModel) sender;
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        private void InitFrom(Template template)
        {
            TemplateModel = template;

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
            
            View = T4UserControlViewModel.Create(Kernel, null, template.View);
            View.PropertyChanged += T4OnPropertyChanged;

            ViewModelCSharp = T4UserControlViewModel.Create(Kernel, null, template.ViewModelCSharp);
            ViewModelCSharp.PropertyChanged += T4OnPropertyChanged;
            CodeBehindCSharp = T4UserControlViewModel.Create(Kernel, null, template.CodeBehindCSharp);
            CodeBehindCSharp.PropertyChanged += T4OnPropertyChanged;

            ViewModelVisualBasic = T4UserControlViewModel.Create(Kernel, null, template.ViewModelVisualBasic);
            ViewModelVisualBasic.PropertyChanged += T4OnPropertyChanged;
            CodeBehindVisualBasic = T4UserControlViewModel.Create(Kernel, null, template.CodeBehindVisualBasic);
            CodeBehindVisualBasic.PropertyChanged += T4OnPropertyChanged;
        }

        public void CopyFrom(TemplateDialogViewModel template)
        {
            TemplateModel = template.TemplateModel;

            IsInternal = template.IsInternal;

            Platforms = new CheckListUserControlViewModel<Platform>(template.Platforms.Items.Select(p => new CheckedItemViewModel<Platform>(p.Value, p.IsChecked)), "All Platforms ");
            FormFactors = new CheckListUserControlViewModel<FormFactor>(template.FormFactors.Items.Select(ff => new CheckedItemViewModel<FormFactor>(ff.Value, ff.IsChecked)), "All Form Factors");

            Framework = template.Framework;
            Name = template.Name;
            Description = template.Description;

            // Deep copy fields.
            Fields = new ListCollectionView(new ObservableCollection<FieldDialogViewModel>((ObservableCollection<FieldDialogViewModel>)template.Fields.SourceCollection));

            View = T4UserControlViewModel.Create(Kernel, null, template.View.Buffer);
            View.PropertyChanged += T4OnPropertyChanged;

            ViewModelCSharp = T4UserControlViewModel.Create(Kernel, null, template.ViewModelCSharp.Buffer);
            ViewModelCSharp.PropertyChanged += T4OnPropertyChanged;
            CodeBehindCSharp = T4UserControlViewModel.Create(Kernel, null, template.CodeBehindCSharp.Buffer);
            CodeBehindCSharp.PropertyChanged += T4OnPropertyChanged;

            ViewModelVisualBasic = T4UserControlViewModel.Create(Kernel, null, template.ViewModelVisualBasic.Buffer);
            ViewModelVisualBasic.PropertyChanged += T4OnPropertyChanged;
            CodeBehindVisualBasic = T4UserControlViewModel.Create(Kernel, null, template.CodeBehindVisualBasic.Buffer);
            CodeBehindVisualBasic.PropertyChanged += T4OnPropertyChanged;
        }
        
        #endregion Public Methods

        #region Virtuals

        protected override void TakePropertyChanged(string propertyName)
        {
            if (propertyName != nameof(BottomError))
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
                BottomError = this[nameof(Name)];
                if (BottomError != null)
                {
                    BottomError = "Template name is required.";
                    return string.Empty;
                }

                BottomError = this[nameof(Description)];
                if (BottomError != null)
                {
                    BottomError = "Template description is required.";
                    return string.Empty;
                }

                if (string.IsNullOrWhiteSpace(View.Buffer))
                {
                    BottomError = "View is required.";
                    return string.Empty;
                }

                var csSatisfied = !string.IsNullOrWhiteSpace(CodeBehindCSharp.Buffer) &&
                                  !string.IsNullOrWhiteSpace(ViewModelCSharp.Buffer);
                var vbSatisfied = !string.IsNullOrWhiteSpace(CodeBehindVisualBasic.Buffer) &&
                                  !string.IsNullOrWhiteSpace(ViewModelVisualBasic.Buffer);
                if (!csSatisfied && !vbSatisfied)
                {
                    BottomError = "Both the C# blocks OR both the VB blocks must be set.";
                    return string.Empty;
                }
                
                BottomError = null;
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
                   
                   !View.IsModified &&
                   !CodeBehindCSharp.IsModified &&
                   !ViewModelCSharp.IsModified &&
                   !CodeBehindVisualBasic.IsModified &&
                   !ViewModelVisualBasic.IsModified &&

                   !_fieldsChanged &&

                   Platforms.CheckedItemsCommaSeparated == _unmodifiedValue.Platforms.CheckedItemsCommaSeparated &&
                   FormFactors.CheckedItemsCommaSeparated == _unmodifiedValue.FormFactors.CheckedItemsCommaSeparated;
        }

        #endregion IDataErrorInfo
    }
}
