namespace MvvmTools.Core.ViewModels
{
    public class ValueDescriptor<T>
    {
        public ValueDescriptor(T value, string description)
        {
            Value = value;
            Description = description;
        }

        public T Value { get; set; }
        public string Description { get; set; }
    }
}