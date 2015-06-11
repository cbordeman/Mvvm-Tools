using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Services;
using MvvmTools.Utilities;
using MvvmTools.ViewModels;
using MvvmTools.Views;
using Ninject;

namespace MvvmTools.Options
{
    /// <summary>
    // Extends a standard dialog functionality for implementing ToolsOptions pages, 
    // with support for the Visual Studio automation model, Windows Forms, and state 
    // persistence through the Visual Studio settings mechanism.
    /// </summary>
	[Guid(Constants.GuidOptionsPage)]
    [ComVisible(true)]
    internal class OptionsPageGeneral : DialogPage
    {
        #region Fields

        private OptionsViewModel _viewModel;

        #endregion Fields

        #region Ctor and Init

        public OptionsPageGeneral()
        {
            _settingsService = MvvmToolsPackage.Kernel.Get<ISettingsService>();
        }

        #endregion Ctor and Init

        #region Properties
        [Category("General")]
        [Description("If multiple classes are found, show the Select File dialog.  Otherwise just selects the first file.")]
        public bool ShowSelectFileDialog { get; set; } = false;

        private readonly ISettingsService _settingsService;

        /// <summary>
        /// Gets the window an instance of DialogPage that it uses as its user interface.
        /// </summary>
		/// <devdoc>
		/// The window this dialog page will use for its UI.
		/// This window handle must be constant, so if you are
		/// returning a Windows Forms control you must make sure
		/// it does not recreate its handle.  If the window object
		/// implements IComponent it will be sited by the 
		/// dialog page so it can get access to global services.
		/// </devdoc>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override IWin32Window Window
        {
            get
            {
                // Note this code is only executed once by Visual Studio,
                // so we aren't creating and recreating the options repeatedly.

                // Create a WinForms container for our WPF General Options page.
                var elementHost = new ElementHost();
                var optionsControl = new OptionsUserControl();

                // Load settings (from defaults if necessary).
                var settings = _settingsService.LoadSettings();

                // Create, initialize, and bind a view model to our user control.
                _viewModel = new OptionsViewModel(settings);
                optionsControl.DataContext = _viewModel;

                // Create a link from the user control back to this to use as necessary.
                optionsControl.OptionsPage = this;

                // Put user control inside the element host and we're done.
                elementHost.Child = optionsControl;

                return elementHost;
            }
        }

        #endregion Properties

        #region Event Handlers
        /// <summary>
        /// Handles "Activate" messages from the Visual Studio environment.
        /// </summary>
        /// <devdoc>
        /// This method is called when Visual Studio wants to activate this page.  
        /// </devdoc>
        /// <remarks>If the Cancel property of the event is set to true, the page is not activated.</remarks>
        protected override void OnActivate(CancelEventArgs e)
        {
            //DialogResult result = WinFormsHelper.ShowMessageBox(Resources.MessageOnActivateEntered, Resources.MessageOnActivateEntered, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            //if (result == DialogResult.Cancel)
            //{
            //    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Cancelled the OnActivate event"));
            //    e.Cancel = true;
            //}

            base.OnActivate(e);
        }

        /// <summary>
        /// Handles "Close" messages from the Visual Studio environment.
        /// </summary>
		/// <devdoc>
		/// This event is raised when the page is closed.
		/// </devdoc>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _viewModel.RevertSettings();

            //WinFormsHelper.ShowMessageBox(Resources.MessageOnClosed);
        }

        /// <summary>
        /// Handles "Deactive" messages from the Visual Studio environment.
        /// </summary>
		/// <devdoc>
		/// This method is called when VS wants to deactivate this
		/// page.  If true is set for the Cancel property of the event, 
		/// the page is not deactivated.
		/// </devdoc>
        /// <remarks>
        /// A "Deactive" message is sent when a dialog page's user interface 
        /// window loses focus or is minimized but is not closed.
        /// </remarks>
		protected override void OnDeactivate(CancelEventArgs e)
        {
            base.OnDeactivate(e);
            //var result = WinFormsHelper.ShowMessageBox(Resources.MessageOnDeactivateEntered, Resources.MessageOnDeactivateEntered, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            //if (result == DialogResult.Cancel)
            //{
            //    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Cancelled the OnDeactivate event"));
            //    e.Cancel = true;
            //}
        }

        public override void ResetSettings()
        {
            base.ResetSettings();

            _viewModel.RevertSettings();
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
            _settingsService.SaveSettings(settings);
            
            //var result = MessageBox.Show(Resources.MessageOnApplyEntered);

            //if (result == DialogResult.Cancel)
            //{
            //    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Cancelled the OnApply event"));
            //    e.ApplyBehavior = DialogPage.ApplyKind.Cancel;
            //}
            //else
            //{
            //    base.OnApply(e);
            //}

            //WinFormsHelper.ShowMessageBox(Resources.MessageOnApply);
        }

        #endregion Event Handlers
    }
}
