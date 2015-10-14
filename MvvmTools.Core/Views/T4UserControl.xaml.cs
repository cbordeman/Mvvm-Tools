using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    /// <summary>
    /// Interaction logic for T4UserControl.xaml
    /// </summary>
    public partial class T4UserControl
    {
        public T4UserControl()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            TextEditor.Options.ConvertTabsToSpaces = true;
            PreviewTextEditor.Options.ConvertTabsToSpaces = true;

            TextEditor.Document.Changed += TextEditorOnDocumentChanged;
        }

        private void TextEditorOnDocumentChanged(object sender, EventArgs eventArgs)
        {
            var vm = (T4UserControlViewModel)DataContext;
            vm.Buffer = TextEditor.Text;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                var vm = (T4UserControlViewModel)args.NewValue;
                using (var stream = GenerateStreamFromString(vm.Buffer))
                    TextEditor.Load(stream);
                using (var stream = GenerateStreamFromString(vm.Preview))
                    PreviewTextEditor.Load(stream);

                vm.PropertyChanged += VmOnPropertyChanged;
            }
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(T4UserControlViewModel.Preview))
            {
                var vm = (T4UserControlViewModel)DataContext;
                if (vm != null)
                    using (var stream = GenerateStreamFromString(vm.Preview))
                        PreviewTextEditor.Load(stream);
            }
        }

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        
        private void InsertFieldButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var vm = (InsertFieldViewModel) button.DataContext;
            TextEditor.Document.Insert(TextEditor.TextArea.Caret.Offset, vm.Value);
            InsertFieldSplitButton.IsOpen = false;
        }
    }
}