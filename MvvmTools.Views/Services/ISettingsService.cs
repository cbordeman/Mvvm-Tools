using System;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using MvvmTools.Core.Models;

namespace MvvmTools.Core.Services
{
    public interface ISettingsService
    {
        MvvmToolsSettings LoadSettings();
        void SaveSettings(MvvmToolsSettings settings);
    }
    
    public class SettingsService : ISettingsService
    {
        #region Data
        private const string SettingsName = "MvvmToolsSettings";
        private const string GoToViewOrViewModelPropName = "GoToViewOrViewModelOption";
        private const string ScaffoldingViewModelAutoPropName = "ScaffoldingViewModelAuto";
        private const string ScaffoldingViewModelPathOffProjectPropName = "ScaffoldingViewModelPathOffProject";
        private const string ScaffoldingViewModelNamespacePropName = "ScaffoldingViewModelNamespace";
        private const string ScaffoldingViewAutoPropName = "ScaffoldingViewAuto";
        private const string ScaffoldingViewPathOffProjectPropName = "ScaffoldingViewPathOffProject";
        private const string ScaffoldingViewNamespacePropName = "ScaffoldingViewNamespace";

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
            // rval starts out containing default values.
            var rval = new MvvmToolsSettings();

            // Get any saved settings
            if (_userSettingsStore.CollectionExists(SettingsName))
            {
                rval.GoToViewOrViewModelOption = GetEnum(GoToViewOrViewModelPropName, GoToViewOrViewModelOption.ShowUi);
                rval.ScaffoldingOptions.ViewModelDescriptor.Auto = GetBool(ScaffoldingViewModelAutoPropName, true);
                rval.ScaffoldingOptions.ViewModelDescriptor.PathOffProject = GetString(ScaffoldingViewModelPathOffProjectPropName, null);
                rval.ScaffoldingOptions.ViewModelDescriptor.Namespace = GetString(ScaffoldingViewModelNamespacePropName, null);
                rval.ScaffoldingOptions.ViewDescriptor.Auto = GetBool(ScaffoldingViewAutoPropName, true);
                rval.ScaffoldingOptions.ViewDescriptor.PathOffProject = GetString(ScaffoldingViewPathOffProjectPropName, null);
                rval.ScaffoldingOptions.ViewDescriptor.Namespace = GetString(ScaffoldingViewNamespacePropName, null);
            }

            return rval;
        }

        public void SaveSettings(MvvmToolsSettings settings)
        {
            if (!_userSettingsStore.CollectionExists(SettingsName))
                _userSettingsStore.CreateCollection(SettingsName);

            SetEnum(GoToViewOrViewModelPropName, settings.GoToViewOrViewModelOption);
            SetBool(ScaffoldingViewModelAutoPropName, settings.ScaffoldingOptions.ViewModelDescriptor.Auto);
            SetString(ScaffoldingViewModelPathOffProjectPropName, settings.ScaffoldingOptions.ViewModelDescriptor.PathOffProject);
            SetString(ScaffoldingViewModelNamespacePropName, settings.ScaffoldingOptions.ViewModelDescriptor.Namespace);
            SetBool(ScaffoldingViewAutoPropName, settings.ScaffoldingOptions.ViewDescriptor.Auto);
            SetString(ScaffoldingViewPathOffProjectPropName, settings.ScaffoldingOptions.ViewDescriptor.PathOffProject);
            SetString(ScaffoldingViewNamespacePropName, settings.ScaffoldingOptions.ViewDescriptor.Namespace);
        }

        #endregion Public Methods

        #region Private Helpers

        private T GetEnum<T>(string settingName, T defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsName, settingName);
            var rval = (T)Enum.Parse(typeof(T), setting);
            return rval;
        }

        private bool GetBool(string settingName, bool defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsName, settingName);
            var rval = String.Equals(setting, "True", StringComparison.OrdinalIgnoreCase);
            return rval;
        }

        private string GetString(string settingName, string defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsName, settingName);
            return setting;
        }

        private void SetEnum<T>(string settingName, T val)
        {
            _userSettingsStore.SetString(SettingsName, settingName, val.ToString());
        }

        private void SetBool(string settingName, bool val)
        {
            _userSettingsStore.SetString(SettingsName, settingName, val.ToString());
        }

        private void SetString(string settingName, string val)
        {
            _userSettingsStore.SetString(SettingsName, settingName, val ?? String.Empty);
        }

        #endregion Private Helpers
    }
}
