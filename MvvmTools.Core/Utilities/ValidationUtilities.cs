using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MvvmTools.Core.Utilities
{
    public static class ValidationUtilities
    {
        private static readonly Regex NamespaceRegex = new Regex("^(?:(?:((?![0-9])[a-zA-Z0-9_]+)\\.?)+)(?<!\\.)$");

        private static readonly Regex SuffixRegex = new Regex(@"^[_a-zA-Z0-9]*$");

        private static readonly Regex NameRegex = new Regex(@"^(?![0-9])[_a-zA-Z0-9]*$");

        public static string ValidateViewModelSuffix(string viewModelSuffix)
        {
            if (string.IsNullOrWhiteSpace(viewModelSuffix))
                return "Empty";

            if (!SuffixRegex.IsMatch(viewModelSuffix))
                return "Invalid";

            return null;
        }

        public static string ValidateNamespace(string @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                return "Empty";

            if (@namespace.Contains(" "))
                return "Contains spaces.";

            if (@namespace == ".")
                return "Invalid";

            // Starting with '.' is OK (a relative namespace), but I don't know to
            // modify the regex to allow it, so I'm just doing it here.
            var ns = @namespace;
            if (ns.StartsWith("."))
                ns = ns.Substring(1);

            if (!NamespaceRegex.IsMatch(ns))
                return "Invalid";

            return null;
        }

        public static string ValidatePathOffProject(string pathOffProject)
        {
            if (string.IsNullOrWhiteSpace(pathOffProject))
                return null;

            if (pathOffProject.StartsWith(" "))
                return "Starts with a space";

            if (pathOffProject.EndsWith(" "))
                return "Ends with a space";

            if (pathOffProject.Contains(" ."))
                return "Contains ' .'";

            if (pathOffProject.Contains(". "))
                return "Contains '. '";

            if (pathOffProject.Contains(".."))
                return "Contains '..'";

            if (pathOffProject.EndsWith("."))
                return "End with '.'";

            if (pathOffProject.StartsWith("."))
                return "Starts with '.'";

            if (pathOffProject.StartsWith("/"))
                return "Starts with '/'";

            if (pathOffProject.EndsWith("/"))
                return "Ends with '/'";

            if (pathOffProject.Contains("/."))
                return "Contains '/.'";

            if (pathOffProject.Contains("./"))
                return "Contains './'";

            if (pathOffProject.Contains("\\"))
                return "Use forward slashes";
            
            var reservedNames = new[] {"CON", "AUX", "PRN", "COM1", "COM2", "LPT1", "LPT2"};

            foreach (var rn in reservedNames)
                if (String.Equals(pathOffProject, rn, StringComparison.OrdinalIgnoreCase))
                    return "Reserved name";

            // Unicode surrogate characters are not allowed in solution folder names.
            foreach (var c in pathOffProject)
                if ((c >= 0xD800 && c <= 0xDBFF) || (c >= 0xDC00 && c <= 0xDFFF))
                    return "Surrogate character(s)";

            var containsInvalidChars = false;

            var invalidChars = Path.GetInvalidPathChars().ToList();
            invalidChars.Add('*');
            invalidChars.Add('?');
            invalidChars.Add(':');
            invalidChars.Add('&');
            invalidChars.Add('#');
            invalidChars.Add('%');
            foreach (var ic in invalidChars)
                if (pathOffProject.Any(c => ic == c))
                {
                    containsInvalidChars = true;
                    break;
                }

            if (containsInvalidChars)
                return "Bad: *?\"|<>:&#%";

            return null;
        }

        public static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Type the name, without suffix";

            if (!NameRegex.IsMatch(name))
                return "Invalid";

            return null;
        }

        public static string ValidateLanguage(string language)
        {
            if (language == null)
                return "Must be C# or VB";

            switch (language.Trim().ToUpper())
            {
                case "C#":
                case "VB":
                    return null;
            }
            return "Must be C# or VB";
        }
    }
}
