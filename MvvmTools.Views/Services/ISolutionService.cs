using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using MvvmTools.Core.Models;
using MvvmTools.Core.Utilities;

// ReSharper disable SuspiciousTypeConversion.Global

namespace MvvmTools.Core.Services
{
    public enum SolutionLoadState
    {
        NoSolution,
        Loading,
        Unloading,
        Loaded
    }

    public interface ISolutionService : IVsSolutionLoadEvents, IVsSolutionEvents3, IVsSolutionEvents4, IVsSolutionEvents5
    {
        /// <summary>
        /// Given a source or XAML file, extracts all the public, non-abstract classes.
        /// </summary>
        /// <param name="pi">A ProjectItem containing the source or markup to be scanned.  If markup, the code behind will scanned instead.</param>
        /// <returns>A list of public, non-abstract classes and their namespaces.</returns>
        List<NamespaceClass> GetClassesInProjectItem(ProjectItem pi);

        /// <summary>
        /// Locates types within the solution corresponding to a set of types, be they views or view models.
        /// </summary>
        /// <param name="viewModelsLocation">If null, searches whole solution for any corresponding view models.
        /// If provided, only that one project will be searched.</param>
        /// <param name="viewsLocation">If null, searches whole solution for any corresponding views.
        /// If provided, only that one project will be searched.</param>
        /// <param name="pi">The project item containing the types for which corresponding views or view models will be located.</param>
        /// <param name="typeNamesInFile">The type names in the 'pi' parameter's source file.  A set of candidate types will be
        /// compiled corresponding to views or viewmodels according to the 'viewSuffixes' and 'viewModelSuffix' parameters.</param>
        /// <param name="viewSuffixes">A set of view suffixes to append to the types in the 'typeNamesInFile' parameters
        /// to aid in locating potential corresponding view types.</param>
        /// <param name="viewModelSuffix">The view model suffix such as 'ViewModel' or 'PresentationModel' to append to the 
        /// types in the 'typeNamesInFile' parameters to aid in locating potential corresponding view model types.</param>
        /// <returns>A list of potential types with their ProjectItem containers.</returns>
        List<ProjectItemAndType> GetRelatedDocuments(
            LocationDescriptor viewModelsLocation, 
            LocationDescriptor viewsLocation, 
            ProjectItem pi, 
            IEnumerable<string> typeNamesInFile, 
            string[] viewSuffixes, 
            string viewModelSuffix);

        /// <summary>
        /// Gets the solution as a project model, and all the projects and solution folders.  Project contents are <b>not</b> included.
        /// </summary>
        /// <returns>A solution tree, with solution folders and projects, but <b>not</b> the contents of the projects.</returns>
        Task<ProjectModel> GetSolution();

        /// <summary>
        /// Gets the list of projects, flattened.
        /// </summary>
        /// <returns></returns>
        Task<List<ProjectModel>> GetProjectsList();

        /// <summary>
        /// Scans solution for the Project matching the unique id.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        Project GetProject(string uniqueId);

        /// <summary>
        /// Gets a project's model with Children filled in.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        ProjectModel GetFullProjectModel(Project project);
    }

    public class SolutionService : ISolutionService
    {
        #region Data

        private readonly IMvvmToolsPackage _mvvmToolsPackage;
        private readonly object _solutionLock = new object();
        private ProjectModel _solution;
        
        #endregion Data

        #region Ctor and Init

        public SolutionService(IMvvmToolsPackage mvvmToolsPackage)
        {
            _mvvmToolsPackage = mvvmToolsPackage;
            var solution = _mvvmToolsPackage.Ide.Solution;
            _solutionLoadState = solution.IsOpen ? SolutionLoadState.Unloading : SolutionLoadState.NoSolution;
        }

        #endregion Ctor and Init

        #region Properties

        #region SolutionLoadState
        private SolutionLoadState _solutionLoadState;
        private SolutionLoadState SolutionLoadState
        {
            get
            {
                lock (_solutionLock)
                    return _solutionLoadState;
            }
            set
            {
                lock (_solutionLock)
                    _solutionLoadState = value;
            }
        }
        #endregion SolutionLoadState

        #endregion Properties

        #region Public Methods

        public async Task<ProjectModel> GetSolution()
        {
            while (true)
            {
                switch (SolutionLoadState)
                {
                    case SolutionLoadState.NoSolution:
                    case SolutionLoadState.Unloading:
                        return null;
                    case SolutionLoadState.Loaded:
                        return _solution;
                    case SolutionLoadState.Loading:
                        // We are receiving load events and updating our internal state in the 
                        // background.  So we wait a bit longer for SolutionLoadState to
                        // change.
                        await Task.Delay(500);
                        break;
                }
            }
        }

