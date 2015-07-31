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

        // If true, automatically appends view type such as 'Pages' to namespace and 
        // PathOffProject.  If view type is "View" then ignored.
        public bool AppendViewType { get; set; }

        public void ApplyInherited(LocationDescriptor inherited)
        {
            if (PathOffProject == inherited.PathOffProject)
                PathOffProject = inherited.PathOffProject;
            if (Namespace == inherited.Namespace)
                Namespace = inherited.Namespace;
            if (ProjectIdentifier == inherited.ProjectIdentifier)
                ProjectIdentifier = inherited.ProjectIdentifier;
            if (AppendViewType == inherited.AppendViewType)
                AppendViewType = inherited.AppendViewType;
        }

        public bool InheritsFully(LocationDescriptor inherited)
        {
            if (PathOffProject != inherited.PathOffProject)
                return false;
            if (Namespace != inherited.Namespace)
                return false;
            if (ProjectIdentifier != inherited.ProjectIdentifier)
                return false;
            if (AppendViewType != inherited.AppendViewType)
                return false;

            return true;
        }
    }
    
}