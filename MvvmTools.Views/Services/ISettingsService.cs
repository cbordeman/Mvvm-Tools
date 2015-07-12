using System;
using System.Collections.Generic;
using System.Linq;
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

        public const string DefaultViewModelSuffix = "ViewModel";
        public static readonly string[] DefaultViewSuffixes = {"View", "Flyout", "UserControl", "Page", "Window", "Dialog"};
        
        private const string SettingsPropName = nameof(SettingsPropName);

        private const string GoToViewOrViewModelPropName = nameof(GoToViewOrViewModelPropName);

        private const string ViewModelSuffixPropName = nameof(ViewModelSuffixPropName);

        private const string ViewSuffixesPropName = nameof(ViewSuffixesPropName);

        private const string ScaffoldingViewModelAutoPropName = nameof(ScaffoldingViewModelAutoPropName);
        private const string ScaffoldingViewModelProjectIdentifierPropName = nameof(ScaffoldingViewModelProjectIdentifierPropName);
        private const string ScaffoldingViewModelPathOffProjectPropName = nameof(ScaffoldingViewModelPathOffProjectPropName);
        private const string ScaffoldingViewModelNamespacePropName = nameof(ScaffoldingViewModelNamespacePropName);
        private const string ScaffoldingViewModelAppendViewTypePropName = nameof(ScaffoldingViewModelAppendViewTypePropName);

        private const string ScaffoldingViewAutoPropName = nameof(ScaffoldingViewAutoPropName);
        private const string ScaffoldingViewProjectIdentifierPropName = nameof(ScaffoldingViewProjectIdentifierPropName);
        private const string ScaffoldingViewPathOffProjectPropName = nameof(ScaffoldingViewPathOffProjectPropName);
        private const string ScaffoldingViewNamespacePropName = nameof(ScaffoldingViewNamespacePropName);
        private const string ScaffoldingViewAppendViewTypePropName = nameof(ScaffoldingViewAppendViewTypePropName);

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
            if (_userSettingsStore.CollectionExists(SettingsPropName))
            {
                rval.GoToViewOrViewModelOption = GetEnum(GoToViewOrViewModelPropName, GoToViewOrViewModelOption.ShowUi);

                rval.ViewModelSuffix = GetString(ViewModelSuffixPropName, DefaultViewModelSuffix);

                rval.ViewSuffixes = GetStringCollection(ViewSuffixesPropName, DefaultViewSuffixes);

                rval.ScaffoldingOptions.ViewModelLocation.Auto = GetBool(ScaffoldingViewModelAutoPropName, true);
                rval.ScaffoldingOptions.ViewModelLocation.ProjectIdentifier = GetString(ScaffoldingViewModelProjectIdentifierPropName, null);
                rval.ScaffoldingOptions.ViewModelLocation.PathOffProject = GetString(ScaffoldingViewModelPathOffProjectPropName, "ViewModels");
                rval.ScaffoldingOptions.ViewModelLocation.Namespace = GetString(ScaffoldingViewModelNamespacePropName, ".ViewModels");
                rval.ScaffoldingOptions.ViewModelLocation.AppendViewType = GetBool(ScaffoldingViewModelAppendViewTypePropName, true);

                rval.ScaffoldingOptions.ViewLocation.Auto = GetBool(ScaffoldingViewAutoPropName, true);
                rval.ScaffoldingOptions.ViewLocation.ProjectIdentifier = GetString(ScaffoldingViewProjectIdentifierPropName, null);
                rval.ScaffoldingOptions.ViewLocation.PathOffProject = GetString(ScaffoldingViewPathOffProjectPropName, "Views");
                rval.ScaffoldingOptions.ViewLocation.Namespace = GetString(ScaffoldingViewNamespacePropName, ".Views");
                rval.ScaffoldingOptions.ViewLocation.AppendViewType = GetBool(ScaffoldingViewAppendViewTypePropName, true);
            }

            return rval;
        }

        public void SaveSettings(MvvmToolsSettings settings)
        {
            if (!_userSettingsStore.CollectionExists(SettingsPropName))
                _userSettingsStore.CreateCollection(SettingsPropName);

            SetEnum(GoToViewOrViewModelPropName, settings.GoToViewOrViewModelOption);

            SetStringCollection(ViewSuffixesPropName, settings.ViewSuffixes);

            SetBool(ScaffoldingViewModelAutoPropName, settings.ScaffoldingOptions.ViewModelLocation.Auto);
            SetString(ScaffoldingViewModelProjectIdentifierPropName, settings.ScaffoldingOptions.ViewModelLocation.ProjectIdentifier);
            SetString(ScaffoldingViewModelPathOffProjectPropName, settings.ScaffoldingOptions.ViewModelLocation.PathOffProject);
            SetString(ScaffoldingViewModelNamespacePropName, settings.ScaffoldingOptions.ViewModelLocation.Namespace);
            SetBool(ScaffoldingViewModelAppendViewTypePropName, settings.ScaffoldingOptions.ViewModelLocation.AppendViewType);

            SetBool(ScaffoldingViewAutoPropName, settings.ScaffoldingOptions.ViewLocation.Auto);
            SetString(ScaffoldingViewProjectIdentifierPropName, settings.ScaffoldingOptions.ViewLocation.ProjectIdentifier);
            SetString(ScaffoldingViewPathOffProjectPropName, settings.ScaffoldingOptions.ViewLocation.PathOffProject);
            SetString(ScaffoldingViewNamespacePropName, settings.ScaffoldingOptions.ViewLocation.Namespace);
            SetBool(ScaffoldingViewAppendViewTypePropName, settings.ScaffoldingOptions.ViewLocation.AppendViewType);
        }

        #endregion Public Methods

        #region Private Helpers

        private T GetEnum<T>(string settingName, T defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            var rval = (T)Enum.Parse(typeof(T), setting);
            return rval;
        }

        private bool GetBool(string settingName, bool defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            var rval = String.Equals(setting, "True", StringComparison.OrdinalIgnoreCase);
            return rval;
        }

        private string GetString(string settingName, string defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            return setting;
        }

        private string[] GetStringCollection(string settingName, string[] defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            return setting.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim()).ToArray();
        }

        private void SetEnum<T>(string settingName, T val)
        {
            _userSettingsStore.SetString(SettingsPropName, settingName, val.ToString());
        }

        private void SetBool(string settingName, bool val)
        {
            _userSettingsStore.SetString(SettingsPropName, settingName, val.ToString());
        }

        private void SetString(string settingName, string val)
        {
            _userSettingsStore.SetString(SettingsPropName, settingName, val ?? String.Empty);
        }

        private void SetStringCollection(string settingName, IEnumerable<string> val)
        {
            var concatenated = String.Join(", ", val);
            _userSettingsStore.SetString(SettingsPropName, settingName, concatenated);
        }

        #endregion Private Helpers
    }
}
