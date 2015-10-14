using System.Collections.Generic;
using System.Linq;
using MvvmTools.Core.Models;
using MvvmTools.Core.ViewModels;

namespace MvvmTools.Core.Views
{
    public class FieldValuesUserControlViewModel : BaseViewModel
    {
        public void Init(List<Field> fields)
        {
            Fields = new List<FieldValueUserControlViewModel>(fields.Select(f => FieldValueUserControlViewModel.CreateFrom(Kernel, f)));
        }

        #region FieldValues
        private List<FieldValueUserControlViewModel> _fields;
        public List<FieldValueUserControlViewModel> Fields
        {
            get { return _fields; }
            set { SetProperty(ref _fields, value); }
        }
        #endregion FieldValues
    }
}
