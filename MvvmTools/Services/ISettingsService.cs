using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using MvvmTools.Models;
using Newtonsoft.Json;

namespace MvvmTools.Services
{
    public interface ISettingsService
    {
        Task<MvvmToolsSettings> LoadSettings();
        void SaveSettings(MvvmToolsSettings settings);
        string DefaultLocalTemplateFolder { get; }
    }

    public class SettingsService : ISettingsService
    {
        #region Data

        private readonly ISolutionService _solutionService;
        private readonly IDialogService _dialogService;

        // The file extension attached to a solution or project filename to store settings.
        // It can't be changed.
        public const string SettingsFileExtension = ".MVVMSettings";

        // Visual Studio's store name for the extension's global properties.
        private const string SettingsPropName = "MVVMTools_Settings";

        // Name of the global settings within Visual Studio's store.
        private const string GoToViewOrViewModelPropName = "GoToViewOrViewModel";
        private const string GoToViewOrViewModelSearchSolutionPropName = "GoToViewOrViewModelSearchSolution";
        private const string ViewSuffixesPropName = "ViewSuffixes";
        private const string LocalTemplateFolderPropName = "LocalTemplateFolder";
        
        // Factory templates.
        
        // The physical store used by Visual Studio to save settings, instead of app.config,
        // which is not supported in VSIX.
        private readonly WritableSettingsStore _userSettingsStore;

        // These are the global view suffixes, but they can be overridden.
        public static readonly string[] DefaultViewSuffixes;

        // These are the defaults used for new solutions.
        public static readonly ProjectOptions SolutionDefaultProjectOptions;

        //private static readonly string TempFolder = Path.GetTempPath();
        
        #endregion Data

        #region Ctor and Init

        static SettingsService()
        {
            DefaultViewSuffixes = new [] { "", "View", "Flyout", "UserControl", "Page", "Window", "Dialog" };
            
            SolutionDefaultProjectOptions = new ProjectOptions
            {
                ViewModelSuffix = "ViewModel",
                ViewModelLocation = new LocationDescriptor
                {
                    Namespace = ".ViewModels",
                    PathOffProject = "ViewModels",
                    ProjectIdentifier = null
                },
                ViewLocation = new LocationDescriptor
                {
                    Namespace = ".Views",
                    PathOffProject = "Views",
                    ProjectIdentifier = null
                }
            };
        }

        public SettingsService(IComponentModel componentModel, ISolutionService solutionService,
            IDialogService dialogService)
        {
            _solutionService = solutionService;
            _dialogService = dialogService;
            var vsServiceProvider = componentModel.GetService<SVsServiceProvider>();
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            _userSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            // Get default local template folder, which is the user's My docs\mvvm tools\templates.
            var mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DefaultLocalTemplateFolder = Path.Combine(mydocs, "MVVM Tools\\Templates");
        }

        #endregion Ctor and Init

        #region Properties

        public ISolutionService SolutionService { get; set; }

        public string DefaultLocalTemplateFolder { get; }

        #endregion Properties

        #region Public Methods

        public ProjectOptions GetProjectOptionsFromSettingsFile(ProjectModel projectModel, ProjectOptions inheritedProjectOptions)
        {
             if (!File.Exists(projectModel.SettingsFile))
            {
                var po = new ProjectOptions {ProjectModel = projectModel};
                po.ApplyInherited(inheritedProjectOptions);
                return po;
            }

            try
            {
                var text = File.ReadAllText(projectModel.SettingsFile);

                // Find first empty line.
                var startPosition = text.IndexOf("\r\n\r\n", StringComparison.OrdinalIgnoreCase);
                if (startPosition == -1)
                    return null;

                var json = text.Substring(startPosition + 4);

                var projectOptions = JsonConvert.DeserializeObject<ProjectOptions>(json);
                // .ProjectModel is excluded from serialization so we set it now.
                projectOptions.ProjectModel = projectModel;

                return projectOptions;
            }
            catch (Exception ex)
            {
                // Can't read or deserialize the file for any reason.
                Trace.WriteLine($"Couldn't read file {projectModel.SettingsFile}.  Error: {ex.Message}");
                var po = new ProjectOptions { ProjectModel = projectModel };
                po.ApplyInherited(inheritedProjectOptions);
                return po;
            }
        }

