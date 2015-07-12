using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using Ninject;
using Ninject.Parameters;

namespace MvvmTools.Core.ViewModels
{
    public class OptionsUserControlViewModel : BaseViewModel
    {
        #region Data
        private MvvmToolsSettings _unmodifiedSettings;
        private readonly Regex _suffixRegex = new Regex(@"^[_a-zA-Z0-9]*$");
        private const string SuffixRegexErrorMessage = "Not a valid suffix.";
        #endregion Data

        #region Ctor and Init

        public OptionsUserControlViewModel(MvvmToolsSettings unmodifiedSettings, IKernel kernel)
        {
            GoToViewOrViewModelOptions = new List<ValueDescriptor<GoToViewOrViewModelOption>>
            {
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ShowUi, "Ask"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseXaml, "If view, open the XAML"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseCodeBehind, "If view, open the code behind"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseFirst, "Always open the first item found")
            };

            // Save the original, unmodified settings.
            _unmodifiedSettings = unmodifiedSettings;

            // Initialize properties.  Note the ctor param 'title' must be unique because
            // it is used as both the Title and the radio button group.

            ProjectItemDescriptorForViewModel = kernel.Get<ProjectItemDescriptorDialogViewModel>(
                new ConstructorArgument("title", "View Models Location"));
            ProjectItemDescriptorForViewModel.PathOffProject =
                unmodifiedSettings.ScaffoldingOptions.ViewModelLocation.PathOffProject;
            ProjectItemDescriptorForViewModel.Namespace =
                unmodifiedSettings.ScaffoldingOptions.ViewModelLocation.Namespace;

            ProjectItemDescriptorForView = kernel.Get<ProjectItemDescriptorDialogViewModel>(
                new ConstructorArgument("title", "Views Location"));
            ProjectItemDescriptorForView.PathOffProject =
                unmodifiedSettings.ScaffoldingOptions.ViewLocation.PathOffProject;
            ProjectItemDescriptorForView.Namespace =
                unmodifiedSettings.ScaffoldingOptions.ViewLocation.Namespace;

            // This actually applies the _unmodifiedSettings to the properties.
            RevertSettings();
        }

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public IDialogService DialogService { get; set; }

        #region GoToViewOrViewModelOptions
        private List<ValueDescriptor<GoToViewOrViewModelOption>> _goToViewOrViewModelOptions;
        public List<ValueDescriptor<GoToViewOrViewModelOption>> GoToViewOrViewModelOptions
        {
            get { return _goToViewOrViewModelOptions; }
            set { SetProperty(ref _goToViewOrViewModelOptions, value); }
        }
        #endregion GoToViewOrViewModelOptions

        #region SelectedGoToViewOrViewModelOption
        private GoToViewOrViewModelOption _selectedGoToViewOrViewModelOption;
        public GoToViewOrViewModelOption SelectedGoToViewOrViewModelOption
        {
            get { return _selectedGoToViewOrViewModelOption; }
            set { SetProperty(ref _selectedGoToViewOrViewModelOption, value); }
        }
        #endregion SelectedGoToViewOrViewModelOption

        #region ProjectItemDescriptorForViewModel
        private ProjectItemDescriptorDialogViewModel _projectItemDescriptorForViewModel;
        public ProjectItemDescriptorDialogViewModel ProjectItemDescriptorForViewModel
        {
            get { return _projectItemDescriptorForViewModel; }
            set { SetProperty(ref _projectItemDescriptorForViewModel, value); }
        }
        #endregion ProjectItemDescriptorForViewModel

        #region ProjectItemDescriptorForView
        private ProjectItemDescriptorDialogViewModel _projectItemDescriptorForView;
        public ProjectItemDescriptorDialogViewModel ProjectItemDescriptorForView
        {
            get { return _projectItemDescriptorForView; }
            set { SetProperty(ref _projectItemDescriptorForView, value); }
        }
        #endregion ProjectItemDescriptorForView

        #region ViewModelSuffix
        private string _viewModelSuffix;
        public string ViewModelSuffix
        {
            get { return _viewModelSuffix; }
            set { SetProperty(ref _viewModelSuffix, value); }
        }
        #endregion ViewModelSuffix

