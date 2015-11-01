using System;
using System.Runtime.Serialization;

// ReSharper disable LocalizableElement

namespace MvvmTools.Core.Models
{
    [DataContract]
    public enum FieldType
    {
        [EnumMember]
        TextBox,
        [EnumMember]
        TextBoxMultiLine,
        [EnumMember]
        CheckBox,
        [EnumMember]
        ComboBox,
        [EnumMember]
        ComboBoxOpen,
        [EnumMember]
        Class
    }

    [DataContract]
    public enum FormFactor
    {
        [EnumMember]
        Phone,
        [EnumMember]
        Tablet,
        [EnumMember]
        Desktop
    }

    [DataContract]
    public enum Platform
    {
        // ReSharper disable once InconsistentNaming
        [EnumMember]
        WPF,
        [EnumMember]
        Silverlight,
        [EnumMember]
        Xamarin,
        // ReSharper disable once InconsistentNaming
        [EnumMember]
        WinRT
    }

    public static class Enums
    {
        public static string GetDescription(FieldType type)
        {
            switch (type)
            {
                case FieldType.TextBox:
                    return "Text Box";
                case FieldType.TextBoxMultiLine:
                    return "Text Box (Multiple Lines)";
                case FieldType.CheckBox:
                    return "Check Box";
                case FieldType.ComboBox:
                    return "Combo Box";
                case FieldType.ComboBoxOpen:
                    return "Combo Box (Free Form)";
                case FieldType.Class:
                    return "Class Selection";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "FieldType unknown.");
            }
        }
    }
}
