using System.Collections.Generic;
using Microsoft.Practices.Prism.Commands;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using Ninject;
using Ninject.Parameters;

namespace MvvmTools.Core.ViewModels
{
    public class OptionsUserControlViewModel : BaseViewModel
    {
        private MvvmToolsSettings _unmodifiedSettings;

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
            ProjectItemDescriptorForViewModel = kernel.Get<ProjectItemDescriptorDialogViewModel>(new ConstructorArgument("title", "View Model Scaffolding Options"));
            ProjectItemDescriptorForView = kernel.Get<ProjectItemDescriptorDialogViewModel>(new ConstructorArgument("title", "View Scaffolding Options"));

            // This actually applies the _unmodifiedSettings to the properties.
            RevertSettings();
        }

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

        #endregion Properties

        #region Commands

        #region OpenScaffoldingDialogCommand
        DelegateCommand<ProjectItemDescriptorDialogViewModel> _openScaffoldingDialogCommand;
        public DelegateCommand<ProjectItemDescriptorDialogViewModel> OpenScaffoldingDialogCommand => _openScaffoldingDialogCommand ?? (_openScaffoldingDialogCommand = new DelegateCommand<ProjectItemDescriptorDialogViewModel>(ExecuteOpenScaffoldingDialogCommand, CanOpenScaffoldingDialogCommand));
        public bool CanOpenScaffoldingDialogCommand(ProjectItemDescriptorDialogViewModel dlgVm) => true;
        public void ExecuteOpenScaffoldingDialogCommand(ProjectItemDescriptorDialogViewModel dlgVm)
        {
            dlgVm.Auto = false;
            bool result = DialogService.ShowDialog(dlgVm);
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
                    ViewModelDescriptor = ProjectItemDescriptorForViewModel.GetDescriptor(),
                    ViewDescriptor = ProjectItemDescriptorForView.GetDescriptor()
                }
            };
            
            return settings;
        }

        public void RevertSettings()
        {
            // Reverts properties to the values saved in _unmodifiedSettings.  This
            // is used to cancel changes.
            SelectedGoToViewOrViewModelOption = _unmodifiedSettings.GoToViewOrViewModelOption;
            ProjectItemDescriptorForViewModel.SetFromDescriptor(_unmodifiedSettings.ScaffoldingOptions.ViewModelDescriptor);
            ProjectItemDescriptorForView.SetFromDescriptor(_unmodifiedSettings.ScaffoldingOptions.ViewDescriptor);
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
        
        #endregion Private Methods
    }
}
