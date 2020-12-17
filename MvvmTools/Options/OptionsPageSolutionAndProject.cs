using System;
using System.Runtime.InteropServices;
using System.Windows;
using MvvmTools.Controls;
using MvvmTools.Services;
using MvvmTools.ViewModels;
using MvvmTools.Views;
using Unity;

namespace MvvmTools.Options
{
    // Note: The Visual Studio designer for this file (WinForms) won't work.

    /// <summary>
    // Extends a standard dialog functionality for implementing ToolsOptions pages, 
    // with support for the Visual Studio automation model, Windows Forms, and state 
    // persistence through the Visual Studio settings mechanism.
    /// </summary>
    [Guid(Constants.GuidOptionsPageSolutionAndProjects)]
    internal class OptionsPageSolutionAndProjects : UIElementDialogPage
    {
        #region Fields

        private readonly ISettingsService _settingsService;
        private readonly OptionsSolutionAndProjectsUserControl _dialog;
        private readonly OptionsViewModel _viewModel;

        #endregion Fields

        #region Ctor and Init

        public OptionsPageSolutionAndProjects()
        {
            _settingsService = MvvmToolsPackage.Container.Resolve<ISettingsService>();

            // Create a WinForms container for our WPF General Options page.
            _dialog = MvvmToolsPackage.Container.Resolve<OptionsSolutionAndProjectsUserControl>();

            // Create, initialize, and bind a view model to our user control.
            // This is a singleton.
            _viewModel = MvvmToolsPackage.Container.Resolve<OptionsViewModel>();
            _dialog.DataContext = _viewModel;
            _viewModel.Init();
        }

        #endregion Ctor and Init

        #region Properties

        protected override UIElement Child => _dialog;

        #endregion Properties

        /// <summary>
        /// Handles "Close" messages from the Visual Studio environment.
        /// </summary>
        /// <devdoc>
        /// This event is raised when the page is closed.
        /// </devdoc>
        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            await _viewModel.RevertSettings();
        }

        public override async void ResetSettings()
        {
            base.ResetSettings();

            await _viewModel.RevertSettings();
        }

        /// <summary>
        /// Handles Apply messages from the Visual Studio environment.
        /// </summary>
        /// <devdoc>
        /// This method is called when VS wants to save the user's 
        /// changes then the dialog is dismissed.
        /// </devdoc>
        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            _viewModel.CheckpointSettings();
            var settings = _viewModel.GetCurrentSettings();
            if (settings != null)
                _settingsService.SaveSettings(settings);
            else
                e.ApplyBehavior = ApplyKind.Cancel;
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            // This call is necessary so that if the grid has the focus
            // it loses it so that changes to the data context are
            // propagated properly!
            MoveFocusToNext();
        }
    }
}
