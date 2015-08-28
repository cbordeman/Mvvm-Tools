using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvvmTools.Shared;
using MvvmTools.Shared.Models;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
    public class MvvmTemplatesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        
        // GET or POST: MvvmTemplates
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Index(string author, bool? showTemplates, string selectedAuthor, string selectedLanguage, int? selectedCategoryId, string search)
        {
            ApplicationUser user = null;

            if (User.Identity.IsAuthenticated)
            {
                user = await db.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                
                if (Request.HttpMethod == "POST")
                {
                    AntiForgery.Validate();

                    // Update user.
                    if (!string.IsNullOrEmpty(author) && showTemplates != null && 
                        (user.Author != author || user.ShowTemplates != showTemplates))
                    {
                        // Update db.
                        user.Author = author;
                        user.ShowTemplates = showTemplates.GetValueOrDefault();
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    // On GET, initialize selectedUser to the current user.  On POST, user 
                    // could have changed it.
                    selectedAuthor = user.Author;
                }
            }

            // Then do search.
            search = search?.Trim();

            // base query
            IQueryable<MvvmTemplate> templates;
            if (user == null)
            {
                templates = from t in db.MvvmTemplates
                            where t.Enabled && t.ApplicationUser.ShowTemplates
                            select t;
            }
            else
            {
                // If logged in, also show all templates for the current user no matter 
                // the user's ShowTemplates flag or Enabled flags on the templates.
                templates = from t in db.MvvmTemplates
                            where t.ApplicationUserId == user.Id ||
                                  (t.Enabled && t.ApplicationUser.ShowTemplates)
                            select t;
            }
            // add author condition
            if (!string.IsNullOrEmpty(selectedAuthor))
                templates = templates.Where(t => t.ApplicationUser.Author == selectedAuthor);
            // add language condition
            if (!string.IsNullOrEmpty(selectedLanguage))
                templates = templates.Where(t => t.Language == selectedLanguage);
            // add category condition
            if (selectedCategoryId != null)
                templates = templates.Where(t => t.MvvmTemplateCategoryId == selectedCategoryId);
            // add search text condition
            if (!string.IsNullOrWhiteSpace(search))
                templates = templates.Where(
                        t => t.Name.ToLower().Contains(search.ToLower()) ||
                             t.View.ToLower().Contains(search.ToLower()) ||
                             t.ViewModel.ToLower().Contains(search));
            // Leave off view and view model text fields since they won't be needed on the client.
            var query = templates.Select(t => new MvvmTemplateDTO
            {
                Author = t.ApplicationUser.Author,
                Name = t.Name,
                Id = t.Id,
                Category = db.MvvmTemplateCategories.FirstOrDefault(c => c.Id == t.MvvmTemplateCategoryId).Name,
                Language = t.Language,
                Enabled = t.Enabled
            });

            string curUserName = user?.UserName;
            var authorsQuery= from u in db.Users
                              where (u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)) ||
                                    (curUserName != null && u.UserName == curUserName) ||
                                    (string.IsNullOrEmpty(selectedAuthor) && u.Author == selectedAuthor)
                              select u;
            var authorsList = await authorsQuery.ToListAsync();

            // Generate model.
            var model = new TemplateIndexViewModel(
                user?.Author,
                user != null && user.ShowTemplates,
                await query.ToListAsync(),
                authorsList,
                selectedAuthor,
                selectedCategoryId.GetValueOrDefault(),
                await db.MvvmTemplateCategories.ToListAsync(),
                string.IsNullOrWhiteSpace(selectedLanguage) ? null : selectedLanguage,
                string.IsNullOrWhiteSpace(search) ? null : search);

            return View(model);
        }

        // GET: MvvmTemplates/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (mvvmTemplate == null)
                return HttpNotFound();

            // Disable access to non-owners if the user's templates aren't shared or the template is disabled.
            if ((!mvvmTemplate.ApplicationUser.ShowTemplates || 
                !mvvmTemplate.Enabled) &&
                !AuthorizeTemplateAccess(mvvmTemplate))
                return new HttpUnauthorizedResult();

            return View(mvvmTemplate);
        }

        // GET: MvvmTemplates/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var model = new TemplateCreateViewModel(await db.MvvmTemplateCategories.ToListAsync());
            return View(model);
        }

        // POST: MvvmTemplates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include = "Enabled,Name,Language,MvvmTemplateCategoryId,Tags,ViewModel,View")] MvvmTemplate mvvmTemplate)
        {
            mvvmTemplate.ApplicationUserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                db.MvvmTemplates.Add(mvvmTemplate);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var model = new TemplateCreateViewModel(await db.MvvmTemplateCategories.ToListAsync());
            return View(model);
        }

        // GET: MvvmTemplates/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (!AuthorizeTemplateAccess(mvvmTemplate))
                return new HttpUnauthorizedResult();
            if (mvvmTemplate == null)
                return HttpNotFound();

            var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories);

            return View(model);
        }

        private bool AdminUserIsLoggedIn => User != null && User.Identity != null && User.Identity.GetUserName() == Secrets.AdminUserName;

        private bool AuthorizeTemplateAccess(MvvmTemplate mvvmTemplate)
        {
            return AdminUserIsLoggedIn || mvvmTemplate.ApplicationUserId == User.Identity.GetUserId();
        }

        // POST: MvvmTemplates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Enabled,Name,MvvmTemplateCategoryId,Language,Tags,ViewModel,View")] MvvmTemplate mvvmTemplate)
        {
            // Have to retrieve and reassign the ApplicationUserId since it wasn't given
            // to the client (a security risk).  Be sure to use AsNoTracking() so EF doesn't
            // think we're attaching a dupe on SaveChanges().
            var t = db.MvvmTemplates.AsNoTracking().First(template => template.Id == mvvmTemplate.Id);
            mvvmTemplate.ApplicationUserId = t.ApplicationUserId;

            if (!AuthorizeTemplateAccess(t))
                return new HttpUnauthorizedResult();
            
            if (ModelState.IsValid)
            {
                db.Entry(mvvmTemplate).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories);
            return View(model);
        }
        
        // GET: MvvmTemplates/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (mvvmTemplate == null)
                return HttpNotFound();
            if (!AuthorizeTemplateAccess(mvvmTemplate))
                return new HttpUnauthorizedResult();
            return View(mvvmTemplate);
        }

        // POST: MvvmTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (!AuthorizeTemplateAccess(mvvmTemplate))
                return new HttpUnauthorizedResult();
            db.MvvmTemplates.Remove(mvvmTemplate);
            await db.SaveChangesAsync();
            //return new RedirectResult(HttpContext.Request.Params["returnUrl"]);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
