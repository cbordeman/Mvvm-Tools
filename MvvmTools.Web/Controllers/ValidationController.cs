using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers
{
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

        public JsonResult AuthorAvailable(string author)
        {
            var um = this.UserManager;
            var foundUser = um.Users.FirstOrDefault(u => author.ToUpper() == u.Author.ToUpper());

            return Json(foundUser == null, JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        public JsonResult NameAvailable(string name, string language, int? id)
        {
            if (name == null || language == null || id == null)
                return Json(false);

            var applicationUserId = User.Identity.GetUserId();
            var templateId = id.Value;

            // Now look for templates with the same name+language+applicationuserid combo, excluding current template.
            var match = db.MvvmTemplates.FirstOrDefault(t => t.ApplicationUserId == applicationUserId &&
                                                             t.Id != templateId &&
                                                             t.Name.ToUpper() == name.ToUpper() &&
                                                             t.Language.ToUpper() == language.ToUpper());
            return Json(match == null);
        }

    }
}