using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using MvvmTools.Core.Models;
using MvvmTools.Core.Utilities;

namespace MvvmTools.Core.Services
{
    public interface ITemplateParseService
    {
        List<Template> ParseTemplates(bool isInternal, [NotNull] string source, [NotNull] string data, out List<ParseError> errors);
    }

    public enum Section
    {
        Template, Field, ViewModelVisualBasic, ViewModelCSharp, CodeBehindVisualBasic, CodeBehindCSharp, View
    }

    public class TemplateParseService : ITemplateParseService
    {
        #region Data

        List<ParseError> _errors;

        #endregion Data

        public List<Template> ParseTemplates(bool isInternal, string source, string data, out List<ParseError> errors)
        {
            errors = _errors = new List<ParseError>();

            // The reason this returns a list is because a template file 
            // can contain multiple templates.

            try
            {
                var rval = new List<Template>();

                Template template = null;

                bool inT4 = false;

                Section? currentSection = null;
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
                            if (template != null)
                            {
                                // A template is already started, fully validate it and add it.
                                AssignT4SbToCorrectSection(currentSection, template, t4Sb);
                                ValidateTemplateProperties(linenum, template);
                                ValidateTemplateT4Sections(linenum, template);
                                rval.Add(template);
                            }

                            // Start new template.
                            template = new Template(isInternal, source);
                            field = null;
                            inT4 = false;

                            currentSection = Section.Template;

                            break;

                        case "[[FIELD]]":
                            if (currentSection != Section.Template && currentSection != Section.Field)
                                _errors.Add(new ParseError("[[Field]] sections must appear between the [[Template]] and the T4 sections ([[View]], [[CodeBehind-*]], and [[ViewModel-*]]).", linenum + 1));

                            if (field == null)
                                // Validate template properties.
                                ValidateTemplateProperties(linenum, template);
                            else
                            {
                                // Already working on a field, validate and add it.
                                ValidateFieldProperties(linenum, field);
                                template.Fields.Add(field);
                            }

                            // Start a new field.
                            field = new Field();

                            currentSection = Section.Field;

                            break;

                        case "[[VIEW]]":
                        case "[[VIEWMODEL-CSHARP]]":
                        case "[[CODEBEHIND-CSHARP]]":
                        case "[[VIEWMODEL-VISUALBASIC]]":
                        case "[[CODEBEHIND-VISUALBASIC]]":
                            if (template == null)
                            {
                                _errors.Add(new ParseError("Expected: [[Template]] section.", linenum + 1));
                                break;
                            }

                            if (field != null)
                            {
                                // Working on a field, validate and add it.
                                ValidateFieldProperties(linenum, field);
                                template.Fields.Add(field);
                                field = null;
                            }
                            else
                                // Validate template properties.
                                ValidateTemplateProperties(linenum, template);

                            AssignT4SbToCorrectSection(currentSection, template, t4Sb);
                            t4Sb = new StringBuilder(4096);

                            switch (line.Trim().ToUpper())
                            {
                                case "[[VIEW]]":
                                    currentSection = Section.View;
                                    break;
                                case "[[VIEWMODEL-CSHARP]]":
                                    currentSection = Section.ViewModelCSharp;
                                    break;
                                case "[[CODEBEHIND-CSHARP]]":
                                    currentSection = Section.CodeBehindCSharp;
                                    break;
                                case "[[VIEWMODEL-VISUALBASIC]]":
                                    currentSection = Section.ViewModelVisualBasic;
                                    break;
                                case "[[CODEBEHIND-VISUALBASIC]]":
                                    currentSection = Section.CodeBehindVisualBasic;
                                    break;
                            }
                            
                            inT4 = true;
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
                                case Section.Template:
                                    HandleTemplatePropertyAssignment(template, split, ref linenum);
                                    break;

                                case Section.Field:
                                    HandleFieldPropertyAssignment(field, split, ref linenum);
                                    break;
                                default:
                                    _errors.Add(new ParseError("Expected whitespace, a single-line comment ('#'), or a section header ('[[Header-Name]]').", linenum + 1));
                                    break;
                            }

                            break;
                    }
                }

