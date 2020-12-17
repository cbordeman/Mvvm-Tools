using MvvmTools.Extensions;
using Unity;

namespace MvvmTools.ViewModels
{
    public class InsertFieldViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        #region Value
        private object _value;
        public object Value
        {
            get { return _value ?? (TypeDesc == "Boolean" ? (object)false : string.Empty); }
            set { _value = value; }
        }
        #endregion Value

        public string Type { get; set; }
        public string TypeDesc { get; set; }

        public static InsertFieldViewModel Create(IUnityContainer container, string name, string type, string description, object @value)
        {
            var vm = container.Resolve<InsertFieldViewModel>();
            vm.Name = name;
            vm.Type = type;
            vm.Description = description;
            vm.Value = @value;
            vm.TypeDesc = vm.Type.ClassFromFullName();
            return vm;
        }
    }
}