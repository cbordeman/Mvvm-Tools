using System;
using System.ComponentModel.Design;

namespace MvvmTools
{
    internal static class Constants
    {
        public static readonly Guid SubMenuGroup = new Guid("a244c5bf-b5d1-471b-9189-507dd1c78957");

        public static readonly CommandID GoToViewOrViewModelCommandId = new CommandID(SubMenuGroup, 0x0100);
        public static readonly CommandID ScaffoldViewAndViewModelCommandId = new CommandID(SubMenuGroup, 0x0200);
        public static readonly CommandID ExtractViewModelFromViewCommandId = new CommandID(SubMenuGroup, 0x0300);
    }
}
