using Ninject;

namespace MvvmTools.Core.ViewModels
{
    public class InsertFieldViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }

        public static InsertFieldViewModel Create(IKernel kernel, string name, string description, string @value)
        {
            var vm = kernel.Get<InsertFieldViewModel>();
            vm.Name = name;
            vm.Description = description;
            vm.Value = @value;
            return vm;
        }
    }
}