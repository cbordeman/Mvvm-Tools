#if !DEBUG
using System;
#endif
using System.Web.Mvc;

namespace MvvmTools.Web.Attributes
{
    public class EnforceHttpsAttribute : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
#if !DEBUG
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                HandleNonHttpsRequest(filterContext);
            }
#endif
        }

        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
#if !DEBUG
            // We should not redirect from http to https because that's a security risk.
            // Instead, [EnforceHttps] should be placed in controllers and actions and http
            // or https explicitly specified on all links on the left and top panels.  All pages
            // but the home page should be http only.
            throw new InvalidOperationException("Requires HTTPS.");
#endif
        }
    }
}
