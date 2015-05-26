//------------------------------------------------------------------------------
// <copyright file="GoToViewOrViewModelCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MvvmTools.Utilities;
using MvvmTools.ViewModels;
using MvvmTools.Views;

// ReSharper disable HeapView.BoxingAllocation

namespace MvvmTools.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GoToViewOrViewModelCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoToViewOrViewModelCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        internal GoToViewOrViewModelCommand(MvvmToolsPackage package)
            : base(package, Constants.GoToViewOrViewModelCommandId)
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
                var classesInFile = SolutionUtilities.GetClassesInProjectItem(Package.ActiveDocument.ProjectItem);

                if (classesInFile.Count == 0)
                {
                    MessageBox.Show("No classes found in file.", "MVVM Tools");
                    return;
                }

                var docs = SolutionUtilities.GetRelatedDocuments(Package.ActiveDocument.ProjectItem, classesInFile.Select(c => c.Class));

                if (docs.Count == 0)
                {
                    string classes = "\n        ";
                    foreach (var c in classesInFile)
                        classes += c.Class + "\n        ";

                    MessageBox.Show(string.Format("Couldn't find any matching view or view model classes.\n\nClasses found in this file ({0}):\n{1}",
                        Package.ActiveDocument.FullName,
                        classes), "MVVM Tools");

                    return;
                }

                if (docs.Count == 1)
                {
                    var win = docs[0].ProjectItem.Open();
                    win.Visible = true;
                    win.Activate();

                    return;
                }

                // Multiple results, let user choose.
                PresentViewViewModelOptions(docs);
            }
        }

        private void PresentViewViewModelOptions(List<ProjectItemAndType> docs)
        {
            var window = new SelectFileDialog();
            var vm = new SelectFileDialogViewModel(docs);
            window.DataContext = vm;

            var result = window.ShowDialog();

            if (result.GetValueOrDefault())
            {
                // Go to the selected project item.
                var win = vm.SelectedDocument.ProjectItem.Open();
                win.Visible = true;
                win.Activate();
            }
        }
    }
}
