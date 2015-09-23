using System.Collections.Generic;

namespace MvvmTools.Core.Models
{
    public class Template
    {
        public Template(bool isInternal, string source)
        {
            IsInternal = isInternal;
            Source = source;

            Fields = new List<Field>();
        }
        
        /// <summary>
        /// True if built in and immutable (except via extension update).
        /// False if part of the user's tpl collection.
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// Source of the template, the filename without the .tpl extension.
        /// This should be the author if contributed.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The supported platforms.
        /// </summary>
        public HashSet<Platform> Platforms { get; set; }

        /// <summary>
        /// The supported form factors: 'Any' or a comma separated combination of: Phone, Tablet, Desktop
        /// </summary>
        public HashSet<FormFactor> FormFactors { get; set; }

        /// <summary>
        /// One of: None, Prism, Catel, Modern UI, MVVM Light, Caliburn, Caliburn.Micro.
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// Name doesn't need to be unique.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Comma separated, for searches.
        /// </summary>
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
        /// A T4 template.  Applies to all languages, though code behind and view model must be language specific.
        /// </summary>
        public string View { get; set; }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string ViewModelCSharp { get; set; }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string ViewModelVisualBasic { get; set; }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string CodeBehindCSharp { get; set; }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string CodeBehindVisualBasic { get; set; }
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
        /// If non-empty, indicates a combobox.
        /// </summary>
        public string[] Choices { get; set; }

        public FieldType? FieldType { get; set; }
    }

    public enum FieldType
    {
        TextBox, TextBoxMultiLine, CheckBox, ComboBox, ComboBoxOpen
    }

    public enum FormFactor
    {
        Phone, Tablet, Desktop
    }

    public enum Platform
    {
        Wpf, Silverlight, Xamarin, WinRt
    }
}
