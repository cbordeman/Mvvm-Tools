using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using MvvmTools.Shared.Models;

namespace MvvmTools.Web.Models
{
    // We derive from one of our code first models to make mapping easier.
    // Because of that, code first and migrations see it as another entity 
    // class, so we apply [NotMapped] so they will not try to add it to 
    // the database.
    [NotMapped]
    public class TemplateCreateViewModel : MvvmTemplate
    {
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> Languages { get; set; }

        public TemplateCreateViewModel(IEnumerable<MvvmTemplateCategory> categories)
        {
            // Initial values.

            // User must turn on manually, that way unfinished templates aren't seen by the public.
            Enabled = false;
            MvvmTemplateCategoryId = 0;
            Language = "";

            // Categories
            Categories = new List<SelectListItem>
            {
                new SelectListItem {Text = "", Value = "", Selected = true }
            };
            var cquery =
                from cg in categories
                orderby cg.Name.ToUpper()
                select new SelectListItem { Text = cg.Name, Value = cg.Id.ToString() };
            Categories.AddRange(cquery);

            // Languages
            Languages = new List<SelectListItem>
            {
                new SelectListItem {Text = "", Value = "", Selected = true },
                new SelectListItem {Text = "C#", Value = "C#"},
                new SelectListItem {Text = "VB", Value = "VB"}
            };
        }
    }

    [NotMapped]
    public class TemplateEditViewModel : MvvmTemplate
    {
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> Languages { get; set; }

        public TemplateEditViewModel(MvvmTemplate template, IEnumerable<MvvmTemplateCategory> categories)
        {
            Name = template.Name;
            Enabled = template.Enabled;
            Tags = template.Tags;
            ViewModel = template.ViewModel;
            View = template.View;
            MvvmTemplateCategoryId = template.MvvmTemplateCategoryId;
            Language = template.Language;
            
            // Categories
            Categories = new List<SelectListItem>();
            var cquery =
                from cg in categories
                orderby cg.Name.ToUpper()
                select new SelectListItem { Text = cg.Name, Value = cg.Id.ToString(), Selected = MvvmTemplateCategoryId == cg.Id };
            Categories.AddRange(cquery);

            // Languages
            Languages = new List<SelectListItem>
            {
                new SelectListItem {Text = "C#", Value = "C#", Selected = Language == "C#"},
                new SelectListItem {Text = "VB", Value = "VB", Selected = Language == "VB"}
            };
        }
    }

    public class TemplateIndexViewModel
    {
        public TemplateIndexViewModel(string author, bool showTemplates, IEnumerable<MvvmTemplateDTO> templates, IEnumerable<ApplicationUser> authors, string selectedAuthor, int selectedCategoryId, IEnumerable<MvvmTemplateCategory> categories, string selectedLanguage, string search)
        {
            Author = author;
            ShowTemplates = showTemplates;
            Templates = templates;
            SelectedCategoryId = selectedCategoryId;
            SelectedAuthor = selectedAuthor;
            SelectedLanguage = selectedLanguage;
            Search = search;

            // Categories
            Categories = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = SelectedCategoryId == 0}
            };
            var cquery =
                from cg in categories
                orderby cg.Name.ToUpper()
                select new SelectListItem { Text = cg.Name, Value = cg.Id.ToString(), Selected = SelectedCategoryId == cg.Id };
            Categories.AddRange(cquery);

            // Languages
            Languages = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = SelectedLanguage == null},
                new SelectListItem {Text = "C#", Value = "C#", Selected = SelectedLanguage == "C#"},
                new SelectListItem {Text = "VB", Value = "VB", Selected = SelectedLanguage == "VB"}
            };

            // Authors
            Authors = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = SelectedAuthor == null},
                new SelectListItem {Text = "Factory", Value = "Factory", Selected = SelectedAuthor == "Factory"},
            };
            var aquery =
                from a in authors
                orderby a.Author.ToUpper()
                where a.Author != "Factory"
                select new SelectListItem { Text = a.Author, Value = a.Author, Selected = SelectedAuthor == a.Author };
            Authors.AddRange(aquery);
        }

        public IEnumerable<MvvmTemplateDTO> Templates { get; set; }

        public string Name { get; set; }

        public List<SelectListItem> Categories { get; set; }
        public int SelectedCategoryId { get; set; }
        public List<SelectListItem> Languages { get; set; }
        public string SelectedLanguage { get; set; }
        public List<SelectListItem> Authors { get; set; }
        public string SelectedAuthor { get; set; }
        public string Search { get; set; }

        [Required]
        [Remote("AuthorAvailable", "Validation", ErrorMessage = "That {0} already exists.")]
        public string Author { get; set; }

        [Display(Name = "Show My Templates")]
        public bool ShowTemplates { get; set; }
    }
}
