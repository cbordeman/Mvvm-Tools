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
        }

        public GoToViewOrViewModelOption GoToViewOrViewModelOption { get; set; }
        public ScaffoldingOptions ScaffoldingOptions { get; set; }
    }
}