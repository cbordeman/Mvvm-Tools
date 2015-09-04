using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MvvmTools.Web.Models;

namespace MvvmTools.Web.Controllers.Api
{
    [System.Web.Mvc.Authorize]
    public class TemplateCategoriesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/TemplateCategories
        public IEnumerable<string> GetTemplateCategories()
        {
            return db.MvvmTemplateCategories.Select(c => c.Name);
        }
    }
}