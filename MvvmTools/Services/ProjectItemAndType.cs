using EnvDTE;

namespace MvvmTools.Services
{
    public class ProjectItemAndType
    {
        public ProjectItemAndType(ProjectItem projectItem, NamespaceClass type)
        {
            this.ProjectItem = projectItem;
            this.Type = type;
        }

        public ProjectItem ProjectItem { get; set; }
        public NamespaceClass Type { get; set; }

        public string RelativeNamespace
        {
            get
            {
                if (this.Type.Namespace == this.ProjectItem?.ContainingProject?.Name)
                    return "(same)";
                if (this.Type.Namespace.StartsWith(this.ProjectItem.ContainingProject.Name))
                    return this.Type.Namespace.Substring(this.ProjectItem.ContainingProject.Name.Length);
                return this.Type.Namespace;
            }
        }
    }
}