using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MvvmTools.Web.Startup))]
namespace MvvmTools.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
