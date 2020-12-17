using System.Runtime.InteropServices;
using System.Text;

namespace MvvmTools.Utilities
{
    internal static class DllImports
    {
        [DllImport("shlwapi.dll", SetLastError = true)]
        public static extern int PathRelativePathTo(StringBuilder pszPath,
            string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
    }
}
