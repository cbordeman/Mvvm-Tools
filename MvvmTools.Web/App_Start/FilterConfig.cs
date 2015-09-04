using System.Web.Mvc;
using MvvmTools.Web.Attributes;

namespace MvvmTools.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ProjectSpecificHttpsAttribute());
        }
    }
}
