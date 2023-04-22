namespace MvvmTools.Services
{
    public class NamespaceClass
    {
        public NamespaceClass(string ns, string @class)
        {
            this.Namespace = ns;
            this.Class = @class;
        }

        public string Namespace { get; set; }
        public string Class { get; set; }
    }
}