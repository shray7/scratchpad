using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(scratchpad.Startup))]
namespace scratchpad
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
