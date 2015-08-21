using System.Collections.Generic;
using MvvmTools.Core.Services;

namespace MvvmTools.Core.ViewModels
{
    public class SelectFileDialogViewModel : BaseViewModel
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
