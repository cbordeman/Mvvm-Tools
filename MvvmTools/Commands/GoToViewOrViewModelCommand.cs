using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.TextManager.Interop;
using MvvmTools.Commands.GoToVM;
using MvvmTools.Models;
using MvvmTools.Services;
using MvvmTools.ViewModels;
using MvvmTools.Views;
using Unity;

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
        public GoToViewOrViewModelCommand(IUnityContainer container)
            : base(Constants.GoToViewOrViewModelCommandId, container)
        {
            SolutionService = container.Resolve<ISolutionService>();
        }

        public ISolutionService SolutionService { get; set; }

        protected override async Task OnExecuteAsync()
        {
            try
            {
                await base.OnExecuteAsync().ConfigureAwait(false);

                var pi = Package.ActiveDocument?.ProjectItem;

                if (pi != null)
                {
                    var classesInFile = SolutionService.GetClassesInProjectItemUsingCodeDom(pi);

                    if (classesInFile.Count == 0)
                    {
                        MessageBox.Show("No classes found in file.", "MVVM Tools");
                        return;
                    }

                    var settings = await SettingsService.LoadSettings().ConfigureAwait(true);

                    // Solution not fully loaded so settings not loaded either.
                    if (settings?.SolutionOptions == null)
                        return;

                    List<ProjectItemAndType> docs;

                    if (!settings.GoToViewOrViewModelSearchSolution)
                    {
                        // ProjectModel from which to derive initial settings.
                        var settingsPm =
                            settings.ProjectOptions.FirstOrDefault(
                                p => p.ProjectModel.ProjectIdentifier == pi.ContainingProject.UniqueName);

                        // This shouldn't be null unless the user adds a new project and then
                        // quickly invokes this command, but better to check it.
                        if (settingsPm == null)
                            settingsPm = settings.SolutionOptions;

                        var viewModelLocationOptions = new LocationDescriptor
                        {
                            Namespace = settingsPm.ViewModelLocation.Namespace,
                            PathOffProject = settingsPm.ViewModelLocation.PathOffProject,
                            ProjectIdentifier = settingsPm.ViewModelLocation.ProjectIdentifier ?? pi.ContainingProject.UniqueName
                        };

                        var viewLocationOptions = new LocationDescriptor()
                        {
                            Namespace = settingsPm.ViewLocation.Namespace,
                            PathOffProject = settingsPm.ViewLocation.PathOffProject,
                            ProjectIdentifier = settingsPm.ViewLocation.ProjectIdentifier ?? pi.ContainingProject.UniqueName
                        };

                        docs = SolutionService.GetRelatedDocumentsUsingCodeDom(
                            viewModelLocationOptions,
                            viewLocationOptions,
                            pi,
                            classesInFile.Select(c => c.Class),
                            new[] { "uc" },
                            settings.ViewSuffixes,
                            settingsPm.ViewModelSuffix).ToList();
                    }
                    else
                    {
                        // Searches the entire solution.
                        docs = (await SolutionService.GetRelatedDocumentsUsingRoslyn(
                            pi,
                            classesInFile.Select(c => c.Class),
                            new[] { "uc" },
                            settings.ViewSuffixes,
                            settings.SolutionOptions.ViewModelSuffix).ConfigureAwait(true)).ToList();
                    }

                    if (!docs.Any())
                    {
                        string classes = "\n        ";
                        foreach (var c in classesInFile)
                            classes += c.Class + "\n        ";

                        MessageBox.Show(
                            $"Couldn't find any matching views or view models.\n\nClasses in this file:\n\n{classes}", "MVVM Tools");

                        return;
                    }

                    if (docs.Count == 1 || settings.GoToViewOrViewModelOption == GoToViewOrViewModelOption.ChooseFirst)
                    {
                        docs.First().Open();
                        return;
                    }

                    // Multiple results.
                    if (settings.GoToViewOrViewModelOption == GoToViewOrViewModelOption.ShowUi)
                    {
                        PresentViewViewModelOptions(docs);
                        return;
                    }

                    // If there are more than one .xaml files or there are more than one code
                    // behind files, then we must show the UI.
                    var countXaml = docs.Count(d => d.Filename.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase));
                    if (countXaml > 1)
                    {
                        PresentViewViewModelOptions(docs);
                        return;
                    }
                    var countCodeBehind = docs.Count(d => d.Filename.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase) ||
                                                          d.Filename.EndsWith(".xaml.vb", StringComparison.OrdinalIgnoreCase));
                    if (countCodeBehind > 1)
                    {
                        PresentViewViewModelOptions(docs);
                        return;
                    }

                    // If the count of files is > 2 now, then we must show UI.
                    var count = docs.Count;
                    if (count > 2)
                    {
                        PresentViewViewModelOptions(docs);
                        return;
                    }

                    // If the remaining two files are xaml and code behind, we can apply the 
                    // 'prefer xaml' or 'prefer code behind' setting.
                    if (string.Compare(docs[0].Filename, docs[1].Filename + ".cs", StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(docs[0].Filename, docs[1].Filename + ".vb", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // First file is code behind, second is XAML.
                        if (settings.GoToViewOrViewModelOption == GoToViewOrViewModelOption.ChooseCodeBehind)
                            //await (new JoinableTaskFactory(null)).SwitchToMainThreadAsync();
                            docs[0].Open();
                        else
                            docs[1].Open();
                    }
                    else if (string.Compare(docs[1].Filename, docs[0].Filename + ".cs", StringComparison.OrdinalIgnoreCase) == 0 ||
                        string.Compare(docs[1].Filename, docs[0].Filename + ".vb", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // First file is XAML, second is code behind.
                        if (settings.GoToViewOrViewModelOption == GoToViewOrViewModelOption.ChooseXaml)
                            docs[0].Open();
                        else
                            docs[1].Open();
                    }
                    else
                    {
                        // The two files are unrelated, must show UI.
                        PresentViewViewModelOptions(docs);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                //throw;
            }
        }

        private void PresentViewViewModelOptions(IEnumerable<ProjectItemAndType> docs)
        {
            var window = new SelectFileDialog();
            var vm = new SelectFileDialogViewModel(docs, Container);
            window.DataContext = vm;

            var result = window.ShowDialog();

            if (result.GetValueOrDefault())
                // Go to the selected project item.
                vm.SelectedDocument.Open();
        }
    }
}
