namespace MvvmTools.Models
{
    public class AssemblyQualifiedType
    {
        public AssemblyQualifiedType(string @class, string assembly)
        {
            Class = @class;
            Assembly = assembly;
        }

        public string Class { get; set; }
        public string Assembly { get; set; }

        public string ClassAndAssembly => (Class ?? string.Empty) + ", " + (Assembly ?? string.Empty);

        public override string ToString()
        {
            return string.IsNullOrEmpty(Class) ? string.Empty : (Class ?? string.Empty) + ", " + (Assembly ?? string.Empty);
        }
    }
}
