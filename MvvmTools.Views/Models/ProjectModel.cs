using System.Collections.Generic;

namespace MvvmTools.Core.Models
{
    public class ProjectModel
    {
        public string Name { get; set; }
        public List<ProjectModel> Children { get; private set; }
        public string ProjectIdentifier { get; set; }
        public ProjectKind Kind { get; set; }

        public ProjectModel(string name, string projectIdentifier, ProjectKind kind)
        {
            Name = name;
            Kind = kind;
            ProjectIdentifier = projectIdentifier;

            Children = new List<ProjectModel>();
        }
    }

    public enum ProjectKind
    {
        Project, SolutionFolder, ProjectFolder
    }
}
