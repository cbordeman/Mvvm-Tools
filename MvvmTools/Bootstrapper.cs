using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using MvvmTools.Commands;
using MvvmTools.Services;
using Ninject;
using Ninject.Infrastructure;

namespace MvvmTools
{
    internal class Bootstrapper
    {


        public static IKernel SetupContainer()
        {
            // Set up Ninject container.

            var kernel = new StandardKernel();
            
            // Our own singleton services.
            kernel.Bind<ISettingsService>().To<SettingsService>().InSingletonScope();
            kernel.Bind<ISolutionService>().To<SolutionService>().InSingletonScope();

            // Commands, which are singletons.
            kernel.Bind<GoToViewOrViewModelCommand>().ToSelf().InSingletonScope();
            kernel.Bind<ScaffoldViewAndViewModelCommand>().ToSelf().InSingletonScope();
            kernel.Bind<ExtractViewModelFromViewCommand>().ToSelf().InSingletonScope();

            return kernel;
        }
    }
}
