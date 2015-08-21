namespace MvvmTools.Core.Utilities
{
    public static class VsUtilities
    {
        /// <summary>
        /// Returns a description given a value from VsConstants.
        /// </summary>
        /// <param name="projectKindId">A value from VsConstants.</param>
        /// <returns></returns>
        public static string GetProjectTypeDescription(string projectKindId)
        {
            switch (projectKindId)
            {
                case null:
                case "":
                    return null;
                case VsConstants.SharedProjectProjectTypeGuid:
                    return "Shared";
                case VsConstants.WebApplicationProjectTypeGuid:
                    return "Web App";
                case VsConstants.CpsProjectTypeGuid:
                    return "CPS";
                case VsConstants.CsharpProjectTypeGuid:
                    return "C#";
                case VsConstants.DeploymentProjectTypeGuid:
                    return "Deployment";
                case VsConstants.DxJsProjectTypeGuid:
                    return "Dx Javascript";
                case VsConstants.FsharpProjectTypeGuid:
                    return "F#";
                case VsConstants.InstallShieldLimitedEditionTypeGuid:
                    return "InstallShield";
                case VsConstants.JsProjectTypeGuid:
                    return "JavaScript";
                case VsConstants.LightSwitchProjectTypeGuid:
                    return "Lightswitch";
                case VsConstants.CppProjectTypeGuid:
                    return "C++";
                case VsConstants.VsProjectKindMisc:
                    return "Miscellaneous";
                case VsConstants.NemerleProjectTypeGuid:
                    return "Nemerle";
                case VsConstants.NomadForVisualStudioProjectTypeGuid:
                    return "Nomad";
                case VsConstants.SynergexProjectTypeGuid:
                    return "Synergex";
                case VsConstants.TDSProjectTypeGuid:
                    return "TDS Project";
                case VsConstants.UnloadedProjectTypeGuid:
                    return "(Unloaded)";
                case VsConstants.VbProjectTypeGuid:
                    return "Visual Basic";
                case VsConstants.WebSiteProjectTypeGuid:
                    return "Web Site";
                case VsConstants.WindowsStoreProjectTypeGuid:
                    return "Store";
                case VsConstants.WixProjectTypeGuid:
                    return "Wix";
                case VsConstants.WcfServiceProjectTypeGuid:
                    return "WCF Service";
            }

            return projectKindId;
        }
    }
}
