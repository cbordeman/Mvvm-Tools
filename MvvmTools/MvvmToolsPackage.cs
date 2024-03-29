﻿using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using MvvmTools.Commands;
using MvvmTools.Options;
using MvvmTools.Services;
using MvvmTools.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Unity;
using Unity.Lifetime;
using Task = System.Threading.Tasks.Task;

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
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(OptionsPageGeneral), "MVVM Tools", "General", 101, 107, true)]
    [ProvideOptionPage(typeof(OptionsPageSolutionAndProjects), "MVVM Tools", "Solution and Projects", 101, 113, true)]
    // This is the magic attribute required so VS can find 3rd party dlls.
    [ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Constants.GuidPackage)]
    ////[ProvideAutoLoad(UIContextGuids.SolutionExists)]
    //[InstalledProductRegistration("MVVM Tools", "Provides access to your corresponding View/ViewModel via Ctrl+E,Q.", "0.5.0.0")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class MvvmToolsPackage : AsyncPackage, IMvvmToolsPackage
        //, IAsyncLoadablePackageInitialize
    {
        public static readonly IUnityContainer Container;

        #region Fields

        private IVsSolution vsSolution;
        private uint solutionEventsCookie;

        #endregion Fields
        
        #region Properties

        /// <summary>
        /// An internal collection of the commands registered by this package.
        /// </summary>
        private readonly ICollection<BaseCommand> commands = new List<BaseCommand>();

        private DTE2 ide;
        public DTE2 Ide => ide ?? (ide = (DTE2)GetService(typeof(DTE)));

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

        private double ideVersion;

        public double IdeVersion => ideVersion != 0 ? ideVersion : (ideVersion = Convert.ToDouble(Ide.Version, CultureInfo.InvariantCulture));

        #endregion Properties

        #region Ctor and Init

        static MvvmToolsPackage()
        {
            Container = new UnityContainer();
        }

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

        #endregion Ctor and Init

        //protected override object GetAutomationObject(string name)
        //{
        //    try
        //    {
        //        Trace.WriteLine($"Getting Automation object: {name}");
        //        return base.GetAutomationObject(name);
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.WriteLine(e);
        //        throw;
        //    }
        //}

        //protected override object GetService(Type serviceType)
        //{
        //    try
        //    {
        //        Trace.WriteLine($"Getting service type: {serviceType.Name}");
        //        return base.GetService(serviceType);
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.WriteLine(e);
        //        throw;
        //    }
            
        //}

        #region Private Helpers

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

        #endregion Private Helpers

        ///// <summary>
        ///// MvvmTools2Package GUID string.
        ///// </summary>
        //public const string PackageGuidString = "6e8a3b1d-0f93-4749-b3e8-39e83c15c82e";

        #region Package Members
        
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            try
            {
                // Add package and package specific services.
                Container.RegisterInstance<IMvvmToolsPackage>(this, new ContainerControlledLifetimeManager());
                Container.RegisterInstance(GetGlobalService(typeof(SComponentModel)) as IComponentModel, new ContainerControlledLifetimeManager());
                Container.RegisterInstance(await GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(false) as IMenuCommandService, new ContainerControlledLifetimeManager());

                // Templating services.
                var tt = await GetServiceAsync(typeof(STextTemplating)).ConfigureAwait(false) as STextTemplating;
                Container.RegisterInstance((ITextTemplating)tt, new ContainerControlledLifetimeManager());
                Container.RegisterInstance((ITextTemplatingEngineHost)tt, new ContainerControlledLifetimeManager());
                Container.RegisterInstance((ITextTemplatingSessionHost)tt, new ContainerControlledLifetimeManager());
                //Kernel.Bind<ITextTemplatingEngine>().ToConstant((ITextTemplatingEngine) tt);

                // Our own singleton services.
                Container.RegisterType<ISettingsService, SettingsService>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IViewFactory, ViewFactory>(new ContainerControlledLifetimeManager());
                Container.RegisterType<IDialogService,DialogService>(new ContainerControlledLifetimeManager());

                // Option view model, a shared singleton because it's easier.
                Container.RegisterType<OptionsViewModel>(new ContainerControlledLifetimeManager());

                Container.RegisterType<ITemplateService, TemplateService>(new ContainerControlledLifetimeManager());

                await JoinableTaskFactory.SwitchToMainThreadAsync();

                // Register commands.
                Container.RegisterType<GoToViewOrViewModelCommand>(new ContainerControlledLifetimeManager());

                // Add solution services.
                Container.RegisterInstance(Ide, new ContainerControlledLifetimeManager());
                Container.RegisterType<ISolutionService, SolutionService>(new ContainerControlledLifetimeManager());
                vsSolution = await GetServiceAsync(typeof(SVsSolution)).ConfigureAwait(false) as IVsSolution;
                Assumes.Present(vsSolution);
                Container.RegisterInstance(vsSolution, new ContainerControlledLifetimeManager());

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var ss = Container.Resolve<ISolutionService>();
                ss.Init();
                
                int? result = vsSolution?.AdviseSolutionEvents(ss, out solutionEventsCookie);

                RegisterCommands();

                Trace.WriteLine($"Solution loaded.  Result: {(result.HasValue ? result.Value.ToString() : "null")}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"MVVM Tools service startup failed: {ex}.");
            }
        }
        
        /// <summary>
        /// Register the package commands (which must exist in the .vsct file).
        /// </summary>
        private void RegisterCommands()
        {
            var menuCommandService = Container.Resolve<IMenuCommandService>();
            if (menuCommandService != null)
            {
                // Create the individual commands, which internally register for command events.
                var gc = Container.Resolve<GoToViewOrViewModelCommand>();
                commands.Add(gc);
                //_commands.Add(Container.Resolve<ScaffoldViewAndViewModelCommand>());
               // _commands.Add(Container.Resolve<ExtractViewModelFromViewCommand>());

                // Add all commands to the menu command service.
                foreach (var command in commands)
                    menuCommandService.AddCommand(command);
                
                //menuCommandService.FindCommand(_commands)
            }
        }

        protected override async void Dispose(bool disposing)
        {
            try
            {
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                if (solutionEventsCookie != 0)
                {
                    vsSolution.UnadviseSolutionEvents(solutionEventsCookie);
                    solutionEventsCookie = 0;
                }

                base.Dispose(disposing);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
            
        }


        #endregion
    }
}
