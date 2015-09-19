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
using MvvmTools.Core.Utilities;
using Newtonsoft.Json;
using Ninject;
using Raven.Client.Linq.Indexing;

namespace MvvmTools.Core.Services
{
    public interface ISettingsService
    {
        Task<MvvmToolsSettings> LoadSettings();
        void SaveSettings(MvvmToolsSettings settings);
        string RefreshToken { get; set; }
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
        
        // Factory templates.
        
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
            LoadTemplates();

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

        private string GetFromResources(string resourceName)
        {
            var assem = GetType().Assembly;

            using (var stream = assem.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                return null;
            }
        }

        private enum Mode
        {

        }

        private List<Template> ParseTemplates([NotNull] string data)
        {
            // The reason this returns a list is because a template file 
            // can contain multiple templates.
            
            try
            {
                var rval = new List<Template>();

                Template template = null;

                bool inT4 = false;

                string currentSection = null;
                var t4Sb = new StringBuilder(4096);
                Field field = null;

                var split = data.Split('\n');
                int linenum = 0;
                for (; linenum < split.Length; linenum++)
                {
                    var line = split[linenum];

                    switch (line.Trim().ToUpper())
                    {
                        case "[[TEMPLATE]]":
                            // [[Template]] must come first, or it must come after a [[CodeBehind]] section.
                            if (currentSection != null && currentSection != "CodeBehind")
                                throw new Exception($"Expected [[Template]], line {linenum + 1}");

                            // If already in a template, save it.
                            if (template != null)
                            {
                                if (currentSection == "CodeBehind")
                                    template.CodeBehind = t4Sb.ToString().Trim();
                                ValidateTemplateT4Sections(linenum, template);
                                rval.Add(template);
                            }
                            
                            // Start new template.
                            template = new Template();
                            field = null;

                            currentSection = "Template";

                            break;

                        case "[[FIELD]]":
                            if (!string.Equals(currentSection, "Field", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(currentSection, "Template", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception($"[[Field]] sections must follow the [[Template]] section, line {linenum + 1}");
                            }
                            if (field == null)
                            {
                                // Validate template properties.
                                ValidateTemplateProperties(linenum, template);
                            }
                            else
                            {
                                // Already working on a field, validate and add it.
                                ValidateFieldProperties(linenum, field);
                                template.Fields.Add(field);
                            }

                            // Start a new field
                            field = new Field();

                            currentSection = "Field";

                            break;

                        case "[[VIEWMODEL]]":
                            if (currentSection != "Field" && currentSection != "Template")
                                throw new Exception($"The [[ViewModel]] section must follow a [[Template]] or [[Field]] section, line {linenum + 1}");

                            if (field != null)
                            {
                                // Working on a field, validate and add it.
                                ValidateFieldProperties(linenum, field);
                                template.Fields.Add(field);
                            }

                            currentSection = "ViewModel";
                            t4Sb = new StringBuilder(4096);
                            inT4 = true;
                            break;
                        case "[[VIEW]]":
                            if (currentSection != "ViewModel")
                                throw new Exception($"The [[View]] section must follow the [[ViewModel]] section, line {linenum + 1}");
                            template.ViewModel = t4Sb.ToString().Trim();
                            currentSection = "View";
                            t4Sb = new StringBuilder(4096);
                            break;
                        case "[[CODEBEHIND]]":
                            if (currentSection != "View")
                                throw new Exception($"The [[CodeBehind]] section must follow the [[View]] section, line {linenum + 1}");
                            template.View = t4Sb.ToString().Trim();
                            currentSection = "CodeBehind";
                            t4Sb = new StringBuilder(4096);
                            break;

                        default:
                            if (inT4)
                            {
                                t4Sb.Append(line);
                                break;
                            }

                            // Non-T4 mode.

                            // Skip comments and empty lines.
                            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                                continue;

                            switch (currentSection)
                            {
                                case "Template":
                                    HandleTemplatePropertyAssignment(template, split, ref linenum);
                                    break;

                                case "Field":
                                    HandleFieldPropertyAssignment(field, split, ref linenum);
                                    break;
                                default:
                                    throw new Exception("Expected a section header.  Line " + (linenum + 1));
                            }

                            break;
                    }
                }

                // End of file, add template.
                if (template != null)
                {
                    if (currentSection == "CodeBehind")
                        template.CodeBehind = t4Sb.ToString().Trim();
                    ValidateTemplateT4Sections(linenum, template);
                    rval.Add(template);
                }

                return rval;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{nameof(ParseTemplates)}() failed: {ex}");
                throw;
            }
        }

        private static void ValidateFieldProperties(int linenum, Field field)
        {
            var msg = "Expected field's \"{0}\" property, line " + (linenum + 1);

            if (string.IsNullOrEmpty(field.Name))
                throw new Exception(string.Format(msg, "Name"));
            if (field.Default == null)
                throw new Exception(string.Format(msg, "Default"));
            if (string.IsNullOrEmpty(field.Prompt))
                throw new Exception(string.Format(msg, "Prompt"));
            if (field.FieldType == null)
                throw new Exception(string.Format(msg, "Type"));

            switch (field.FieldType.Value)
            {
                case FieldType.CheckBox:
                    if (field.Open != null)
                        throw new Exception("Only ComboBox fields can have the 'Open' property.  Line: " + (linenum + 1));
                    if (field.MultiLine != null)
                        throw new Exception("Only TextBox fields can have the 'MultiLine' property.  Line: " + (linenum + 1));
                    break;
                case FieldType.ComboBox:
                    if (field.MultiLine != null)
                        throw new Exception("Only TextBox fields can have the 'MultiLine' property.  Line: " + (linenum + 1));
                    break;
                case FieldType.TextBox:
                    if (field.Open != null)
                        throw new Exception("Only ComboBox fields can have the 'Open' property.  Line: " + (linenum + 1));
                    break;
            }
        }

        private static void ValidateTemplateProperties(int linenum, Template template)
        {
            var msg = "Expected template's \"{0}\" property, line " + (linenum + 1);

            if (string.IsNullOrEmpty(template.Platforms))
                throw new Exception(string.Format(msg, "Platforms"));
            if (string.IsNullOrEmpty(template.Framework))
                throw new Exception(string.Format(msg, "Framework"));
            if (string.IsNullOrEmpty(template.Language))
                throw new Exception(string.Format(msg, "Language"));
            if (string.IsNullOrEmpty(template.Name))
                throw new Exception(string.Format(msg, "Name"));
        }

        private static void ValidateTemplateT4Sections(int linenum, Template template)
        {
            var msg = "Expected \"[[{0}]]\" section, line " + (linenum + 1);

            if (template.ViewModel == null)
                throw new Exception(string.Format(msg, "ViewModel"));
            if (template.View == null)
                throw new Exception(string.Format(msg, "View"));
            if (template.CodeBehind == null)
                throw new Exception(string.Format(msg, "CodeBehind"));
        }

        private void HandleFieldPropertyAssignment(Field field, string[] split, ref int linenum)
        {
            // Save this for error reporting.
            int firstline = linenum;

            // Parses a property across multiple lines.
            string name;
            StringBuilder sbValue;
            ParseProperty(split, ref linenum, out name, out sbValue);

            // Assign to field in template and validate.
            switch (name.ToUpper())
            {
                case "NAME":
                    field.Name = sbValue.ToString().Trim();
                    var msg = ValidationUtilities.ValidateName(field.Name);
                    if (string.IsNullOrEmpty(field.Name) || msg != null)
                        throw new Exception("Field's Name property is required and must be a valid C# or VB identifier.  Line " + (firstline + 1));
                    break;

                case "DESCRIPTION":
                    field.Description = sbValue.ToString().Trim();
                    break;

                case "TYPE":
                    field.FieldType = ConvertToFieldType(sbValue.ToString().Trim());
                    if (field.FieldType == null)
                        throw new Exception("Field's Type property must be one of: TextBox, CheckBox, or ComboBox.  Line " + (firstline + 1));
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "DEFAULT":
                    field.Default = sbValue.ToString().Trim();
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "PROMPT":
                    field.Prompt = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(field.Prompt) || field.Prompt.EndsWith(":") || field.Prompt.EndsWith("."))
                        throw new Exception("Field's Prompt property is required and must NOT end in a period ('.') or a colon (':').  Line " + (firstline + 1));
                    break;

                case "MULTILINE":
                    field.MultiLine = ConvertToBoolean(sbValue.ToString().Trim());
                    if (field.MultiLine == null)
                        throw new Exception("Field's MultiLine property must be True or False.  Line " + (firstline + 1));
                    break;

                case "CHOICES":
                    field.Choices = sbValue.ToString().Trim().Split('|');
                    if (field.Choices.Length == 0)
                        throw new Exception("Field's Choices property, if specified, must not be empty.  Line " + (firstline + 1));
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "OPEN":
                    field.Open = ConvertToBoolean(sbValue.ToString().Trim());
                    if (field.Open == null)
                        throw new Exception("Field's Open property, if specified, must be True or False.  Line " + (firstline + 1));
                    break;

                default:
                    throw new Exception($"Field property \"{name}\" is not valid.  Expected one of: Name, Description, Default, MultiLine, Choices, or Open.  Line {firstline + 1}");
            }
        }

        private void EnsureFieldPropertiesAreCompatible(Field field, int firstline)
        {
            // If both are known, make sure default value and field type are compatible.
            if (field.FieldType != null && field.Default != null)
            {
                if (field.FieldType == FieldType.CheckBox && ConvertToBoolean(field.Default) == null)
                    throw new Exception("Field's Type is CheckBox, therefore Default value must be True or False.  Line " + (firstline + 1));
            }

            // If ComboBox and the Default and Choices are known, verify Default is in Choices.
            if (field.FieldType == FieldType.ComboBox && field.Default != null && field.Choices != null)
            {
                if (!field.Choices.Any(c => c.Equals(field.Default, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("Field's Default value doesn't exist in the Choices list.  Line " + (firstline + 1));
            }
        }
        
        private FieldType? ConvertToFieldType(string ft)
        {
            switch (ft.ToUpper())
            {
                case "CHECKBOX":
                    return FieldType.CheckBox;
                case "COMBOBOX":
                    return FieldType.ComboBox;
                case "TEXTBOX":
                    return FieldType.TextBox;
            }
            return null;
        }

        private bool? ConvertToBoolean(string fv)
        {
            switch (fv.ToUpper())
            {
                case "TRUE":
                    return true;
                case "FALSE":
                    return false;
                default:
                    return null;
            }
        }

        // Parses a property in Name: Value format.  If pipes are at the end of the line,
        // linenumber is advanced.
        private static void ParseProperty(string[] split, ref int linenum, out string name, out StringBuilder sbValue)
        {
            // Save this for error reporting.
            int firstline = linenum;

            name = null;
            sbValue = new StringBuilder();
            for (; linenum < split.Length; linenum++)
            {
                var line = split[linenum].Trim();

                if (name != null)
                {
                    // Line continuation
                    if (line.EndsWith("|", StringComparison.Ordinal))
                        sbValue.AppendLine(line.Substring(0, line.Length - 1).TrimStart());
                    else
                    {
                        sbValue.Append(line.TrimStart());
                        break;
                    }
                }
                else
                {
                    // Find variable name
                    var colon = line.IndexOf(":", StringComparison.Ordinal);
                    if (colon == -1)
                        throw new Exception("Expected a colon, line " + (linenum + 1));
                    name = line.Substring(0, colon).TrimStart();

                    // Start value.
                    if (line.EndsWith("|", StringComparison.Ordinal))
                        sbValue.AppendLine(line.Substring(colon + 1, line.Length - colon - 2).TrimStart());
                    else
                    {
                        sbValue.Append(line.Substring(colon + 1, line.Length - colon - 1).TrimStart());
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Expected a property name, followed by a colon, line " + (firstline + 1));
        }

        private void HandleTemplatePropertyAssignment(Template template, string[] split, ref int linenum)
        {
            // Save this for error reporting.
            int firstline = linenum;

            // Parses a property across multiple lines.
            string name;
            StringBuilder sbValue;
            ParseProperty(split, ref linenum, out name, out sbValue);

            // Assign to field in template and validate.
            string msg;
            switch (name.ToUpper())
            {
                case "NAME":
                    template.Name = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Name))
                        throw new Exception("Template Name cannot be empty.  Line " + (firstline + 1));
                    break;

                case "DESCRIPTION":
                    template.Description = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Description))
                        throw new Exception("Template Description is required.  Line " + (firstline + 1));
                    break;

                case "FRAMEWORK":
                    template.Framework = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Framework))
                        throw new Exception("Template Framework must be 'None' or the name of the requisite framework, such as Prism, MVVM Light, or Caliburn.  Line " + (firstline + 1));
                    break;

                case "PLATFORMS":
                    template.Platforms = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Platforms))
                        throw new Exception("Template Platforms must be 'Any' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.  For Universal apps, use WinRT.  Line " + (firstline + 1));
                    msg = ValidationUtilities.ValidatePlatforms(template.Platforms);
                    if (msg != null)
                        throw new Exception(msg + "  Line " + (firstline + 1));
                    break;

                case "LANGUAGE":
                    template.Language = sbValue.ToString().Trim();
                    msg = ValidationUtilities.ValidateLanguage(template.Language);
                    if (msg != null)
                        throw new Exception(msg + "  Line " + (firstline + 1));
                    break;

                case "TAGS":
                    template.Tags = sbValue.ToString().Trim();
                    break;
                default:
                    throw new Exception($"Template property \"{name}\" is not valid.  Expected one of: Name, Description, Framework, Platforms, Language, or Tags.  Line {firstline + 1}");
            }
        }

