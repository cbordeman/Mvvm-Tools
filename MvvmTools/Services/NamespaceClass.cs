namespace MvvmTools.Services
{
    public class NamespaceClass
    {
        public NamespaceClass(string @namespace, string @class)
        {
            this.Namespace = @namespace;
            this.Class = @class;
        }

        public string Namespace { get; set; }
        public string Class { get; set; }
    }
}