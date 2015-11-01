using System;
using System.Collections.Generic;
using System.Linq;

namespace MvvmTools.Core.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Case-Insensitive version of string collection Contains.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool ContainsInsensitive(this IEnumerable<string> self, string item)
        {
            return self.Any(s => s.ContainsInsensitive(item));
        }

        /// <summary>
        /// Case-Sensitive version of string collection Contains.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool ContainsSensitive(this IEnumerable<string> self, string item)
        {
            return self.Any(o => o.Contains(item));
        }


        public static bool ContainsInsensitive(this string self, string toCheck)
        {
            return self?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string LastSegment(this string self, string separator)
        {
            if (self == null)
                return string.Empty;
            var lastSep = self.LastIndexOf(separator, StringComparison.Ordinal);
            if (lastSep == -1)
                return self;
            if (lastSep == self.Length - 1)
                return string.Empty;
            var rval = self.Substring(lastSep + 1);
            return rval;
        }

        public static string NamespaceFromFullName(this string self)
        {
            if (self == null)
                return string.Empty;
            var lastDot = self.LastIndexOf(".", StringComparison.Ordinal);
            if (lastDot == -1)
                return self;
            var rval = self.Substring(0, lastDot);
            return rval;
        }

        public static string XamlNamespaceFromAssemblyQualifiedName(this string self)
        {
            if (self == null)
                return string.Empty;
            var firstComma = self.IndexOf(',', 0);
            // If no comma (or nothing after comma), treat like there was 
            // no assembly part, just a class FullName.
            if (firstComma == -1 || firstComma >= self.Length)
                return self.NamespaceFromFullName();

            // Both parts are present, so strip the type off and return
            // namespace only + comma + assembly.
            var typePart = self.Substring(0, firstComma);
            var assPart = self.Substring(firstComma);  // includes leading comma
            var ns = typePart.NamespaceFromFullName();
            return ns + assPart;
        }

        public static string NamespaceFromAssemblyQualifiedName(this string self)
        {
            if (self == null)
                return string.Empty;
            var firstComma = self.IndexOf(',', 0);
            // If no comma (or nothing after comma), treat like there was 
            // no assembly part, just a class FullName.
            if (firstComma == -1 || firstComma >= self.Length)
                return self.NamespaceFromFullName();

            // Both parts are present, so strip the type off and return
            // namespace only.
            var typePart = self.Substring(0, firstComma);
            var ns = typePart.NamespaceFromFullName();
            return ns;
        }

        //public static string XamlNamespaceFromAssemblyQualifiedName(this string self)
        //{
        //    if (self == null)
        //        return string.Empty;
        //    var firstComma = self.IndexOf(',', 0);
        //    // If no comma (or nothing after comma), treat like there was 
        //    // no assembly part, just a class FullName.
        //    if (firstComma == -1 || firstComma >= self.Length)
        //        return self.NamespaceFromFullName();

        //    // Both parts are present, so strip the type off and return
        //    // namespace only + comma + assembly.
        //    var typePart = self.Substring(0, firstComma);
        //    var assPart = self.Substring(firstComma);  // includes leading comma
        //    var ns = typePart.NamespaceFromFullName();
        //    return ns + assPart;
        //}

        public static string ClassFromFullName(this string self)
        {
            return self.LastSegment(".");
        }

        public static string LastFolder(this string self)
        {
            if (self == null)
                return string.Empty;
            if (self.Contains("\\"))
                return self.LastSegment("\\");
            return self.LastSegment("/");
        }

        public static int LineCount(this string self)
        {
            if (self == null)
                return 0;
            return self.Split('\r').Length;
        }
    }
}
