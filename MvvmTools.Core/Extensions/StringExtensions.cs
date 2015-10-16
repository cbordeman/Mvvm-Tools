namespace MvvmTools.Core.Extensions
{
    public static class StringExtensions
    {
        public static string LastSegment(this string self, char separator)
        {
            if (self == null)
                return null;
            var split = self.Split(separator);
            return split.Length == 0 ? null : split[split.Length - 1];
        }
    }
}
