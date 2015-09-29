using MvvmTools.Core.Models;

namespace MvvmTools.Core.Services
{
    public class ParseError
    {
        public ParseError(Template template, Field field, string source, string error)
        {
            Template = template;
            Field = field;
            Source = source;
            Error = error;
        }

        public Template Template { get; set; }
        public Field Field { get; set; }
        public string Source { get; set; }
        public string Error { get; private set; }
    }
}
