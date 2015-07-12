namespace MvvmTools.Core.Models
{
    public class LocationDescriptor
    {
        public LocationDescriptor()
        {
            Auto = true;
            AppendViewType = true;
        }

        // If auto is true, all other options are treated as if null or default.
        public bool Auto { get; set; }
        
        // The current project is assumed if null.
        public string ProjectIdentifier { get; set; }
        
        // "Views" or "ViewModels" if null.  Can contain forward slashes for nested project folders.
        public string PathOffProject { get; set; }
        
        // ".Views" or ".ViewModels" if null.  Relative to project's default namespace
        // if starts with a dot.
        public string Namespace { get; set; }

        // If true (default), automatically appends view type such as 'Pages' to namespace and 
        // PathOffProject.  If view type is "View" then ignored.
        public bool AppendViewType { get; set; }
    }
    
}