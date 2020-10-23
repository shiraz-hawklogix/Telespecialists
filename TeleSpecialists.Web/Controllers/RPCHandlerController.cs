using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.ViewModels;
using System.Web.Mvc;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Model;
using System;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Helpers;
using System.Configuration;

namespace TeleSpecialists.Controllers
{
    [AuthenticateSignalRequest]
    public class RPCHandlerController : BaseController
    {
        // GET: SignalRHandler
        private PhysicianStatusSnoozeService _physicianStatusSnoozeService;
        public RPCHandlerController()
        {
            _physicianStatusSnoozeService = new PhysicianStatusSnoozeService();
        }


        public JsonResult GetOnlineUsers()
        {
            List<string> OnlineUsers = new List<string>();
            try
            {
                if (WebSocketEventHandler.ConnectedClients != null && WebSocketEventHandler.ConnectedClients.Count > 0)
                {
                    OnlineUsers = WebSocketEventHandler.ConnectedClients.Select(x => ((WebSocketEventHandler)x)).Where(m => string.IsNullOrEmpty(m.ServerUrl) && !string.IsNullOrEmpty(m.UserId)).Select(m => m.UserId).Distinct().ToList();
                    return Json(OnlineUsers, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { OnlineUsers = new List<string>() }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { OnlineUsers = new List<string>() }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult CallJsMethod(SocketSendRequestModel model)
        {
            var allUsers = WebSocketEventHandler.ConnectedClients.Select(x => ((WebSocketEventHandler)x));
            var users = allUsers.Where(m => string.IsNullOrEmpty(m.ServerUrl))
                                .Where(x => model.Recievers.Contains(x.UserId)).ToList();
            bool success = false;
            users.ForEach(client =>
            {
                var data = Functions.EncodeTo64UTF8(Newtonsoft.Json.JsonConvert.SerializeObject(model.ResponseModel));
                client.Send(data);
                success = true;
            });

            return Json(new { success = success });
        }

        [HttpPost]
        public ActionResult RefreshPhysicianStatus(List<string> UserIds)
        {
            var userDetailList = new AdminService().GetAspNetUsers().Where(m => UserIds.Contains(m.Id)).Select(m => new { m.Id, m.status_key }).ToList();

            if (Settings.RPCMode == RPCMode.SignalR)
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                var physicianOnlineWindows = PhysicianCasePopupHub.ConnectedUsers
                                                            .Where(m => UserIds.Contains(m.UserId))
                                                            .ToList();
                physicianOnlineWindows.ForEach(window =>
                {
                    foreach (var physician in userDetailList)
                    {
                        hubContext.Clients.Client(window.ConnectionId).syncPhysicianStatusTime((physician.status_key == PhysicianStatus.TPA.ToInt()));
                    }
                });
            }
            else if (Settings.RPCMode == RPCMode.WebSocket)
            {
                foreach (var physician in userDetailList)
                {
                    if (physician.status_key == PhysicianStatus.TPA.ToInt())
                    {
                        var dataList = new List<object>();
                        dataList.Add((true));

                        new WebSocketEventHandler().CallJSMethod(UserIds, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus", Data = dataList });
                    }
                    else
                    {
                        new WebSocketEventHandler().CallJSMethod(UserIds, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus" });

                    }
                }

            }
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ShowPhysicianStatusSnoozePopup(List<PhysicianStatusSnoozeViewModel> model)
        {
            var popupUserIds = model.Select(m => m.UserId);
            var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();

            if (Settings.RPCMode == RPCMode.SignalR)
            {
                var onlinePhysicians = PhysicianCasePopupHub.ConnectedUsers
                                                                .Where(m => popupUserIds.Contains(m.UserId))
                                                                .ToList();
                onlinePhysicians.ForEach(physician =>
                {
                    var physician_status = model.FirstOrDefault(m => m.UserId == physician.UserId);
                    if (physician_status != null)
                    {
                        hubContext.Clients.Client(physician.ConnectionId).showPhysicianStatusSnoozePopup(physician_status.phs_key, true);
                        // Adding zero time entry in log to skip the iterations after the popup is displayed to user
                    }
                });
            }
            else if (Settings.RPCMode == RPCMode.WebSocket)
            {
                var socketOnlinePhysician = new WebSocketEventHandler()._chatClients.Where(x => popupUserIds.Contains(((WebSocketEventHandler)x).UserId)).ToList();


                socketOnlinePhysician.ForEach(physician =>
                {

                    //WebSocketEventHandler socket = new WebSocketEventHandler();
                    var phyId = ((WebSocketEventHandler)physician).UserId;
                    var physician_status = model.FirstOrDefault(m => m.UserId == phyId);
                    if (physician_status != null)
                    {
                        var methodParams = new List<object>();
                        methodParams.Add(physician_status.phs_key.ToString());
                        methodParams.Add(true);
                        new WebSocketEventHandler().CallJSMethod(phyId, new SocketResponseModel { MethodName = "showPhysicianStatusSnoozePopup_def", Data = methodParams });
                    }
                });


            }

            model.ForEach(m =>
            {
                _physicianStatusSnoozeService.Create(new physician_status_snooze
                {
                    pss_phs_key = m.phs_key,
                    pss_user_key = m.UserId,
                    pss_created_date = DateTime.Now.ToEST(),
                    pss_snooze_time = new TimeSpan(0, 0, 0, 0),
                    pss_createe_by_name = "Signal R - Physician Status Mover",
                    pss_created_by = Guid.Empty.ToString(),
                    pss_is_active = true,
                    pss_is_latest_snooze = true
                });
            });

            return Json(new { success = true });
        }

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _physicianStatusSnoozeService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
