using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SmscMeitrackProtocol.Startup))]
namespace SmscMeitrackProtocol
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