        #region ViewSuffixes
        private ObservableCollection<StringViewModel> _viewSuffixes;
        public ObservableCollection<StringViewModel> ViewSuffixes
        {
            get { return _viewSuffixes; }
            set
            {
                if (SetProperty(ref _viewSuffixes, value))
                {
                    if (this.ViewSuffixesView != null)
                        ViewSuffixesView.CurrentChanged -= ViewSuffixesViewOnCurrentChanged;
                    ViewSuffixesView = new ListCollectionView(value);
                    ViewSuffixesView.CurrentChanged += ViewSuffixesViewOnCurrentChanged;
                }
            }
        }
        #endregion ViewSuffixes

        #region ViewSuffixesView
        private ListCollectionView _viewSuffixesView;
        public ListCollectionView ViewSuffixesView
        {
            get { return _viewSuffixesView; }
            set { SetProperty(ref _viewSuffixesView, value); }
        }
        #endregion ViewSuffixesView

        #endregion Properties

        #region Commands

        #region OpenScaffoldingDialogCommand
        DelegateCommand<ProjectItemDescriptorDialogViewModel> _openScaffoldingDialogCommand;
        public DelegateCommand<ProjectItemDescriptorDialogViewModel> OpenScaffoldingDialogCommand => _openScaffoldingDialogCommand ?? (_openScaffoldingDialogCommand = new DelegateCommand<ProjectItemDescriptorDialogViewModel>(ExecuteOpenScaffoldingDialogCommand, CanOpenScaffoldingDialogCommand));
        public bool CanOpenScaffoldingDialogCommand(ProjectItemDescriptorDialogViewModel dlgVm) => true;
        public void ExecuteOpenScaffoldingDialogCommand(ProjectItemDescriptorDialogViewModel dlgVm)
        {
            dlgVm.Auto = false;
            dlgVm.InitializeFromSolution();
            bool result = DialogService.ShowDialog(dlgVm);
        }
        #endregion
        
        #region ResetViewModelSuffixCommand
        DelegateCommand _resetViewModelSuffixCommand;
        public DelegateCommand ResetViewModelSuffixCommand => _resetViewModelSuffixCommand ?? (_resetViewModelSuffixCommand = new DelegateCommand(ExecuteResetViewModelSuffixCommand, CanResetViewModelSuffixCommand));
        public bool CanResetViewModelSuffixCommand() => true;
        public void ExecuteResetViewModelSuffixCommand()
        {
            this.ViewModelSuffix = SettingsService.DefaultViewModelSuffix;
        }
        #endregion

        #region RestoreDefaultViewSuffixesCommand
        DelegateCommand _restoreDefaultViewSuffixesCommand;
        public DelegateCommand RestoreDefaultViewSuffixesCommand => _restoreDefaultViewSuffixesCommand ?? (_restoreDefaultViewSuffixesCommand = new DelegateCommand(ExecuteRestoreDefaultViewSuffixesCommand, CanRestoreDefaultViewSuffixesCommand));
        public bool CanRestoreDefaultViewSuffixesCommand() => true;
        public void ExecuteRestoreDefaultViewSuffixesCommand()
        {
            this.ViewSuffixes = new ObservableCollection<StringViewModel>(SettingsService.DefaultViewSuffixes.Select(s => new StringViewModel(s)));
        }

        #endregion
        
        #region DeleteViewSuffixCommand
        DelegateCommand _deleteViewSuffixCommand;
        public DelegateCommand DeleteViewSuffixCommand => _deleteViewSuffixCommand ?? (_deleteViewSuffixCommand = new DelegateCommand(ExecuteDeleteViewSuffixCommand, CanDeleteViewSuffixCommand));
        public bool CanDeleteViewSuffixCommand() => this.ViewSuffixesView.CurrentItem != null;
        public void ExecuteDeleteViewSuffixCommand()
        {
            this.ViewSuffixesView.Remove(ViewSuffixesView.CurrentItem);
        }
        #endregion
        
