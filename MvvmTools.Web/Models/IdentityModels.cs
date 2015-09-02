using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MvvmTools.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        /// <summary>
        /// The name to show under 'Author' for each mvvm template created by this user.
        /// </summary>
        [Required]
        [StringLength(100)]
        [Index(IsUnique = true)]
        [Remote("AuthorAvailable", "Validation", ErrorMessage = "That {0} already exists.")]
        public string Author { get; set; }

        /// <summary>
        /// Set to false to disable all this user's templates.
        /// </summary>
        [Display(Prompt = "Share My Templates")]
        public bool ShowTemplates { get; set; }
        
        /// <summary>
        /// The set of templates belonging to this user.
        /// </summary>
        public virtual ICollection<MvvmTemplate> MvvmTemplates { get; set; }
    }
}