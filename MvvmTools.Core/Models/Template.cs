using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MvvmTools.Core.Models
{
    [DataContract]
    public class Template
    {
        public Template()
        {
            
        }

        public Template(bool isInternal, string name)
        {
            IsInternal = isInternal;
            Name = name;

            Fields = new List<Field>();
        }

        /// <summary>
        /// True if built in and immutable (except via extension update).
        /// False if part of the user's tpl collection.
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// The supported platforms.
        /// </summary>
        [DataMember]
        public HashSet<Platform> Platforms { get; set; }

        /// <summary>
        /// The supported form factors: 'Any' or a comma separated combination of: Phone, Tablet, Desktop
        /// </summary>
        [DataMember]
        public HashSet<FormFactor> FormFactors { get; set; }

        /// <summary>
        /// One of: None, Prism, Catel, Modern UI, MVVM Light, Caliburn, Caliburn.Micro.
        /// </summary>
        [DataMember]
        public string Framework { get; set; }

        /// <summary>
        /// Name doesn't need to be unique.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Comma separated, for searches.
        /// </summary>
        [DataMember]
        public string Tags { get; set; }

        /// <summary>
        /// If this contains urls, will be turned into hyperlinks.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// A list of fields of type SimpleField&gt;T&lt;, SelectField&gt;T&lt;, or ComboField&gt;T&lt;.
        /// </summary>
        [DataMember]
        public List<Field> Fields { get; set; }

        /// <summary>
        /// A T4 template.  Applies to all languages, though code behind and view model must be language specific.
        /// </summary>
        public string View { get; set; }

        [DataMember(Name = "View", EmitDefaultValue = false)]
        private CDataWrapper ViewCData
        {
            get { return View; }
            set { View = value; }
        }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string ViewModelCSharp { get; set; }

        [DataMember(Name = "ViewModelCSharp", EmitDefaultValue = false)]
        private CDataWrapper ViewModelCSharpCData
        {
            get { return ViewModelCSharp; }
            set { ViewModelCSharp = value; }
        }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string ViewModelVisualBasic { get; set; }

        [DataMember(Name = "ViewModelVisualBasic", EmitDefaultValue = false)]
        private CDataWrapper ViewModelVisualBasicCData
        {
            get { return ViewModelVisualBasic; }
            set { ViewModelVisualBasic = value; }
        }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string CodeBehindCSharp { get; set; }

        [DataMember(Name = "CodeBehindCSharp", EmitDefaultValue = false)]
        private CDataWrapper CodeBehindCSharpCData
        {
            get { return CodeBehindCSharp; }
            set { CodeBehindCSharp = value; }
        }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string CodeBehindVisualBasic { get; set; }

        [DataMember(Name = "CodeBehindVisualBasic", EmitDefaultValue = false)]
        private CDataWrapper CodeBehindVisualBasicCData
        {
            get { return CodeBehindVisualBasic; }
            set { CodeBehindVisualBasic = value; }
        }
    }

    [DataContract]
    public class Field
    {
        public Field()
        {
            
        }

        public Field(string name, FieldType fieldType, string @default, string prompt, string choices = null, string description = null)
        {
            Name = name;
            Default = @default;
            Prompt = prompt;
            Description = description;
            FieldType = fieldType;
            if (choices != null)
                Choices = choices.Split('|').Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// The property name, must be a valid C# identifier.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Default value when presented to user.
        /// </summary>
        [DataMember]
        public string Default { get; set; }

        /// <summary>
        /// Required.  Goes before the field.
        /// </summary>
        [DataMember]
        public string Prompt { get; set; }

        /// <summary>
        /// Optional, but can contain hyperlinks and line breaks.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// If non-empty, indicates a combobox.
        /// </summary>
        [DataMember]
        public string[] Choices { get; set; }

        [DataMember]
        public FieldType FieldType { get; set; }
    }

    [DataContract]
    public enum FieldType
    {
        [EnumMember]TextBox,
        [EnumMember]TextBoxMultiLine,
        [EnumMember]CheckBox,
        [EnumMember]ComboBox,
        [EnumMember]ComboBoxOpen
    }

    [DataContract]
    public enum FormFactor
    {
        [EnumMember]Phone,
        [EnumMember]Tablet,
        [EnumMember]Desktop
    }

    [DataContract]
    public enum Platform
    {
        [EnumMember]Wpf,
        [EnumMember]Silverlight,
        [EnumMember]Xamarin,
        [EnumMember]WinRt
    }
}
