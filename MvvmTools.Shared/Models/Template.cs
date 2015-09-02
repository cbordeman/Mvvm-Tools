namespace MvvmTools.Shared.Models
{
    public class Template
    {
        public Template()
        {
            
        }

        public Template(int id, bool enabled, string author, string name, string language, string category, string tags, string viewModel, string view)
        {
            Id = id;
            Enabled = enabled;
            Author = author;
            Name = name;
            Language = language;
            Category = category;
            Tags = tags;
            ViewModel = viewModel;
            View = view;
        }

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
