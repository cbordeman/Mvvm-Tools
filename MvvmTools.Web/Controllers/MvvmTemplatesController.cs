using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        //// GET: MvvmTemplates
        //[HandleError]
        //public async Task<ActionResult> Index(string selectedAuthor, string selectedLanguage, int? selectedCategoryId, string search)
        //{
        //    ApplicationUser user = null;
        //    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
        //        user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

        //    search = search?.Trim();

        //    // base query
        //    var templates = from t in db.MvvmTemplates
        //                    where (t.Enabled && t.ApplicationUser.ShowTemplates)
        //                    select t;
        //    if (AdminUserIsLoggedIn)
        //        templates = from t in db.MvvmTemplates
        //                    select t;
        //    // add author condition
        //    if (!string.IsNullOrEmpty(selectedAuthor))
        //        templates = templates.Where(t => t.ApplicationUser.Author == selectedAuthor);
        //    // add language condition
        //    if (!string.IsNullOrEmpty(selectedLanguage))
        //        templates = templates.Where(t => t.Language == selectedLanguage);
        //    // add category condition
        //    if (selectedCategoryId != null)
        //        templates = templates.Where(t => t.MvvmTemplateCategoryId == selectedCategoryId);
        //    // add search text condition
        //    if (!string.IsNullOrWhiteSpace(search))
        //        templates = templates.Where(
        //                t => t.Name.ToLower().Contains(search) ||
        //                     t.Tags.ToLower().Contains(search) ||
        //                     t.View.ToLower().Contains(search) ||
        //                     t.ViewModel.ToLower().Contains(search));
        //    // Leave off view and view model text fields since they won't be needed on the client.
        //    var query = templates.Select(t => new MvvmTemplateDTO
        //    {
        //        Author = t.ApplicationUser.Author,
        //        Name = t.Name,
        //        Id = t.Id,
        //        Category = db.MvvmTemplateCategories.FirstOrDefault(c => t.MvvmTemplateCategoryId == c.Id).Name,
        //        Language = t.Language,
        //    });
            
        //    // Generate model.
        //    var model = new TemplateIndexViewModel(
        //        user?.Author,
        //        false,
        //        await query.ToListAsync(),
        //        await db.Users.Where(u => u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)).ToListAsync(),
        //        selectedAuthor,
        //        selectedCategoryId.GetValueOrDefault(),
        //        await db.MvvmTemplateCategories.ToListAsync(),
        //        string.IsNullOrWhiteSpace(selectedLanguage) ? null : selectedLanguage,
        //        string.IsNullOrWhiteSpace(search) ? null : search);
            
        //    return View(model);
        //}

        // GET or POST: MvvmTemplates
        [Authorize]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> Index(string author, bool? showTemplates, string selectedAuthor, string selectedLanguage, int? selectedCategoryId, string search)
        {
            ApplicationUser user = null;

            if (User.Identity.IsAuthenticated)
            {
                user = await db.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                
                if (Request.HttpMethod == "POST")
                {
                    // Update user.
                    if (user.Author != author || user.ShowTemplates != showTemplates)
                    {
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
            var templates = from t in db.MvvmTemplates
                            select t;
            if (user == null)
            {
                templates = from t in templates
                            where t.ApplicationUserId == user.Id ||
                                  (t.Enabled && t.ApplicationUser.ShowTemplates)
                            select t;
            }
            else
            {
                templates = from t in templates
                            where t.Enabled && t.ApplicationUser.ShowTemplates
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
                        t => t.Name.ToLower().Contains(search) ||
                             t.View.ToLower().Contains(search) ||
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

            // Generate model.
            var model = new TemplateIndexViewModel(
                user.Author,
                user.ShowTemplates,
                await query.ToListAsync(),
                await db.Users.Where(u => u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)).ToListAsync(),
                author,
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

            // Disable access to non-owners if the template is disabled or all the user's 
            // templates are not shared.
            if ((mvvmTemplate.ApplicationUser.ShowTemplates && mvvmTemplate.Enabled) ||
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

            var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories, mvvmTemplate.MvvmTemplateCategoryId, mvvmTemplate.Language);

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
        public async Task<ActionResult> Edit(
            int? id, 
            [Bind(Prefix = "Template.Enabled")]bool? enabled,
            [Bind(Prefix = "Template.Name")]string name, 
            string selectedLanguage, int? selectedCategoryId,
            [Bind(Prefix = "Template.Tags")]string tags,
            [Bind(Prefix = "Template.ViewModel")]string viewmodel,
            [Bind(Prefix = "Template.View")]string view)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var id2 = id.Value;
            var mvvmTemplate = db.MvvmTemplates.FirstOrDefault(t => t.Id == id2);
            if (mvvmTemplate == null)
                return new HttpNotFoundResult();
            if (!AuthorizeTemplateAccess(mvvmTemplate))
                return new HttpUnauthorizedResult();

            mvvmTemplate.Enabled = enabled.GetValueOrDefault();
            mvvmTemplate.Language = selectedLanguage;
            mvvmTemplate.MvvmTemplateCategoryId = selectedCategoryId.GetValueOrDefault();
            mvvmTemplate.Name = name;
            mvvmTemplate.Tags = tags;
            mvvmTemplate.View = view;
            mvvmTemplate.ViewModel = viewmodel;

            try
            {
                db.Entry(mvvmTemplate).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return new RedirectResult(HttpContext.Request.Params["returnUrl"]);
            }
            catch (Exception)
            {
                var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories, mvvmTemplate.MvvmTemplateCategoryId, mvvmTemplate.Language);
                return View(model);
            }
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
            return new RedirectResult(HttpContext.Request.Params["returnUrl"]);
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
