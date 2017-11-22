using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(UpdaterWebServer.Startup))]

namespace UpdaterWebServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
