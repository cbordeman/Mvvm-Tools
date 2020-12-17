using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using MvvmTools.Models;
using MvvmTools.Services;
using MvvmTools.Utilities;
using Unity;

// ReSharper disable once ExplicitCallerInfoArgument

namespace MvvmTools.ViewModels
{

    public class ScaffoldDialogViewModel : BaseDialogViewModel, IDataErrorInfo
    {
        #region Data

        private MvvmToolsSettings _settings;
        private IList<ProjectOptions> _projOptions;

        private List<InsertFieldViewModel> _predefinedFields;
        private List<InsertFieldViewModel> _customFields;

        private Project _viewProject;
        private Project _viewModelProject;

        #endregion Data

        #region Ctor and Init

        public ScaffoldDialogViewModel(IDialogService dialogService,
            IMvvmToolsPackage package,
            ISolutionService solutionService,
            ITemplateService templateService,
            ISettingsService settingsService,
            IUnityContainer container) : base(dialogService, container)
        {
            Package = package;
            SolutionService = solutionService;
            TemplateService = templateService;
            SettingsService = settingsService;
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

            TemplateBrowse = Container.Resolve<TemplateBrowseUserControlViewModel>();
            TemplateBrowse.Init(_settings.LocalTemplateFolder);

            FieldValues = Container.Resolve<FieldValuesUserControlViewModel>();

            View = Container.Resolve<T4UserControlViewModel>();
            CodeBehindCSharp = Container.Resolve<T4UserControlViewModel>();
            ViewModelCSharp = Container.Resolve<T4UserControlViewModel>();
            CodeBehindVisualBasic = Container.Resolve<T4UserControlViewModel>();
            ViewModelVisualBasic = Container.Resolve<T4UserControlViewModel>();

            IsBusy = false;
        }

        #endregion Ctor and Init

        #region Properties

        public IMvvmToolsPackage Package { get; set; }
        public ISettingsService SettingsService { get; set; }
        public ISolutionService SolutionService { get; set; }
        public ITemplateService TemplateService { get; set; }

        #region BottomError
        private string _bottomError;
        public string BottomError
        {
            get { return _bottomError; }
            set { SetProperty(ref _bottomError, value); }
        }
        #endregion BottomError

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
                    LocationForView = Container.Resolve<LocationScaffoldUserControlViewModel>();
                    LocationForView.Init(_projOptions, value.ViewLocation, SettingsProject);
                    LocationForViewModel = Container.Resolve<LocationScaffoldUserControlViewModel>();
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

        #region FieldValues
        public FieldValuesUserControlViewModel FieldValues { get; set; }
        #endregion FieldValues

        #region SubLocation
        private string _subLocation;
        public string SubLocation
        {
            get { return _subLocation ?? string.Empty; }
            set { SetProperty(ref _subLocation, value); }
        }
        #endregion SubLocation

        #endregion Properties

        #region Commands

        #region SelectTemplateCommand
        DelegateCommand<TemplateDialogViewModel> _selectCommand;
        public DelegateCommand<TemplateDialogViewModel> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<TemplateDialogViewModel>(ExecuteSelectCommand, CanSelectCommand));
        public bool CanSelectCommand(TemplateDialogViewModel t) => true;
        public void ExecuteSelectCommand(TemplateDialogViewModel t)
        {
            PageNumber++;

            // Only do something if user changed the template.  The user
            // might have simply pressed 'Back' and is now going forward
            // without changing to another template.
            if (SelectedTemplate != t)
            {
                SelectedTemplate = t;

                // Template was selected.
                View.Init(SelectedTemplate.View.Buffer);
                CodeBehindCSharp.Init(SelectedTemplate.CodeBehindCSharp.Buffer);
                ViewModelCSharp.Init(SelectedTemplate.ViewModelCSharp.Buffer);
                CodeBehindVisualBasic.Init(SelectedTemplate.CodeBehindVisualBasic.Buffer);
                ViewModelVisualBasic.Init(SelectedTemplate.ViewModelVisualBasic.Buffer);

                FieldValues.Init((ObservableCollection<FieldDialogViewModel>) t.Fields.SourceCollection);
            }
        }