        private void LoadTemplates()
        {
            var rval = new List<Template>();
            try
            {
                // Factory templates.
                var factoryTemplatesText = GetFromResources("MvvmTools.Core.FactoryTemplates.tpl");
                var tmp1 = ParseTemplates(factoryTemplatesText);
                rval.AddRange(tmp1);

                // Local folder.
                var localTemplateFolder = GetString(LocalTemplateFolderPropName, _defaultLocalTemplateFolder);
                if (File.Exists(localTemplateFolder))
                {
                    var tmp2 = AddTemplatesFolder(localTemplateFolder);
                    rval.AddRange(tmp2);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{nameof(LoadTemplates)}() failed: {ex}");
                throw;
            }
        }

        private IEnumerable<Template> AddTemplatesFolder(string sourceDirectory)
        {
            var folder = sourceDirectory;
            var files = Directory.EnumerateFiles(folder, "*.tpl", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                string contents = null;
                var fn = Path.Combine(sourceDirectory, f);
                try
                {
                    contents = File.ReadAllText(fn, Encoding.UTF8);
                }
                catch (Exception ex1)
                {
                    Trace.WriteLine($"Can't read MVVM template file {fn}: {ex1.Message}");

                    // Fall back on cached version.
                    fn = Path.Combine(TempFolder, f);
                    try
                    {
                        contents = File.ReadAllText(fn, Encoding.UTF8);
                    }
                    catch (Exception ex2)
                    {
                        Trace.WriteLine($"Can't read fallback cache MVVM template file {fn}: {ex2.Message}");
                    }
                }
                if (!string.IsNullOrWhiteSpace(contents))
                {
                    var template = ParseTemplate(contents);
                    yield return template;
                }
            }
        }
        
        private Template ParseTemplate(string contents)
        {
            return null;
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