        public async Task<List<ProjectModel>> GetProjectsList()
        {
            var rval = new List<ProjectModel>();

            try
            {
                var solution = await GetSolution();

                AddProjectsFlattenedRecursive(rval, solution.Children);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Error in GetProjectsList() of: {0}: {1}", this, ex));
                return rval;
            }
            return rval;
        }

        // Used from GetProjectsList().
        private void AddProjectsFlattenedRecursive(List<ProjectModel> projects, IEnumerable<ProjectModel> solutionTree, string prefix = null)
        {
            foreach (var p in solutionTree)
            {
                switch (p.Kind)
                {
                    case ProjectKind.Project:
                        projects.Add(new ProjectModel(
                            prefix + p.Name,
                            p.FullPath,
                            p.ProjectIdentifier,
                            p.Kind,
                            p.KindId));
                        break;
                    case ProjectKind.ProjectFolder:
                        return;
                }

                AddProjectsFlattenedRecursive(
                    projects,
                    p.Children,
                    string.Concat(prefix, p.Name, "/"));
            }
        }
    
        public Project GetProject(string uniqueId)
        {
            var solution = _mvvmToolsPackage.Ide.Solution;
            // Loop through solution's top level projects.
            foreach (var p in solution.Projects.Cast<Project>().Where(p => p.Name != "Solution Items"))
            {
                var project = FindProjectRecursive(p, uniqueId);
                if (project != null)
                    return project;
            }
            return null;

        }

        public ProjectModel GetFullProjectModel(Project project)
        {
            var rval = ConvertProjectToProjectModel(project);
            foreach (ProjectItem pi in project.ProjectItems)
            {
                var child = GetProjectItemsModelsRecursive(pi);
                rval.Children.Add(child);
            }

            return rval;
        }

        // Used by GetProjectModel().  Gets project items and folders.
        private ProjectModel GetProjectItemsModelsRecursive(ProjectItem projectItem)
        {
            ProjectModel rval = new ProjectModel(projectItem.Name, projectItem.Name, null, 
                projectItem.Kind == VsConstants.VsProjectItemKindPhysicalFolder ? ProjectKind.ProjectFolder : ProjectKind.Item, 
                projectItem.Kind);

            if (projectItem.ProjectItems == null)
                return rval;

            if (projectItem.SubProject != null)
                ;

            // Look through all this project's items.
            foreach (ProjectItem pi in projectItem.ProjectItems)
            {
                // Recursive call
                var child = GetProjectItemsModelsRecursive(pi);
                rval.Children.Add(child);

                if (pi.SubProject != null)
                    ;
            }

            return rval;
        }

