using System.Web.Mvc;
using MvvmTools.Web.Attributes;

namespace MvvmTools.Web.Controllers
{
    [EnforceHttps]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

    }
}