        /// <summary>
        /// This is async because it must wait for the solution to finish its operation(s).
        /// </summary>
        /// <returns></returns>
        public async Task<MvvmToolsSettings> LoadSettings()
        {
            // rval starts out containing default values, exceptLocalTemplateFolder, which is null
            // so we use our DefaultLocalTemplateFolder, which is a subfolder off My Docs.
            var rval = new MvvmToolsSettings
            {
                GoToViewOrViewModelOption = GetEnum(GoToViewOrViewModelPropName, GoToViewOrViewModelOption.ShowUi),
                GoToViewOrViewModelSearchSolution = GetBool(GoToViewOrViewModelSearchSolutionPropName, true),
                ViewSuffixes = GetStringCollection(ViewSuffixesPropName, DefaultViewSuffixes),
                LocalTemplateFolder = GetString(LocalTemplateFolderPropName, DefaultLocalTemplateFolder)
            };

            // Get any saved settings

            // Make sure the LocalTemplateFolder setting exists in our saved values.  It might not
            // because the setting is being introduced in version 0.5 of MVVM Tools.
            if (string.IsNullOrWhiteSpace(rval.LocalTemplateFolder))
                rval.LocalTemplateFolder = DefaultLocalTemplateFolder;

            try
            {
                if (!Directory.Exists(rval.LocalTemplateFolder))
                    // This creates all the intermediate folders as well, if they don't exist.
                    Directory.CreateDirectory(rval.LocalTemplateFolder);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"In {nameof(SettingsService)}.{nameof(LoadSettings)}(), failed to check existence of or create local template folder.  Value: {rval.LocalTemplateFolder}" + Environment.NewLine + ex);
                await _dialogService.ShowMessage("Error",
                    "Couldn't read or create the local template folder, which was:" + Environment.NewLine +
                    Environment.NewLine + "\t" + rval.LocalTemplateFolder + Environment.NewLine + ex);
                throw;
            }
            
            // Get solution's structure.  Waits for solution operations to complete.
            var solution = await _solutionService.GetSolution();
            if (solution == null)
                return rval;

            // Get options for solution.  Apply global defaults to solution 
            // file if it doesn't exist.
            var solutionOptions = GetProjectOptionsFromSettingsFile(solution, SolutionDefaultProjectOptions);
            rval.SolutionOptions = solutionOptions;

            // Get options for each project.  Inherited solution options are 
            // applied for each project file if it doesn't exist.
            AddProjectOptionsFlattenedRecursive(solutionOptions, rval.ProjectOptions, solution.Children);

            return rval;
        }

        private void AddProjectOptionsFlattenedRecursive(ProjectOptions inherited, ICollection<ProjectOptions> projectOptionsCollection, IEnumerable<ProjectModel> solutionTree, string prefix = null)
        {
            foreach (var p in solutionTree)
            {
                switch (p.Kind)
                {
                    case ProjectKind.Project:
                        var projectModel = new ProjectModel(
                            prefix + p.Name,
                            p.FullPath,
                            p.ProjectIdentifier, 
                            p.Kind,
                            p.KindId,
                            p.RootNamespace);
                        var projectOptions = GetProjectOptionsFromSettingsFile(projectModel, inherited);
                        projectOptionsCollection.Add(projectOptions);

                        break;
                    case ProjectKind.ProjectFolder:
                        return;
                }

                // The recursive call.
                AddProjectOptionsFlattenedRecursive(
                    inherited,
                    projectOptionsCollection,
                    p.Children,
                    prefix + p.Name + '\\');
            }
        }

        public void SaveSettings([NotNull] MvvmToolsSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            if (!_userSettingsStore.CollectionExists(SettingsPropName))
                _userSettingsStore.CreateCollection(SettingsPropName);

            SetEnum(GoToViewOrViewModelPropName, settings.GoToViewOrViewModelOption);
            SetBool(GoToViewOrViewModelSearchSolutionPropName, settings.GoToViewOrViewModelSearchSolution);
            SetStringCollection(ViewSuffixesPropName, settings.ViewSuffixes);
            SetString(LocalTemplateFolderPropName, settings.LocalTemplateFolder);

            // If a solution is loaded...
            if (settings.SolutionOptions != null)
            {
                // Save solution and project option files, or delete them if they 
                // match the inherited values.

                SaveOrDeleteSettingsFile(settings.SolutionOptions, SolutionDefaultProjectOptions);
                foreach (var p in settings.ProjectOptions)
                    if (!string.IsNullOrWhiteSpace(p?.ProjectModel?.SettingsFile))
                        SaveOrDeleteSettingsFile(p, settings.SolutionOptions);
            }
        }

