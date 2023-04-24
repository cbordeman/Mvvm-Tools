using EnvDTE;
using EnvDTE80;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MvvmTools.Commands.GoToVM;
using MvvmTools.Models;
using MvvmTools.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Debugger = System.Diagnostics.Debugger;
using Project = EnvDTE.Project;

#pragma warning disable VSTHRD010

namespace MvvmTools.Services
{
    public class SolutionService : ISolutionService
    {
        #region Data

        // Beyond this number of documents, we don't parse the project's documents,
        // but instead just look at the filenames, assuming they contain one class
        // per file.
        private const int ProjectDocumentLimit = 10;

        private readonly IMvvmToolsPackage mvvmToolsPackage;
        private readonly IVsSolution vsSolution;
        private readonly object solutionLock = new();
        private ProjectModel solutionModel;

        #endregion Data

        #region Ctor and Init

        public SolutionService(IMvvmToolsPackage mvvmToolsPackage,
            IVsSolution vsSolution)
        {
            this.mvvmToolsPackage = mvvmToolsPackage;
            this.vsSolution = vsSolution;
        }

        public void Init()
        {
            ErrorHandler.ThrowOnFailure(vsSolution
                .GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));
            solutionLoadState = value is true ? SolutionLoadState.Loaded : SolutionLoadState.NoSolution;
            if (solutionLoadState == SolutionLoadState.Loaded)
                ReloadSolution();
        }

        #endregion Ctor and Init

        #region Events

        public event EventHandler SolutionLoadStateChanged;

        #endregion Events

        #region Properties

