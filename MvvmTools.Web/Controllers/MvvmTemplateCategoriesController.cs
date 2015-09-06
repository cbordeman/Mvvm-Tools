using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvvmTools.Shared;
using MvvmTools.Web.Attributes;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
    [EnforceHttps]
    [Authorize]
    public class MvvmTemplateCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

		private bool AdminUserIsLoggedIn => User != null && User.Identity != null && User.Identity.GetUserName() == Secrets.AdminUserName;

		// GET: MvvmTemplateCategories
        public async Task<ActionResult> Index()
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            return View(await db.MvvmTemplateCategories.ToListAsync());
        }

        // GET: MvvmTemplateCategories/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MvvmTemplateCategory mvvmTemplateCategory = await db.MvvmTemplateCategories.FindAsync(id);
            if (mvvmTemplateCategory == null)
            {
                return HttpNotFound();
            }
            return View(mvvmTemplateCategory);
        }

        // GET: MvvmTemplateCategories/Create
        public ActionResult Create()
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            return View();
        }

        // POST: MvvmTemplateCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] MvvmTemplateCategory mvvmTemplateCategory)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                db.MvvmTemplateCategories.Add(mvvmTemplateCategory);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(mvvmTemplateCategory);
        }

        // GET: MvvmTemplateCategories/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MvvmTemplateCategory mvvmTemplateCategory = await db.MvvmTemplateCategories.FindAsync(id);
            if (mvvmTemplateCategory == null)
            {
                return HttpNotFound();
            }
            return View(mvvmTemplateCategory);
        }

        // POST: MvvmTemplateCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] MvvmTemplateCategory mvvmTemplateCategory)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                db.Entry(mvvmTemplateCategory).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(mvvmTemplateCategory);
        }

        // GET: MvvmTemplateCategories/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MvvmTemplateCategory mvvmTemplateCategory = await db.MvvmTemplateCategories.FindAsync(id);
            if (mvvmTemplateCategory == null)
            {
                return HttpNotFound();
            }
            return View(mvvmTemplateCategory);
        }

        // POST: MvvmTemplateCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!AdminUserIsLoggedIn)
                return new HttpUnauthorizedResult();

            MvvmTemplateCategory mvvmTemplateCategory = await db.MvvmTemplateCategories.FindAsync(id);
            db.MvvmTemplateCategories.Remove(mvvmTemplateCategory);
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