        private void SaveOrDeleteSettingsFile(
            [NotNull] ProjectOptions projectOptions,
            [NotNull] ProjectOptions inherited)
        {
            if (projectOptions == null)
                throw new ArgumentNullException(nameof(projectOptions));
            if (inherited == null)
                throw new ArgumentNullException(nameof(inherited));
            
            // If settings matches inheritied, delete the settings file.  Otherwise,
            // save it alongside the project or solution file.
            
            if (projectOptions.InheritsFully(inherited))
                DeleteFileIfExists(projectOptions.ProjectModel.SettingsFile);
            else
                SaveProjectOptionsToSettingsFile(projectOptions);
        }

        private void DeleteFileIfExists([NotNull] string fn)
        {
            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            if (File.Exists(fn))
            {
                try
                {
                    //SourceControlUtilities.EnsureCheckedOutIfExists(fn);

                    File.Delete(fn);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Couldn't delete {fn}.  Error: {ex.Message}");
                }
            }
        }

        public void SaveProjectOptionsToSettingsFile(ProjectOptions projectOptions)
        {
            try
            {
                const string header =
                "# This is a settings file generated by the MVVM Tools Visual Studio extension.  A\r\n" +
                "# similar file is generated and stored alongside your solution file and each project\r\n" +
                "# file.\r\n" +
                "# \r\n" +
                "# Please DO NOT modify this file directly.  Instead, open a solution and open\r\n" +
                "# Tools => Options => MVVM Tools, where all these MVVM settings files are manipulated.\r\n" +
                "# \r\n" +
                "# The MVVM Tools extension is available via\r\n" +
                "# Tools => Extensions and Updates, or at\r\n" +
                "# https://visualstudiogallery.msdn.microsoft.com/978ed555-9f0d-44a2-884c-9084844ac469\r\n" +
                "# The source is available on GitHub at https://github.com/cbordeman/Mvvm-Tools/issues  if\r\n" +
                "# you'd like to contribute!\r\n" +
                "# \r\n" +
                "# Enjoy!\r\n" +
                "\r\n";

                var text = header + JsonConvert.SerializeObject(projectOptions, Formatting.Indented);

                // Read original text so we don't overwrite an unchanged file.
                bool textMatches = false;
                if (File.Exists(projectOptions.ProjectModel.SettingsFile))
                {
                    var existingText = File.ReadAllText(projectOptions.ProjectModel.SettingsFile);
                    if (text == existingText)
                        textMatches = true;
                }
                if (!textMatches)
                    File.WriteAllText(projectOptions.ProjectModel.SettingsFile, text);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Couldn't save {projectOptions.ProjectModel?.SettingsFile}.  Error: {ex.Message}");
                throw;
            }
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

        // ReSharper disable once UnusedMember.Local
        private int GetInt32(string settingName, int defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            try
            {
                return Convert.ToInt32(setting);
            }
            catch
            {
                return defaultValue;
            }
        }

        private string[] GetStringCollection(string settingName, string[] defaultValue)
        {
            if (!_userSettingsStore.PropertyExists(SettingsPropName, settingName))
                return defaultValue;

            var setting = _userSettingsStore.GetString(SettingsPropName, settingName);
            return setting.Split(',').Select(s=>s.Trim()).ToArray();
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

        // ReSharper disable once UnusedMember.Local
        private void SetInt32(string settingName, int val)
        {
            _userSettingsStore.SetString(SettingsPropName, settingName, val.ToString());
        }

        private void SetStringCollection(string settingName, IEnumerable<string> val)
        {
            var concatenated = String.Join(", ", val);
            _userSettingsStore.SetString(SettingsPropName, settingName, concatenated);
        }

        #endregion Private Helpers
    }
}
