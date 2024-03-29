﻿using MvvmTools.Services;
using System.Collections.Generic;
using Unity;

namespace MvvmTools.ViewModels
{
    public class SelectFileDialogViewModel : BaseViewModel
    {
        public List<ProjectItemAndType> Documents { get; set; }

        public SelectFileDialogViewModel(IEnumerable<ProjectItemAndType> documents, IUnityContainer container) : base(container)
        {
            Documents = new List<ProjectItemAndType>(documents);
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