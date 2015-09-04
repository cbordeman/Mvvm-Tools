using System;
using System.Web.Mvc;

namespace MvvmTools.Web.Attributes
{
    public class ProjectSpecificHttpsAttribute : RequireHttpsAttribute
    {
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly.

            if (!String.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("SSL is required.  Use HTTPS.");
            }

            // redirect to HTTPS version of page
            string url = "https://" + filterContext.HttpContext.Request.Url.Host + filterContext.HttpContext.Request.RawUrl;
#if DEBUG
            // Change to the debug ssl port.
            url = url.Replace(":60821", ":44300");
#endif
            filterContext.Result = new RedirectResult(url);
        }
    }
}
