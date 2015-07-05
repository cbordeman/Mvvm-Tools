namespace MvvmTools.Core.Models
{
    public class ProjectItemDescriptor
    {
        public ProjectItemDescriptor()
        {
            Auto = true;
        }

        public bool Auto { get; set; }
        public string PathOffProject { get; set; }
        public string Namespace { get; set; }
    }
}