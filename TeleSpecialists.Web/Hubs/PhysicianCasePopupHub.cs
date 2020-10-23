using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.Web.Hubs
{
    public class PhysicianCasePopupHub : Hub
    {
        public class SignalRUser
        {
            public string UserId { get; set; }
            public string ConnectionId { get; set; }
            public int? cas_key { get; set; }
        }

        private CaseService _caseService { get; set; }
        private CaseAssignHistoryService _caseAssignHistoryService { get; set; }
        public PhysicianCasePopupHub()
        {
            _caseService = new CaseService();
            _caseAssignHistoryService = new CaseAssignHistoryService();

        }

        public static List<SignalRUser> ConnectedUsers = new List<SignalRUser>();
        public void Connect()
        {
            var id = Context.ConnectionId;
            if (ConnectedUsers != null)
            {
                lock (ConnectedUsers)
                {
                    if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
                    {
                        var userId = Context.User.Identity.GetUserId();
                        ConnectedUsers.Add(new SignalRUser { ConnectionId = id, UserId = userId });
                        #region Commented Code
                        // send to caller  
                        //  Clients.Caller.onConnected(id, userId, ConnectedUsers);
                        //  Clients.Caller.sendMessage(id, userId, ConnectedUsers);
                        #endregion
                    }
                }

            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
          
            Connect();
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
          
           Connect();
            return base.OnReconnected();
        }



        public void RejectCase(int id)
        {


            //Removing the reference of case of current physician in case of reject
            var currentUserId = Context.User.Identity.GetUserId();
            var currentUser = _caseAssignHistoryService.GetRequests(id, PhysicianCaseAssignQueue.WaitingForAction)
                                                       .Where(m => m.cah_phy_key == currentUserId)
                                            .FirstOrDefault();

            var model = _caseService.GetDetailsWithoutTimeConversion(id);
            if (currentUser != null)
            {
                var currentQueueItem = _caseAssignHistoryService.GetDetails(currentUser.cah_key);
                currentQueueItem.cah_action_time = DateTime.Now.ToEST();
                currentQueueItem.cah_action_time_utc = DateTime.UtcNow;
                currentQueueItem.cah_action = PhysicianCaseAssignQueue.Rejected.ToString();
                currentQueueItem.cah_modified_date = DateTime.Now.ToEST();
                currentQueueItem.cah_modified_by = Context.User.Identity.GetUserId();
                _caseAssignHistoryService.Edit(currentQueueItem);
                LinkCaseWithUser(null, currentUserId); // added to handle disconnection scenario
                ClosePopupForUser(currentUserId);


                if (model.cas_phy_key == null)
                {
                    #region Finding Next User or sending popup to navigator if all done

                    var physicianQueue = _caseAssignHistoryService.GetInQueuePhysicians(id)
                                                          .Select(m => new
                                                          {
                                                              m.cah_phy_key,
                                                              m.cah_key,
                                                              m.cah_sort_order
                                                          })
                                                          .ToList();
                    var nextPhysician = physicianQueue
                                                    .Where(m => m.cah_phy_key != model.cas_created_by)
                                                    .OrderBy(m => m.cah_sort_order)
                                                    .FirstOrDefault();


                    if (nextPhysician != null)
                    {
                        var nextPhysicianConnections = ConnectedUsers.Where(m => m.UserId == nextPhysician.cah_phy_key)
                                                             .ToList();
                        if (nextPhysicianConnections.Count() > 0)
                        {
                            var nextQueueItem = _caseAssignHistoryService.GetDetails(nextPhysician.cah_key);
                            nextQueueItem.cah_request_sent_time = DateTime.Now.ToEST();
                            nextQueueItem.cah_request_sent_time_utc = DateTime.UtcNow;
                            nextQueueItem.cah_action = PhysicianCaseAssignQueue.WaitingForAction.ToString();
                            _caseAssignHistoryService.Edit(nextQueueItem);

                            nextPhysicianConnections.ForEach(physician =>
                            {
                                Clients.Client(physician.ConnectionId).showPhysicianCasePopup(id);
                            });

                            LinkCaseWithUser(id, nextPhysician.cah_phy_key); // added to handle disconnection scenario
                        }
                        else
                        {
                            ShowNavigatorPopup(model);

                        }

                    }

                    else
                    {
                        ShowNavigatorPopup(model);
                    }

                    #endregion
                }
                else
                {
                    CleanCaseData(id);
                }

            }

        }

        private void ShowNavigatorPopup(@case model)
        {

            // checking the entry in log so the in any case the popup is not displayed to navigator multiple times
            var navigatorEntry = _caseAssignHistoryService.GetRequests(model.cas_key, PhysicianCaseAssignQueue.InQueue)
                                                     .Where(m => m.cah_phy_key == model.cas_created_by)
                                          .FirstOrDefault();

            if (navigatorEntry != null)
            {
                #region Sending Alert to Physician in case of All Physicians reject the case


                var navigatorList = ConnectedUsers.Where(m => m.UserId == model.cas_created_by)
                                                        .ToList();

                navigatorList.ForEach(navigator =>
                {
                    Clients.Client(navigator.ConnectionId).showNavigatorCasePopup(model.cas_key);

                });

                // re updating the case status from in queue to open in case no physician accept the case
                model.cas_cst_key = CaseStatus.Open.ToInt();
                _caseService.EditCaseOnly(model);
                PhysicianCasePopupHub.CleanCaseData(model.cas_key); // added to handle disconnection event

                #endregion
                navigatorEntry.cah_action = PhysicianCaseAssignQueue.NotAcceptedByAll.ToString();
                _caseAssignHistoryService.Edit(navigatorEntry);
            }
        }

        public void ClosePopupForCurrentUser()
        {
            ClosePopupForUser(Context.User.Identity.GetUserId());
        }

        public void ClosePopupForUser(string userId)
        {
            if (ConnectedUsers != null)
            {
                var allWindows = ConnectedUsers.Where(m => m.UserId == userId).ToList();
                allWindows.ForEach(item =>
                {
                    Clients.Client(item.ConnectionId).closeCasePopup(item.ConnectionId);
                });
            }
        }
        public static void LinkCaseWithUser(int? cas_key, string userId)
        {
            if (ConnectedUsers != null)
            {
                var userConnections = ConnectedUsers.Where(m => m.UserId == userId).ToList();
                userConnections.ForEach(m =>
                {
                    m.cas_key = cas_key;
                });
            }
        }

        public static void CleanCaseData(int cas_key)
        {
            if (ConnectedUsers != null)
            {
                var userConnections = ConnectedUsers.Where(m => m.cas_key == cas_key).ToList();
                userConnections.ForEach(m =>
                {
                    m.cas_key = null;
                });
            }
        }

        public static void CleanUsersData(string UserId)
        {
            if (ConnectedUsers != null)
                lock (ConnectedUsers)
                {
                    ConnectedUsers.RemoveAll(m => m.UserId == UserId);
                }
        }



        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null && item.cas_key.HasValue)
            {
                RejectCase(item.cas_key.Value);
            }
            lock (ConnectedUsers)
            {
                ConnectedUsers.Remove(item);
            }
            return base.OnDisconnected(stopCalled);
        }


    }
}