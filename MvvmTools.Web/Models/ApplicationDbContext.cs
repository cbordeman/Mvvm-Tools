using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MvvmTools.Web.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<ApplicationUser>().Property(u => u.MvvmTemplates)..Has(b => b.ListComments)
            //    .WithRequired(c => c.Post)
            //    .HasForeignKey(c => c.PostId)
            //    .WillCascadeOnDelete(true);
        }

        public DbSet<MvvmTemplate> MvvmTemplates { get; set; }
        public DbSet<MvvmTemplateCategory> MvvmTemplateCategories { get; set; }
    }
}