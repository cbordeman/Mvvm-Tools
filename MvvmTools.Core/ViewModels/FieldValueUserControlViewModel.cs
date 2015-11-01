using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmTools.Core.Controls;
using MvvmTools.Core.Extensions;
using MvvmTools.Core.Models;
using Ninject;

// ReSharper disable PossibleInvalidOperationException

namespace MvvmTools.Core.ViewModels
{
    public class FieldValueUserControlViewModel : FieldDialogViewModel, ISuggestionsProvider
    {
        public new static FieldValueUserControlViewModel CreateFrom(IKernel kernel, FieldDialogViewModel field)
        {
            var fvvm = kernel.Get<FieldValueUserControlViewModel>();
            fvvm.CopyFrom(field);
            return fvvm;
        }

        #region Properties

        public Predicate<object> FilterOnType
        {
            get
            {
                return o =>
                {
                    // If nothing typed, show all items.
                    if (string.IsNullOrWhiteSpace(DefaultString))
                        return true;

                    // If matching an existing item, show all items.
                    //if (_typesSource.Any(ts => ts.ToString().Equals(DefaultString, StringComparison.OrdinalIgnoreCase)))
                    //    return true;

                    // Case-insensitive contains.
                    var aqt = (string) o;
                    //if (string.IsNullOrEmpty(aqt.Class) && string.IsNullOrEmpty(aqt.Assembly))
                    //    return true;
                    var rval = aqt.ContainsInsensitive(DefaultString);
                    return rval;
                };
            }
        }

        #region Types
        private ObservableCollection<AssemblyQualifiedType> _types;
        public ObservableCollection<AssemblyQualifiedType> Types
        {
            get { return _types; }
            set { SetProperty(ref _types, value); }
        }
        #endregion Types

        #region ShowTextBox

        public bool ShowTextBox => SelectedFieldType.Value == FieldType.TextBox || SelectedFieldType.Value == FieldType.TextBoxMultiLine;
        public bool ShowTextBoxMultiline => SelectedFieldType.Value == FieldType.TextBoxMultiLine;
        public bool ShowCheckBox => SelectedFieldType.Value == FieldType.CheckBox;
        public bool ShowComboBox => SelectedFieldType.Value == FieldType.ComboBox || SelectedFieldType.Value == FieldType.ComboBoxOpen;
        public bool ShowComboBoxOpen => SelectedFieldType.Value == FieldType.ComboBoxOpen;
        public bool ShowClass => SelectedFieldType.Value == FieldType.Class;

        public string PromptWithColon => Prompt + ':';

        public List<string> ChoicesAsString => ChoicesSource.Select(c => c.Value).ToList();

        #endregion ShowTextBox

        #endregion Properties

        #region Virtuals

        public override void CopyFrom(FieldDialogViewModel field)
        {
            base.CopyFrom(field);

            if (field.SelectedFieldType == FieldType.Class)
            {
                Types = new ObservableCollection<AssemblyQualifiedType>
                {
                    new AssemblyQualifiedType("", null),
                    new AssemblyQualifiedType("My.NamespaceA.Type1", "My.Assembly"),
                    new AssemblyQualifiedType("My.NamespaceA.Type2", "My.Assembly"),
                    new AssemblyQualifiedType("My.NamespaceB.Type1", "My.Assembly"),
                    new AssemblyQualifiedType("Another.Namespace.And.AUserControl", "Another.Assembly"),
                    new AssemblyQualifiedType("Another.Namespace.And.SomeWindow", "Another.Assembly"),
                    new AssemblyQualifiedType("Another.Namespace.And.Some2Window", "Another.Assembly")
                };
            }
        }

        #endregion Virtuals

        public Task<object[]> GetSuggestions(string filter, CancellationToken ct)
        {
            if (Types == null || string.IsNullOrEmpty(filter))
                return Task.FromResult((object[])null);
            return Task.FromResult(Types.Where(s => s.ClassAndAssembly.ContainsInsensitive(filter)).Cast<object>().ToArray());
        }
    }
}