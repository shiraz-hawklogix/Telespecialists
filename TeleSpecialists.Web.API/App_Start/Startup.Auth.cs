using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeleSpecialists.Web.API.Models;
using TeleSpecialists.Web.API.Providers;

[assembly: OwinStartup(typeof(TeleSpecialists.Web.API.Startup))]
namespace TeleSpecialists.Web.API
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            //app.UseOAuthBearerTokens(OAuthOptions);

            // oAuth with EF and Identity
            app.CreatePerOwinContext<OwinAuthDbContext>(() => new OwinAuthDbContext());
            app.CreatePerOwinContext<UserManager<ApplicationUser>>(CreateManager);
            app.UseOAuthAuthorizationServer(OAuthOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //app.UseWelcomePage();
        }

        static Startup()
        {
            // OAuth without EF
            /*
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = new OAuthAppProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(2),
                AllowInsecureHttp = true
            };
            */

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = new OAuthEFProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                AllowInsecureHttp = true,
            };
        }

        private static UserManager<ApplicationUser> CreateManager(IdentityFactoryOptions<UserManager<ApplicationUser>> options, IOwinContext context)
        {
            var userStore = new UserStore<ApplicationUser>(context.Get<OwinAuthDbContext>());
            var owinManager = new UserManager<ApplicationUser>(userStore);
            return owinManager;
        }
    }
}