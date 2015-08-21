using System.IO;
using EnvDTE;

namespace MvvmTools.Core.Utilities
{
    public static class SourceControlUtilities
    {
        public static void EnsureCheckedOutIfExists(this Project project, string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                
                if (project.DTE.SourceControl != null &&
                    project.DTE.SourceControl.IsItemUnderSCC(path) &&
                    !project.DTE.SourceControl.IsItemCheckedOut(path))
                {
                    // Check out the item
                    project.DTE.SourceControl.CheckOutItem(path);
                }
            }
        }
    }
}