        #region AddViewSuffixCommand
        DelegateCommand _addViewSuffixCommand;
        public DelegateCommand AddViewSuffixCommand => _addViewSuffixCommand ?? (_addViewSuffixCommand = new DelegateCommand(ExecuteAddViewSuffixCommand, CanAddViewSuffixCommand));
        public bool CanAddViewSuffixCommand() => true;
        public void ExecuteAddViewSuffixCommand()
        {
            var vm = Kernel.Get<StringDialogViewModel>();
            vm.Add("Add View Suffix", "View Suffix:", this.ViewSuffixes.Select(s => s.Value),
                _suffixRegex, SuffixRegexErrorMessage);
            if (this.DialogService.ShowDialog(vm))
            {
                var newItem = new StringViewModel(vm.Value);
                this.ViewSuffixesView.AddNewItem(newItem);
                this.ViewSuffixesView.MoveCurrentToPosition(this.ViewSuffixes.IndexOf(newItem));
            }
        }
        #endregion

        #region EditViewSuffixCommand
        DelegateCommand _editViewSuffixCommand;
        public DelegateCommand EditViewSuffixCommand => _editViewSuffixCommand ?? (_editViewSuffixCommand = new DelegateCommand(ExecuteEditViewSuffixCommand, CanEditViewSuffixCommand));
        public bool CanEditViewSuffixCommand() => this.ViewSuffixesView.CurrentItem != null;
        public void ExecuteEditViewSuffixCommand()
        {
            var cur = this.ViewSuffixesView.CurrentItem as StringViewModel;

            var vm = Kernel.Get<StringDialogViewModel>();
            vm.Edit("Edit View Suffix", "View Suffix:", cur.Value, this.ViewSuffixes.Select(s => s.Value),
                _suffixRegex, SuffixRegexErrorMessage);
            if (this.DialogService.ShowDialog(vm))
                cur.Value = vm.Value;
        }
        #endregion

        #endregion Commands

        #region Public Methods

        public MvvmToolsSettings GetCurrentSettings()
        {
            // Extracts settings from view model properties.  These are the 
            // 'current' settings, while the unmodified settings values are store in
            // _unmodifiedSettings.
            var settings = new MvvmToolsSettings
            {
                GoToViewOrViewModelOption = SelectedGoToViewOrViewModelOption,
                ScaffoldingOptions =
                {
                    ViewModelLocation = ProjectItemDescriptorForViewModel.GetDescriptor(),
                    ViewLocation = ProjectItemDescriptorForView.GetDescriptor()
                },
                ViewModelSuffix = ViewModelSuffix,
                ViewSuffixes = ViewSuffixes.Select(s => s.Value).ToArray()
            };
            
            return settings;
        }

        public void RevertSettings()
        {
            // Reverts properties to the values saved in _unmodifiedSettings.  This
            // is used to cancel changes.
            SelectedGoToViewOrViewModelOption = _unmodifiedSettings.GoToViewOrViewModelOption;
            ProjectItemDescriptorForViewModel.SetFromDescriptor(_unmodifiedSettings.ScaffoldingOptions.ViewModelLocation);
            ProjectItemDescriptorForView.SetFromDescriptor(_unmodifiedSettings.ScaffoldingOptions.ViewLocation);
            ViewModelSuffix = _unmodifiedSettings.ViewModelSuffix;
            ViewSuffixes = new ObservableCollection<StringViewModel>(_unmodifiedSettings.ViewSuffixes.Select(s => new StringViewModel(s)));
        }

        public void CheckpointSettings()
        {
            // Saves the view model properties into _unmodifiedSettings so that future calls
            // to RevertSettings() can go back to this point.  This method is typically called 
            // when the user hits Apply or OK on the settings window. 
            _unmodifiedSettings = GetCurrentSettings();
        }

        #endregion Public Methods

        #region Private Methods

        private void ViewSuffixesViewOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            DeleteViewSuffixCommand.RaiseCanExecuteChanged();
            EditViewSuffixCommand.RaiseCanExecuteChanged();
        }

        #endregion Private Methods
    }
}
