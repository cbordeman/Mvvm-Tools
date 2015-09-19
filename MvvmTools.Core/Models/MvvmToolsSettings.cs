using System.Collections.Generic;
using MvvmTools.Core.Services;

namespace MvvmTools.Core.Models
{
    /// <summary>
    /// This class contains the entire Mvvm Tools configuration settings.
    /// </summary>
    public class MvvmToolsSettings
    {
        public MvvmToolsSettings()
        {
            // Set default values.
            GoToViewOrViewModelOption = GoToViewOrViewModelOption.ShowUi;
            ProjectOptions = new List<ProjectOptions>();

            ViewSuffixes = SettingsService.DefaultViewSuffixes;
        }

        public GoToViewOrViewModelOption GoToViewOrViewModelOption { get; set; }
        public bool GoToViewOrViewModelSearchSolution { get; set; }

        public string[] ViewSuffixes { get; set; }
        
        // Configuration settings for the solutions.
        public ProjectOptions SolutionOptions { get; set; }

        // Where the user's local templates are stored.
        public string LocalTemplateFolder { get; set; }

        // Contains the list of configuration settings for the projects.
        public IList<ProjectOptions> ProjectOptions { get; set; }
        
        public IList<Template> FactoryTemplates { get; set; }
        public IList<Template> LocalTemplates { get; set; }
    }
}