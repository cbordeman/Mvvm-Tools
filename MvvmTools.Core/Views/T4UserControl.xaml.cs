using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MvvmTools.Core.Extensions;
using MvvmTools.Core.Services;
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

            FontSizeComboBox.ItemsSource = new[]
            {
                8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20, 20, 24, 26, 28, 30, 36
            };
            
            DataContextChanged += OnDataContextChanged;

            TextEditor.Options.ConvertTabsToSpaces = true;
            PreviewTextEditor.Options.ConvertTabsToSpaces = true;

            TextEditor.Document.Changed += TextEditorOnDocumentChanged;
        }

        private bool _documentChanging;

        private void TextEditorOnDocumentChanged(object sender, EventArgs eventArgs)
        {
            _documentChanging = true;
            var vm = (T4UserControlViewModel)DataContext;
            vm.Buffer = TextEditor.Text;
            _documentChanging = false;
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
                if (vm != null && vm.Errors?.Count == 0)
                    using (var stream = GenerateStreamFromString(vm.Preview))
                        PreviewTextEditor.Load(stream);
            }
            else if (args.PropertyName == nameof(T4UserControlViewModel.Buffer))
            {
                if (_documentChanging)
                    return;
                var vm = (T4UserControlViewModel)DataContext;
                if (vm != null)
                    using (var stream = GenerateStreamFromString(vm.Buffer))
                        TextEditor.Load(stream);
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
            TextEditor.Document.Insert(TextEditor.TextArea.Caret.Offset, vm.Name);
            InsertFieldSplitButton.IsOpen = false;
            TextEditor.Focus();
        }

        private void InsertFieldWithBracketsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var vm = (InsertFieldViewModel)button.DataContext;
            TextEditor.Document.Insert(TextEditor.TextArea.Caret.Offset, $"<#= {vm.Name} #>");
            InsertFieldSplitButton.IsOpen = false;
            TextEditor.Focus();
        }

        private void ErrorsDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Cast<T4Error>().Any())
            {
                var error = e.AddedItems[0] as T4Error;
                if (error != null)
                {
                    TextEditor.TextArea.Caret.Line = error.Line;
                    TextEditor.TextArea.Caret.Column = error.Column;
                    TextEditor.Focus();
                }
            }
        }

        #region MyFontSize
        public int MyFontSize
        {
            get { return (int)GetValue(MyFontSizeProperty); }
            set { SetValue(MyFontSizeProperty, value); }
        }
        public static readonly DependencyProperty MyFontSizeProperty =
            DependencyProperty.Register("MyFontSize", typeof(int), typeof(T4UserControl), new PropertyMetadata(13));
        #endregion FontSize

        private void ViewHeaderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (T4UserControlViewModel)DataContext;

            MessageBox.Show(
                "These directives will be automatically added to the top of this T4 buffer before transformation:\n\n" +
                vm.HeaderFirstPart + "\n\n...in addition to <#@ parameter #> tags for each custom field.  See the Insert Field button.", "Header");
            
            TextEditor.Focus();
        }
    }
}