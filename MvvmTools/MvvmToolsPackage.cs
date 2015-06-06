//------------------------------------------------------------------------------
// <copyright file="GoToViewOrViewModelCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Commands;
using MvvmTools.Options;

namespace MvvmTools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideOptionPage(typeof(OptionsPageGeneral), "MVVM Tools", "General", 101, 107, true)]
    //[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Constants.GuidPackage)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class MvvmToolsPackage : Package
    {
        public MvvmToolsPackage()
        {
            // Inside this method you can place any initialization code that does not require 
            // any Visual Studio service because at this point the package object is created but 
            // not sited yet inside Visual Studio environment. The place to do all the other 
            // initialization is the Initialize method.

            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

            if (Application.Current != null)
                Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        /// <summary>
        /// Called when a DispatcherUnhandledException is raised by Visual Studio.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        /// The <see cref="DispatcherUnhandledExceptionEventArgs" /> instance containing the event data.
        /// </param>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Caught and marked as handled the following DispatcherUnhandledException raised in Visual Studio:\n{0}", e.Exception));
            e.Handled = true;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            GoToViewOrViewModelCommand.Initialize(this);
            ScaffoldViewAndViewModelCommand.Initialize(this);
            ExtractViewModelFromViewCommand.Initialize(this);

            base.Initialize();

            RegisterCommands();
        }

        /// <summary>
        /// Register the package commands (which must exist in the .vsct file).
        /// </summary>
        private void RegisterCommands()
        {
            var menuCommandService = MenuCommandService;
            if (menuCommandService != null)
            {
                // Create the individual commands, which internally register for command events.
                _commands.Add(GoToViewOrViewModelCommand.Instance);
                _commands.Add(ScaffoldViewAndViewModelCommand.Instance);

                // Add all commands to the menu command service.
                foreach (var command in _commands)
                {
                    menuCommandService.AddCommand(command);
                }
            }
        }

        #endregion

        #region Data

        /// <summary>
        /// An internal collection of the commands registered by this package.
        /// </summary>
        private readonly ICollection<BaseCommand> _commands = new List<BaseCommand>();

        /// <summary>
        /// The IComponentModel service.
        /// </summary>
        private IComponentModel _componentModel;

        /// <summary>
        /// Gets the IComponentModel service.
        /// </summary>
        public IComponentModel ComponentModel
        {
            get { return _componentModel ?? (_componentModel = GetGlobalService(typeof(SComponentModel)) as IComponentModel); }
        }

        /// <summary>
        /// Gets the currently active document, otherwise null.
        /// </summary>
        public Document ActiveDocument
        {
            get
            {
                try
                {
                    return Ide.ActiveDocument;
                }
                catch (Exception)
                {
                    // If a project property page is active, accessing the ActiveDocument causes an exception.
                    return null;
                }
            }
        }

        private DTE2 _ide;
        /// <summary>
        /// Gets the top level application instance of the VS IDE that is executing this package.
        /// </summary>
        public DTE2 Ide
        {
            get { return _ide ?? (_ide = (DTE2)GetService(typeof(DTE))); }
        }

        /// <summary>
        /// Gets the version of the running IDE instance.
        /// </summary>
        public double IdeVersion => Convert.ToDouble(Ide.Version, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the menu command service.
        /// </summary>
        public OleMenuCommandService MenuCommandService
        {
            get { return GetService(typeof(IMenuCommandService)) as OleMenuCommandService; }
        }

        public new object GetGlobalService(Type t)
        {
            return Package.GetGlobalService(t);
        }

        #endregion Data
    }
}
