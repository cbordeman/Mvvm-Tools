using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using MvvmTools.Shared.Models;

namespace MvvmTools.Web.Models
{
    public class TemplateEditViewModel
    {
        public MvvmTemplate Template { get; set; }
        public int SelectedCategoryId { get; set; }
        public string SelectedLanguage { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> Languages { get; set; }

        public TemplateEditViewModel(MvvmTemplate template, IEnumerable<MvvmTemplateCategory> categories, int selectedCategoryId, string selectedLanguage)
        {
            Template = template;
            SelectedCategoryId = selectedCategoryId;
            SelectedLanguage = selectedLanguage;

            // Categories
            Categories = new List<SelectListItem>();
            var cquery =
                from cg in categories
                orderby cg.Name.ToUpper()
                select new SelectListItem { Text = cg.Name, Value = cg.Id.ToString(), Selected = SelectedCategoryId == cg.Id };
            Categories.AddRange(cquery);

            // Languages
            Languages = new List<SelectListItem>
            {
                new SelectListItem {Text = "C#", Value = "C#", Selected = SelectedLanguage == "C#"},
                new SelectListItem {Text = "VB", Value = "VB", Selected = SelectedLanguage == "VB"}
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
        public string Author { get; set; }

        [Display(Name = "Show Templates")]
        public bool ShowTemplates { get; set; }
    }
}
