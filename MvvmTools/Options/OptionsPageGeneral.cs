using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Core.Services;
using MvvmTools.Core.ViewModels;
using MvvmTools.Core.Views;
using Ninject;

namespace MvvmTools.Options
{
    // Note: The Visual Studio designer for this file (WinForms) won't work.

    /// <summary>
    // Extends a standard dialog functionality for implementing ToolsOptions pages, 
    // with support for the Visual Studio automation model, Windows Forms, and state 
    // persistence through the Visual Studio settings mechanism.
    /// </summary>
    [Guid(Constants.GuidOptionsPageGeneral)]
    internal class OptionsPageGeneral : Core.Controls.UIElementDialogPage
    {
        #region Fields

        private readonly OptionsGeneralUserControl _child;
        private readonly OptionsViewModel _viewModel;

        #endregion Fields

        #region Ctor and Init

        public OptionsPageGeneral()
        {
            _settingsService = MvvmToolsPackage.Kernel.Get<ISettingsService>();

            _child = MvvmToolsPackage.Kernel.Get<OptionsGeneralUserControl>();

            // Create, initialize, and bind a view model to our user control.
            // This is a singleton.
            _viewModel = MvvmToolsPackage.Kernel.Get<OptionsViewModel>();
            _child.DataContext = _viewModel;

            _viewModel.Init();
            
        }

        #endregion Ctor and Init

        #region Properties

        private readonly ISettingsService _settingsService;

        protected override UIElement Child => _child;

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