        private void GetFieldValues()
        {
            _predefinedFields = new List<InsertFieldViewModel>();

            // Basic
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "Name", "System.String", "Bare name, without suffix.", Name));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "OrgSubfolfer", "System.String", "Organizational subfolder (optional).", SubLocation));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewSuffix", "System.String", "The view suffix.", SelectedViewSuffix));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewModelSuffix", "System.String", "The view model suffix.", ViewModelSuffix));

            _viewProject = SolutionService.GetProject(LocationForView.ProjectIdentifier);
            var projModel = SolutionService.GetFullProjectModel(_viewProject);

            var name = Name + SelectedViewSuffix;
            string @namespace;
            if (LocationForView.Namespace.StartsWith("."))
                @namespace = projModel.RootNamespace + LocationForView.Namespace;
            else
                @namespace = LocationForView.Namespace;
            var path = Path.GetDirectoryName(projModel.FullPath);
            // ReSharper disable once AssignNullToNotNullAttribute
            path = Path.Combine(path, LocationForView.PathOffProject);
            if (!string.IsNullOrEmpty(SubLocation))
            {
                path = Path.Combine(path, SubLocation);
                @namespace += "." + SubLocation.Replace("/", ".");
            }
            var fullName = @namespace + '.' + name;
            path = path.Replace("/", "\\");

            // View
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewName", "System.String", "The view class.", name));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewNamespace", "System.String", "The view namespace.", @namespace));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewFullName", "System.String", "Full name of view class, including namespace.", fullName));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "XamlFilePath", "System.String", "Full path of the xaml file.", Path.Combine(path, name + ".xaml")));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "CodeBehindFilePath", "System.String", "Full path of the xaml.cs file.", Path.Combine(path, name + ".xaml.cs")));

            _viewModelProject = SolutionService.GetProject(LocationForViewModel.ProjectIdentifier);
            projModel = SolutionService.GetFullProjectModel(_viewModelProject);

            name = Name + ViewModelSuffix;
            if (LocationForView.Namespace.StartsWith("."))
                @namespace = projModel.RootNamespace + LocationForViewModel.Namespace;
            else
                @namespace = LocationForViewModel.Namespace;
            path = Path.GetDirectoryName(projModel.FullPath);
            // ReSharper disable once AssignNullToNotNullAttribute
            path = Path.Combine(path, LocationForViewModel.PathOffProject);
            if (!string.IsNullOrEmpty(SubLocation))
            {
                path = Path.Combine(path, SubLocation);
                @namespace += "." + SubLocation.Replace("/", ".");
            }
            fullName = @namespace + '.' + name;
            path = path.Replace("/", "\\");

            // View Model
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewModelName", "System.String", "View model class.", name));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewModelNamespace", "System.String", "View model namespace.", @namespace));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewModelFullName", "System.String", "Full name of view model class, including namespace.", fullName));
            _predefinedFields.Add(InsertFieldViewModel.Create(Container, "ViewModelFilePath", "System.String", "Full path of the view model class file.", Path.Combine(path, name + ".cs")));

            // Custom fields
            _customFields = new List<InsertFieldViewModel>();
            foreach (var f in FieldValues.Fields)
            {
                object val;
                string type;
                // ReSharper disable once PossibleInvalidOperationException
                switch (f.SelectedFieldType.Value)
                {
                    case FieldType.TextBox:
                    case FieldType.TextBoxMultiLine:
                    case FieldType.ComboBox:
                    case FieldType.ComboBoxOpen:
                    case FieldType.Class:
                        val = f.DefaultString;
                        type = "System.String";
                        break;
                    case FieldType.CheckBox:
                        val = f.DefaultBoolean;
                        type = "System.Boolean";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _customFields.Add(InsertFieldViewModel.Create(Container, f.Name, type, f.Description, val));
            }
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
        public bool CanOkCommand() => Error == null && PageNumber != 1 && (PageNumber < 3 || BottomError == null);
        public void ExecuteOkCommand()
        {
            PageNumber++;
            switch (PageNumber)
            {
                case 3:
                    // Field values were entered.
                    GetFieldValues();
                    
                    View.ResetFieldValues(_predefinedFields, _customFields);
                    CodeBehindCSharp.ResetFieldValues(_predefinedFields, _customFields);
                    ViewModelCSharp.ResetFieldValues(_predefinedFields, _customFields);
                    CodeBehindVisualBasic.ResetFieldValues(_predefinedFields, _customFields);
                    ViewModelVisualBasic.ResetFieldValues(_predefinedFields, _customFields);
                    break;
                case 4:

                    break;
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

        private void SetBottomError()
        {
            if (string.IsNullOrWhiteSpace(View.Buffer))
            {
                BottomError = "View is required.";
                return;
            }

            var csSatisfied = !string.IsNullOrWhiteSpace(CodeBehindCSharp.Buffer) &&
                              !string.IsNullOrWhiteSpace(ViewModelCSharp.Buffer);
            var vbSatisfied = !string.IsNullOrWhiteSpace(CodeBehindVisualBasic.Buffer) &&
                              !string.IsNullOrWhiteSpace(ViewModelVisualBasic.Buffer);
            if (!csSatisfied && !vbSatisfied)
            {
                BottomError = "Both the C# blocks OR both the VB blocks must be set.";
                return;
            }

            BottomError = null;

            OkCommand.RaiseCanExecuteChanged();
        }

        #endregion Private Methods

        #region Virtuals

        protected override void TakePropertyChanged(string propertyName)
        {
            if (propertyName.EndsWith(nameof(View.Buffer)))
            {
                SetBottomError();
                OkCommand.RaiseCanExecuteChanged();
            }
        }

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
                    case nameof(SubLocation):
                        if (!string.IsNullOrEmpty(SubLocation))
                        {
                            var e = ValidationUtilities.ValidatePathOffProject(SubLocation);
                            if (e != null)
                                return e;
                            var split = SubLocation.Split('/');
                            foreach (var s in split)
                            {
                                e = ValidationUtilities.ValidateName(s);
                                if (e != null)
                                    return e;
                            }
                        }
                        return null;
                }
                return null;
            }
        }

        public string Error => ValidationUtilities.ValidateViewModelSuffix(ViewModelSuffix) != null || ValidationUtilities.ValidateName(Name) != null || string.IsNullOrWhiteSpace(SelectedViewSuffix) || LocationForViewModel == null || LocationForViewModel.Error != null || LocationForView == null || LocationForView.Error != null ? string.Empty : null;

        #endregion IDataErrorInfo
    }
}
