namespace MvvmTools.Core.Models
{
    public class LocationDescriptor
    {
        // The current project is assumed if null.
        public string ProjectIdentifier { get; set; }
        
        // "Views" or "ViewModels" if null.  Can contain forward slashes for nested project folders.
        public string PathOffProject { get; set; }
        
        // ".Views" or ".ViewModels" if null.  Relative to project's default namespace
        // if starts with a dot.
        public string Namespace { get; set; }

        internal void ApplyInherited(LocationDescriptor inherited)
        {
            PathOffProject = inherited.PathOffProject;
            Namespace = inherited.Namespace;
            ProjectIdentifier = inherited.ProjectIdentifier;
        }

        public bool InheritsFully(LocationDescriptor inherited)
        {
            if (PathOffProject != inherited.PathOffProject)
                return false;
            if (Namespace != inherited.Namespace)
                return false;
            if (ProjectIdentifier != inherited.ProjectIdentifier)
                return false;
            
            return true;
        }
    }
    
}