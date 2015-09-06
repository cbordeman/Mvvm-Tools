using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvvmTools.Shared;
using MvvmTools.Web.Attributes;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
    [EnforceHttps]
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ValidationController()
        {
            
        }

        public ValidationController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }


        public JsonResult UserNameAvailable(string username)
        {
            var um = this.UserManager;
            var foundUser = um.Users.FirstOrDefault(u => username.ToUpper() == u.UserName.ToUpper());

            return Json(foundUser == null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EmailAvailable(string email)
        {
            var um = this.UserManager;
            var foundUser = um.Users.FirstOrDefault(u => email.ToUpper() == u.Email.ToUpper());

            return Json(foundUser == null, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public JsonResult AuthorAvailable(string author)
        {
            var um = this.UserManager;
            var foundUser = um.Users.FirstOrDefault(u => author.ToUpper() == u.Author.ToUpper());

            return Json(foundUser == null, JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        public async Task<JsonResult> NameAvailable(string name, string language, int? id)
        {
            if (name == null || language == null)
                return Json(false, JsonRequestBehavior.AllowGet);

            // Assume for now the logged in user is the one editing the record.
            var applicationUserId = User.Identity.GetUserId();

            if (User.Identity.IsAuthenticated && User.Identity.GetUserName() == Secrets.AdminUserName)
            {
                if (!id.HasValue)
                    return Json(true, JsonRequestBehavior.AllowGet);

                // Admin is editing the record, which may belong to another user, so
                // do the additional work of getting the edited record's owner.
                var template = await db.MvvmTemplates.FindAsync(id.Value);
                if (template == null)
                    return Json(false, JsonRequestBehavior.AllowGet);
                applicationUserId = template.ApplicationUserId;
            }

            var templateId = id.GetValueOrDefault();

            // Now look for templates with the same name+language+applicationuserid combo, excluding current template.
            var match = await db.MvvmTemplates.FirstOrDefaultAsync(t => t.ApplicationUserId == applicationUserId &&
                                                                   t.Id != templateId &&
                                                                   t.Name.ToUpper() == name.ToUpper() &&
                                                                   t.Language.ToUpper() == language.ToUpper());
            return Json(match == null, JsonRequestBehavior.AllowGet);
        }

    }
}