using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MvvmTools.Commands;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using MvvmTools.Options;
using Ninject;

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
    [ProvideBindingPath]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Constants.GuidPackage)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [Export(typeof(IMvvmToolsPackage))]
    public sealed class MvvmToolsPackage : Package, IMvvmToolsPackage
    {
        #region Fields

        private IVsSolution _solution;
        private uint _solutionEventsCookie = 0;
        
        #endregion Fields

        #region Ctor and Init

        static MvvmToolsPackage()
        {
            Kernel = new StandardKernel();
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

        #region Properties

        /// <summary>
        /// An internal collection of the commands registered by this package.
        /// </summary>
        private ICollection<BaseCommand> _commands = new List<BaseCommand>();

        private DTE2 _ide;
        public DTE2 Ide => _ide ?? (_ide = (DTE2)GetService(typeof(DTE)));

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

        private double _ideVersion;
        
        public double IdeVersion => _ideVersion != 0 ? _ideVersion : (_ideVersion = Convert.ToDouble(Ide.Version, CultureInfo.InvariantCulture));

        #region Kernel

        internal static readonly IKernel Kernel;

        #endregion Kernel

        #endregion Properties

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

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // Set up Ninject container

            //Kernel.Bind<IOutputService>().To<OutputService>().InSingletonScope();
            //var os = Kernel.Get<IOutputService>();
            
            try
            {
                // Add package and package specific services.
                Kernel.Bind<IMvvmToolsPackage>().ToConstant(this);
                Kernel.Bind<IComponentModel>().ToConstant(GetGlobalService(typeof(SComponentModel)) as IComponentModel);
                Kernel.Bind<IMenuCommandService>().ToConstant(GetService(typeof(IMenuCommandService)) as OleMenuCommandService);

                // Our own singleton services.
                Kernel.Bind<ISettingsService>().To<SettingsService>().InSingletonScope();
                Kernel.Bind<IViewFactory>().To<ViewFactory>().InSingletonScope();
                Kernel.Bind<IDialogService>().To<DialogService>().InSingletonScope();

                // Commands, which are singletons.
                Kernel.Bind<GoToViewOrViewModelCommand>().ToSelf().InSingletonScope();
                Kernel.Bind<ScaffoldViewAndViewModelCommand>().ToSelf().InSingletonScope();
                Kernel.Bind<ExtractViewModelFromViewCommand>().ToSelf().InSingletonScope();

                ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(Kernel));

                // Add solution services.
                Kernel.Bind<DTE2>().ToConstant(Ide);
                Kernel.Bind<ISolutionService>().To<SolutionService>().InSingletonScope();
                var ss = Kernel.Get<ISolutionService>();
                _solution = base.GetService(typeof(SVsSolution)) as IVsSolution;
                _solution?.AdviseSolutionEvents(ss, out _solutionEventsCookie);
                Kernel.Bind<IVsSolution>().ToConstant(_solution);

                base.Initialize();

                RegisterCommands();
            }
            catch (Exception ex)
            {
                //os.WriteLine($"MVVM Tools service startup failed: {ex.Message}.");
            }
        }

        /// <summary>
        /// Register the package commands (which must exist in the .vsct file).
        /// </summary>
        private void RegisterCommands()
        {
            var menuCommandService = Kernel.Get<IMenuCommandService>();
            if (menuCommandService != null)
            {
                // Create the individual commands, which internally register for command events.
                _commands.Add(Kernel.Get<GoToViewOrViewModelCommand>());
                _commands.Add(Kernel.Get<ScaffoldViewAndViewModelCommand>());
                _commands.Add(Kernel.Get<ExtractViewModelFromViewCommand>());

                // Add all commands to the menu command service.
                foreach (var command in _commands)
                    menuCommandService.AddCommand(command);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_solutionEventsCookie != 0)
            {
                _solution.UnadviseSolutionEvents(_solutionEventsCookie);
                _solutionEventsCookie = 0;
            }

            base.Dispose(disposing);
        }
        
        #endregion
            
    }
    
}
