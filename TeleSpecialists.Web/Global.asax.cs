using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TeleSpecialists
{
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Below code is to remove returnUrl from login 
        /// </summary>
        private const String ReturnUrlRegexPattern = @"\?ReturnUrl=.*$";
        public MvcApplication()
        {
            PreSendRequestHeaders += MvcApplicationOnPreSendRequestHeaders;
        }
        private void MvcApplicationOnPreSendRequestHeaders(object sender, EventArgs e)
        {
            String redirectUrl = Response.RedirectLocation;
            if (String.IsNullOrEmpty(redirectUrl) || !Regex.IsMatch(redirectUrl, ReturnUrlRegexPattern))
                return;
            Response.RedirectLocation = Regex.Replace(redirectUrl, ReturnUrlRegexPattern, String.Empty);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RequiredAttributeAdapter.SelfRegister();

            // Added by Danyal
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            MvcHandler.DisableMvcResponseHeader = true;
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            Context.Response.Headers.Remove("X-Powered-By");
            Context.Response.Headers.Remove("X-AspNet-Version");
            Context.Response.Headers.Remove("X-AspNetMvc-Version");
            Context.Response.Headers.Remove("Server");
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

            }

            if (BLL.Settings.EnableCustomErrors)
            {
                var httpContext = ((MvcApplication)sender).Context;

                #region Clearing Response
                httpContext.ClearError();
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = ex is System.Web.HttpException ? ((System.Web.HttpException)ex).GetHttpCode() : 500;
                httpContext.Response.TrySkipIisCustomErrors = true;
                #endregion

                if (httpContext.Response.StatusCode == 401 || httpContext.Response.StatusCode == 404)
                {
                    httpContext.Response.Redirect("~/Error/NotFound");
                }
                else
                {                  
                    httpContext.Response.Redirect("~/Error/ErrorOccured");
                }
            }
        }

        
    }
}
