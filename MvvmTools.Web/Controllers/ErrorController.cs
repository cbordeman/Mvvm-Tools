using System.Web.Mvc;

namespace MvvmTools.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ViewResult Index()
        {
            return View("Error");
        }
        public ViewResult NotFound()
        {
            Response.StatusCode = 404;
            return View("NotFound");
        }
    }
}