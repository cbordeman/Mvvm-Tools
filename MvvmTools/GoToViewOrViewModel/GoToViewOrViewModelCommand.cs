//------------------------------------------------------------------------------
// <copyright file="GoToViewOrViewModelCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using EnvDTE;

// ReSharper disable HeapView.BoxingAllocation

namespace MvvmTools.GoToViewOrViewModel
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GoToViewOrViewModelCommand : BaseCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid MenuGroup = new Guid("a244c5bf-b5d1-471b-9189-507dd1c78957");

        //private readonly MvvmToolsPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToViewOrViewModelCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        internal GoToViewOrViewModelCommand(MvvmToolsPackage package)
            : base(package, new CommandID(MenuGroup, CommandId))
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GoToViewOrViewModelCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(MvvmToolsPackage package)
        {
            Instance = new GoToViewOrViewModelCommand(package);
        }

        protected override void OnExecute()
        {
            base.OnExecute();

            if (Package.ActiveDocument?.ProjectItem != null)
            {
                //string classes = String.Empty;
                //var classesInFile = GetClassesInProjectItem(Package.ActiveDocument.ProjectItem);
                //foreach (var c in classesInFile)
                //    classes += c + "\n";

                //var docs = GetRelatedDocuments(Package.ActiveDocument.ProjectItem);
                //var ds = String.Empty;
                //foreach (var d in docs)
                //    ds += d.Type + " in " + d.ProjectItem.Name + "'\n";

                //MessageBox.Show(string.Format("Name: {0}\nFull Name: {1}\nClasses: {2}\nCandidates: {3}",
                //    Package.ActiveDocument.Name,
                //    Package.ActiveDocument.FullName,
                //    classes,
                //    ds));

                var docs = GetRelatedDocuments(Package.ActiveDocument.ProjectItem);
                
                // For now we just open the first document.  Need to build a UI to choose.
                if (docs.Count > 0)
                {
                    var win = docs[0].ProjectItem.Open();
                    win.Visible = true;
                    win.Activate();
                }
            }
        }

        private List<string> GetClassesInProjectItem(ProjectItem pi)
        {
            var rval = new List<string>();

            if (pi.Name == null)
                return rval;

            var isXaml = pi.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase);

            if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase) &&
                !isXaml)
                return rval;

            // If has children, that is the source file, check that instead.
            if (pi.ProjectItems != null && pi.ProjectItems.Count != 0)
                foreach (ProjectItem p in pi.ProjectItems)
                    pi = p;

            // If not a part of a project or not compiled, code model will be empty 
            // and there's nothing we can do.
            if (pi?.FileCodeModel == null)
                return rval;

            FileCodeModel fileCm = pi.FileCodeModel;
            CodeElements elts = null;
            elts = fileCm.CodeElements;
            CodeElement elt = null;
            int i = 0;
            //MessageBox.Show("about to walk top-level code elements ...");
            for (i = 1; i <= fileCm.CodeElements.Count; i++)
            {
                elt = elts.Item(i);
                CollapseElt(rval, elt, elts, i, isXaml);

                // If a xaml.cs code behind file, the first class must be the view type, so we can stop early.
                if (isXaml && rval.Count > 0)
                    break;
            }

            return rval;
        }

        private void CollapseElt(List<string> classes, CodeElement elt, CodeElements elts, long loc, bool getFirstClassOnly)
        {
            EditPoint epStart = null;
            EditPoint epEnd = null;
            epStart = elt.StartPoint.CreateEditPoint();
            // Do this because we move it later.
            epEnd = elt.EndPoint.CreateEditPoint();
            epStart.EndOfLine();
            if (((elt.IsCodeType) && (elt.Kind == vsCMElement.vsCMElementClass)))
            {
                CodeClass ct = null;
                ct = ((CodeClass)(elt));
                classes.Add(ct.Name);

                //CodeElements mems = null;
                //mems = ct.Members;
                //int i = 0;
                //for (i = 1; i <= ct.Members.Count; i++)
                //{
                //    CollapseElt(mems.Item(i), mems, i);
                //}
            }
            else if ((elt.Kind == vsCMElement.vsCMElementNamespace))
            {
                //MessageBox.Show("got a namespace, named: " + elt.Name);
                CodeNamespace cns = null;
                cns = ((CodeNamespace)(elt));
                //MessageBox.Show("set cns = elt, named: " + cns.Name);

                CodeElements mems_vb = null;
                mems_vb = cns.Members;
                //MessageBox.Show("got cns.members");
                int i = 0;

                // Loop through child classes.
                for (i = 1; i <= cns.Members.Count; i++)
                {
                    CollapseElt(classes, mems_vb.Item(i), mems_vb, i, getFirstClassOnly);
                    
                    // If a xaml.cs code behind file, the first class must be the view type, so we can stop early.
                    if (getFirstClassOnly && classes.Count > 0)
                        return;
                }
            }
        }

        private static readonly string[] ViewSuffixes = { "View", "Flyout", "UserControl", "Page", "Window" };

        public List<ProjectItemAndType> GetRelatedDocuments(ProjectItem pi)
        {
            var rval = new List<ProjectItemAndType>();

            var typeNamesInFile = GetClassesInProjectItem(pi);

            if (typeNamesInFile.Count == 0)
                return rval;

            var candidateTypeNames = GetTypeCandidates(typeNamesInFile);

            var documents = new List<ProjectItemAndType>();

            // Look for the candidate types in current project only.
            documents = FindDocumentsContainingTypes(
                Package.ActiveDocument.ProjectItem.ContainingProject, candidateTypeNames);

            // If that fails, look through all projects in the solution.
            if (!documents.Any())
            {
                var solution = this.Package.ActiveDocument.DTE?.Solution;
                if (solution != null)
                {
                    foreach (Project project in solution.Projects)
                    {
                        if (project == Package.ActiveDocument.ProjectItem.ContainingProject)
                            continue;

                        var docs = FindDocumentsContainingTypes(project, candidateTypeNames);
                        documents.AddRange(docs);
                    }
                }
            }

            rval = documents.Distinct(new ProjectItemAndTypeEqualityComparer()).ToList();

            return rval;
        }

        private List<string> GetTypeCandidates(IEnumerable<string> typeNamesInFile)
        {
            var candidates = new List<string>();

            // For each type name in the file, create a list of candidates.
            foreach (var typeName in typeNamesInFile)
            {
                // If a view model...
                if (typeName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
                {
                    // Remove ViewModel from end and add all the possible suffixes.
                    var baseName = typeName.Substring(0, typeName.Length - 9);
                    foreach (var suffix in ViewSuffixes)
                    {
                        var candidate = baseName + suffix;
                        candidates.Add(candidate);
                    }

                    // Add base if it ends in one of the view suffixes.
                    foreach (var suffix in ViewSuffixes)
                        if (baseName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        {
                            candidates.Add(baseName);
                            break;
                        }
                }

                foreach (var suffix in ViewSuffixes)
                {
                    if (typeName.EndsWith(suffix))
                    {
                        // Remove suffix and add ViewModel.
                        var baseName = typeName.Substring(0, typeName.Length - suffix.Length);
                        var candidate = baseName + "ViewModel";
                        candidates.Add(candidate);

                        // Just add ViewModel
                        candidate = typeName + "ViewModel";
                        candidates.Add(candidate);
                    }
                }
            }

            return candidates;
        }

        private List<ProjectItemAndType> FindDocumentsContainingTypes(Project project, List<string> typesToFind)
        {
            var results = new List<ProjectItemAndType>();

            FindDocumentsContainingTypesRecursive(project.ProjectItems, typesToFind, null, results);

            return results;
        }

        private void FindDocumentsContainingTypesRecursive(ProjectItems projectItems, List<string> typesToFind, ProjectItem parentProjectItem, List<ProjectItemAndType> results)
        {
            if (typesToFind.Count == 0 || projectItems == null)
                return;

            var tmpResults = new List<ProjectItemAndType>();

            foreach (ProjectItem pi in projectItems)
            {
                // Exclude the document we're on.
                if (pi == Package.ActiveDocument.ProjectItem)
                    continue;

                // Recursive call
                if (pi.Name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                    FindDocumentsContainingTypesRecursive(pi.ProjectItems, typesToFind, pi, tmpResults);
                else
                    FindDocumentsContainingTypesRecursive(pi.ProjectItems ?? pi.SubProject?.ProjectItems, typesToFind,
                        null, tmpResults);

                // Only search source files.
                if (!pi.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                    !pi.Name.EndsWith(".vb", StringComparison.OrdinalIgnoreCase))
                    continue;

                var classesInProjectItem = GetClassesInProjectItem(pi);

                foreach (var c in classesInProjectItem)
                {
                    if (typesToFind.Contains(c, StringComparer.OrdinalIgnoreCase))
                    {
                        if (parentProjectItem != null)
                        {
                            // Parent is the xaml file corresponding to this xaml.cs.  We save it as a result
                            // but don't save the matching class because that would look odd, and later we will
                            // call Distinct() on the entire result set, removing any extra .XAML results 
                            // because they all have null for the type.
                            tmpResults.Add(new ProjectItemAndType(parentProjectItem, null));
                        }

                        tmpResults.Add(new ProjectItemAndType(pi, c));
                    }
                }
            }

            //// Check for associated designer files.
            //foreach (var d in tmpResults)
            //{
            //    if (d.ProjectItem.Name.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            //    {
            //        // Switch to the XAML file.
            //        var xamlFile = d.ProjectItem.Name.Substring(0, d.ProjectItem.Name.Length - ".cs".Length);
            //        foreach (ProjectItem pi in projectItems)
            //            if (string.Equals(pi.Name, xamlFile, StringComparison.OrdinalIgnoreCase))
            //            {
            //                d.ProjectItem = pi;
            //                break;
            //            }
            //    }
            //}

            results.AddRange(tmpResults);
        }

        internal class ProjectItemAndType
        {
            public ProjectItemAndType(ProjectItem projectItem, string type)
            {
                ProjectItem = projectItem;
                Type = type;
            }

            public ProjectItem ProjectItem { get; set; }
            public string Type { get; set; }
        }

        public class ProjectItemAndTypeEqualityComparer : IEqualityComparer<ProjectItemAndType>
        {
            public bool Equals(ProjectItemAndType x, ProjectItemAndType y)
            {
                return x.ProjectItem.Name == y.ProjectItem.Name &&
                       x.Type == y.Type;
            }

            public int GetHashCode(ProjectItemAndType obj)
            {
                return (obj.ProjectItem.Name + ";" + obj.Type).GetHashCode();
            }
        }
    }
}
