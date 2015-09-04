using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CacheCow.Server;

namespace MvvmTools.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Manually installed WebAPI 2.2 after making an MVC project.
            GlobalConfiguration.Configure(WebApiConfig.Register);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Add in memory CacheCow cache handler.
            var cacheHandler = new CachingHandler(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configuration.MessageHandlers.Add(cacheHandler);

            BeginRequest += Application_BeginRequest;
            PreSendRequestHeaders += OnPreSendRequestHeaders;

            MvcHandler.DisableMvcResponseHeader = true;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs eventArgs)
        {
            // Have to do this, in addition to the equivalent in the web.config
            // for some reason.
            HttpContext.Current.Response.Headers.Remove("Server");
        }

        /// <summary> Handles the BeginRequest event of the Application control. </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.IsSecureConnection)
            {
                Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
            }
        }
    }
}
