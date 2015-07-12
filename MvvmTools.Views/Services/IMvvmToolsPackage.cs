using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace MvvmTools.Core.Services
{
    public interface IMvvmToolsPackage : IVsPackage, IServiceProvider, IOleCommandTarget, IVsPersistSolutionOpts, IServiceContainer, System.IServiceProvider, IVsUserSettings, IVsUserSettingsMigration, IVsToolWindowFactory, IVsToolboxItemProvider
    {
        Document ActiveDocument { get; }
        double IdeVersion { get; }
        DTE2 Ide { get; }
    }
}
