using System.Collections.Generic;
using Controls.ViewModels;
using MvvmTools.Utilities;

namespace MvvmTools.ViewModels
{
    public class SelectFileDialogViewModel : BaseViewModel
    {
        public List<ProjectItemAndType> Documents { get; set; }

        public SelectFileDialogViewModel(List<ProjectItemAndType> documents)
        {
            Documents = documents;
        }
    }
}
