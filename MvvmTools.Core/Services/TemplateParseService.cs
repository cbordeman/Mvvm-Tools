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
        List<Template> ParseTemplates([NotNull] string data, out List<ParseError> errors);
    }

    public class TemplateParseService : ITemplateParseService
    {
        #region Data

        List<ParseError> _errors;

        #endregion Data

        public List<Template> ParseTemplates(string data, out List<ParseError> errors)
        {
            errors = _errors = new List<ParseError>();

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
                                _errors.Add(new ParseError("Expected [[Template]].", linenum + 1));

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
                                _errors.Add(new ParseError("[[Field]] sections must follow the [[Template]] section.", linenum + 1));
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
                                _errors.Add(new ParseError("The [[ViewModel]] section must follow a [[Template]] or [[Field]] section.", linenum + 1));

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
                                _errors.Add(new ParseError("The [[View]] section must follow the [[ViewModel]] section.", linenum + 1));
                            template.ViewModel = t4Sb.ToString().Trim();
                            currentSection = "View";
                            t4Sb = new StringBuilder(4096);
                            break;
                        case "[[CODEBEHIND]]":
                            if (currentSection != "View")
                                _errors.Add(new ParseError("The [[CodeBehind]] section must follow the [[View]] section.", linenum + 1));
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
                                    _errors.Add(new ParseError("Expected a section header.", linenum + 1));
                                    break;
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

        private void ValidateTemplateT4Sections(int linenum, Template template)
        {
            var msg = "Expected \"[[{0}]]\" section.";

            if (template.ViewModel == null)
                _errors.Add(new ParseError(string.Format(msg, "ViewModel"), linenum + 1));
            if (template.View == null)
                _errors.Add(new ParseError(string.Format(msg, "View"), linenum + 1));
            if (template.CodeBehind == null)
                _errors.Add(new ParseError(string.Format(msg, "CodeBehind"), linenum + 1));
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
                        _errors.Add(new ParseError("Field's Type property must be one of: TextBox, CheckBox, or ComboBox.", linenum + 1));
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

                case "MULTILINE":
                    field.MultiLine = ConvertToBoolean(sbValue.ToString().Trim());
                    if (field.MultiLine == null)
                        _errors.Add(new ParseError("Field's MultiLine property must be True or False.", firstline + 1));
                    break;

                case "CHOICES":
                    field.Choices = sbValue.ToString().Trim().Split('|');
                    if (field.Choices.Length == 0)
                        _errors.Add(new ParseError("Field's Choices property, if specified, must not be empty.", firstline + 1));
                    EnsureFieldPropertiesAreCompatible(field, firstline);
                    break;

                case "OPEN":
                    field.Open = ConvertToBoolean(sbValue.ToString().Trim());
                    if (field.Open == null)
                        _errors.Add(new ParseError("Field's Open property, if specified, must be True or False.", firstline + 1));
                    break;

                default:
                    _errors.Add(new ParseError($"Field property \"{name}\" is not valid.  Expected one of: Name, Description, Default, MultiLine, Choices, or Open.", firstline + 1));
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
            if (field.FieldType == FieldType.ComboBox && field.Default != null && field.Choices != null)
            {
                if (!field.Choices.Any(c => c.Equals(field.Default, StringComparison.OrdinalIgnoreCase)))
                    _errors.Add(new ParseError("Field's Default value doesn't exist in the Choices list.", firstline + 1));
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
            string msg;
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
                    template.Platforms = sbValue.ToString().Trim();
                    if (string.IsNullOrWhiteSpace(template.Platforms))
                        _errors.Add(new ParseError("Template Platforms must be 'Any' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.  For Universal apps, use WinRT.", firstline + 1));
                    msg = ValidationUtilities.ValidatePlatforms(template.Platforms);
                    if (msg != null)
                        _errors.Add(new ParseError(msg, firstline + 1));
                    break;

                case "LANGUAGE":
                    template.Language = sbValue.ToString().Trim();
                    msg = ValidationUtilities.ValidateLanguage(template.Language);
                    if (msg != null)
                        _errors.Add(new ParseError(msg, firstline + 1));
                    break;

                case "TAGS":
                    template.Tags = sbValue.ToString().Trim();
                    break;
                default:
                    _errors.Add(new ParseError($"Template property \"{name}\" is not valid.  Expected one of: Name, Description, Framework, Platforms, Language, or Tags.", firstline + 1));
                    break;
            }
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

            if (field.FieldType != null)
                switch (field.FieldType.Value)
                {
                    case FieldType.CheckBox:
                        if (field.Open != null)
                            _errors.Add(new ParseError("Only ComboBox fields can have the 'Open' property.", linenum + 1));
                        if (field.MultiLine != null)
                            _errors.Add(new ParseError("Only TextBox fields can have the 'MultiLine' property.", linenum + 1));
                        break;
                    case FieldType.ComboBox:
                        if (field.MultiLine != null)
                            _errors.Add(new ParseError("Only TextBox fields can have the 'MultiLine' property.", linenum + 1));
                        break;
                    case FieldType.TextBox:
                        if (field.Open != null)
                            _errors.Add(new ParseError("Only ComboBox fields can have the 'Open' property.", linenum + 1));
                        break;
                }
        }

        private void ValidateTemplateProperties(int linenum, Template template)
        {
            var msg = "Expected template's \"{0}\" property, line " + (linenum + 1);

            if (string.IsNullOrEmpty(template.Platforms))
                _errors.Add(new ParseError(string.Format(msg, "Platforms"), linenum + 1));
            if (string.IsNullOrEmpty(template.Framework))
                _errors.Add(new ParseError(string.Format(msg, "Framework"), linenum + 1));
            if (string.IsNullOrEmpty(template.Language))
                _errors.Add(new ParseError(string.Format(msg, "Language"), linenum + 1));
            if (string.IsNullOrEmpty(template.Name))
                _errors.Add(new ParseError(string.Format(msg, "Name"), linenum + 1));
        }

    }
}
