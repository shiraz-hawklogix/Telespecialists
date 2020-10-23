using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using TeleSpecialists.Web.API.Models;

namespace TeleSpecialists.Web.API.Providers
{
    public class OAuthEFProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            /*
            string clientId;
            string clientSecret;

            context.TryGetFormCredentials(out clientId, out clientSecret);

            var doubleCheck = await userManager.FindByIdAsync(context.ClientId);
            */

            context.Validated();

            //if (context.ClientId == null)
            //{
            //    context.Validated();
            //}
            //return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            if (context.Request.Method != "POST")
            {
                context.SetError("invalid_request", "Invalid request");
                return;
            }

            UserManager<ApplicationUser> userManager = context.OwinContext.GetUserManager<UserManager<ApplicationUser>>();
            ApplicationUser user = null;
            string[] passwords = null;

            try
            {
                // API Password = Base64(username:password:secret_key)

                byte[] data = System.Convert.FromBase64String(context.Password);
                string base64Decoded = System.Text.ASCIIEncoding.UTF8.GetString(data);

                if (!string.IsNullOrEmpty(base64Decoded))
                {
                    passwords = base64Decoded.Split(':');

                    user = await userManager.FindAsync(context.UserName, passwords[1]);
                }
                else
                {
                    context.SetError("invalid_password", "Password is not valid");
                    return;
                }
            }
            catch (Exception ex)
            {
                // Could not retrieve the user due to error.
                context.SetError("server_error", ex.Message);
                //context.Rejected();
                return;
            }
            if (user != null && passwords != null)
            {
                //ClaimsIdentity identity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ExternalBearer);
                string APIRoleName = System.Configuration.ConfigurationManager.AppSettings.Get("APIRoleName");

                if (user.APISecretKey == passwords[2] && userManager.IsInRole(user.Id, APIRoleName))
                {
                    ClaimsIdentity identity = await userManager.CreateIdentityAsync(user, Startup.OAuthOptions.AuthenticationType);
                    context.Validated(identity);
                }
                else
                {
                    context.SetError("invalid_grant", "Access information you provided is not valid.");
                }
            }
            else
            {
                context.SetError("invalid_grant", "Invalid username or password'");
                //context.Rejected();
            }
        }

        /*
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            //return base.TokenEndpoint(context);

            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
        */
    }
}