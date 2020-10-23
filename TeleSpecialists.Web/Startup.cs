using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TeleSpecialists.Startup))]
namespace TeleSpecialists
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
