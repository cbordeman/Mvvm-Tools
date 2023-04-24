using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using MvvmTools.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE80;
using JetBrains.Annotations;
using Microsoft.VisualStudio;
using MvvmTools.Utilities;

// ReSharper disable SuspiciousTypeConversion.Global

namespace MvvmTools.Services
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
        /// Called when solution is loaded.
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Gets the root namespace (&apos;default&apos; namespace set in project properties.  Important
        /// when a class is not in a namespace or when doing VB where namespaces are relative to the project
        /// and often not present.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        string GetProjectRootNamespace(Project p);

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
        /// <param name="viewPrefixes">A set of view prefixes to remove from the types in the 'typeNamesInFile' values.</param>
        /// <param name="viewSuffixes">A set of view suffixes to append to the types in the 'typeNamesInFile' values.</param>
        /// <param name="viewModelSuffix">The view model suffix such as 'ViewModel' or 'PresentationModel' to append to the 
        /// types in the 'typeNamesInFile' parameters to aid in locating potential corresponding view model types.</param>
        /// <returns>A list of potential types with their ProjectItem containers.</returns>
        List<ProjectItemAndType> GetRelatedDocuments(
            LocationDescriptor viewModelsLocation,
            LocationDescriptor viewsLocation,
            ProjectItem pi,
            IEnumerable<string> typeNamesInFile,
            string[] viewPrefixes,
            string[] viewSuffixes,
            string viewModelSuffix);

        /// <summary>
        /// Gets the solution as a project model, and all the projects and solution folders.  Project contents are <b>not</b> included.
        /// </summary>
        /// <returns>A solution tree, with solution folders and projects, but <b>not</b> the contents of the projects.</returns>
        Task<ProjectModel> GetSolution();

        /// <summary>
        /// Lets consumers know when to call our methods again because the solution or some project has been unloaded, added, removed, or loaded.
        /// </summary>
        event EventHandler SolutionLoadStateChanged;

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

}
