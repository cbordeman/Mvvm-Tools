using System.Collections.Generic;

namespace MvvmTools.Core.Models
{
    public class Template
    {
        public Template()
        {
            Fields = new List<Field>();
        }

        public Template(string platforms, string framework, string name, string description, string language, string tags, List<Field> fields, string viewModel, string view, string codeBehind)
        {
            Platforms = platforms;
            Framework = framework;

            Name = name;
            Description = description;
            Language = language;
            Tags = tags;
            Fields = fields;

            ViewModel = viewModel;
            View = view;
            CodeBehind = codeBehind;

            Fields = new List<Field>();
        }

        /// <summary>
        /// 'Any' or a comma separated combination of: WPF, Silverlight, Xamarin, or WinRT.  For Universal apps, use WinRT.
        /// </summary>
        public string Platforms { get; set; }

        /// <summary>
        /// One of: None, Prism, Catel, Modern UI, MVVM Light, Caliburn, Caliburn.Micro.
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// Name must be unique, except for the Language.
        /// </summary>
        public string Name { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }

        /// <summary>
        /// If this contains urls, will be turned into hyperlinks.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of fields of type SimpleField&gt;T&lt;, SelectField&gt;T&lt;, or ComboField&gt;T&lt;.
        /// </summary>
        public List<Field> Fields { get; set; }

        /// <summary>
        /// A T4 template.
        /// </summary>
        public string ViewModel { get; set; }

        /// <summary>
        /// A T4 template.
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// A T4 Template.
        /// </summary>
        public string CodeBehind { get; set; }
    }

    public class Field
    {
        /// <summary>
        /// The property name, must be a valid C# identifier.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default value when presented to user.
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Required.  Goes before the field.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Optional, but can contain hyperlinks and line breaks.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Applies to text fields.
        /// </summary>
        public bool? MultiLine { get; set; }

        /// <summary>
        /// If non-empty, indicates a combobox.
        /// </summary>
        public string[] Choices { get; set; }

        /// <summary>
        /// Applies to combobox, indicates the user can type any free form value.
        /// </summary>
        public bool? Open { get; set; }

        public FieldType? FieldType { get; set; }
    }

    public enum FieldType
    {
        TextBox, CheckBox, ComboBox
    }
}
