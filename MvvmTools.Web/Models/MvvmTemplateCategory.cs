using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvvmTools.Web.Models
{
    public class MvvmTemplateCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [StringLength(60)]
        public string Name { get; set; }
    }
}
