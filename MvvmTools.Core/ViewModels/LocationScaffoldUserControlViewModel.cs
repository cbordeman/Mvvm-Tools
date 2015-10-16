using System.Collections.Generic;
using System.ComponentModel;
using MvvmTools.Core.Models;
using MvvmTools.Core.Utilities;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace MvvmTools.Core.ViewModels
{
    public class LocationScaffoldUserControlViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Data

        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties

        #region Projects
        private IList<ProjectOptions> _projects;
        public IList<ProjectOptions> Projects
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

        #endregion Properties

        #region Commands

        
        #endregion Commands

        #region Virtuals

        #endregion Virtuals

        #region Public Methods

        public void Init(IList<ProjectOptions> projects, LocationDescriptor descriptor, ProjectOptions settingsProject)
        {
            Projects =  projects;
            
            // If descriptor's ProjectId is null, use settingsProject's.
            ProjectIdentifier = descriptor.ProjectIdentifier ?? settingsProject.ProjectModel.ProjectIdentifier;
            PathOffProject = descriptor.PathOffProject;
            Namespace = descriptor.Namespace;
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

        public string Error => ValidationUtilities.ValidatePathOffProject(PathOffProject) != null ||
                               ValidationUtilities.ValidateNamespace(Namespace) != null
            ? "There are errors"
            : null;

        #endregion IDataErrorInfo
    }
}