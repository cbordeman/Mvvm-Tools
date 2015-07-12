using System.Collections.Generic;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class ProjectItemDescriptorDialogViewModel : BaseDialogViewModel
    {
        #region Data

        #endregion Data

        #region Ctor and Init

        public ProjectItemDescriptorDialogViewModel(string title)
        {
            Title = GroupName = title;
        }

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public ISolutionService SolutionService { get; set; }

        // Used soley as the RadioButton controls' GroupName in OptionsUserControl.xaml.  
        // This value shouldn't be modified.  The Auto property is also used in
        // in OptionsUserControl.xaml.
        public string GroupName { get; set; }

        #region Auto
        private bool _auto;
        public bool Auto
        {
            get { return _auto; }
            set { SetProperty(ref _auto, value); }
        }
        #endregion Auto

        #region Projects
        private List<ProjectModel> _projects;
        public List<ProjectModel> Projects
        {
            get { return _projects; }
            set { SetProperty(ref _projects, value); }
        }
        #endregion Projects

        #region ProjectIdentifier
        private string _projectIdentifier;
        public string ProjectIdentifier
        {
            get { return _projectIdentifier; }
            set { SetProperty(ref _projectIdentifier, value); }
        }
        #endregion ProjectIdentifier

        #region PathOffProject
        private string _pathOffProject;
        public string PathOffProject
        {
            get { return _pathOffProject; }
            set { SetProperty(ref _pathOffProject, value); }
        }
        #endregion PathOffProject

        #region Namespace
        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set { SetProperty(ref _namespace, value); }
        }
        #endregion Namespace

        #region AppendViewType
        private bool _appendViewType;
        public bool AppendViewType
        {
            get { return _appendViewType; }
            set { SetProperty(ref _appendViewType, value); }
        }
        #endregion AppendViewType

        #endregion Properties

        #region Commands



        #endregion Commands

        #region Virtuals



        #endregion Virtuals

        #region Public Methods

        public void SetFromDescriptor(LocationDescriptor descriptor)
        {
            _auto = descriptor.Auto;
            _projectIdentifier = descriptor.ProjectIdentifier;
            _pathOffProject = descriptor.PathOffProject;
            _namespace = descriptor.Namespace;
            _appendViewType = descriptor.AppendViewType;
        }

        public LocationDescriptor GetDescriptor()
        {
            return new LocationDescriptor
            {
                Auto = Auto,
                ProjectIdentifier = ProjectIdentifier,
                PathOffProject = PathOffProject,
                Namespace = Namespace,
                AppendViewType = AppendViewType
            };
        }

        // Scans the solution and initializes.
        public void InitializeFromSolution()
        {
            var solutionTree = SolutionService.GetSolutionTree();

            var projects = new List<ProjectModel>
            {
                new ProjectModel("(current project)", null, ProjectKind.Project)
            };
            AddProjectsFlattenedRecursive(projects, solutionTree);

            // Have to save and restore the project id because the XAML binding engine nulls it.
            var save = this.ProjectIdentifier;
            Projects = projects;
            this.ProjectIdentifier = save;
        }

        private void AddProjectsFlattenedRecursive(List<ProjectModel> projects, IEnumerable<ProjectModel> solutionTree, string prefix = null)
        {
            foreach (var p in solutionTree)
            {
                switch (p.Kind)
                {
                    case ProjectKind.Project:
                        projects.Add(new ProjectModel(
                            prefix + p.Name, 
                            p.ProjectIdentifier, p.Kind));
                        break;
                    case ProjectKind.ProjectFolder:
                        return;
                }

                AddProjectsFlattenedRecursive(
                    projects,
                    p.Children, 
                    prefix + p.Name + '\\');
            }
        }

        #endregion Public Methods

        #region Private Helpers and Event Handlers

        #endregion Private Helpers and Event Handlers
    }
}