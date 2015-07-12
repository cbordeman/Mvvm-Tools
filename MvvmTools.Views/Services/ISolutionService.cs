using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using MvvmTools.Core.Models;

namespace MvvmTools.Core.Services
{
    public interface ISolutionService
    {
        List<NamespaceClass> GetClassesInProjectItem(ProjectItem pi);
        List<ProjectItemAndType> GetRelatedDocuments(ProjectItem pi, IEnumerable<string> typeNamesInFile, string[] viewSuffixes, string viewModelSuffix);
        List<string> GetTypeCandidates(IEnumerable<string> typeNamesInFile, string[] viewSuffixes, string viewModelSuffix);
        List<ProjectItemAndType> FindDocumentsContainingTypes(Project project, Project excludeProject, ProjectItem excludeProjectItem, List<string> typesToFind);
        List<ProjectModel> GetSolutionTree();
    }

    public class SolutionService : ISolutionService
    {
        #region Data

        private const string SolutionFolderKind = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        private const string ProjectFolderKind = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

        private readonly IMvvmToolsPackage _mvvmToolsPackage;

        #endregion Data

        #region Ctor and Init

        public SolutionService(IMvvmToolsPackage mvvmToolsPackage)
        {
            _mvvmToolsPackage = mvvmToolsPackage;
        }

        #endregion Ctor and Init

        #region Public Methods

        public List<ProjectModel> GetSolutionTree()
        {
            var solution = _mvvmToolsPackage.Ide.Solution;

            var topLevelProjects = solution.Projects.Cast<Project>().Where(p => p.Name != "Solution Items").ToArray();
            var topLevelProjectModels = new List<ProjectModel>();
            foreach (Project p in topLevelProjects)
            {
                var projectModels = GetProjectModelsRecursive(p);
                topLevelProjectModels.AddRange(projectModels);
            }
            
            return topLevelProjectModels;
        }

        private static List<ProjectModel> GetProjectModelsRecursive(Project project)
        {
            var rval = new List<ProjectModel>();

            // Add the project.
            var projectModel = new ProjectModel(
                project.Name, 
                project.UniqueName,
                project.Kind == SolutionFolderKind ? ProjectKind.SolutionFolder : ProjectKind.Project);
            rval.Add(projectModel);

            // Look through all this project's items and recursively add any 
            // which are sub-projects and folders.
            foreach (ProjectItem pi in project.ProjectItems)
            {
                var itemProjectModels = new List<ProjectModel>();
                if (pi.Kind == ProjectFolderKind)
                {
                    if (pi.Name != "Properties")
                    {
                        var folderModels = GetProjectFolderModelsRecursive(pi);
                        itemProjectModels.AddRange(folderModels);
                    }
                }
                else if (pi.SubProject != null)
                {
                    // Recursive call.
                    itemProjectModels = GetProjectModelsRecursive(pi.SubProject);
                }
                projectModel.Children.AddRange(itemProjectModels);
            }

            return rval;
        }

        private static List<ProjectModel> GetProjectFolderModelsRecursive(ProjectItem projectFolder)
        {
            var rval = new List<ProjectModel>();

            // Add node for projectFolder.
            var projectFolderModel = new ProjectModel(
                            projectFolder.Name,
                            projectFolder.Name,
                            ProjectKind.ProjectFolder);
            rval.Add(projectFolderModel);

            if (projectFolder.ProjectItems == null)
                return rval;

            // Collect project folder's Project Items that are either SubProject or other
            // project folders, then add them as children of this project folder.
            foreach (ProjectItem pi in projectFolder.ProjectItems)
            {
                if (pi.Kind == ProjectFolderKind)
                {
                    var subFolderModels = GetProjectFolderModelsRecursive(pi);
                    projectFolderModel.Children.AddRange(subFolderModels);
                }
            }
            return rval;
        }

