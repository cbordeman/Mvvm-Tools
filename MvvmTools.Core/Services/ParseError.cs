namespace MvvmTools.Core.Services
{
    public class ParseError
    {
        public ParseError(int lineNumber, string error)
        {
            LineNumber = lineNumber;
            Error = error;
        }

        public int LineNumber { get; set; }
        public string Error { get; private set; }
    }
}
