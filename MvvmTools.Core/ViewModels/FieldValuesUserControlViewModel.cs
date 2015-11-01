using System.Collections.Generic;
using MvvmTools.Core.Models;

namespace MvvmTools.Core.ViewModels
{
    public class FieldValuesUserControlViewModel : BaseViewModel
    {
        public void Init(IEnumerable<FieldDialogViewModel> fields)
        {
            var fields2 = new List<FieldValueUserControlViewModel>();
            foreach (var f in fields)
            {
                var nf = FieldValueUserControlViewModel.CreateFrom(Kernel, f);
                fields2.Add(nf);
            }
            Fields = fields2;
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
