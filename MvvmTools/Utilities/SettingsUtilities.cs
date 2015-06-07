using System;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace MvvmTools.Utilities
{
    public static class SettingsUtilities
    {
        private const string SettingsName = "MvvmToolsSettings";
        private const string GoToViewOrViewModelOptionPropertyName = "GoToViewOrViewModelOption";

        private static IComponentModel _componentModel;
        private static SVsServiceProvider _vsServiceProvider;
        private static ShellSettingsManager _shellSettingsManager;
        private static WritableSettingsStore _userSettingsStore;

        static SettingsUtilities()
        {
            _componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            _vsServiceProvider = _componentModel.GetService<SVsServiceProvider>();
            _shellSettingsManager = new ShellSettingsManager(_vsServiceProvider);
            _userSettingsStore = _shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        public static MvvmToolsSettings LoadSettings()
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

        private static T GetEnum<T>(string settingName)
        {
            var setting = _userSettingsStore.GetString(SettingsName, settingName);
            var rval = (T)Enum.Parse(typeof(T), setting);
            return rval;
        }

        private static void SetEnum<T>(string settingName, T val)
        {
            _userSettingsStore.SetString(SettingsName, settingName, val.ToString());
        }

        public static void SaveSettings(MvvmToolsSettings settings)
        {
            if (!_userSettingsStore.CollectionExists(SettingsName))
                _userSettingsStore.CreateCollection(SettingsName);

            SetEnum(GoToViewOrViewModelOptionPropertyName, settings.GoToViewOrViewModelOption);
        }
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
