using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web.Mvc;
using MvvmTools.Shared.Models;

namespace MvvmTools.Web.Models
{
    /// <summary>
    /// A view and view model template.
    /// </summary>
    public class MvvmTemplate
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User that owns this template.
        /// </summary>
        [Index("UK_ApplicationUserId_Name_Language", 0, IsUnique = true)]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        [StringLength(100)]
        [Index("UK_ApplicationUserId_Name_Language", 1, IsUnique = true)]
        public string Name { get; set; }

        /// <summary>
        /// 'VB' or 'C#'
        /// </summary>
        [Required]
        [StringLength(15)]
        [Index("UK_ApplicationUserId_Name_Language", 2, IsUnique = true)]
        public string Language { get; set; }

        /// <summary>
        /// A single value.
        /// </summary>
        [Required]
        public int MvvmTemplateCategoryId { get; set; }
        public virtual MvvmTemplateCategory MvvmTemplateCategory { get; set; }

        /// <summary>
        /// A comma separated list for searching.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Text of the view model, with parameters inside.
        /// </summary>
        [Required]
        public string ViewModel { get; set; }

        /// <summary>
        /// Text of the view, with parameters inside.
        /// </summary>
        [Required]
        public string View { get; set; }

        /// <summary>
        /// If true, the template is shared.  Set to false while making modifications or to hide from others.
        /// </summary>
        [Required]
        public bool Enabled { get; set; }
    }

    public class TemplateIndexModel
    {
        public TemplateIndexModel(IEnumerable<MvvmTemplateDTO> templates, IEnumerable<ApplicationUser> authors, string author, int categoryId, IEnumerable<MvvmTemplateCategory> categories, string language, string search)
        {
            Templates = templates;
            CategoryId = categoryId;
            Author = author;
            Language = language;
            Search = search;

            // Categories
            Categories = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = CategoryId == 0}
            };
            var cquery =
                from cg in categories
                orderby cg.Name.ToUpper()
                select new SelectListItem { Text = cg.Name, Value = cg.Id.ToString(), Selected = CategoryId == cg.Id };
            Categories.AddRange(cquery);

            // Languages
            Languages = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = Language == null},
                new SelectListItem {Text = "C#", Value = "C#", Selected = Language == "C#"},
                new SelectListItem {Text = "VB", Value = "VB", Selected = Language == "VB"}
            };

            // Authors
            Authors = new List<SelectListItem>
            {
                new SelectListItem {Text = "All", Value = "", Selected = Author == null},
                new SelectListItem {Text = "Factory", Value = "Factory", Selected = Author == "Factory"},
            };
            var aquery =
                from a in authors
                orderby a.Author.ToUpper()
                where a.Author != "Factory"
                select new SelectListItem {Text = a.Author, Value = a.Author, Selected = Author == a.Author};
            Authors.AddRange(aquery);
        }

        public IEnumerable<MvvmTemplateDTO> Templates { get; set; }

        public string Name { get; set; }
        
        public List<SelectListItem> Categories { get; set; }
        public int CategoryId { get; set; }
        public List<SelectListItem> Languages { get; set; }
        public string Language { get; set; }
        public List<SelectListItem> Authors { get; set; }
        public string Author { get; set; }
        public string Search { get; set; }
    }
}