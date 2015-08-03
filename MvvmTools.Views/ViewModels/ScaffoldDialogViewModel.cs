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

        #endregion Data

        #region Ctor and Init

        public async Task Init()
        {
            Title = "Scaffold View and ViewModel";

            IsBusy = true;

            Projects = await SolutionService.GetProjectsList();
            
            _settings = await SettingsService.LoadSettings();

            var projId = Package.ActiveDocument?.ProjectItem?.ContainingProject?.UniqueName ??
                         Projects.FirstOrDefault()?.ProjectIdentifier;
            SettingsProject = Projects.FirstOrDefault(p => p.ProjectIdentifier == projId);

            Prefix = null;

            ViewSuffixes = new ObservableCollection<string>(_settings.ViewSuffixes) {null};
            SelectedViewSuffix = null;

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

        #region Prefix
        private string _prefix;
        public string Prefix
        {
            get { return _prefix; }
            set { SetProperty(ref _prefix, value); }
        }
        #endregion Prefix

        #region Projects
        private List<ProjectModel> _projects;
        public List<ProjectModel> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }
        #endregion ProjectModels

        #region SettingsProject
        private ProjectModel _settingsProject;
        public ProjectModel SettingsProject
        {
            get { return _settingsProject; }
            set
            {
                if (SetProperty(ref _settingsProject, value))
                {
                    
                }
            }
        }
        #endregion SettingsProject

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

        // Passed in inherited view model could be from SettingsService (for the solution),
        // or from the solution (for the projects) (mutable). 
        private async Task<ProjectOptionsUserControlViewModel> CreateProjectOptionsUserControlViewModel(ProjectOptions projectOptions, ProjectOptionsUserControlViewModel inherited,
            bool isProject)
        {
            var rval = Kernel.Get<ProjectOptionsUserControlViewModel>();

            await rval.Initialize(
                projectOptions,
                isProject,
                inherited,
                projectOptions.ViewModelSuffix,
                Kernel.Get<LocationDescriptorUserControlViewModel>(),
                Kernel.Get<LocationDescriptorUserControlViewModel>());

            return rval;
        }


        #endregion Private Methods

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ViewModelSuffix")
                    return ValidationUtilities.ValidateViewModelSuffix(ViewModelSuffix);
                return null;
            }
        }

        public string Error => null;

        #endregion IDataErrorInfo
    }
}
