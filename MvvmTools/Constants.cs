using System;
using System.ComponentModel.Design;

namespace MvvmTools
{
    internal static class Constants
    {
        public const string GuidPackage = "e4370a0b-2a09-437a-9d02-04b2d52dc044";
        public const string GuidSubMenuGroup = "a244c5bf-b5d1-471b-9189-507dd1c78957";
        public const string GuidPageGeneral = "719F6FA2-A486-4A8E-A84D-0D7F1DB45448";

        public static readonly CommandID GoToViewOrViewModelCommandId = new CommandID(new Guid(GuidSubMenuGroup), 0x0100);
        public static readonly CommandID ScaffoldViewAndViewModelCommandId = new CommandID(new Guid(GuidSubMenuGroup), 0x0200);
        public static readonly CommandID ExtractViewModelFromViewCommandId = new CommandID(new Guid(GuidSubMenuGroup), 0x0300);
    }
}
