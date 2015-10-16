using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MvvmTools.Core.Extensions;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using MvvmTools.Core.Views;
using Ninject;

// ReSharper disable once ExplicitCallerInfoArgument

namespace MvvmTools.Core.ViewModels
{

    public class ScaffoldDialogViewModel : BaseDialogViewModel, IDataErrorInfo
    {
        #region Data

        private MvvmToolsSettings _settings;
        private IList<ProjectOptions> _projOptions;
        
        #endregion Data

        #region Ctor and Init

        public ScaffoldDialogViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        public async Task Init()
        {
            Title = "Scaffold View and ViewModel";

            IsBusy = true;

            _settings = await SettingsService.LoadSettings();

            Projects = _settings.ProjectOptions;

            _projOptions = new List<ProjectOptions>(Projects);
            
            var projId = Package.ActiveDocument?.ProjectItem?.ContainingProject?.UniqueName ??
                         Projects.FirstOrDefault()?.ProjectModel.ProjectIdentifier;
            SettingsProject = Projects.FirstOrDefault(p => p.ProjectModel.ProjectIdentifier == projId);

            Name = null;

            ViewSuffixes = new ObservableCollection<string>(_settings.ViewSuffixes);
            ViewSuffixes.Insert(0, string.Empty);
            SelectedViewSuffix = ViewSuffixes[0];

            TemplateBrowse = Kernel.Get<TemplateBrowseUserControlViewModel>();
            TemplateBrowse.Init(_settings.LocalTemplateFolder);

            FieldValues = Kernel.Get<FieldValuesUserControlViewModel>();

            View = Kernel.Get<T4UserControlViewModel>();
            CodeBehindCSharp = Kernel.Get<T4UserControlViewModel>();
            ViewModelCSharp = Kernel.Get<T4UserControlViewModel>();
            CodeBehindVisualBasic = Kernel.Get<T4UserControlViewModel>();
            ViewModelVisualBasic = Kernel.Get<T4UserControlViewModel>();

            IsBusy = false;
        }

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public IMvvmToolsPackage Package { get; set; }

        [Inject]
        public ISettingsService SettingsService { get; set; }

        [Inject]
        public ISolutionService SolutionService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }
        
        #region TemplateBrowseUserControl
        private TemplateBrowseUserControlViewModel _templateBrowse;
        public TemplateBrowseUserControlViewModel TemplateBrowse
        {
            get { return _templateBrowse; }
            set { SetProperty(ref _templateBrowse, value); }
        }
        #endregion TemplateBrowseUserControl

        #region LocationForView
        private LocationScaffoldUserControlViewModel _locationForView;
        public LocationScaffoldUserControlViewModel LocationForView
        {
            get { return _locationForView; }
            set
            {
                if (_locationForView != null)
                    _locationForView.PropertyChanged -= OnPropertyChanged;
                // We've removed the PropertyChanged handler, so we don't check the return
                // value of SetProperty(), we just resubscribe.
                SetProperty(ref _locationForView, value);
                if (value != null)
                    _locationForView.PropertyChanged += OnPropertyChanged;
            }
        }
        #endregion LocationForView

        #region LocationForViewModel
        private LocationScaffoldUserControlViewModel _locationForViewModel;
        public LocationScaffoldUserControlViewModel LocationForViewModel
        {
            get { return _locationForViewModel; }
            set
            {
                if (_locationForViewModel != null)
                    _locationForViewModel.PropertyChanged -= OnPropertyChanged;
                // We've removed the PropertyChanged handler, so we don't check the return
                // value of SetProperty(), we just resubscribe.
                SetProperty(ref _locationForViewModel, value);
                if (value != null)
                    _locationForViewModel.PropertyChanged += OnPropertyChanged;
            }
        }
        #endregion LocationForViewModel

