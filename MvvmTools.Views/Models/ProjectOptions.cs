using Newtonsoft.Json;

namespace MvvmTools.Core.Models
{
    /// <summary>
    /// This class stores all settings pertaining to both the view and view model
    /// configuration of either a solution or a single project.
    /// </summary>
    public class ProjectOptions
    {
        public ProjectOptions()
        {
            ViewModelLocation = new LocationDescriptor();
            ViewLocation = new LocationDescriptor();
        }

        [JsonIgnore]
        public ProjectModel ProjectModel { get; set; }

        public string ViewModelSuffix { get; set; }
        
        public LocationDescriptor ViewModelLocation { get; set; }
        public LocationDescriptor ViewLocation { get; set; }

        internal void ApplyInherited(ProjectOptions inherited)
        {
            ViewModelSuffix = inherited.ViewModelSuffix;

            ViewModelLocation.ApplyInherited(inherited.ViewModelLocation);
            ViewLocation.ApplyInherited(inherited.ViewLocation);
        }

        public bool InheritsFully(ProjectOptions inherited)
        {
            if (ViewModelSuffix != inherited.ViewModelSuffix)
                return false;

            if (!ViewModelLocation.InheritsFully(inherited.ViewModelLocation))
                return false;
            if (!ViewLocation.InheritsFully(inherited.ViewLocation))
                return false;

            return true;
        }
    }
}