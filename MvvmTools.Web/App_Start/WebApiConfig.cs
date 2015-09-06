using System.Web.Http;
using MvvmTools.Web.Attributes;

namespace MvvmTools.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.SuppressHostPrincipal();
            
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // WebAPI when dealing with JSON & JavaScript.  Setup JSON serialization 
            // to serialize classes to camel (std. Json format).
            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            // Web Api should always be https.
#if !DEBUG
            config.MessageHandlers.Add(new EnforceHttpsHandler());
#endif
        }
    }
}