        #region Projects
        private IList<ProjectOptions> _projects;
        public IList<ProjectOptions> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }
        #endregion ProjectModels

        #region SettingsProject
        private ProjectOptions _settingsProject;
        public ProjectOptions SettingsProject
        {
            get { return _settingsProject; }
            set
            {
                if (SetProperty(ref _settingsProject, value))
                {
                    ViewModelSuffix = value.ViewModelSuffix;
                    LocationForView = Kernel.Get<LocationScaffoldUserControlViewModel>();
                    LocationForView.Init(_projOptions, value.ViewLocation, SettingsProject);
                    LocationForViewModel = Kernel.Get<LocationScaffoldUserControlViewModel>();
                    LocationForViewModel.Init(_projOptions, value.ViewModelLocation, SettingsProject);
                }
            }
        }
        #endregion SettingsProject

        #region Name
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region ViewModelSuffix
        private string _viewModelSuffix;
        public string ViewModelSuffix
        {
            get { return _viewModelSuffix; }
            set { SetProperty(ref _viewModelSuffix, value); }
        }
        #endregion ViewModelSuffix

        #region IsBusy
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        #endregion IsBusy
        
        #region ViewSuffixes
        private ObservableCollection<string> _viewSuffixes;
        public ObservableCollection<string> ViewSuffixes
        {
            get { return _viewSuffixes; }
            set { SetProperty(ref _viewSuffixes, value); }
        }
        #endregion ViewSuffixes

        #region SelectedViewSuffix
        private string _selectedViewSuffix;
        public string SelectedViewSuffix
        {
            get { return _selectedViewSuffix; }
            set { SetProperty(ref _selectedViewSuffix, value); }
        }
        #endregion SelectedViewSuffix
        
        #region SelectedTemplate
        private TemplateDialogViewModel _selectedTemplate;
        public TemplateDialogViewModel SelectedTemplate
        {
            get { return _selectedTemplate; }
            set { SetProperty(ref _selectedTemplate, value); }
        }
        #endregion SelectedTemplate

        #region PageNumber
        private int _pageNumber;
        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                if (SetProperty(ref _pageNumber, value))
                {
                    BackCommand.RaiseCanExecuteChanged();
                    OkCommand.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion PageNumber

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

        public FieldValuesUserControlViewModel FieldValues { get; set; }

        #endregion Properties

        #region Commands

        #region SelectTemplateCommand
        DelegateCommand<TemplateDialogViewModel> _selectCommand;
        public DelegateCommand<TemplateDialogViewModel> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<TemplateDialogViewModel>(ExecuteSelectCommand, CanSelectCommand));
        public bool CanSelectCommand(TemplateDialogViewModel t) => true;
        public void ExecuteSelectCommand(TemplateDialogViewModel t)
        {
            SelectedTemplate = t;
            
            FieldValues.Init((ObservableCollection<FieldDialogViewModel>)t.Fields.SourceCollection);
            PageNumber++;
        }

        private List<InsertFieldViewModel> GetPredefinedFieldValues()
        {
            var rval = new List<InsertFieldViewModel>();

            rval.Add(InsertFieldViewModel.Create(Kernel, "Name", "Bare name, with suffix.", Name));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewSuffix", "The view suffix.", SelectedViewSuffix));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewModelSuffix", "The view suffix.", ViewModelSuffix));

            var proj = SolutionService.GetProject(LocationForView.ProjectIdentifier);
            var projModel = SolutionService.GetFullProjectModel(proj);

            var name = Name + SelectedViewSuffix;
            string @namespace;
            if (LocationForView.Namespace.StartsWith("."))
                @namespace = projModel.RootNamespace + LocationForView.Namespace;
            else
                @namespace = LocationForView.Namespace;
            var fullName = @namespace + '.' + name;
            var path = Path.GetDirectoryName(projModel.FullPath);
            path = Path.Combine(path, LocationForView.PathOffProject);
            path = path.Replace("/", "\\");

            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewName", "The view class.", name));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewNamespace", "The view namespace.", @namespace));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewFullName", "Full name of view class, including namespace.", fullName));
            rval.Add(InsertFieldViewModel.Create(Kernel, "XamlFilePath", "Full path of the xaml file.", Path.Combine(path, name + ".xaml")));
            rval.Add(InsertFieldViewModel.Create(Kernel, "CodeBehindFilePath", "Full path of the xaml.cs file.", Path.Combine(path, name + ".xaml.cs")));

            proj = SolutionService.GetProject(LocationForViewModel.ProjectIdentifier);
            projModel = SolutionService.GetFullProjectModel(proj);

            name = Name + ViewModelSuffix;
            if (LocationForView.Namespace.StartsWith("."))
                @namespace = projModel.RootNamespace + LocationForViewModel.Namespace;
            else
                @namespace = LocationForViewModel.Namespace;
            fullName = @namespace + '.' + name;
            path = Path.GetDirectoryName(projModel.FullPath);
            path = Path.Combine(path, LocationForViewModel.PathOffProject);
            path = path.Replace("/", "\\");

            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewModelName", "View model class.", name));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewModelNamespace", "View model namespace.", @namespace));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewModelFullName", "Full name of view model class, including namespace.", fullName));
            rval.Add(InsertFieldViewModel.Create(Kernel, "ViewModelFilePath", "Full path of the view model class file.", Path.Combine(path, name + ".cs")));
            
            return rval;
        }
        
        private List<InsertFieldViewModel> GetCustomFieldValues()
        {
            var rval = new List<InsertFieldViewModel>();
            foreach (var f in FieldValues.Fields)
            {
                string val = null;
                if (f.SelectedFieldType != null)
                    switch (f.SelectedFieldType.Value)
                    {
                        case FieldType.TextBox:
                        case FieldType.TextBoxMultiLine:
                        case FieldType.ComboBox:
                        case FieldType.ComboBoxOpen:
                            val = f.DefaultString;
                            break;
                        case FieldType.CheckBox:
                            val = f.DefaultBoolean.ToString();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                rval.Add(InsertFieldViewModel.Create(Kernel, f.Name, f.Description, val));
            }
            return rval;
        }

        #endregion TemplateCommand

        #region BackCommand

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand => _backCommand ?? (_backCommand = new DelegateCommand(ExecuteBackCommand, CanBackCommand));
        public bool CanBackCommand() => PageNumber > 0;

        public void ExecuteBackCommand()
        {
            PageNumber--;
        }

        #endregion BackCommand

        #region OkCommand

        private DelegateCommand _okCommand;
        public DelegateCommand OkCommand => _okCommand ?? (_okCommand = new DelegateCommand(ExecuteOkCommand, CanOkCommand));
        public bool CanOkCommand() => Error == null && PageNumber != 1;

        public void ExecuteOkCommand()
        {
            PageNumber++;
            if (PageNumber == 3)
            {
                var predefinedFields = GetPredefinedFieldValues();
                var customFields = GetCustomFieldValues();

                View.Init(null, SelectedTemplate.View.Buffer, predefinedFields, customFields);
                CodeBehindCSharp.Init(null, SelectedTemplate.CodeBehindCSharp.Buffer, predefinedFields, customFields);
                ViewModelCSharp.Init(null, SelectedTemplate.ViewModelCSharp.Buffer, predefinedFields, customFields);
                CodeBehindVisualBasic.Init(null, SelectedTemplate.CodeBehindVisualBasic.Buffer, predefinedFields, customFields);
                ViewModelVisualBasic.Init(null, SelectedTemplate.ViewModelVisualBasic.Buffer, predefinedFields, customFields);
            }
        }

        #endregion OkCommand

        #endregion Commands

        #region Private Methods

        // Called on our own PropertyChanged and on the view and view model locator property's PropertyChanged events.
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            OkCommand.RaiseCanExecuteChanged();
        }

        #endregion Private Methods

        #region Virtuals

        #endregion Virtuals

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(ViewModelSuffix):
                        return ValidationUtilities.ValidateViewModelSuffix(ViewModelSuffix);
                    case nameof(Name):
                        return ValidationUtilities.ValidateName(Name);
                    case nameof(SettingsProject):
                        return SettingsProject == null ? "Required" : null;
                    case nameof(SelectedViewSuffix):
                        return string.IsNullOrWhiteSpace(SelectedViewSuffix) ? "Required" : null;
                }
                return null;
            }
        }

        public string Error => ValidationUtilities.ValidateViewModelSuffix(ViewModelSuffix) != null || ValidationUtilities.ValidateName(Name) != null || string.IsNullOrWhiteSpace(SelectedViewSuffix) || LocationForViewModel == null || LocationForViewModel.Error != null || LocationForView == null || LocationForView.Error != null ? string.Empty : null;

        #endregion IDataErrorInfo
    }
}
