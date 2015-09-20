namespace MvvmTools.Core.Services
{
    public class ParseError
    {
        public ParseError(string error, int lineNumber)
        {
            Error = error;
            LineNumber = lineNumber;
        }

        public string Error { get; private set; }
        public int LineNumber { get; private set; }
    }
}
