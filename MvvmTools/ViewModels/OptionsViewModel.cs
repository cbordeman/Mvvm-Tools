using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using MvvmTools.Models;
using MvvmTools.Services;
using MvvmTools.Utilities;
using Unity;

// ReSharper disable ExplicitCallerInfoArgument

namespace MvvmTools.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        #region Data

        private bool _isInitialized;

        private readonly ISolutionService _solutionService;
        private readonly ISettingsService _settingsService;
        //private readonly ITemplateService _templateService;

        private MvvmToolsSettings _unmodifiedSettings;

        private static readonly Regex SuffixRegex = new Regex(@"^[_a-zA-Z0-9]*$");
        private const string SuffixRegexErrorMessage = "Not a valid view suffix.";

        // Used to store original values of the solutions' properties so they can
        // be applied to inherited properties in each project.
        private object _oldValue;

        #endregion Data

        #region Ctor and Init

        public OptionsViewModel(IUnityContainer container, 
            ISolutionService solutionService, 
            ISettingsService settingsService, 
            IDialogService dialogService,
            ISettingsService settingsSvc)
            :base(container)
        {
            DialogService = dialogService;
            SettingsSvc = settingsSvc;
            _solutionService = solutionService;
            _settingsService = settingsService;
            //_templateService = templateService;
        }

        public void Init()
        {
            if (_isInitialized)
                return;
            _isInitialized = true;

            TemplateBrowseUserControlViewModel = Container.Resolve<TemplateBrowseUserControlViewModel>();
            
            // Note the framework filter is setup in RefreshTemplates() because it's not based on an enum.

            GoToViewOrViewModelOptions = new List<ValueDescriptor<GoToViewOrViewModelOption>>
                        {
                            new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ShowUi, "Ask"),
                            new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseXaml, "If view, open the XAML"),
                            new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseCodeBehind, "If view, open the code behind"),
                            new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseFirst, "Always open the first item found")
                        };

            // This sets IsBusy = true, starts to wait on the solution to finish loading,
            // and then returns immediately.  After the solution loads, IsBusy is set
            // to false and the view model finishes initializing itself.  It also subscribes
            // to the solution service's SolutionLoadStateChanged event so it can do all
            // this again if necessary.
            SolutionServiceOnSolutionLoadStateChanged(null, null);
        }

        private void SolutionServiceOnSolutionLoadStateChanged(object sender, EventArgs eventArgs)
        {
            // Something happened in the solution or a project, respond by reloading solution 
            // and project state.  Current state is ignored/discarded.

            // Start by preventing a recursive call.
            _solutionService.SolutionLoadStateChanged -= SolutionServiceOnSolutionLoadStateChanged;

            IsBusy = true;

            // Load settings.  This takes a while because the solution may not be fully
            // loaded yet or some other solution or project operation may be under way.
            var loadSettingsTask = SettingsSvc.LoadSettings();

            loadSettingsTask.ContinueWith(
                task =>
                {
                    IsBusy = false;

                    var settings = task.Result;

                    this.TemplateBrowseUserControlViewModel.Init(settings.LocalTemplateFolder);

                    // Save the original, unmodified settings.
                    _unmodifiedSettings = settings;
                    
                    // This actually applies the _unmodifiedSettings to the properties.
                    RevertSettings().Forget();
                    
                    // Now subscribe to the Solution Service's SolutionLoadStateChanged event so
                    // we can do all this over again when the solution or projects change again.
                    _solutionService.SolutionLoadStateChanged += SolutionServiceOnSolutionLoadStateChanged;

                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        #endregion Ctor and Init

        #region Properties

        public IDialogService DialogService { get; set; }

        public ISettingsService SettingsSvc { get; set; }

        #region IsBusy
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        #endregion IsBusy

        #region TemplateBrowseUserControlViewModel
        private TemplateBrowseUserControlViewModel _templateBrowseUserControlViewModel;
        public TemplateBrowseUserControlViewModel TemplateBrowseUserControlViewModel
        {
            get { return _templateBrowseUserControlViewModel; }
            set { SetProperty(ref _templateBrowseUserControlViewModel, value); }
        }
        #endregion TemplateBrowseUserControlViewModel

        #region ViewSuffixesView
        private ListCollectionView _viewSuffixesView;
        public ListCollectionView ViewSuffixesView
        {
            get { return _viewSuffixesView; }
            set { SetProperty(ref _viewSuffixesView, value); }
        }
        #endregion ViewSuffixesView

        #region ViewSuffixes
        private ObservableCollection<StringViewModel> _viewSuffixes;
        public ObservableCollection<StringViewModel> ViewSuffixes
        {
            get { return _viewSuffixes; }
            set
            {
                if (SetProperty(ref _viewSuffixes, value))
                {
                    if (ViewSuffixesView != null)
                        ViewSuffixesView.CurrentChanged -= ViewSuffixesViewOnCurrentChanged;
                    ViewSuffixesView = new ListCollectionView(value);
                    ViewSuffixesView.CurrentChanged += ViewSuffixesViewOnCurrentChanged;
                }
            }
        }
        #endregion ViewSuffixes

        #region LocalTemplateFolder
        private string _localTemplateFolder;
        public string LocalTemplateFolder
        {
            get { return _localTemplateFolder; }
            set
            {
                if (SetProperty(ref _localTemplateFolder, value))
                {
                    // Must be present, and non-relative.
                    if (string.IsNullOrWhiteSpace(_localTemplateFolder) || !Path.IsPathRooted(_localTemplateFolder))
                        LocalTemplateFolder = _settingsService.DefaultLocalTemplateFolder;

                    _templateBrowseUserControlViewModel.ChangeLocalTemplatesFolder(value);
                }
            }
        }

        #endregion LocalTemplateFolder

        #region ProjectsOptions
        private List<ProjectOptionsUserControlViewModel> _projectsOptions;
        public List<ProjectOptionsUserControlViewModel> ProjectsOptions
        {
            get { return _projectsOptions; }
            set
            {
                if (SetProperty(ref _projectsOptions, value))
                    SelectedProjectOption = value.FirstOrDefault();
            }
        }
        #endregion ProjectsOptions

        #region SelectedProjectOption
        private ProjectOptionsUserControlViewModel _selectedProjectOption;
        public ProjectOptionsUserControlViewModel SelectedProjectOption
        {
            get { return _selectedProjectOption; }
            set
            {
                if (SetProperty(ref _selectedProjectOption, value))
                    NotifyPropertyChanged(nameof(ShowResetAll));
            }
        }
        #endregion SelectedProjectOption

        #region ShowResetAll
        public bool ShowResetAll => SelectedProjectOption != null && SelectedProjectOption == ProjectsOptions?[0];
        #endregion ShowResetAll

        #region GoToViewOrViewModelOptions
        private List<ValueDescriptor<GoToViewOrViewModelOption>> _goToViewOrViewModelOptions;
        public List<ValueDescriptor<GoToViewOrViewModelOption>> GoToViewOrViewModelOptions
        {
            get { return _goToViewOrViewModelOptions; }
            set { SetProperty(ref _goToViewOrViewModelOptions, value); }
        }
        #endregion GoToViewOrViewModelOptions

        #region GoToViewOrViewModelSearchSolution
        private bool _goToViewOrViewModelSearchSolution;
        public bool GoToViewOrViewModelSearchSolution
        {
            get { return _goToViewOrViewModelSearchSolution; }
            set { SetProperty(ref _goToViewOrViewModelSearchSolution, value); }
        }
        #endregion GoToViewOrViewModelSearchSolution

        #region SelectedGoToViewOrViewModelOption
        private GoToViewOrViewModelOption _selectedGoToViewOrViewModelOption;
        public GoToViewOrViewModelOption SelectedGoToViewOrViewModelOption
        {
            get { return _selectedGoToViewOrViewModelOption; }
            set { SetProperty(ref _selectedGoToViewOrViewModelOption, value); }
        }
        #endregion SelectedGoToViewOrViewModelOption
        
        #endregion Properties

        #region Public Methods

        public MvvmToolsSettings GetCurrentSettings()
        {
            if (ProjectsOptions == null || ViewSuffixes == null)
                return null;

            // Extracts settings from view model properties.  These are the 
            // 'current' settings, while the unmodified settings values are stored in
            // _unmodifiedSettings.
            var settings = new MvvmToolsSettings
            {
                GoToViewOrViewModelOption = SelectedGoToViewOrViewModelOption,
                GoToViewOrViewModelSearchSolution = GoToViewOrViewModelSearchSolution,
                ViewSuffixes = ViewSuffixes.Select(vs => vs.Value).ToArray(),
                LocalTemplateFolder = LocalTemplateFolder
            };

            for (int index = 0; index < ProjectsOptions.Count; index++)
            {
                // First item is the solution, subsequent items are the projects.
                if (index == 0)
                    settings.SolutionOptions = ConvertToProjectOptions(ProjectsOptions[0]);
                else
                {
                    var po = ProjectsOptions[index];
                    settings.ProjectOptions.Add(ConvertToProjectOptions(po));
                }
            }

            return settings;
        }

        private static ProjectOptions ConvertToProjectOptions(ProjectOptionsUserControlViewModel projectOptionsVm)
        {
            var rval = new ProjectOptions
            {
                ProjectModel = projectOptionsVm.ProjectModel,
                ViewModelSuffix = projectOptionsVm.ViewModelSuffix,
                ViewLocation = projectOptionsVm.LocationDescriptorForView.GetDescriptor(),
                ViewModelLocation = projectOptionsVm.LocationDescriptorForViewModel.GetDescriptor()
            };

            return rval;
        }

        public async Task RevertSettings()
        {
            // If we haven't fully loaded and the user cancels the dialog,
            // _unmodifiedSettings will be null so abort.
            if (_unmodifiedSettings == null)
                return;

            // Reverts properties to the values saved in _unmodifiedSettings.  This
            // is used to cancel changes or to set the properties' initial values.

            // Unsubscribe from solution events on the old solution.
            var oldSolutionVm = ProjectsOptions?.FirstOrDefault();
            if (oldSolutionVm != null)
            {
                oldSolutionVm.PropertyChanging -= SolutionProjectOptionsVmOnPropertyChanging;
                oldSolutionVm.LocationDescriptorForViewModel.PropertyChanging -= LocationDescriptorOnPropertyChanging;
                oldSolutionVm.LocationDescriptorForView.PropertyChanging -= LocationDescriptorOnPropertyChanging;

                oldSolutionVm.PropertyChanged -= SolutionProjectOptionsVmOnPropertyChanged;
                oldSolutionVm.LocationDescriptorForViewModel.PropertyChanged -= LocationDescriptorForViewModelOnPropertyChanged;
                oldSolutionVm.LocationDescriptorForView.PropertyChanged -= LocationDescriptorForViewOnPropertyChanged;
            }

            SelectedGoToViewOrViewModelOption = _unmodifiedSettings.GoToViewOrViewModelOption;
            GoToViewOrViewModelSearchSolution = _unmodifiedSettings.GoToViewOrViewModelSearchSolution;
            ViewSuffixes = new ObservableCollection<StringViewModel>(_unmodifiedSettings.ViewSuffixes.Select(s => StringViewModel.CreateFromString(Container, s)));
            LocalTemplateFolder = _unmodifiedSettings.LocalTemplateFolder;

            // Add solution and other projects.
            var tmp = new List<ProjectOptionsUserControlViewModel>();
            if (_unmodifiedSettings.SolutionOptions != null)
            {
                // Create global vm defaults using the global defaults.
                var solutiondefaultProjectOptionsVm = await CreateProjectOptionsUserControlViewModel(SettingsService.SolutionDefaultProjectOptions, null, false);
                // Use the global defaults in the solution vm.
                var solutionProjectOptionsVm = await CreateProjectOptionsUserControlViewModel(_unmodifiedSettings.SolutionOptions, solutiondefaultProjectOptionsVm, false);
                tmp.Add(solutionProjectOptionsVm);

                // Subscribe to solution PropertyChanging and PropertyChanged events so we can
                // pass them down to the projects.
                solutionProjectOptionsVm.PropertyChanging += SolutionProjectOptionsVmOnPropertyChanging;
                solutionProjectOptionsVm.LocationDescriptorForViewModel.PropertyChanging += LocationDescriptorOnPropertyChanging;
                solutionProjectOptionsVm.LocationDescriptorForView.PropertyChanging += LocationDescriptorOnPropertyChanging;

                solutionProjectOptionsVm.PropertyChanged += SolutionProjectOptionsVmOnPropertyChanged;
                solutionProjectOptionsVm.LocationDescriptorForViewModel.PropertyChanged += LocationDescriptorForViewModelOnPropertyChanged;
                solutionProjectOptionsVm.LocationDescriptorForView.PropertyChanged += LocationDescriptorForViewOnPropertyChanged;

                // Use the solution vm as the inherited for all the projects.
                foreach (var po in _unmodifiedSettings.ProjectOptions)
                    tmp.Add(await CreateProjectOptionsUserControlViewModel(po, solutionProjectOptionsVm, true));
            }
            ProjectsOptions = tmp;

            ResetAllToInheritedCommand.RaiseCanExecuteChanged();
        }

        private void LocationDescriptorOnPropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            // Save inherited values so in PropertyChanged event we can copy
            // them into any property in the projects that are unchanged .
            // Note that we define inheritance as a value that remains unchanged
            // from the solution's value.

            LocationDescriptorUserControlViewModel location;

            switch (args.PropertyName)
            {
                case "ProjectIdentifier":
                    location = (LocationDescriptorUserControlViewModel)sender;
                    _oldValue = location.ProjectIdentifier;
                    break;
                case "PathOffProject":
                    location = (LocationDescriptorUserControlViewModel)sender;
                    _oldValue = location.PathOffProject;
                    break;
                case "Namespace":
                    location = (LocationDescriptorUserControlViewModel)sender;
                    _oldValue = location.Namespace;
                    break;
            }
        }

        private void SolutionProjectOptionsVmOnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            switch (propertyChangingEventArgs.PropertyName)
            {
                case "ViewModelSuffix":
                    var solution = (ProjectOptionsUserControlViewModel)sender;
                    _oldValue = solution.ViewModelSuffix;
                    break;
            }
        }

        private void LocationDescriptorForViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // This logic is moved to a method for consolidation.
            UpdateLocationDescriptor(args.PropertyName, false);
        }

        private void LocationDescriptorForViewOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // This logic is moved to a method for consolidation.
            UpdateLocationDescriptor(args.PropertyName, true);
        }

        private void UpdateLocationDescriptor(string propertyName, bool isView)
        {
            // This method is called by each of the PropertyChanged handlers, once
            // with isView==true, and once isView==false.
            // The path here is to use the _oldValue (which is the property value 
            // we saved in the PropertyChanging event handler), and if the
            // solution's original value (_oldValue) is the same as the location
            // descriptor vm (for the chainged property), we copy in (inherit)
            // the the project's location descriptor.

            // Index 0 is the solution, so we start at 1.
            for (int index = 1; index < ProjectsOptions.Count; index++)
            {
                var projectOptions = ProjectsOptions[index];

                var solution = ProjectsOptions[0];

                switch (propertyName)
                {
                    case "ProjectIdentifier":
                        if (isView)
                        {
                            if (projectOptions.LocationDescriptorForView.ProjectIdentifier == (string)_oldValue)
                                projectOptions.LocationDescriptorForView.ProjectIdentifier = solution.LocationDescriptorForView.ProjectIdentifier;
                        }
                        else
                        {
                            if (projectOptions.LocationDescriptorForViewModel.ProjectIdentifier == (string)_oldValue)
                                projectOptions.LocationDescriptorForViewModel.ProjectIdentifier = solution.LocationDescriptorForViewModel.ProjectIdentifier;
                        }
                        ResetAllToInheritedCommand.RaiseCanExecuteChanged();
                        break;
                    case "PathOffProject":
                        if (isView)
                        {
                            if (projectOptions.LocationDescriptorForView.PathOffProject == (string)_oldValue)
                                projectOptions.LocationDescriptorForView.PathOffProject = solution.LocationDescriptorForView.PathOffProject;
                        }
                        else
                        {
                            if (projectOptions.LocationDescriptorForViewModel.PathOffProject == (string)_oldValue)
                                projectOptions.LocationDescriptorForViewModel.PathOffProject = solution.LocationDescriptorForViewModel.PathOffProject;
                        }
                        ResetAllToInheritedCommand.RaiseCanExecuteChanged();
                        break;
                    case "Namespace":
                        if (isView)
                        {
                            if (projectOptions.LocationDescriptorForView.Namespace == (string)_oldValue)
                                projectOptions.LocationDescriptorForView.Namespace = solution.LocationDescriptorForView.Namespace;
                        }
                        else
                        {
                            if (projectOptions.LocationDescriptorForViewModel.Namespace == (string)_oldValue)
                                projectOptions.LocationDescriptorForViewModel.Namespace = solution.LocationDescriptorForViewModel.Namespace;
                        }
                        ResetAllToInheritedCommand.RaiseCanExecuteChanged();
                        break;
                }
            }
        }

        private void SolutionProjectOptionsVmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var solution = (ProjectOptionsUserControlViewModel)sender;

            // Index 0 is the solution, so we start at 1.
            for (int index = 1; index < ProjectsOptions.Count; index++)
            {
                var po = ProjectsOptions[index];

                switch (args.PropertyName)
                {
                    case "ViewModelSuffix":
                        // if the solution's old suffix was the same as the project's current
                        // value, update it to match the solution's new suffix.  If the old value
                        // was different from the project's suffix, then the user has changed
                        // it and the suffix is no longer considered inherited so we do nothing.
                        if (po.ViewModelSuffix == (string)_oldValue)
                            po.ViewModelSuffix = solution.ViewModelSuffix;
                        ResetAllToInheritedCommand.RaiseCanExecuteChanged();
                        break;
                }
            }
        }

        public void CheckpointSettings()
        {
            // Saves the view model properties into _unmodifiedSettings so that future calls
            // to RevertSettings() can go back to this point.  This method is typically called 
            // when the user hits Apply or OK on the settings window. 
            _unmodifiedSettings = GetCurrentSettings();
        }

        #endregion Public Methods

        #region Commands
        
        #region CreateLocalTemplateFolderCommand
        DelegateCommand _createLocalTemplateFolderCommand;
        public DelegateCommand CreateLocalTemplateFolderCommand => _createLocalTemplateFolderCommand ?? (_createLocalTemplateFolderCommand = new DelegateCommand(ExecuteCreateLocalTemplateFolderCommand, CanCreateLocalTemplateFolderCommand));
        public bool CanCreateLocalTemplateFolderCommand() => true;
        public void ExecuteCreateLocalTemplateFolderCommand()
        {
            // Create directory if it doesn't exist.
            try
            {
                if (Directory.Exists(LocalTemplateFolder))
                    DialogService.ShowMessage("Exists", "The folder already exists.");
                else
                {
                    Directory.CreateDirectory(LocalTemplateFolder);
                    DialogService.ShowMessage("Created", "Folder created successfully.");
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowMessage("Error", $"The folder \"{LocalTemplateFolder}\" couldn't be created.");
                Trace.WriteLine($"In {nameof(OptionalAttribute)}.{nameof(ExecuteCreateLocalTemplateFolderCommand)}(), couldn't create local template folder (and subfolders).  Folder: {LocalTemplateFolder}.  Error: {ex.Message}");
            }
        }
        #endregion
        
        #region OpenLocalTemplateFolderCommand
        DelegateCommand _openLocalTemplateFolderCommand;
        public DelegateCommand OpenLocalTemplateFolderCommand => _openLocalTemplateFolderCommand ?? (_openLocalTemplateFolderCommand = new DelegateCommand(ExecuteOpenLocalTemplateFolderCommand, CanOpenLocalTemplateFolderCommand));
        public bool CanOpenLocalTemplateFolderCommand() => true;
        public void ExecuteOpenLocalTemplateFolderCommand()
        {
            try
            {
                // Create directory if it doesn't exist.
                if (!Directory.Exists(LocalTemplateFolder))
                    Directory.CreateDirectory(LocalTemplateFolder);
                Process.Start(LocalTemplateFolder);
            }
            catch (Exception ex)
            {
                DialogService.ShowMessage("Error", $"The folder \"{LocalTemplateFolder}\" doesn't exist and couldn't be created.");
                Trace.WriteLine($"In {nameof(OptionsViewModel)}.{nameof(ExecuteCreateLocalTemplateFolderCommand)}(), couldn't create local template folder (and subfolders).  Folder: {LocalTemplateFolder}.  Error: {ex.Message}");
            }
        }
        #endregion
        
        #region ResetAllToInheritedCommand
        DelegateCommand _resetAllToInheritedCommand;
        public DelegateCommand ResetAllToInheritedCommand => _resetAllToInheritedCommand ?? (_resetAllToInheritedCommand = new DelegateCommand(ExecuteResetAllToInheritedCommand, CanResetAllToInheritedCommand));
        public bool CanResetAllToInheritedCommand()
        {
            if (ProjectsOptions == null)
                return false;

            // Return true is at least one project is not inherited.

            // Solution is at index 0, so we start at 1.
            for (var i = 1; i < ProjectsOptions.Count; i++)
            {
                var po = ProjectsOptions[i];

                // Reset project to inherited values.
                if (!po.IsInherited)
                    return true;
            }
            return false;
        }

        public void ExecuteResetAllToInheritedCommand()
        {
            // Solution is at index 0, so we start at 1.
            for (var i = 1; i < ProjectsOptions.Count; i++)
            {
                var po = ProjectsOptions[i];

                // Reset project to inherited values.
                po.ResetToInherited();
            }
        }
        #endregion

        #region RestoreDefaultViewSuffixesCommand
        DelegateCommand _restoreDefaultViewSuffixesCommand;
        public DelegateCommand RestoreDefaultViewSuffixesCommand => _restoreDefaultViewSuffixesCommand ?? (_restoreDefaultViewSuffixesCommand = new DelegateCommand(ExecuteRestoreDefaultViewSuffixesCommand, CanRestoreDefaultViewSuffixesCommand));
        public bool CanRestoreDefaultViewSuffixesCommand() => true;
        public void ExecuteRestoreDefaultViewSuffixesCommand()
        {
            try
            {
                ViewSuffixes = new ObservableCollection<StringViewModel>(SettingsService.DefaultViewSuffixes.Select(s => StringViewModel.CreateFromString(Container, s)));
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region DeleteViewSuffixCommand
        DelegateCommand _deleteViewSuffixCommand;
        public DelegateCommand DeleteViewSuffixCommand => _deleteViewSuffixCommand ?? (_deleteViewSuffixCommand = new DelegateCommand(ExecuteDeleteViewSuffixCommand, CanDeleteViewSuffixCommand));
        public bool CanDeleteViewSuffixCommand() => ViewSuffixesView?.CurrentItem != null;
        public async void ExecuteDeleteViewSuffixCommand()
        {
            try
            {
                var vs = (StringViewModel) ViewSuffixesView?.CurrentItem;
                if ((await DialogService.Ask("Delete View Suffix?", $"Delete view suffix \"{vs?.Value}?\"", AskButton.OKCancel)) == AskResult.OK)
                    ViewSuffixesView?.Remove(ViewSuffixesView.CurrentItem);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        #endregion
        
        #region AddViewSuffixCommand
        DelegateCommand _addViewSuffixCommand;
        public DelegateCommand AddViewSuffixCommand => _addViewSuffixCommand ?? (_addViewSuffixCommand = new DelegateCommand(ExecuteAddViewSuffixCommand, CanAddViewSuffixCommand));
        public bool CanAddViewSuffixCommand() => true;
        public void ExecuteAddViewSuffixCommand()
        {
            try
            {
                var vm = Container.Resolve<StringDialogViewModel>();
                vm.Add(true, "Add View Suffix", "View Suffix:", ViewSuffixes?.Select(s => s.Value),
                    SuffixRegex, SuffixRegexErrorMessage);

                if (DialogService.ShowDialog(vm))
                {
                    var newItem = StringViewModel.CreateFromString(Container, vm.Value);
                    ViewSuffixesView.AddNewItem(newItem);
                    // ReSharper disable once PossibleNullReferenceException
                    ViewSuffixesView.MoveCurrentTo(newItem);
                }
            }
            catch
            {
                // ignored
            }
        }
        #endregion

        #region EditViewSuffixCommand
        DelegateCommand _editViewSuffixCommand;
        public DelegateCommand EditViewSuffixCommand => _editViewSuffixCommand ?? (_editViewSuffixCommand = new DelegateCommand(ExecuteEditViewSuffixCommand, CanEditViewSuffixCommand));
        public bool CanEditViewSuffixCommand() => ViewSuffixesView?.CurrentItem != null;
        public void ExecuteEditViewSuffixCommand()
        {
            try
            {
                var cur = ViewSuffixesView.CurrentItem as StringViewModel;
                Debug.Assert(cur != null);

                var vm = Container.Resolve<StringDialogViewModel>();
                vm.Edit(true, "Edit View Suffix", "View Suffix:", cur.Value, ViewSuffixes.Select(s => s.Value),
                    SuffixRegex, SuffixRegexErrorMessage);
                if (DialogService.ShowDialog(vm))
                    cur.Value = vm.Value;
            }
            catch
            {
                // ignored
            }
        }
        #endregion

        #endregion Commands

        #region Private Methods

        private void ViewSuffixesViewOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            DeleteViewSuffixCommand.RaiseCanExecuteChanged();
            EditViewSuffixCommand.RaiseCanExecuteChanged();
        }

        // Passed in inherited view model could be from SettingsService (for the solution),
        // or from the solution (for the projects) (mutable). 
        private async Task<ProjectOptionsUserControlViewModel> CreateProjectOptionsUserControlViewModel(ProjectOptions projectOptions, ProjectOptionsUserControlViewModel inherited,
            bool isProject)
        {
            var rval = Container.Resolve<ProjectOptionsUserControlViewModel>();

            await rval.Initialize(
                projectOptions,
                isProject,
                inherited,
                projectOptions.ViewModelSuffix,
                Container.Resolve<LocationDescriptorUserControlViewModel>(),
                Container.Resolve<LocationDescriptorUserControlViewModel>());

            return rval;
        }
        
        #endregion Private Methods
    }
}