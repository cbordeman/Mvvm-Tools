using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using Ninject;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace MvvmTools.Core.ViewModels
{
    public class LocationDescriptorUserControlViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Data

        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public ISolutionService SolutionService { get; set; }

        public LocationDescriptorUserControlViewModel Inherited { get; set; }

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
            set
            {
                if (SetProperty(ref _projectIdentifier, value))
                    ResetProjectIdentifierCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion ProjectIdentifier

        #region PathOffProject
        private string _pathOffProject;
        public string PathOffProject
        {
            get { return _pathOffProject; }
            set
            {
                if (SetProperty(ref _pathOffProject, value))
                    ResetPathOffProjectCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion PathOffProject

        #region Namespace
        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set
            {
                if (SetProperty(ref _namespace, value))
                    ResetNamespaceCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion Namespace

        #region AppendViewType
        private bool _appendViewType;
        public bool AppendViewType
        {
            get { return _appendViewType; }
            set
            {
                if (SetProperty(ref _appendViewType, value))
                    ResetAppendViewTypeCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion AppendViewType

        #endregion Properties

        #region Commands

        #region ResetPathOffProjectCommand
        DelegateCommand _resetPathOffProjectCommand;
        public DelegateCommand ResetPathOffProjectCommand => _resetPathOffProjectCommand ?? (_resetPathOffProjectCommand = new DelegateCommand(ExecuteResetPathOffProjectCommand, CanResetPathOffProjectCommand));
        public bool CanResetPathOffProjectCommand() => PathOffProject != Inherited?.PathOffProject;
        public void ExecuteResetPathOffProjectCommand()
        {
            PathOffProject = Inherited.PathOffProject;
        }
        #endregion

        #region ResetNamespaceCommand
        DelegateCommand _resetNamespaceCommand;
        public DelegateCommand ResetNamespaceCommand => _resetNamespaceCommand ?? (_resetNamespaceCommand = new DelegateCommand(ExecuteResetNamespaceCommand, CanResetNamespaceCommand));
        public bool CanResetNamespaceCommand() => Namespace != Inherited?.Namespace;
        public void ExecuteResetNamespaceCommand()
        {
            Namespace = Inherited.Namespace;
        }
        #endregion
        
        #region ResetProjectIdentifierCommand
        DelegateCommand _resetProjectIdentifierCommand;
        public DelegateCommand ResetProjectIdentifierCommand => _resetProjectIdentifierCommand ?? (_resetProjectIdentifierCommand = new DelegateCommand(ExecuteResetProjectIdentifierCommand, CanResetProjectIdentifierCommand));
        public bool CanResetProjectIdentifierCommand() => ProjectIdentifier != Inherited?.ProjectIdentifier;
        public void ExecuteResetProjectIdentifierCommand()
        {
            ProjectIdentifier = Inherited.ProjectIdentifier;
        }
        #endregion

        #region ResetAppendViewTypeCommand
        DelegateCommand _resetAppendViewTypeCommand;
        public DelegateCommand ResetAppendViewTypeCommand => _resetAppendViewTypeCommand ?? (_resetAppendViewTypeCommand = new DelegateCommand(ExecuteResetAppendViewTypeCommand, CanResetAppendViewTypeCommand));
        public bool CanResetAppendViewTypeCommand() => AppendViewType != Inherited?.AppendViewType;
        public void ExecuteResetAppendViewTypeCommand()
        {
            AppendViewType = Inherited.AppendViewType;
        }
        #endregion
        
        #endregion Commands

        #region Virtuals

        #endregion Virtuals

        #region Public Methods

        public void SetFromDescriptor(LocationDescriptor descriptor)
        {
            _projectIdentifier = descriptor.ProjectIdentifier;
            _pathOffProject = descriptor.PathOffProject;
            _namespace = descriptor.Namespace;
            _appendViewType = descriptor.AppendViewType;
        }

        public LocationDescriptor GetDescriptor()
        {
            return new LocationDescriptor
            {
                ProjectIdentifier = ProjectIdentifier,
                PathOffProject = PathOffProject,
                Namespace = Namespace,
                AppendViewType = AppendViewType
            };
        }

        // Scans the solution and initializes.
        public async Task InitializeFromSolution()
        {
            var solution = await SolutionService.GetSolution();

            var projects = new List<ProjectModel>
            {
                new ProjectModel("(current project)", null, null, ProjectKind.Project, null)
            };
            AddProjectsFlattenedRecursive(projects, solution.Children);

            // Have to save and restore the project id because the XAML binding engine nulls it.
            var save = ProjectIdentifier;
            Projects = projects;
            ProjectIdentifier = save;
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
                            p.FullPath,
                            p.ProjectIdentifier, 
                            p.Kind,
                            p.KindId));
                        break;
                    case ProjectKind.ProjectFolder:
                        return;
                }

                AddProjectsFlattenedRecursive(
                    projects,
                    p.Children, 
                    prefix + p.Name + '/');
            }
        }

        public bool IsInherited
        {
            get
            {
                if (Inherited == null)
                    return false;

                if (ProjectIdentifier != Inherited.ProjectIdentifier)
                    return false;
                if (PathOffProject != Inherited.PathOffProject)
                    return false;
                if (Namespace != Inherited.Namespace)
                    return false;
                if (ProjectIdentifier != Inherited.ProjectIdentifier)
                    return false;
                if (AppendViewType != Inherited.AppendViewType)
                    return false;

                return true;
            }
        }

        public void ResetToInherited()
        {
            ProjectIdentifier = Inherited.ProjectIdentifier;
            PathOffProject = Inherited.PathOffProject;
            Namespace = Inherited.Namespace;
            ProjectIdentifier = Inherited.ProjectIdentifier;
            AppendViewType = Inherited.AppendViewType;
        }

        #endregion Public Methods

        #region Private Helpers and Event Handlers

        #endregion Private Helpers and Event Handlers

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "PathOffProject":
                        return ValidationUtilities.ValidatePathOffProject(PathOffProject);

                    case "Namespace":
                        return ValidationUtilities.ValidateNamespace(Namespace);
                }
                return null;
            }
        }
        
        public string Error => null;

        #endregion IDataErrorInfo
    }
}