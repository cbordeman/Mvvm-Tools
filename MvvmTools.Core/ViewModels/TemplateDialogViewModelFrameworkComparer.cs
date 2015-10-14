using System;
using System.Collections.Generic;

namespace MvvmTools.Core.ViewModels
{
    internal class TemplateDialogViewModelFrameworkComparer : IEqualityComparer<TemplateDialogViewModel>
    {
        public bool Equals(TemplateDialogViewModel x, TemplateDialogViewModel y)
        {
            return string.Equals(x.Framework, y.Framework, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(TemplateDialogViewModel obj) => obj.Framework.GetHashCode();
    }
}