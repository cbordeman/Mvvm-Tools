using System.Collections.Generic;
using Controls.ViewModels;
using MvvmTools.Services;

namespace MvvmTools.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private MvvmToolsSettings _checkpointedSettings;

        internal OptionsViewModel(MvvmToolsSettings checkpointedSettings)
        {
            _checkpointedSettings = checkpointedSettings;

            GoToViewOrViewModelOptions = new List<ValueDescriptor<GoToViewOrViewModelOption>>()
            {
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ShowUi, "Ask"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseXaml, "If view, open the XAML"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseCodeBehind, "If view, open the code behind"),
                new ValueDescriptor<GoToViewOrViewModelOption>(GoToViewOrViewModelOption.ChooseFirst, "Always open the first item found")
            };

            // This actually applies the settings from _checkpointedSettings to the properties.
            RevertSettings();
        }

        #region GoToViewOrViewModelOptions
        private List<ValueDescriptor<GoToViewOrViewModelOption>> _goToViewOrViewModelOptions;
        public List<ValueDescriptor<GoToViewOrViewModelOption>> GoToViewOrViewModelOptions
        {
            get { return _goToViewOrViewModelOptions; }
            set { SetProperty(ref _goToViewOrViewModelOptions, value); }
        }
        #endregion GoToViewOrViewModelOptions

        #region SelectedGoToViewOrViewModelOption
        private GoToViewOrViewModelOption _selectedGoToViewOrViewModelOption;
        public GoToViewOrViewModelOption SelectedGoToViewOrViewModelOption
        {
            get { return _selectedGoToViewOrViewModelOption; }
            set { SetProperty(ref _selectedGoToViewOrViewModelOption, value); }
        }
        #endregion SelectedGoToViewOrViewModelOption

        internal MvvmToolsSettings GetCurrentSettings()
        {
            // Extracts settings from view model properties.  These are considered the 
            // 'current' settings, while the original settings values are store in
            // _checkpointedSettings.
            var settings = new MvvmToolsSettings
            {
                GoToViewOrViewModelOption = SelectedGoToViewOrViewModelOption
            };
            return settings;
        }

        public void RevertSettings()
        {
            // Reverts properties to the values saved in _checkpointedSettings.  This
            // is used to cancel changes.
            this.SelectedGoToViewOrViewModelOption = _checkpointedSettings.GoToViewOrViewModelOption;
        }

        public void CheckpointSettings()
        {
            // Saves the view model properties into _checkpointedSettings so that future calls
            // to RevertSettings() can go back to this point.  This method is typically called 
            // when the user hits Apply or OK on the settings window. 
            _checkpointedSettings = GetCurrentSettings();
        }
    }

    public class ValueDescriptor<T>
    {
        public ValueDescriptor(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public T Value { get; set; }
        public string Description { get; set; }
    }

    
}
