using MvvmTools.Core.Models;

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

        public string GroupName { get; private set; }

        #region Auto
        private bool _auto;
        public bool Auto
        {
            get { return _auto; }
            set { SetProperty(ref _auto, value); }
        }
        #endregion Auto

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

        public void SetFromDescriptor(ProjectItemDescriptor descriptor)
        {
            _auto = descriptor.Auto;
            _pathOffProject = descriptor.PathOffProject;
            _namespace = descriptor.Namespace;
        }

        public ProjectItemDescriptor GetDescriptor()
        {
            return new ProjectItemDescriptor
            {
                Auto = Auto,
                PathOffProject = PathOffProject,
                Namespace = Namespace
            };
        }

        #endregion Public Methods

        #region Private Helpers and Event Handlers



        #endregion Private Helpers and Event Handlers
    }
}