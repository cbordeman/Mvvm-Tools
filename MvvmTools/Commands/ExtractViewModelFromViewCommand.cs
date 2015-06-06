//------------------------------------------------------------------------------
// <copyright file="ExtractViewModelFromViewCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using MvvmTools.Utilities;

// ReSharper disable HeapView.BoxingAllocation

namespace MvvmTools.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExtractViewModelFromViewCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractViewModelFromViewCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        internal ExtractViewModelFromViewCommand(MvvmToolsPackage package)
            : base(package, Constants.ExtractViewModelFromViewCommandId)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExtractViewModelFromViewCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(MvvmToolsPackage package)
        {
            Instance = new ExtractViewModelFromViewCommand(package);
        }

        protected override void OnExecute()
        {
            base.OnExecute();

            if (Package.ActiveDocument?.ProjectItem != null)
            {
                var classesInFile = SolutionUtilities.GetClassesInProjectItem(Package.ActiveDocument.ProjectItem);

                if (classesInFile.Count == 0)
                {
                    MessageBox.Show("No classes found in file.", "MVVM Tools");
                    return;
                }

                var docs = SolutionUtilities.GetRelatedDocuments(Package.ActiveDocument.ProjectItem, classesInFile.Select(c => c.Class));

                if (docs.Count == 0)
                {
                    var classes = "\n        ";
                    foreach (var c in classesInFile)
                        classes += c.Class + "\n        ";

                    MessageBox.Show(string.Format("Couldn't find any corresponding classes.\n\nClasses found in this file ({0}):\n{1}",
                        Package.ActiveDocument.FullName,
                        classes), "MVVM Tools");

                    return;
                }

                if (docs.Count == 1)
                {
                    var win = docs[0].ProjectItem.Open();
                    win.Visible = true;
                    win.Activate();
                }
                else
                {
                    // Multiple result, let user choose.
                    PresentViewViewModelOptions(docs);
                }
            }
        }

        private void PresentViewViewModelOptions(List<ProjectItemAndType> docs)
        {
            var window = new DialogWindow
            {
                Title = "Select File - MVVM Tools",
                Width = 600,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true
            };

            var grid = new Grid {Margin = new Thickness(5, 5, 5, 0)};
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            
            // Choices in a ListBox.
            var listView = new ListView { ItemsSource = docs };
            var gv = new GridView();
            gv.Columns.Add(CreateGridViewColumn("File", ".ProjectItem.Name"));
            gv.Columns.Add(CreateGridViewColumn("Class", "Type.Class"));
            gv.Columns.Add(CreateGridViewColumn("Project", "ProjectItem.ContainingProject.Name"));
            gv.Columns.Add(CreateGridViewColumn("Namespace", "RelativeNamespace"));
            listView.View = gv;
            listView.Loaded += (sender, args) =>
            {
                listView.Focus();
                listView.SelectedItem = listView.Items[0];
                // Have to do this because the ListView doesn't fully select the first item, user would
                // otherwise have to press down twice to get the selection to move to the second item.
                PressKey(listView, Key.Down);
            };
            grid.Children.Add(listView);

            // Bottom StackPanel for the OK and Cancel buttons.
            var btnStackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0),
                Orientation = Orientation.Horizontal
            };
            Grid.SetRow(btnStackPanel, 1);
            grid.Children.Add(btnStackPanel);

            // Add OK and Cancel buttons.
            var okBtn = new DialogButton
            {
                Content = "_OK",
                IsDefault = true,
                Height = 30,
                VerticalAlignment = VerticalAlignment.Top
            };

            okBtn.Click += (sender, args) =>
            {
                if (listView.SelectedItem != null)
                    window.DialogResult = true;
            };
            var cancelBtn = new DialogButton
            {
                Content = "Cancel",
                Height = 30,
                VerticalAlignment = VerticalAlignment.Top
            };
            cancelBtn.Click += (sender, args) => { window.DialogResult = false; };

            //// Weirdly, the OK button wants to change to a really tall height.  This line is a hack to make it right.
            //okBtn.Loaded += (sender, args) => { okBtn.Height = cancelBtn.ActualHeight; };

            btnStackPanel.Children.Add(okBtn);
            btnStackPanel.Children.Add(cancelBtn);

            window.Content = grid;

            var result = window.ShowDialog();

            if (result.GetValueOrDefault())
            {
                // Go to the selected project item.
                var win = ((ProjectItemAndType)listView.SelectedValue).ProjectItem.Open();
                win.Visible = true;
                win.Activate();
            }
        }

        private void PressKey(Visual targetVisual, Key key)
        {
            var target = Keyboard.FocusedElement;    // Target element
            var routedEvent = Keyboard.KeyDownEvent; // Event to send

            target.RaiseEvent(
              new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(targetVisual),
                0,
                key)
              { RoutedEvent = routedEvent }
            );
        }

        private GridViewColumn CreateGridViewColumn(string header, string binding)
        {
            var col = new GridViewColumn { Header = header };
            //col.HeaderContainerStyle.Setters.Add(new Setter
            //{
            //    Property = FrameworkElement.HorizontalAlignmentProperty,
            //    Value = HorizontalAlignment.Left
            //});
            var dt = new DataTemplate();
            var tbFactory = new FrameworkElementFactory(typeof(TextBlock));
            tbFactory.SetBinding(TextBlock.TextProperty, new Binding(binding));
            col.CellTemplate = dt;
            col.CellTemplate.VisualTree = tbFactory;

            return col;
        }
    }
}