        #region SolutionLoadState
        private SolutionLoadState solutionLoadState;
        private SolutionLoadState SolutionLoadState
        {
            get
            {
                lock (solutionLock)
                    return solutionLoadState;
            }
            set
            {
                lock (solutionLock)
                    solutionLoadState = value;
                SolutionLoadStateChanged?.Invoke(this, null);
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
                        return solutionModel;
                    case SolutionLoadState.Loading:
                        // We are receiving load events and updating our internal state in the 
                        // background.  So we wait a bit longer for SolutionLoadState to
                        // change.
                        await Task.Delay(500).ConfigureAwait(true);
                        break;
                }
            }
        }

        public async Task<List<ProjectModel>> GetProjectsList()
        {
            var rval = new List<ProjectModel>();

            try
            {
                var solution = await GetSolution().ConfigureAwait(false);

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
                            p.KindId,
                            p.RootNamespace));
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
            var solution = mvvmToolsPackage.Ide.Solution;
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
            var rval = new ProjectModel(projectItem.Name, projectItem.Name, null,
                projectItem.Kind == VsConstants.VsProjectItemKindPhysicalFolder ? ProjectKind.ProjectFolder : ProjectKind.Item,
                projectItem.Kind, null);

            if (projectItem.ProjectItems == null)
                return rval;

            if (projectItem.SubProject != null)
            {
            }

            // Look through all this project's items.
            foreach (ProjectItem pi in projectItem.ProjectItems)
            {
                // Recursive call
                var child = GetProjectItemsModelsRecursive(pi);
                rval.Children.Add(child);

                if (pi.SubProject != null)
                {
                }
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

        public string GetProjectRootNamespace(Project p)
        {
            string rval = null;
            if (p?.Properties != null)
            {
                foreach (Property property in p.Properties)
                {
                    try
                    {
                        if (property.Name == "RootNamespace")
                        {
                            rval = property.Value.ToString();
                            break;
                        }
                    }
                    catch
                    {
                        // You can't read some property's values.
                    }
                }
            }
            return rval;
        }

        public List<NamespaceClass> GetClassesInProjectItemUsingCodeDom([CanBeNull] ProjectItem pi)
        {
            // Switch off UI thread.
            //await TaskScheduler.Default;

            var rval = new List<NamespaceClass>();

            if (pi?.Name == null)
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
            if (pi.FileCodeModel == null)
                return rval;

            var isXaml = pi.Name.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase) ||
                         pi.Name.EndsWith(".xaml.vb", StringComparison.OrdinalIgnoreCase);

            // If no namespace in file, then the project's default namespace is used.  This is
            // common in VB projects but rare in C#.
            var rootNamespace = GetProjectRootNamespace(pi.ContainingProject);

            var fileCm = (FileCodeModel2)pi.FileCodeModel;

            if (fileCm?.CodeElements == null) return rval;

            foreach (CodeElement2 ce in fileCm.CodeElements)
            {
                FindClassesRecursiveUsingCodeDom(rval, ce, isXaml, rootNamespace);

                // If a xaml.cs or xaml.vb code behind file, the first class must be the view type, so we can stop early.
                if (isXaml && rval.Count > 0)
                    break;
            }

            return rval;
        }

        public async Task<List<ProjectItemAndType>> 
            GetRelatedDocumentsUsingRoslyn(
            ProjectItem pi,
            IEnumerable<string> typeNamesInFile,
            string[] viewPrefixes,
            string[] viewSuffixes,
            string viewModelSuffix)
        {
            GetTypeCandidates(typeNamesInFile, viewPrefixes, viewSuffixes, viewModelSuffix,
                out var viewCandidateTypeNames,
                out var viewModelCandidateTypeNames);

            var allCandidateTypes = viewModelCandidateTypeNames.Union(viewCandidateTypeNames).Distinct().ToList();

            // Search whole solution.

            var cm = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            var ws = (Workspace)cm.GetService<VisualStudioWorkspace>();
            
            var solution = ws.CurrentSolution;
            
            // Look for the candidate types in current project first, excluding the selected project item.
            List<ProjectItemAndType> rval = new();

            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var projects = solution.Projects.ToArray();
            
            foreach (var project in projects)
            {
                var docs = project.Documents.ToList();
                //bool analyzeEveryDocumentInProject = docs.Count <= ProjectDocumentLimit;
                //if (analyzeEveryDocumentInProject)
                //    Trace.WriteLine($"MVVM Tools: For project {project.Name}: {docs.Count} docs  is less than the limit of {ProjectDocumentLimit}), scanning all docs.");
                foreach (var document in docs)
                {
                    try
                    {
                        // Never analyze generated code.
                        if (document.Name.Contains(".g.") ||
                            document.Name.Contains(".generated."))
                            continue;

                        //if (document.Name == "LoanProposalBorrowerIncome.xaml.cs")
                        //    Debugger.Break();
                        //if (document.Name == "LoanProposalBorrowerIncome.xaml")
                        //    Debugger.Break();
                        
                        if (rval.Any(x => ((RoslynProjectItemAndType)x).FilePath == document.FilePath))
                            continue;

                        // For larger projects, we assume the filename is the same
                        // as the class name, and only get the symbols in the file
                        // if that's true.  We'll miss some classes, but checking the
                        // classes in every file is extremely slow and just doesn't
                        // work for larger solutions.
                        //if (!analyzeEveryDocumentInProject)
                        //{
                            var fn = Path.GetFileNameWithoutExtension(document.Name);
                            fn = Path.GetFileNameWithoutExtension(fn);
                            if (!allCandidateTypes.Contains(fn, StringComparer.OrdinalIgnoreCase))
                                continue;
                        //}
                        

                        var syntaxRootNode = await document.GetSyntaxRootAsync().ConfigureAwait(false);
                        var classVisitor = new ClassVirtualizationVisitor(
                            ws,
                            project.Name,
                            solution,
                            project,
                            allCandidateTypes);

                        classVisitor.Visit(syntaxRootNode);

                        foreach (var item in classVisitor.Items)
                        {
                            if (rval.Any(x => ((RoslynProjectItemAndType)x).FilePath == item.FilePath))
                                continue;
                            rval.Add(item);
                        }
                        //rval.AddRange(classVisitor.Items);
                    }
                    catch (Exception e)
                    {
                        // Eat
                        Trace.WriteLine(e);
                    }
                }
            }
            return rval;
        }
        
        public List<ProjectItemAndType> 
            GetRelatedDocumentsUsingCodeDom(
            LocationDescriptor viewModelsLocation,
            LocationDescriptor viewsLocation,
            ProjectItem pi,
            IEnumerable<string> typeNamesInFile,
            string[] viewPrefixes,
            string[] viewSuffixes,
            string viewModelSuffix)
        {
            GetTypeCandidates(typeNamesInFile, viewPrefixes, viewSuffixes, viewModelSuffix,
                out var viewCandidateTypeNames,
                out var viewModelCandidateTypeNames);

            var viewModelsProject = GetProject(viewModelsLocation.ProjectIdentifier);
            var viewsProject = GetProject(viewsLocation.ProjectIdentifier);

            // Search views project first.
            var rval = new List<ProjectItemAndType>();
            rval.AddRange(FindDocumentsContainingTypesUsingCodeDom(viewsLocation, viewsProject, null, pi, viewCandidateTypeNames));
            // Then, search view models project, excluding any xaml files.
            var vmDocs = FindDocumentsContainingTypesUsingCodeDom(viewModelsLocation, viewModelsProject, null, pi, viewModelCandidateTypeNames);
            rval.AddRange(vmDocs.Where(d => d.Filename.IndexOf(".xaml.", StringComparison.OrdinalIgnoreCase) == -1));

            return rval;
        }

        public void GetTypeCandidates(IEnumerable<string> typeNamesInFile, string[] viewPrefixes, string[] viewSuffixes, string viewModelSuffix, out List<string> viewModelsTypeCandidates, out List<string> viewsTypeCandidates)
        {
            viewModelsTypeCandidates = new List<string>();
            viewsTypeCandidates = new List<string>();

            // For each type name in the file, create a list of candidates.
            foreach (var typeName in typeNamesInFile)
            {
                // If a view model...
                if (viewModelSuffix == string.Empty || typeName.EndsWith(viewModelSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    // Remove ViewModel from end and add all the possible suffixes.
                    var baseName = typeName.Substring(0, typeName.Length - viewModelSuffix.Length);
                    foreach (var suffix in viewSuffixes)
                    {
                        var candidate = baseName + suffix;
                        viewModelsTypeCandidates.Add(candidate);

                        if (viewPrefixes == null) continue;

                        foreach (var viewPrefix in viewPrefixes)
                        {
                            candidate = viewPrefix + baseName + suffix;
                            viewModelsTypeCandidates.Add(candidate);
                        }
                    }

                    // Add base if it ends in one of the view suffixes.
                    foreach (var suffix in viewSuffixes)
                        if (string.IsNullOrEmpty(suffix) || baseName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            if (viewModelsTypeCandidates.All(c => c != baseName))
                            {
                                viewModelsTypeCandidates.Add(baseName);

                                if (viewPrefixes == null) continue;

                                foreach (var viewPrefix in viewPrefixes)
                                {
                                    var candidate = viewPrefix + baseName;
                                    viewModelsTypeCandidates.Add(candidate);
                                }
                            }
                            break;
                        }
                }

                foreach (var viewSuffix in viewSuffixes)
                {
                    // Remove prefixes and suffix and add ViewModel.
                    var prefixes = viewPrefixes ?? new string[] { };
                    foreach (var viewPrefix in prefixes)
                    {
                        // Remove prefix and suffix and add ViewModel.
                        var baseName = typeName;
                        if (viewPrefix != string.Empty &&
                            typeName.StartsWith(viewPrefix, StringComparison.OrdinalIgnoreCase))
                            baseName = baseName.Substring(viewPrefix.Length);
                        if (viewSuffix != string.Empty && baseName.EndsWith(viewSuffix))
                            baseName = baseName.Substring(0, baseName.Length - viewSuffix.Length);

                        if (baseName != typeName)
                        {
                            var candidate = baseName + viewModelSuffix;
                            viewsTypeCandidates.Add(candidate);
                        }

                        if (viewModelSuffix != string.Empty)
                        {
                            var candidate = baseName + viewModelSuffix;
                            if (!viewsTypeCandidates.Contains(candidate))
                                viewsTypeCandidates.Add(candidate);
                        }
                    }
                }
            }
        }

        private IEnumerable<ProjectItemAndType> FindDocumentsContainingTypesUsingCodeDom(
            LocationDescriptor options,
            Project project,
            Project excludeProject,
            ProjectItem excludeProjectItem,
            List<string> typesToFind)
        {
            var results = new List<DteProjectItemAndType>();

            if (typesToFind.Count == 0)
                return results;

            var itemsToSearch = this.LocateProjectItemsWithinFolders(project, options.PathOffProject);
            FindDocumentsContainingTypesRecursiveForCodeDom(excludeProjectItem, excludeProject, itemsToSearch, typesToFind, null, results);

            return results;
        }

        // Used by FindDocumentsContainingTypes().
        private IEnumerable<ProjectItem> LocateProjectItemsWithinFolders(Project project, string pathOffProject)
        {
            if (pathOffProject == string.Empty)
                return project.ProjectItems.Cast<ProjectItem>();

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
            string rootNamespace = null;
            try
            {
                rootNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();
            }
            catch
            {
                // ignored
            }

            // Sometimes projects throw on .FullName.
            string fullName = null;
            try
            {
                fullName = project.FullName;
            }
            catch
            {
                // ignored
            }

            var projectModel = new ProjectModel(
                project.Name,
                fullName,
                project.UniqueName,
                project.Kind == VsConstants.VsProjectItemKindSolutionFolder
                    ? ProjectKind.SolutionFolder
                    : ProjectKind.Project,
                project.Kind,
                rootNamespace);

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
        private void FindClassesRecursiveUsingCodeDom(List<NamespaceClass> classes, CodeElement2 codeElement, bool isXaml, string rootNamespace)
        {
            try
            {
                if (codeElement.Kind == vsCMElement.vsCMElementClass)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var ct = (CodeClass2)codeElement;
                    classes.Add(new NamespaceClass(ct.Namespace?.Name ?? rootNamespace, ct.Name));
                }
                else if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    foreach (CodeElement2 childElement in codeElement.Children)
                    {
                        FindClassesRecursiveUsingCodeDom(classes, childElement, isXaml, rootNamespace);

                        // If a xaml.cs or xaml.vb code behind file, the first class must be the view type, so we can stop early.
                        if (isXaml && classes.Count > 0)
                            return;
                    }
                }
            }
            catch
            {
                // Eat
            }
        }

        private void FindDocumentsContainingTypesRecursiveForCodeDom(
            ProjectItem excludeProjectItem,
            Project excludeProject,
            IEnumerable<ProjectItem> projectItems,
            List<string> typesToFind,
            ProjectItem parentProjectItem,
            List<DteProjectItemAndType> results)
        {
            if (typesToFind.Count == 0 || projectItems == null)
                return;

            var tmpResults = new List<DteProjectItemAndType>();

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
                        FindDocumentsContainingTypesRecursiveForCodeDom(excludeProjectItem, excludeProject,
                            pi.ProjectItems.Cast<ProjectItem>(), typesToFind,
                            pi, tmpResults);
                }
                else
                {
                    var items = pi.ProjectItems ?? pi.SubProject?.ProjectItems;
                    if (items != null)
                        FindDocumentsContainingTypesRecursiveForCodeDom(excludeProjectItem, excludeProject,
                            items.Cast<ProjectItem>(), typesToFind,
                            null, tmpResults);
                }

                // Only search source files.
                if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Search the classes in the project item.
                var classesInProjectItem = GetClassesInProjectItemUsingCodeDom(pi);

                var xamlSaved = false;
                foreach (var c in classesInProjectItem)
                {
                    if (typesToFind.Contains(c.Class, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!xamlSaved && parentProjectItem != null)
                        {
                            // Parent is the xaml file corresponding to this xaml.cs or xaml.vb.  We save it once.
                            tmpResults.Add(new DteProjectItemAndType(parentProjectItem, c));
                            xamlSaved = true;
                        }

                        tmpResults.Add(new DteProjectItemAndType(pi, c));
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
            lock (solutionLock)
            {
                var solution = mvvmToolsPackage.Ide.Solution;

                var sm = new ProjectModel(
                    Path.GetFileNameWithoutExtension(solution.FullName),
                    solution.FullName,
                    null,
                    ProjectKind.Solution,
                    null,
                    null);

                // Add each of the top level projects and children to the local solutionModel.
                var topLevelProjects = solution.Projects.Cast<Project>().Where(p => p.Name != "Solution Items").ToArray();

                foreach (var p in topLevelProjects)
                {
                    var projectModels = GetProjectModelsRecursive(p);
                    sm.Children.AddRange(projectModels);
                }

                // Set the backing field as to not create a deadlock on _solutionLock.
                this.solutionModel = sm;
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
            //ReloadSolution();

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
}

