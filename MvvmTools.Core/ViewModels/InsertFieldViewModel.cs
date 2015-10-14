using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class InsertFieldViewModel
    {
        public FieldDialogViewModel Field { get; set; }
        public string Value { get; set; }

        public static InsertFieldViewModel Create(IKernel kernel, FieldDialogViewModel field, string @value)
        {
            var vm = kernel.Get<InsertFieldViewModel>();
            vm.Init(field, @value);
            return vm;
        }

        public void Init(FieldDialogViewModel field, string @value)
        {
            Field = field;
            Value = @value;
        }
    }
}