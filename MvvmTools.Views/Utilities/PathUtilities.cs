using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MvvmTools.Core.Utilities
{
    internal class PathUtilities
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            int fromAttr = GetPathAttribute(fromPath);
            int toAttr = GetPathAttribute(toPath);

            var path = new StringBuilder(260); // MAX_PATH
            if (DllImports.PathRelativePathTo(
                path,
                fromPath,
                fromAttr,
                toPath,
                toAttr) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix");
            }
            var p = path.ToString();
            if (p.StartsWith(".\\"))
                return p.Substring(2);
            else
                return p;
        }

        private static int GetPathAttribute(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                return FILE_ATTRIBUTE_DIRECTORY;
            }

            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                return FILE_ATTRIBUTE_NORMAL;
            }

            throw new FileNotFoundException();
        }

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;


        public static string SmartTruncate(string path, int maxWidth)
        {
            if (maxWidth < 6)
            {
                string message = "Argument must be greater than or equalTo 6";
                throw new ArgumentOutOfRangeException("maxWidth", message);
            }

            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length <= maxWidth)
            {
                return path;
            }

            // get the leaf folder name of this directory path
            // e.g. if the path is C:\documents\projects\visualstudio\, we want to get the 'visualstudio' part.
            string folder =
                path.Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ??
                String.Empty;
            // surround the folder name with the pair of \ characters.
            folder = Path.DirectorySeparatorChar + folder + Path.DirectorySeparatorChar;

            string root = Path.GetPathRoot(path);
            int remainingWidth = maxWidth - root.Length - 3; // 3 = length(ellipsis)

            // is the directory name too big? 
            if (folder.Length >= remainingWidth)
            {
                // yes drop leading backslash and eat into name
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}...{1}",
                    root,
                    folder.Substring(folder.Length - remainingWidth));
            }
            else
            {
                // no, show like VS solution explorer (drive+ellipsis+end)
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "{0}...{1}",
                    root,
                    folder);
            }
        }

        public static string EscapePSPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            // The and [ the ] characters are interpreted as wildcard delimiters. Escape them first.
            path = path.Replace("[", "`[").Replace("]", "`]");

            if (path.Contains("'"))
            {
                // If the path has an apostrophe, then use double quotes to enclose it.
                // However, in that case, if the path also has $ characters in it, they
                // will be interpreted as variables. Thus we escape the $ characters.
                return "\"" + path.Replace("$", "`$") + "\"";
            }
            else
            {
                // if the path doesn't have apostrophe, then it's safe to enclose it with apostrophes
                return "'" + path + "'";
            }
        }
    }
}

