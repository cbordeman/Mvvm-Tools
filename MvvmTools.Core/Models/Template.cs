using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Data;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Models
{
    [DataContract]
    public class Template
    {
        public Template()
        {
            
        }

        public Template(TemplateDialogViewModel vm)
        {
            IsInternal = vm.IsInternal;

            Platforms = new HashSet<Platform>(vm.Platforms.CheckedItems);
            FormFactors = new HashSet<FormFactor>(vm.FormFactors.CheckedItems);
            
            Framework = vm.Framework ?? string.Empty;
            Name = vm.Name ?? string.Empty;
            Description = vm.Description ?? string.Empty;

            // Deep copy fields.
            Fields = new List<Field>(((ObservableCollection<FieldDialogViewModel>)vm.Fields.SourceCollection).Select(f => new Field(f)));

            View = vm.View.Buffer ?? string.Empty;

            ViewModelCSharp = vm.ViewModelCSharp.Buffer ?? string.Empty;
            CodeBehindCSharp = vm.CodeBehindCSharp.Buffer ?? string.Empty;

            ViewModelVisualBasic = vm.ViewModelVisualBasic.Buffer ?? string.Empty;
            CodeBehindVisualBasic = vm.CodeBehindVisualBasic.Buffer ?? string.Empty;
        }


        public Template(bool isInternal, string name)
        {
            IsInternal = isInternal;

            Name = name;

            Fields = new List<Field>();
        }

        #region IsInternal
#pragma warning disable 649
        private bool _isInternal;
#pragma warning restore 649
        /// <summary>
        /// True if built in and immutable (except via extension update).
        /// False if part of the user's tpl collection.
        /// </summary>
        public bool IsInternal
        {
            get { return _isInternal; }
            
            // ReSharper disable once ValueParameterNotUsed
            set
            {
#if !DEBUG
                // In debug mode, no templates are seen as internal.
                // This is so that we can edit internal template.
                _isInternal = value;
#endif
            }
        }
        #endregion IsInternal

        /// <summary>
        /// Name doesn't need to be unique.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// If this contains urls, will be turned into hyperlinks.
        /// </summary>
        [DataMember(Order = 2)]
        public string Description { get; set; }

        /// <summary>
        /// The supported platforms.
        /// </summary>
        [DataMember(Order = 3)]
        public HashSet<Platform> Platforms { get; set; }

        /// <summary>
        /// The supported form factors: 'Any' or a comma separated combination of: Phone, Tablet, Desktop
        /// </summary>
        [DataMember(Order = 4)]
        public HashSet<FormFactor> FormFactors { get; set; }

        /// <summary>
        /// One of: None, Prism, Catel, Modern UI, MVVM Light, Caliburn, Caliburn.Micro.
        /// </summary>
        [DataMember(Order = 5)]
        public string Framework { get; set; }

        /// <summary>
        /// Comma separated, for searches.
        /// </summary>
        [DataMember(Order = 6)]
        public string Tags { get; set; }

        /// <summary>
        /// A list of fields of type SimpleField&gt;T&lt;, SelectField&gt;T&lt;, or ComboField&gt;T&lt;.
        /// </summary>
        [DataMember(Order = 7)]
        public List<Field> Fields { get; set; }

        /// <summary>
        /// A T4 template.  Applies to all languages, though code behind and view model must be language specific.
        /// </summary>
        public string View { get; set; }

        [DataMember(Name = "View", EmitDefaultValue = false, Order = 8)]
        private CDataWrapper ViewCData
        {
            get { return View; }
            set { View = value; }
        }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string CodeBehindCSharp { get; set; }

        [DataMember(Name = "CodeBehindCSharp", EmitDefaultValue = false, Order = 9)]
        private CDataWrapper CodeBehindCSharpCData
        {
            get { return CodeBehindCSharp; }
            set { CodeBehindCSharp = value; }
        }

        /// <summary>
        /// A T4 template, C# version.
        /// </summary>
        public string ViewModelCSharp { get; set; }

        [DataMember(Name = "ViewModelCSharp", EmitDefaultValue = false, Order = 10)]
        private CDataWrapper ViewModelCSharpCData
        {
            get { return ViewModelCSharp; }
            set { ViewModelCSharp = value; }
        }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string CodeBehindVisualBasic { get; set; }

        [DataMember(Name = "CodeBehindVisualBasic", EmitDefaultValue = false, Order = 11)]
        private CDataWrapper CodeBehindVisualBasicCData
        {
            get { return CodeBehindVisualBasic; }
            set { CodeBehindVisualBasic = value; }
        }

        /// <summary>
        /// A T4 template, VB version.
        /// </summary>
        public string ViewModelVisualBasic { get; set; }

        [DataMember(Name = "ViewModelVisualBasic", EmitDefaultValue = false, Order = 12)]
        private CDataWrapper ViewModelVisualBasicCData
        {
            get { return ViewModelVisualBasic; }
            set { ViewModelVisualBasic = value; }
        }
    }
}
