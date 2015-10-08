using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Models
{
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

        public Field(FieldDialogViewModel vm)
        {
            Name = vm.Name;
            Default = vm.SelectedFieldType == FieldType.CheckBox ? vm.DefaultBoolean.ToString() : vm.DefaultString;
            Prompt = vm.Prompt;
            Description = vm.Description;
            Debug.Assert(vm.SelectedFieldType.HasValue, "FieldType shouldn't be null.");
            FieldType = vm.SelectedFieldType.Value;
            if (vm.Choices != null)
                Choices = ((ObservableCollection<StringViewModel>) vm.Choices.SourceCollection).Select(c => c.Value).ToArray();
        }

        /// <summary>
        /// The property name, must be a valid C# identifier.
        /// </summary>
        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public FieldType FieldType { get; set; }

        /// <summary>
        /// Optional, but can contain hyperlinks and line breaks.
        /// </summary>
        [DataMember(Order = 3)]
        public string Description { get; set; }

        /// <summary>
        /// Default value when presented to user.
        /// </summary>
        [DataMember(Order = 4)]
        public string Default { get; set; }

        /// <summary>
        /// Required.  Goes before the field.
        /// </summary>
        [DataMember(Order = 5)]
        public string Prompt { get; set; }

        /// <summary>
        /// If non-empty, indicates a combobox.
        /// </summary>
        [DataMember(Order = 6)]
        public string[] Choices { get; set; }
    }
}