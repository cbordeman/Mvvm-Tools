using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvvmTools.Shared.Models;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
    public class MvvmTemplatesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: MvvmTemplates
        public async Task<ActionResult> Index(string selectedAuthor, string selectedLanguage, int? selectedCategoryId, string search)
        {

            ApplicationUser user = null;
            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                user = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);

            search = search?.Trim();

            // base query
            var templates = from t in db.MvvmTemplates
                            where t.Enabled && t.ApplicationUser.ShowTemplates
                            select t;
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
                             t.Tags.ToLower().Contains(search) ||
                             t.View.ToLower().Contains(search) ||
                             t.ViewModel.ToLower().Contains(search));
            // Leave off view and view model text fields since they won't be needed on the client.
            var query = templates.Select(t => new MvvmTemplateDTO
            {
                Author = t.ApplicationUser.Author,
                Name = t.Name,
                Id = t.Id,
                Category = db.MvvmTemplateCategories.FirstOrDefault(c => t.MvvmTemplateCategoryId == c.Id).Name,
                Language = t.Language,
            });
            
            // Generate model.
            var model = new TemplateIndexViewModel(
                user?.Author,
                false,
                await query.ToListAsync(),
                await db.Users.Where(u => u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)).ToListAsync(),
                selectedAuthor,
                selectedCategoryId.GetValueOrDefault(),
                await db.MvvmTemplateCategories.ToListAsync(),
                string.IsNullOrWhiteSpace(selectedLanguage) ? null : selectedLanguage,
                string.IsNullOrWhiteSpace(search) ? null : search);
            
            return View(model);
        }

        // GET or POST: MvvmTemplates
        [Authorize]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> MyIndex(string author, bool? showTemplates, string selectedAuthor, string selectedLanguage, int? selectedCategoryId, string search)
        {
            var user = db.Users.First(u => u.UserName == User.Identity.Name);

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

            // Then do search.
            search = search?.Trim();

            // base query
            var templates = from t in db.MvvmTemplates
                            where t.ApplicationUserId == user.Id ||
                                  (t.Enabled && t.ApplicationUser.ShowTemplates)
                            select t;
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
            return View(mvvmTemplate);
        }

        // GET: MvvmTemplates/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MvvmTemplates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Language,Category,Tags,ViewModel,View")] MvvmTemplate mvvmTemplate)
        {
            if (ModelState.IsValid)
            {
                db.MvvmTemplates.Add(mvvmTemplate);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(mvvmTemplate);
        }

        // GET: MvvmTemplates/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (mvvmTemplate == null)
                return HttpNotFound();

            var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories, mvvmTemplate.MvvmTemplateCategoryId, mvvmTemplate.Language);

            return View(model);
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
            if (!string.Equals(User.Identity.GetUserId(), mvvmTemplate.ApplicationUserId))
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
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                var model = new TemplateEditViewModel(mvvmTemplate, db.MvvmTemplateCategories, mvvmTemplate.MvvmTemplateCategoryId, mvvmTemplate.Language);
                return View(model);
            }
        }

        // GET: MvvmTemplates/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            if (mvvmTemplate == null)
            {
                return HttpNotFound();
            }
            return View(mvvmTemplate);
        }

        // POST: MvvmTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var mvvmTemplate = await db.MvvmTemplates.FindAsync(id);
            db.MvvmTemplates.Remove(mvvmTemplate);
            await db.SaveChangesAsync();
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
