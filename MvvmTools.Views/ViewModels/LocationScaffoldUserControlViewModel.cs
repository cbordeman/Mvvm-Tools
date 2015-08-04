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

        public void Init(IList<ProjectOptions> projects, LocationDescriptor descriptor)
        {
            Projects =  projects;
            
            ProjectIdentifier = descriptor.ProjectIdentifier;
            PathOffProject = descriptor.PathOffProject;
            Namespace = descriptor.Namespace;
            AppendViewType = descriptor.AppendViewType;
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