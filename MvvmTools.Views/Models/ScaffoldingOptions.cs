namespace MvvmTools.Core.Models
{
    public class ScaffoldingOptions
    {
        public ScaffoldingOptions()
        {
            ViewModelLocation = new LocationDescriptor();
            ViewLocation = new LocationDescriptor();
        }

        public LocationDescriptor ViewModelLocation { get; set; }
        public LocationDescriptor ViewLocation { get; set; }
    }
}