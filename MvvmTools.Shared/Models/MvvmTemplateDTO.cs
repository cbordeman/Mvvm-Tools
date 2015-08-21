namespace MvvmTools.Shared.Models
{
    public class MvvmTemplateDTO
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public string ViewModel { get; set; }
        public string View { get; set; }
    }
}
