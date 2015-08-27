using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MvvmTools.Shared;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            // Add admin user.
            if (!(context.Users.Any(u => u.UserName == Secrets.AdminUserName)))
            {
                var userToInsert = new ApplicationUser
                {
                    Email = Secrets.AdminEmail,
                    UserName = Secrets.AdminUserName,
                    PhoneNumber = Secrets.AdminPhone,
                    Author = "Factory"
                };
                userManager.Create(userToInsert, Secrets.AdminPassword);
            }

            var adminUser = context.Users.First(u => u.UserName == Secrets.AdminUserName);
            adminUser.ShowTemplates = true;
            adminUser.LockoutEnabled = false;

            SaveChanges(context);

            context.MvvmTemplateCategories.AddOrUpdate(
                p => p.Name,
                new MvvmTemplateCategory {Name = "Prism"},
                new MvvmTemplateCategory {Name = "Caliburn.Micro"},
                new MvvmTemplateCategory {Name = "MVVM Light"}
                );
            
            SaveChanges(context);

            var prismCategoryId = context.MvvmTemplateCategories.First(c => c.Name == "Prism").Id;
            var caliburnMicroCategoryId = context.MvvmTemplateCategories.First(c => c.Name == "Caliburn.Micro").Id;
            var mvvmLightCategoryId = context.MvvmTemplateCategories.First(c => c.Name == "MVVM Light").Id;

            context.MvvmTemplates.AddOrUpdate(
                t => new {t.ApplicationUserId, t.Name, t.Language},
                new MvvmTemplate
                {
                    ApplicationUserId = adminUser.Id,
                    MvvmTemplateCategoryId = prismCategoryId,
                    Name = "Prism w/ ViewModelLocator",
                    Language = "C#",
                    Enabled = true,
                    View = ".",
                    ViewModel = ".",
                    Tags = "."
                },
                new MvvmTemplate
                {
                    ApplicationUserId = adminUser.Id,
                    MvvmTemplateCategoryId = caliburnMicroCategoryId,
                    Name = "Caliburn.Micro Standard",
                    Language = "C#",
                    Enabled = true,
                    View = ".",
                    ViewModel = ".",
                    Tags = "."
                },
                new MvvmTemplate
                {
                    ApplicationUserId = adminUser.Id,
                    MvvmTemplateCategoryId = mvvmLightCategoryId,
                    Name = "MVVM Light Standard",
                    Language = "C#",
                    Enabled = true,
                    View = ".",
                    ViewModel = ".",
                    Tags = "."
                });

            SaveChanges(context);
        }

        /// <summary>
        /// Wrapper for SaveChanges adding the Validation Messages to the generated exception
        /// </summary>
        /// <param name="context">The context.</param>
        private void SaveChanges(DbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType().Name);
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" + sb, ex
                    ); // Add the original exception as the innerException
            }
        }
    }
}
