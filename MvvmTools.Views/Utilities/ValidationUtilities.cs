using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MvvmTools.Core.Utilities
{
    public static class ValidationUtilities
    {
        private static readonly Regex NamespaceRegex = new Regex("^(?:(?:((?![0-9])[a-zA-Z0-9_]+)\\.?)+)(?<!\\.)$");

        public static string ValidateNamespace(string @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                return "Empty.";

            if (@namespace.Contains(" "))
                return "Cannot contain spaces.";

            if (@namespace == ".")
                return "Invalid.";

            // Starting with '.' is OK (a relative namespace), but I don't know to
            // modify the regex to allow it, so I'm just doing it here.
            var ns = @namespace;
            if (ns.StartsWith("."))
                ns = ns.Substring(1);

            if (!NamespaceRegex.IsMatch(ns))
                return "Invalid.";

            return null;
        }

        public static string ValidatePathOffProject(string pathOffProject)
        {
            if (string.IsNullOrWhiteSpace(pathOffProject))
                return "Empty.";

            if (pathOffProject.StartsWith(" "))
                return "Cannot start with a space.";

            if (pathOffProject.EndsWith(" "))
                return "Cannot end with a space.";

            if (pathOffProject.Contains(" ."))
                return "Cannot contain ' .'.";

            if (pathOffProject.Contains(". "))
                return "Cannot contain '. '.";

            if (pathOffProject.Contains(".."))
                return "Cannot contain '..'.";

            if (pathOffProject.EndsWith("."))
                return "Cannot end with '.'.";

            if (pathOffProject.StartsWith("."))
                return "Cannot start with '.'.";

            if (pathOffProject.StartsWith("\\"))
                return "Cannot start with '\\'.";

            if (pathOffProject.EndsWith("\\"))
                return "Cannot end with '\\'.";

            if (pathOffProject.Contains("\\."))
                return "Cannot contain '\\'.";

            if (pathOffProject.Contains(".\\"))
                return "Cannot contain '.\\'.";

            if (pathOffProject.Contains("\\\\"))
                return "Cannot contain '\\\\'.";

            var reservedNames = new[] {"CON", "AUX", "PRN", "COM1", "COM2", "LPT1", "LPT2"};

            foreach (var rn in reservedNames)
                if (string.Equals(pathOffProject, rn, StringComparison.OrdinalIgnoreCase))
                    return "System reserved name.";

            // Unicode surrogate characters are not allowed in solution folder names.
            foreach (var c in pathOffProject)
                if ((c >= 0xD800 && c <= 0xDBFF) || (c >= 0xDC00 && c <= 0xDFFF))
                    return "Surrogate character(s).";

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
                return "Cannot contain *?\"/|<>:&#%";

            return null;
        }

    }
}