                // End of file, add template.
                if (template != null)
                {
                    AssignT4SbToCorrectSection(currentSection, template, t4Sb);
                    ValidateTemplateProperties(linenum, template);
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

        private void AssignT4SbToCorrectSection([NotNull] Section? currentSection, [NotNull] Template template, StringBuilder t4Sb)
        {
            if (t4Sb == null)
                return;

            if (currentSection == null)
                throw new ArgumentNullException(nameof(currentSection));
            if (template == null)
                throw new ArgumentNullException(nameof(template));
        
            switch (currentSection.Value)
            {
                case Section.CodeBehindCSharp:
                    template.CodeBehindCSharp = t4Sb.ToString().Trim();
                    break;
                case Section.CodeBehindVisualBasic:
                    template.CodeBehindVisualBasic = t4Sb.ToString().Trim();
                    break;
                case Section.ViewModelCSharp:
                    template.ViewModelCSharp = t4Sb.ToString().Trim();
                    break;
                case Section.ViewModelVisualBasic:
                    template.ViewModelVisualBasic = t4Sb.ToString().Trim();
                    break;
                case Section.View:
                    template.View = t4Sb.ToString().Trim();
                    break;
            }
        }

        private void ValidateTemplateT4Sections(int linenum, Template template)
        {
            const string msg = "Expected \"[[{0}]]\" section.";

            if (template.View == null)
                _errors.Add(new ParseError(string.Format(msg, "View"), linenum + 1));

            // If provided none of the T4 sections, error.
            if (template.ViewModelVisualBasic == null &&
                template.CodeBehindVisualBasic == null &&
                template.ViewModelCSharp == null && 
                template.CodeBehindCSharp == null)
            {
                _errors.Add(new ParseError("Please provide sections: ViewModel-VisualBasic and CodeBehind-VisualBasic, OR ViewModel-CSharp and CodeBehind-CSharp, OR all four sections.", linenum + 1));
            }

            // If provided one of the VB sections, must also provide the other VB section.
            if (template.ViewModelVisualBasic != null || template.CodeBehindVisualBasic != null)
            {
                if (template.ViewModelVisualBasic == null)
                    _errors.Add(new ParseError(string.Format(msg, "ViewModel-VisualBasic"), linenum + 1));
                if (template.CodeBehindVisualBasic == null)
                    _errors.Add(new ParseError(string.Format(msg, "CodeBehind-VisualBasic"), linenum + 1));
            }

            // If provided one of the C# sections, must also provide the other C# section.
            if (template.ViewModelCSharp != null || template.CodeBehindCSharp != null)
            {
                if (template.ViewModelCSharp == null)
                    _errors.Add(new ParseError(string.Format(msg, "ViewModel-CSharp"), linenum + 1));
                if (template.CodeBehindCSharp == null)
                    _errors.Add(new ParseError(string.Format(msg, "CodeBehind-CSharp"), linenum + 1));
            }
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
                        _errors.Add(new ParseError("Field's Name property is required and must be a valid C# or VB identifier.", linenum + 1));
                    break;

                case "DESCRIPTION":
                    field.Description = sbValue.ToString().Trim();
                    break;

                case "TYPE":
                    field.FieldType = ConvertToFieldType(sbValue.ToString().Trim());
                    if (field.FieldType == null)
                        _errors.Add(new ParseError("Field's Type property must be one of: TextBox, TextBoxMultiLine, CheckBox, ComboBox, or ComboBoxOpen.", linenum + 1));
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "DEFAULT":
                    field.Default = sbValue.ToString().Trim();
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "PROMPT":
                    field.Prompt = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(field.Prompt) || field.Prompt.EndsWith(":") || field.Prompt.EndsWith("."))
                        _errors.Add(new ParseError("Field's Prompt property is required and must NOT end in a period ('.') or a colon (':').", firstline + 1));
                    break;

                case "CHOICES":
                    field.Choices = sbValue.ToString().Trim().Split('|');
                    // Trim each choice.
                    for (int index = 0; index < field.Choices.Length; index++)
                        field.Choices[index] = field.Choices[index].Trim();
                    if (field.Choices.Length == 0)
                        _errors.Add(new ParseError("Field's Choices property, if specified, must not be empty, and values should be separated by pipe ('|') symbols.", firstline + 1));
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                default:
                    _errors.Add(new ParseError($"Field property \"{name}\" is not valid.  Expected one of: Name, Description, Default, or Choices.", firstline + 1));
                    break;
            }
        }

        private void EnsureFieldPropertiesAreCompatible(Field field, int firstline)
        {
            // If both are known, make sure default value and field type are compatible.
            if (field.FieldType != null && field.Default != null)
            {
                if (field.FieldType == FieldType.CheckBox && ConvertToBoolean(field.Default) == null)
                    _errors.Add(new ParseError("Field's Type is CheckBox, therefore Default value must be True or False.", firstline + 1));
            }

            // If ComboBox and the Default and Choices are known, verify Default is in Choices.
            // Note that ComboBoxOpen doesn't care if Default is in Choices.
            if (field.FieldType == FieldType.ComboBox && field.Default != null && field.Choices != null)
            {
                if (!field.Choices.Any(c => c.Equals(field.Default, StringComparison.OrdinalIgnoreCase)))
                    _errors.Add(new ParseError("Field's Default value doesn't exist in the Choices list.  To avoid this check, change field's type to ComboBoxOpen", firstline + 1));
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
                case "COMBOBOXOPEN":
                    return FieldType.ComboBoxOpen;
                case "TEXTBOX":
                    return FieldType.TextBox;
                case "TEXTBOXMULTILINE":
                    return FieldType.TextBoxMultiLine;
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
        private void ParseProperty(string[] split, ref int linenum, out string name, out StringBuilder sbValue)
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
                        _errors.Add(new ParseError("Expected a colon.", firstline + 1));
                    else
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
                _errors.Add(new ParseError("Expected a property name, followed by a colon.", firstline + 1));

            name = name ?? string.Empty;
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
            switch (name.ToUpper())
            {
                case "NAME":
                    template.Name = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Name))
                        _errors.Add(new ParseError("Template Name is required.", firstline + 1));
                    break;

                case "DESCRIPTION":
                    template.Description = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Description))
                        _errors.Add(new ParseError("Template Description is required.", firstline + 1));
                    break;

                case "FRAMEWORK":
                    template.Framework = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Framework))
                        _errors.Add(new ParseError("Template Framework must be 'None' or the name of the requisite framework, such as Prism, MVVM Light, or Caliburn.", firstline + 1));
                    break;

                case "PLATFORMS":
                    template.Platforms = GetPlatformsHashSet(sbValue.ToString().Trim(), firstline);
                    if (template.Platforms == null)
                        _errors.Add(new ParseError("Template Platforms must be 'All' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.  For Universal apps, use WinRT.", firstline + 1));
                    break;

                case "FORM FACTORS":
                    template.FormFactors = GetFormFactorsHashSet(sbValue.ToString().Trim(), firstline);
                    if (template.FormFactors == null)
                        _errors.Add(new ParseError("Template Form Factors must be 'All' or a comma separated combination of: Phone, Tablet, Desktop.", firstline + 1));
                    break;


                case "TAGS":
                    template.Tags = sbValue.ToString().Trim();
                    break;

                default:
                    _errors.Add(new ParseError($"Template property \"{name}\" is not valid.  Expected one of: Name, Description, Framework, Platforms, or Tags.", firstline + 1));
                    break;
            }
        }

        private HashSet<Platform> GetPlatformsHashSet(string platforms, int linenum)
        {
            // 'All' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.
            if (platforms.Equals("All", StringComparison.OrdinalIgnoreCase))
                return new HashSet<Platform> {Platform.Silverlight, Platform.Wpf, Platform.WinRt, Platform.Xamarin};
            var rval = new HashSet<Platform>();
            foreach (var p in platforms.Split(','))
            {
                switch (p.Trim().ToUpper())
                {
                    case "WPF":
                        rval.Add(Platform.Wpf);
                        break;
                    case "SILVERLIGHT":
                        rval.Add(Platform.Silverlight);
                        break;
                    case "XAMARIN":
                        rval.Add(Platform.Xamarin);
                        break;
                    case "WINRT":
                        rval.Add(Platform.WinRt);
                        break;
                    default:
                        _errors.Add(new ParseError($"Unexpected Platform value: '{p.Trim()}'.", linenum));
                        break;
                }
            }
            return rval;
        }

        private HashSet<FormFactor> GetFormFactorsHashSet(string formFactors, int linenum)
        {
            // 'All' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.
            if (formFactors.Equals("All", StringComparison.OrdinalIgnoreCase))
                return new HashSet<FormFactor> { FormFactor.Phone, FormFactor.Tablet, FormFactor.Desktop };
            var rval = new HashSet<FormFactor>();
            foreach (var p in formFactors.Split(','))
            {
                switch (p.Trim().ToUpper())
                {
                    case "PHONE":
                        rval.Add(FormFactor.Phone);
                        break;
                    case "TABLET":
                        rval.Add(FormFactor.Tablet);
                        break;
                    case "DESKTOP":
                        rval.Add(FormFactor.Desktop);
                        break;
                    default:
                        _errors.Add(new ParseError($"Unexpected Form Factor value: '{p.Trim()}'.", linenum));
                        break;
                }
            }
            return rval;
        }


        private void ValidateFieldProperties(int linenum, Field field)
        {
            var msg = "Expected field's \"{0}\" property, line " + (linenum + 1);

            if (string.IsNullOrEmpty(field.Name))
                _errors.Add(new ParseError(string.Format(msg, "Name"), linenum + 1));
            if (field.Default == null)
                _errors.Add(new ParseError(string.Format(msg, "Default"), linenum + 1));
            if (string.IsNullOrEmpty(field.Prompt))
                _errors.Add(new ParseError(string.Format(msg, "Prompt"), linenum + 1));
            if (field.FieldType == null)
                _errors.Add(new ParseError(string.Format(msg, "Type"), linenum + 1));
            
        }

        private void ValidateTemplateProperties(int linenum, Template template)
        {
            var msg = "Expected template's \"{0}\" property, line " + (linenum + 1);

            if (template.Platforms == null || template.Platforms.Count == 0)
                _errors.Add(new ParseError(string.Format(msg, "Platforms"), linenum + 1));
            if (template.FormFactors == null || template.FormFactors.Count == 0)
                _errors.Add(new ParseError(string.Format(msg, "Form Factors"), linenum + 1));
            if (string.IsNullOrEmpty(template.Framework))
                _errors.Add(new ParseError(string.Format(msg, "Framework"), linenum + 1));
            if (string.IsNullOrEmpty(template.Name))
                _errors.Add(new ParseError(string.Format(msg, "Name"), linenum + 1));
        }

    }
}
