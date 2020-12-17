using System;
using System.ComponentModel.Design;

namespace MvvmTools
{
    internal static class Constants
    {                                    //6e8a3b1d-0f93-4749-b3e8-39e83c15c82e
        public const string GuidPackage = "e4370a0b-2a09-437a-9d02-04b2d52dc044";
        public const string GuidMvvmToolsTopLevelMenuGroup = "a244c5bf-b5d1-471b-9189-507dd1c78957";
        public const string GuidOptionsPageGeneral = "719F6FA2-A486-4A8E-A84D-0D7F1DB45448";
        public const string GuidOptionsPageSolutionAndProjects = "BA6E7A35-59A9-4FB0-8BEE-3D2CC777DBF9";
        public const string GuidOptionsPageTemplateOptions = "1E4EAB14-D4FC-4FB6-83F6-2DEA7314FC12";
        public const string GuidOptionsPageTemplateMaintenance = "96C77D2A-0889-461E-923C-7F510B1E58BB";

        public static readonly CommandID GoToViewOrViewModelCommandId = new CommandID(new Guid(GuidMvvmToolsTopLevelMenuGroup), 0x0100);
        public static readonly CommandID ScaffoldViewAndViewModelCommandId = new CommandID(new Guid(GuidMvvmToolsTopLevelMenuGroup), 0x0200);
        public static readonly CommandID ExtractViewModelFromViewCommandId = new CommandID(new Guid(GuidMvvmToolsTopLevelMenuGroup), 0x0300);
    }
}