        public List<NamespaceClass> GetClassesInProjectItem(ProjectItem pi)
        {
            var rval = new List<NamespaceClass>();

            if (pi.Name == null)
                return rval;

            if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase) &&
                !pi.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                return rval;

            // If has children, that is the source file, check that instead.
            if (pi.ProjectItems != null && pi.ProjectItems.Count != 0)
                foreach (ProjectItem p in pi.ProjectItems)
                    pi = p;

            // If not a part of a project or not compiled, code model will be empty 
            // and there's nothing we can do.
            if (pi?.FileCodeModel == null)
                return rval;

            var isXaml = pi.Name.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase) ||
                         pi.Name.EndsWith(".xaml.vb", StringComparison.OrdinalIgnoreCase);
            var fileCm = (FileCodeModel2)pi.FileCodeModel;
            if (fileCm?.CodeElements != null)
            {
                foreach (CodeElement2 ce in fileCm.CodeElements)
                {
                    FindClassesRecursive(rval, ce, isXaml);

                    // If a xaml.cs or xaml.vb code behind file, the first class must be the view type, so we can stop early.
                    if (isXaml && rval.Count > 0)
                        break;
                }
            }

            return rval;
        }

        public List<ProjectItemAndType> GetRelatedDocuments(ProjectItem pi, IEnumerable<string> typeNamesInFile, string[] viewSuffixes, string viewModelSuffix)
        {
            var candidateTypeNames = GetTypeCandidates(typeNamesInFile, viewSuffixes, viewModelSuffix);

            // Look for the candidate types in current project first, excluding the selected project item.
            var documents = FindDocumentsContainingTypes(pi.ContainingProject, null, pi, candidateTypeNames);

            // Then add candidates from the rest of the solution.
            var solution = pi.DTE?.Solution;
            if (solution != null)
            {
                foreach (Project project in solution.Projects)
                {
                    if (project == pi.ContainingProject)
                        continue;

                    var docs = FindDocumentsContainingTypes(project, pi.ContainingProject, pi, candidateTypeNames);
                    documents.AddRange(docs);
                }
            }

            var rval = documents.Distinct(new ProjectItemAndTypeEqualityComparer()).ToList();

            return rval;
        }

        public List<string> GetTypeCandidates(IEnumerable<string> typeNamesInFile, string[] viewSuffixes, string viewModelSuffix)
        {
            var candidates = new List<string>();

            // For each type name in the file, create a list of candidates.
            foreach (var typeName in typeNamesInFile)
            {
                // If a view model...
                if (typeName.EndsWith(viewModelSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove ViewModel from end and add all the possible suffixes.
                    var baseName = typeName.Substring(0, typeName.Length - 9);
                    foreach (var suffix in viewSuffixes)
                    {
                        var candidate = baseName + suffix;
                        candidates.Add(candidate);
                    }

                    // Add base if it ends in one of the view suffixes.
                    foreach (var suffix in viewSuffixes)
                        if (baseName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            candidates.Add(baseName);
                            break;
                        }
                }

                foreach (var suffix in viewSuffixes)
                {
                    if (typeName.EndsWith(suffix))
                    {
                        // Remove suffix and add ViewModel.
                        var baseName = typeName.Substring(0, typeName.Length - suffix.Length);
                        var candidate = baseName + viewModelSuffix;
                        candidates.Add(candidate);

                        // Just add ViewModel
                        candidate = typeName + viewModelSuffix;
                        candidates.Add(candidate);
                    }
                }
            }

            return candidates;
        }

        public List<ProjectItemAndType> FindDocumentsContainingTypes(Project project, Project excludeProject, ProjectItem excludeProjectItem, List<string> typesToFind)
        {
            var results = new List<ProjectItemAndType>();

            FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject, project.ProjectItems, typesToFind, null, results);

            return results;
        }
        
        #endregion Public Methods

        #region Private Methods
        
        // Recursively examine code elements.
        private void FindClassesRecursive(List<NamespaceClass> classes, CodeElement2 codeElement, bool isXaml)
        {
            try
            {
                if (codeElement.Kind == vsCMElement.vsCMElementClass)
                {
                    var ct = (CodeClass2)codeElement;
                    classes.Add(new NamespaceClass(ct.Namespace.Name, ct.Name));
                }
                else if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    foreach (CodeElement2 childElement in codeElement.Children)
                    {
                        FindClassesRecursive(classes, childElement, isXaml);

                        // If a xaml.cs or xaml.vb code behind file, the first class must be the view type, so we can stop early.
                        if (isXaml && classes.Count > 0)
                            return;
                    }
                }
            }
            catch
            {
                //Console.WriteLine(new string('\t', tabs) + "codeElement without name: {0}", codeElement.Kind.ToString());
            }
        }

        private void FindDocumentsContainingTypesRecursive(ProjectItem excludeProjectItem, Project excludeProject, ProjectItems projectItems, List<string> typesToFind, ProjectItem parentProjectItem, List<ProjectItemAndType> results)
        {
            if (typesToFind.Count == 0 || projectItems == null)
                return;

            var tmpResults = new List<ProjectItemAndType>();

            foreach (ProjectItem pi in projectItems)
            {
                // Exclude the document we're on.
                if (pi == excludeProjectItem)
                    continue;

                // Exclude the project already searched.
                if (excludeProject != null && pi.ContainingProject != null &&
                    pi.ContainingProject == excludeProject)
                    return;

                // Recursive call
                if (pi.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                    FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject, pi.ProjectItems, typesToFind,
                        pi, tmpResults);
                else
                    FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject, pi.ProjectItems ?? pi.SubProject?.ProjectItems, typesToFind,
                        null, tmpResults);

                // Only search source files.
                if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
                    continue;

                var classesInProjectItem = GetClassesInProjectItem(pi);

                var xamlSaved = false;
                foreach (var c in classesInProjectItem)
                {
                    if (typesToFind.Contains(c.Class, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!xamlSaved && parentProjectItem != null)
                        {
                            // Parent is the xaml file corresponding to this xaml.cs or xaml.vb.  We save it once.
                            tmpResults.Add(new ProjectItemAndType(parentProjectItem, c));
                            xamlSaved = true;
                        }

                        tmpResults.Add(new ProjectItemAndType(pi, c));
                    }
                }
            }

            results.AddRange(tmpResults);
        }

        private class ProjectItemAndTypeEqualityComparer : IEqualityComparer<ProjectItemAndType>
        {
            public bool Equals(ProjectItemAndType x, ProjectItemAndType y)
            {
                return String.Equals(x.ProjectItem.Name, y.ProjectItem.Name, StringComparison.OrdinalIgnoreCase) &&
                       String.Equals(x.Type.Class, y.Type.Class, StringComparison.OrdinalIgnoreCase) &&
                       String.Equals(x.Type.Namespace, y.Type.Namespace, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(ProjectItemAndType obj)
            {
                return ($"{obj.ProjectItem.Name};{obj.Type.Namespace}.{obj.Type.Class}").GetHashCode();
            }
        }

        #endregion Private Methods
    }

    public class ProjectItemAndType
    {
        public ProjectItemAndType(ProjectItem projectItem, NamespaceClass type)
        {
            ProjectItem = projectItem;
            Type = type;
        }

        public ProjectItem ProjectItem { get; set; }
        public NamespaceClass Type { get; set; }

        public string RelativeNamespace
        {
            get
            {
                if (Type.Namespace.StartsWith(ProjectItem.ContainingProject.Name))
                    return Type.Namespace.Substring(ProjectItem.ContainingProject.Name.Length);
                return Type.Namespace;
            }
        }
    }

    public class NamespaceClass
    {
        public NamespaceClass(string @namespace, string @class)
        {
            Namespace = @namespace;
            Class = @class;
        }

        public string Namespace { get; set; }
        public string Class { get; set; }
    }
}
