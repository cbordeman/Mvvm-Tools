using MvvmTools.Core.Services;

namespace MvvmTools.Core.Models
{
    public class ScaffoldingOptions
    {
        public ScaffoldingOptions()
        {
            ViewModelDescriptor = new ProjectItemDescriptor();
            ViewDescriptor = new ProjectItemDescriptor();
        }

        public ProjectItemDescriptor ViewModelDescriptor { get; set; }
        public ProjectItemDescriptor ViewDescriptor { get; set; }
    }
}