using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace MvvmTools.Services
{
    internal interface ISettingsService
    {
        MvvmToolsSettings LoadSettings();
        void SaveSettings(MvvmToolsSettings settings);
    }

    [Export(typeof(ISettingsService))]
    internal class SettingsService : ISettingsService
    {
        #region Data
        private const string SettingsName = "MvvmToolsSettings";
        private const string GoToViewOrViewModelOptionPropertyName = "GoToViewOrViewModelOption";
        
        private readonly WritableSettingsStore _userSettingsStore;

        #endregion Data

        #region Ctor and Init

        public SettingsService(IComponentModel componentModel)
        {
            var vsServiceProvider = componentModel.GetService<SVsServiceProvider>();
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            _userSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        #endregion Ctor and Init

        #region Public Methods

        public MvvmToolsSettings LoadSettings()
        {
            var rval = new MvvmToolsSettings();

            // Get any saved settings
            if (_userSettingsStore.CollectionExists(SettingsName))
            {
                if (_userSettingsStore.PropertyExists(SettingsName, GoToViewOrViewModelOptionPropertyName))
                    rval.GoToViewOrViewModelOption = GetEnum<GoToViewOrViewModelOption>(GoToViewOrViewModelOptionPropertyName);
            }
            else
            {
                // Set defaults
                rval.GoToViewOrViewModelOption = GoToViewOrViewModelOption.ShowUi;
            }

            return rval;
        }

        public void SaveSettings(MvvmToolsSettings settings)
        {
            if (!_userSettingsStore.CollectionExists(SettingsName))
                _userSettingsStore.CreateCollection(SettingsName);

            SetEnum(GoToViewOrViewModelOptionPropertyName, settings.GoToViewOrViewModelOption);
        }

        #endregion Public Methods

        #region Private Helpers

        private T GetEnum<T>(string settingName)
        {
            var setting = _userSettingsStore.GetString(SettingsName, settingName);
            var rval = (T)Enum.Parse(typeof(T), setting);
            return rval;
        }

        private void SetEnum<T>(string settingName, T val)
        {
            _userSettingsStore.SetString(SettingsName, settingName, val.ToString());
        }

        #endregion Private Helpers
    }

    public class MvvmToolsSettings
    {
        public GoToViewOrViewModelOption GoToViewOrViewModelOption { get; set; }
    }

    public enum GoToViewOrViewModelOption
    {
        ShowUi,
        ChooseXaml,
        ChooseCodeBehind,
        ChooseFirst
    }
}
