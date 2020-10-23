using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace TeleSpecialists.Web.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            //System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            //MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            Context.Response.Headers.Remove("X-Powered-By");
            Context.Response.Headers.Remove("X-AspNet-Version");
            Context.Response.Headers.Remove("X-AspNetMvc-Version");
            Context.Response.Headers.Remove("Server");
        }
    }
}
