using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using MvvmTools.Core.Models;
using Newtonsoft.Json;
using Ninject;
using Task = System.Threading.Tasks.Task;

namespace MvvmTools.Core.Services
{
    public interface ISettingsService
    {
        Task<MvvmToolsSettings> LoadSettings();
        void SaveSettings(MvvmToolsSettings settings);
        string RefreshToken { get; set; }
    }

    public struct OneDriveSource
    {
        public OneDriveSource(string userId, string folder)
        {
            UserId = userId;
            Folder = folder;
        }

        public string UserId;
        public string Folder;
    }
    
    public class SettingsService : ISettingsService
    {
        #region Data

        // The file extension attached to a solution or project filename to store settings.
        // It can't be changed.
        public const string SettingsFileExtension = ".MVVMSettings";

        // Visual Studio's store name for the extension's global properties.
        private const string SettingsPropName = "MVVMTools_Settings";

        // Name of the global settings within Visual Studio's store.
        private const string GoToViewOrViewModelPropName = "GoToViewOrViewModel";
        private const string GoToViewOrViewModelSearchSolutionPropName = "GoToViewOrViewModelSearchSolution";
        private const string ViewSuffixesPropName = "ViewSuffixes";

        private readonly string _defaultLocalTemplateFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MVVM Tools\\Templates");
        private const string LocalTemplateFolderPropName = "LocalTemplateFolder";
        private const string CachedTemplateCountPropName = "CachedTemplateCount";
        private const string CachedTemplatePrefix = "CachedTemplate#";

        // Factory templates.
        private readonly OneDriveSource[] _templateFactoryTemplateSources =
        {
            new OneDriveSource("cbordeman", "MvvmTools\\Templates"), 
        };
        
        // The physical store used by Visual Studio to save settings, instead of app.config,
        // which is not supported in VSIX.
        private readonly WritableSettingsStore _userSettingsStore;

        // These are the global view suffixes, but they can be overridden.
        public static readonly string[] DefaultViewSuffixes;

        // These are the defaults used for new solutions.
        public static readonly ProjectOptions SolutionDefaultProjectOptions;

        private static readonly string TempFolder = Path.GetTempPath();

        #endregion Data

        #region Ctor and Init

        static SettingsService()
        {
            DefaultViewSuffixes = new [] { "View", "Flyout", "UserControl", "Page", "Window", "Dialog" };
            
            SolutionDefaultProjectOptions = new ProjectOptions
            {
                ViewModelSuffix = "ViewModel",
                ViewModelLocation = new LocationDescriptor
                {
                    AppendViewType = true,
                    Namespace = ".ViewModels",
                    PathOffProject = "ViewModels",
                    ProjectIdentifier = null
                },
                ViewLocation = new LocationDescriptor
                {
                    AppendViewType = true,
                    Namespace = ".Views",
                    PathOffProject = "Views",
                    ProjectIdentifier = null
                }
            };
        }

