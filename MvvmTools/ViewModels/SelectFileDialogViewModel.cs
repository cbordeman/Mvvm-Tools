using System.Collections.Generic;
using Controls.ViewModels;
using MvvmTools.Services;
using MvvmTools.Utilities;

namespace MvvmTools.ViewModels
{
    internal class SelectFileDialogViewModel : BaseViewModel
    {
        public List<ProjectItemAndType> Documents { get; set; }

        public SelectFileDialogViewModel(List<ProjectItemAndType> documents)
        {
            Documents = documents;
        }

        #region SelectedDocument
        private ProjectItemAndType _selectedDocument;
        public ProjectItemAndType SelectedDocument
        {
            get { return _selectedDocument; }
            set { SetProperty(ref _selectedDocument, value); }
        }
        #endregion SelectedDocument

    }
}
