using Microsoft.VisualStudio.Settings;
using MvvmTools.Core.Services;

namespace MvvmTools.Core.Models
{
    public class MvvmToolsSettings
    {
        public MvvmToolsSettings()
        {
            // Set default values.
            GoToViewOrViewModelOption = GoToViewOrViewModelOption.ShowUi;
            ScaffoldingOptions = new ScaffoldingOptions();
            ViewSuffixes = SettingsService.DefaultViewSuffixes;
        }

        public GoToViewOrViewModelOption GoToViewOrViewModelOption { get; set; }
        public string ViewModelSuffix { get; set; }
        public string[] ViewSuffixes { get; set; }
        public ScaffoldingOptions ScaffoldingOptions { get; set; }
    }
}