using System.Collections.Generic;
using System.Linq;
using MvvmTools.Core.Models;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class FieldValueUserControlViewModel : FieldDialogViewModel
    {
        public static FieldValueUserControlViewModel CreateFrom(IKernel kernel, Field field)
        {
            var fvvm = kernel.Get<FieldValueUserControlViewModel>();
            fvvm.CopyFrom(field);
            return fvvm;
        }

        #region Properties

        #region ShowTextBox

        public bool ShowTextBox => SelectedFieldType.Value == FieldType.TextBox;
        public bool ShowTextBoxMultiline => SelectedFieldType.Value == FieldType.TextBoxMultiLine;
        public bool ShowCheckBox => SelectedFieldType.Value == FieldType.CheckBox;
        public bool ShowComboBox => SelectedFieldType.Value == FieldType.ComboBox;
        public bool ShowComboBoxOpen => SelectedFieldType.Value == FieldType.ComboBoxOpen;

        public string PromptWithColon => Prompt + ':';

        public List<string> ChoicesAsString => ChoicesSource.Select(c => c.Value).ToList();

        #endregion ShowTextBox

        #endregion Properties
    }
}