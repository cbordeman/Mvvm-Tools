using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvvmTools.Shared.Models;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
    public class MvvmTemplatesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: MvvmTemplates
        public async Task<ActionResult> Index(string author, string language, int? categoryId, string search)
        {
            search = search?.Trim();

            // base query
            var templates = from t in db.MvvmTemplates
                            where t.Enabled
                            select t;
            // add language condition
            if (!string.IsNullOrEmpty(language))
                templates = templates.Where(t => t.Language == language);
            // add category condition
            if (categoryId != null)
                templates = templates.Where(t => t.MvvmTemplateCategoryId == categoryId);
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
            var model = new TemplateIndexModel(
                await query.ToListAsync(),
                await db.Users.Where(u => u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)).ToListAsync(),
                author,
                categoryId.GetValueOrDefault(),
                await db.MvvmTemplateCategories.ToListAsync(),
                string.IsNullOrWhiteSpace(language) ? null : language,
                string.IsNullOrWhiteSpace(search) ? null : search);
            
            return View(model);
        }

        // GET: MvvmTemplates
        [Authorize]
        public async Task<ActionResult> MyIndex(string author, string language, int? categoryId, string search)
        {
            search = search?.Trim();

            // base query
            var templates = from t in db.MvvmTemplates
                            select t;
            // add language condition
            if (!string.IsNullOrEmpty(language))
                templates = templates.Where(t => t.Language == language);
            // add category condition
            if (categoryId != null)
                templates = templates.Where(t => t.MvvmTemplateCategoryId == categoryId);
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
            var model = new TemplateIndexModel(
                await query.ToListAsync(),
                await db.Users.Where(u => u.ShowTemplates && u.MvvmTemplates.Any(t => t.Enabled)).ToListAsync(),
                author,
                categoryId.GetValueOrDefault(),
                await db.MvvmTemplateCategories.ToListAsync(),
                string.IsNullOrWhiteSpace(language) ? null : language,
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
            return View(mvvmTemplate);
        }

        // POST: MvvmTemplates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Language,Category,Tags,ViewModel,View")] MvvmTemplate mvvmTemplate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mvvmTemplate).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(mvvmTemplate);
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