        public SettingsService(IComponentModel componentModel, IKernel kernel)
        {
            var vsServiceProvider = componentModel.GetService<SVsServiceProvider>();
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            _userSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public ISolutionService SolutionService { get; set; }

        public string RefreshToken
        {
            get { return GetString(nameof(RefreshToken), null); }
            set { SetString(nameof(RefreshToken), value); }
        }

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

        public async Task<MvvmToolsSettings> LoadSettings()
        {
            // rval starts out containing default values.
            var rval = new MvvmToolsSettings();

            // Get any saved settings
            if (_userSettingsStore.CollectionExists(SettingsPropName))
            {
                rval.GoToViewOrViewModelOption = GetEnum(GoToViewOrViewModelPropName, GoToViewOrViewModelOption.ShowUi);
                rval.GoToViewOrViewModelSearchSolution = GetBool(GoToViewOrViewModelSearchSolutionPropName, true);
                rval.ViewSuffixes = GetStringCollection(ViewSuffixesPropName, DefaultViewSuffixes);
            }

            // Load template urls.
            //LoadTemplates();

            // Get solution's ProjectOptions.
            var solution = await SolutionService.GetSolution();
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

        //private IList<MvvmTemplate> LoadTemplates()
        //{
        //    var rval = new List<MvvmTemplate>();
        //    try
        //    {
        //        var oneDriveTasks = new List<Task>();

        //        // Add onedrive template sources.
        //        foreach (var fs in _templateFactoryTemplateSources)
        //            oneDriveTasks.Add(AddTemplatesOneDriveUrl(fs.UserId, fs.Folder));

        //        Task.WaitAll(oneDriveTasks.ToArray());

        //        foreach (var t in oneDriveTasks)
        //        {
        //            // Check was successful, if not load from cache.

        //        }

        //        // Local folder.
        //        var localTemplateFolder = GetString(LocalTemplateFolderPropName, _defaultLocalTemplateFolder);
        //        if (File.Exists(LocalTemplateFolderPropName))
        //        {
        //            var tmp = AddTemplatesFolder(localTemplateFolder);
        //            rval.AddRange(tmp);
        //        }

        //        return rval;
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine($"LoadTemplates() failed: {ex}");
        //        return rval;
        //    }
        //}

        //private IEnumerable<MvvmTemplate> AddTemplatesFolder(string sourceDirectory)
        //{
        //    var folder = sourceDirectory;
        //    var files = Directory.EnumerateFiles(folder, "*.tpl", SearchOption.AllDirectories);
        //    foreach (var f in files)
        //    {
        //        string contents = null;
        //        var fn = Path.Combine(sourceDirectory, f);
        //        try
        //        {
        //            contents = File.ReadAllText(fn, Encoding.UTF8);
        //        }
        //        catch (Exception ex1)
        //        {
        //            Trace.WriteLine($"Can't read MVVM template file {fn}: {ex1.Message}");

        //            // Fall back on cached version.
        //            fn = Path.Combine(TempFolder, f);
        //            try
        //            {
        //                contents = File.ReadAllText(fn, Encoding.UTF8);
        //            }
        //            catch (Exception ex2)
        //            {
        //                Trace.WriteLine($"Can't read fallback cache MVVM template file {fn}: {ex2.Message}");
        //            }
        //        }
        //        if (!string.IsNullOrWhiteSpace(contents))
        //        {
        //            var template = ParseTemplate(contents);
        //            yield return template;
        //        }
        //    }
        //}




    //    LiveConnectClient liveClient;

    //    private async Task<int> UploadFileToOneDrive()
    //    {
    //        try
    //        {
    //            // create OneDrive auth client
    //            var authClient = new LiveAuthClient();

    //            //  ask for both read and write access to the OneDrive
    //            LiveLoginResult result = await authClient.LoginAsync(new string[] { "wl.skydrive" });

    //            //  if login successful 
    //            if (result.Status == LiveConnectSessionStatus.Connected)
    //            {
    //                //  create a OneDrive client
    //                liveClient = new LiveConnectClient(result.Session);

    //                //  create a local file
    //                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("filename", CreationCollisionOption.ReplaceExisting);

    //                //  copy some txt to local file
    //                MemoryStream ms = new MemoryStream();
    //                DataContractSerializer serializer = new DataContractSerializer(typeof(string));
    //                serializer.WriteObject(ms, "Hello OneDrive World!!");

    //                using (Stream fileStream = await file.OpenStreamForWriteAsync())
    //                {
    //                    ms.Seek(0, SeekOrigin.Begin);
    //                    await ms.CopyToAsync(fileStream);
    //                    await fileStream.FlushAsync();
    //                }

    //                //  create a folder
    //                string folderID = await GetFolderID("folderone");

    //                if (string.IsNullOrEmpty(folderID))
    //                {
    //                    //  return error
    //                    return 0;
    //                }

    //                //  upload local file to OneDrive
    //                await liveClient.BackgroundUploadAsync(folderID, file.Name, file, OverwriteOption.Overwrite);

    //                return 1;
    //            }
    //        }
    //        catch
    //        {
    //        }
    //        //  return error
    //        return 0;
    //    }
    //    Copy this below code to create a folder on OneDrive

    //public async Task<string> GetFolderID(string folderName)
    //    {
    //        try
    //        {
    //            string queryString = "me/skydrive/files?filter=folders";
    //            //  get all folders
    //            LiveOperationResult loResults = await liveClient.GetAsync(queryString);
    //            dynamic folders = loResults.Result;

    //            foreach (dynamic folder in folders.data)
    //            {
    //                if (string.Compare(folder.name, folderName, StringComparison.OrdinalIgnoreCase) == 0)
    //                {
    //                    //  found our folder
    //                    return folder.id;
    //                }
    //            }

    //            //  folder not found

    //            //  create folder
    //            Dictionary<string, object> folderDetails = new Dictionary<string, object>();
    //            folderDetails.Add("name", folderName);
    //            loResults = await liveClient.PostAsync("me/skydrive", folderDetails);
    //            folders = loResults.Result;

    //            // return folder id
    //            return folders.id;
    //        }
    //        catch
    //        {
    //            return string.Empty;
    //        }
    //    }
    //    Now copy this below download function

    //public async Task<int> DownloadFileFromOneDrive()
    //    {
    //        try
    //        {
    //            string fileID = string.Empty;

    //            //  get folder ID
    //            string folderID = await GetFolderID("folderone");

    //            if (string.IsNullOrEmpty(folderID))
    //            {
    //                return 0; // doesnt exists
    //            }

    //            //  get list of files in this folder
    //            LiveOperationResult loResults = await liveClient.GetAsync(folderID + "/files");
    //            List<object> folder = loResults.Result["data"] as List<object>;

    //            //  search for our file 
    //            foreach (object fileDetails in folder)
    //            {
    //                IDictionary<string, object> file = fileDetails as IDictionary<string, object>;
    //                if (string.Compare(file["name"].ToString(), "filename", StringComparison.OrdinalIgnoreCase) == 0)
    //                {
    //                    //  found our file
    //                    fileID = file["id"].ToString();
    //                    break;
    //                }
    //            }

    //            if (string.IsNullOrEmpty(fileID))
    //            {
    //                //  file doesnt exists
    //                return 0;
    //            }

    //            //  create local file
    //            StorageFile localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("filename_from_onedrive", CreationCollisionOption.ReplaceExisting);

    //            //  download file from OneDrive
    //            await liveClient.BackgroundDownloadAsync(fileID + "/content", localFile);

    //            return 1;
    //        }
    //        catch
    //        {
    //        }
    //        return 0;
    //    }

        //private async Task<List<MvvmTemplate>> AddTemplatesOneDriveUrl(string userId, string folder)
        //{
        //    try
        //    {
        //        //await _liveSessionService.SignIn();
        //        //if (_liveSessionService != null)
        //        //{
                    
        //        //}
        //        //else
        //        //{
        //        //    // Go to cache.
        //        //}

        //        //// Create OneDrive auth client.
        //        //var authClient = new LiveAuthClient(Secrets.LiveClientId);

        //        //// Ask for read access.
        //        //var result = await authClient.InitializeAsync(new[] { "wl.skydrive" });

        //        ////  if login successful 
        //        //if (result.Status == LiveConnectSessionStatus.Connected)
        //        //{
        //        //    //  create a OneDrive client
        //        //    liveClient = new LiveConnectClient(result.Session);

        //        //    var items = await liveClient.GetAsync("/MvvmTools/Templates");

        //        //    var publicDocsFolder = LiveSDKHelper.SkyDriveHelper.GetPublicDocumentsFolder(userId);
        //        //    return null;
        //        //}
        //        //else
        //        //{
        //        //    // Go to cache.
        //        //}

        //    }
        //    catch (Exception ex)
        //    {
                
        //        throw;
        //    }
            
        //    return null;
            
        //    //var folder = sourceDirectoryUrl;
        //    //var files = Directory.EnumerateFiles(folder, "*.tpl", SearchOption.AllDirectories);
        //    //foreach (var f in files)
        //    //{
        //    //    string contents = null;
        //    //    var fn = Path.Combine(sourceDirectoryUrl, f);
        //    //    try
        //    //    {
        //    //        contents = File.ReadAllText(fn, Encoding.UTF8);
        //    //    }
        //    //    catch (Exception ex1)
        //    //    {
        //    //        Trace.WriteLine($"Can't read MVVM template file {fn}: {ex1.Message}");

        //    //        // Fall back on cached version.
        //    //        fn = Path.Combine(TempFolder, f);
        //    //        try
        //    //        {
        //    //            contents = File.ReadAllText(fn, Encoding.UTF8);
        //    //        }
        //    //        catch (Exception ex2)
        //    //        {
        //    //            Trace.WriteLine($"Can't read fallback cache MVVM template file {fn}: {ex2.Message}");
        //    //        }
        //    //    }
        //    //    if (!string.IsNullOrWhiteSpace(contents))
        //    //    {
        //    //        var template = ParseTemplate(contents);
        //    //        rval.Add(template);
        //    //    }
        //    //}

        //}

        //private MvvmTemplate ParseTemplate(string contents)
        //{
        //    return null;
        //}

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
                            p.KindId);
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
                "# The MVVM Tools extension (by Chris Bordeman cbordeman@outlook.com) is available via\r\n" +
                "# Tools => Extensions and Updates, or at\r\n" +
                "# https://visualstudiogallery.msdn.microsoft.com/978ed555-9f0d-44a2-884c-9084844ac469\r\n" +
                "# The source is available on GitHub at https://github.com/cbordeman/Mvvm-Tools if\r\n" +
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
