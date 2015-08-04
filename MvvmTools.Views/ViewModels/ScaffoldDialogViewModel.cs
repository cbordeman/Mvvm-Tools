using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
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

        public async Task Init()
        {
            Title = "Scaffold View and ViewModel";

            IsBusy = true;

            _settings = await SettingsService.LoadSettings();

            Projects = _settings.ProjectOptions;

            _projOptions = new List<ProjectOptions>(Projects);
            _projOptions.Insert(0, new ProjectOptions { ProjectModel = new ProjectModel("(current project)", null, null, ProjectKind.Project, null) });

            var projId = Package.ActiveDocument?.ProjectItem?.ContainingProject?.UniqueName ??
                         Projects.FirstOrDefault()?.ProjectModel.ProjectIdentifier;
            SettingsProject = Projects.FirstOrDefault(p => p.ProjectModel.ProjectIdentifier == projId);

            Name = null;

            ViewSuffixes = new ObservableCollection<string>(_settings.ViewSuffixes);
            ViewSuffixes.Insert(0, string.Empty);
            SelectedViewSuffix = ViewSuffixes[0];

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

        #region LocationForView
        private LocationScaffoldUserControlViewModel _locationForView;
        public LocationScaffoldUserControlViewModel LocationForView
        {
            get { return _locationForView; }
            set { SetProperty(ref _locationForView, value); }
        }
        #endregion LocationForView

        #region LocationForViewModel
        private LocationScaffoldUserControlViewModel _locationForViewModel;
        public LocationScaffoldUserControlViewModel LocationForViewModel
        {
            get { return _locationForViewModel; }
            set { SetProperty(ref _locationForViewModel, value); }
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
                    LocationForView = new LocationScaffoldUserControlViewModel();
                    LocationForView.Init(_projOptions, value.ViewLocation);
                    LocationForViewModel = new LocationScaffoldUserControlViewModel();
                    LocationForViewModel.Init(_projOptions, value.ViewModelLocation);
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

        #endregion Properties

        #region Commands


        #endregion Commands

        #region Private Methods
        
        #endregion Private Methods

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "ViewModelSuffix":
                        return ValidationUtilities.ValidateViewModelSuffix(ViewModelSuffix);
                    case "Name":
                        return ValidationUtilities.ValidateName(Name);
                    case "SelectedViewSuffix":
                        return string.IsNullOrWhiteSpace(SelectedViewSuffix) ? "Select the view suffix." : null;
                }
                return null;
            }
        }

        public string Error => null;

        #endregion IDataErrorInfo
    }
}