        // Used by GetProject() to recursively locate a project.
        private static Project FindProjectRecursive(Project project, string uniqueId)
        {
            if (project.UniqueName == uniqueId)
                return project;

            // Look through all this project's items.
            foreach (ProjectItem pi in project.ProjectItems)
            {
                if (pi.SubProject != null)
                {
                    // Recursive call.
                    var p = FindProjectRecursive(pi.SubProject, uniqueId);
                    if (p != null)
                        return p;
                }
            }

            return null;
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
        
        public List<ProjectItemAndType> GetRelatedDocuments(
            LocationDescriptor viewModelsLocation,
            LocationDescriptor viewsLocation,
            ProjectItem pi, 
            IEnumerable<string> typeNamesInFile, 
            string[] viewSuffixes, 
            string viewModelSuffix)
        {
            List<string> viewModelCandidateTypeNames;
            List<string> viewCandidateTypeNames;
            GetTypeCandidates(typeNamesInFile, viewSuffixes, viewModelSuffix,
                out viewCandidateTypeNames,
                out viewModelCandidateTypeNames);

            List<ProjectItemAndType> rval;

            if (viewModelsLocation == null)
            {
                // Search whole solution.
                var allCandidateTypes = viewModelCandidateTypeNames.Union(viewCandidateTypeNames).Distinct().ToList();

                // Look for the candidate types in current project first, excluding the selected project item.
                rval = FindDocumentsContainingTypes(null, pi.ContainingProject, null, pi, allCandidateTypes);

                // Then add candidates from the rest of the solution.
                var solution = pi.DTE?.Solution;
                if (solution != null)
                {
                    foreach (Project project in solution.Projects)
                    {
                        if (project == pi.ContainingProject)
                            continue;

                        var docs = FindDocumentsContainingTypes(null, project, pi.ContainingProject, pi, allCandidateTypes);
                        rval.AddRange(docs);
                    }
                }
                return rval;
            }

            Project viewModelsProject = GetProject(viewModelsLocation.ProjectIdentifier);
            Project viewsProject = GetProject(viewsLocation.ProjectIdentifier);
            
            // Search views project first.
            rval = FindDocumentsContainingTypes(viewsLocation, viewsProject, null, pi, viewCandidateTypeNames);
            // Then, search view models project, excluding any xaml files.
            var vmDocs = FindDocumentsContainingTypes(viewModelsLocation, viewModelsProject, null, pi, viewModelCandidateTypeNames);
            rval.AddRange(vmDocs.Where(d => d.ProjectItem.Name.IndexOf(".xaml.", StringComparison.OrdinalIgnoreCase) == -1));

            return rval;
        }
        
        public void GetTypeCandidates(IEnumerable<string> typeNamesInFile, string[] viewSuffixes, string viewModelSuffix, out List<string> viewModelsTypeCandidates, out List<string> viewsTypeCandidates)
        {
            viewModelsTypeCandidates = new List<string>();
            viewsTypeCandidates = new List<string>();

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
                        viewModelsTypeCandidates.Add(candidate);
                    }

                    // Add base if it ends in one of the view suffixes.
                    foreach (var suffix in viewSuffixes)
                        if (baseName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            viewModelsTypeCandidates.Add(baseName);
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
                        viewsTypeCandidates.Add(candidate);

                        // Just add ViewModel
                        candidate = typeName + viewModelSuffix;
                        viewsTypeCandidates.Add(candidate);
                    }
                }
            }
        }

        private List<ProjectItemAndType> FindDocumentsContainingTypes(
            LocationDescriptor options,
            Project project, 
            Project excludeProject, 
            ProjectItem excludeProjectItem, 
            List<string> typesToFind)
        {
            var results = new List<ProjectItemAndType>();

            if (typesToFind.Count == 0)
                return results;

            IEnumerable<ProjectItem> itemsToSearch;
            if (options != null)
                itemsToSearch = LocateProjectItemsWithinFolders(project, options.PathOffProject);
            else
                itemsToSearch = project.ProjectItems.Cast<ProjectItem>();
            
            FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject, itemsToSearch, typesToFind, null, results);
            
            return results;
        }

        // Used by FindDocumentsContainingTypes().
        private IEnumerable<ProjectItem> LocateProjectItemsWithinFolders(Project project, string pathOffProject)
        {
            // Get the ProjectItems for the folder specified (usually 
            // ViewModels, but could be several levels deep).
            var folders = pathOffProject.Split('/').ToList();
            var rval = LocateProjectItemsWithinFoldersRecursive(folders, project.ProjectItems.Cast<ProjectItem>());

            return rval;
        }

        // Used by LocateProjectItemsAccordingToOptions().
        private IEnumerable<ProjectItem> LocateProjectItemsWithinFoldersRecursive(List<string> folders, IEnumerable<ProjectItem> projectItems)
        {
            // return No more folder
            if (folders.Count == 0)
                return projectItems;

            var folder1 = folders.First();
            // pop the first folder.
            folders.RemoveAt(0);

            foreach (var pi in projectItems)
            {
                if (pi.Kind == VsConstants.VsProjectItemKindPhysicalFolder &&
                    string.Equals(pi.Name, folder1, StringComparison.OrdinalIgnoreCase))
                {
                    // Recursive call with one fewer folders.
                    var subItems = LocateProjectItemsWithinFoldersRecursive(folders, pi.ProjectItems.Cast<ProjectItem>());
                    return subItems;
                }
            }

            return new List<ProjectItem>();
        }

        #endregion Public Methods

        #region Private Methods

        // This just converts the main properties, it doesn't recurse through the children.
        private static ProjectModel ConvertProjectToProjectModel(Project project)
        {
            // Sometimes unloaded or some project types projects throw on .FullName.
            string fullName = null;
            try
            {
                fullName = project.FullName;
                // Add the project.
            }
            catch
            {
                // Ignored.
            }

            var projectModel = new ProjectModel(
                    project.Name,
                    fullName,
                    project.UniqueName,
                    project.Kind == VsConstants.VsProjectItemKindSolutionFolder
                        ? ProjectKind.SolutionFolder
                        : ProjectKind.Project,
                    project.Kind);

            return projectModel;
        }

        private static bool IsSupportedProjectKind(string kind)
        {
            switch (kind)
            {
                case VsConstants.VsProjectKindMisc:
                    return false;
                default:
                    return true;
            }
        }

        private static List<ProjectModel> GetProjectModelsRecursive(Project project)
        {
            var rval = new List<ProjectModel>();

            if (project.ProjectItems == null)
                return rval;

            var projectModel = ConvertProjectToProjectModel(project);

            if (IsSupportedProjectKind(project.Kind))
                rval.Add(projectModel);

            // Look through all this project's items and recursively add any 
            // which are sub-projects and folders.
            foreach (ProjectItem pi in project.ProjectItems)
            {
                var itemProjectModels = new List<ProjectModel>();
                if (pi.SubProject != null)
                {
                    // Recursive call.
                    itemProjectModels = GetProjectModelsRecursive(pi.SubProject);
                }
                projectModel.Children.AddRange(itemProjectModels);
            }

            return rval;
        }

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

        private void FindDocumentsContainingTypesRecursive(
            ProjectItem excludeProjectItem, 
            Project excludeProject, 
            IEnumerable<ProjectItem> projectItems, 
            List<string> typesToFind, 
            ProjectItem parentProjectItem, List<ProjectItemAndType> results)
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
                {
                    if (pi.ProjectItems != null)
                        FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject,
                            pi.ProjectItems.Cast<ProjectItem>(), typesToFind,
                            pi, tmpResults);
                }
                else
                {
                    var items = pi.ProjectItems ?? pi.SubProject?.ProjectItems;
                    if (items != null)
                        FindDocumentsContainingTypesRecursive(excludeProjectItem, excludeProject,
                            items.Cast<ProjectItem>(), typesToFind,
                            null, tmpResults);
                }

                // Only search source files.
                if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Search the classes in the project item.
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

        //private class ProjectItemAndTypeEqualityComparer : IEqualityComparer<ProjectItemAndType>
        //{
        //    public bool Equals(ProjectItemAndType x, ProjectItemAndType y)
        //    {
        //        return String.Equals(x.ProjectItem.Name, y.ProjectItem.Name, StringComparison.OrdinalIgnoreCase) &&
        //               String.Equals(x.Type.Class, y.Type.Class, StringComparison.OrdinalIgnoreCase) &&
        //               String.Equals(x.Type.Namespace, y.Type.Namespace, StringComparison.OrdinalIgnoreCase);
        //    }

        //    public int GetHashCode(ProjectItemAndType obj)
        //    {
        //        return ($"{obj.ProjectItem.Name};{obj.Type.Namespace}.{obj.Type.Class}").GetHashCode();
        //    }
        //}

        private void ReloadSolution()
        {
            SolutionLoadState = SolutionLoadState.Loading;

            // Load solution into _solution.
            lock (_solutionLock)
            {
                var solution = _mvvmToolsPackage.Ide.Solution;

                var solutionModel = new ProjectModel(
                    Path.GetFileNameWithoutExtension(solution.FullName),
                    solution.FullName,
                    null,
                    ProjectKind.Solution,
                    null);

                // Add each of the top level projects and children to the local solutionModel.
                var topLevelProjects = solution.Projects.Cast<Project>().Where(p => p.Name != "Solution Items").ToArray();
                foreach (var p in topLevelProjects)
                {
                    var projectModels = GetProjectModelsRecursive(p);
                    solutionModel.Children.AddRange(projectModels);
                }

                // Set the backing field as to not create a deadlock on _solutionLock.
                _solution = solutionModel;
            }

            SolutionLoadState = SolutionLoadState.Loaded;
        }

        #endregion Private Methods

        #region IVsSolutionXXXXX

        // Several interfaces implemented here.

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            SolutionLoadState = SolutionLoadState.Loading;
            return VsConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VsConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VsConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VsConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            ReloadSolution();

            return VsConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            ReloadSolution();

            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnBeforeCloseSolution(object pUnkReserved)
        {
            SolutionLoadState = SolutionLoadState.Unloading;
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionLoadState = SolutionLoadState.NoSolution;
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnAfterMergeSolution(object pUnkReserved)
        {
            return VsConstants.S_OK;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents3.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnBeforeCloseSolution(object pUnkReserved)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionLoadState = SolutionLoadState.NoSolution;
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnAfterMergeSolution(object pUnkReserved)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents2.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            SolutionLoadState = SolutionLoadState.Unloading;
            return VsConstants.S_OK;
        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionLoadState = SolutionLoadState.NoSolution;
            return VsConstants.S_OK;
        }

        public int OnAfterRenameProject(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        public int OnQueryChangeProjectParent(IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, ref int pfCancel)
        {
            return VsConstants.S_OK;
        }

        public int OnAfterChangeProjectParent(IVsHierarchy pHierarchy)
        {
            return VsConstants.S_OK;
        }

        public int OnAfterAsynchOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VsConstants.S_OK;
        }

        public void OnBeforeOpenProject(ref Guid guidProjectId, ref Guid guidProjectType, string pszFileName)
        {
        }

        #endregion IVsSolutionXXXXX
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
