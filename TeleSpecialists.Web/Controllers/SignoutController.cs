using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.Web.Hubs;

namespace TeleSpecialists.Web.Controllers
{
    public class SignoutController : Controller
    {
        // GET: Signout
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();


            #region refreshing all windows logout
            if (BLL.Settings.RPCMode == RPCMode.SignalR)
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                var connections = PhysicianCasePopupHub.ConnectedUsers.Where(m => m.UserId == userId).ToList();
                string browser = Request.UserAgent.ToLower();

                connections.ForEach(m =>
                {
                    hubContext.Clients.Client(m.ConnectionId).reloadPageForUser(browser);
                });
            }
            else if (BLL.Settings.RPCMode == RPCMode.WebSocket)
            {
                var paramData = new List<object>();
                paramData.Add(Request.UserAgent.ToLower());
                new WebSocketEventHandler().CallJSMethod(userId, new SocketResponseModel { MethodName = "reloadPageForUser_def", Data = paramData });
            }

            #endregion

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            if (!string.IsNullOrEmpty(userId))
                Web.Hubs.PhysicianCasePopupHub.CleanUsersData(userId);
            return RedirectToAction("Login", "Account");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

    }
}