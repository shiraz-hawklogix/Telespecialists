using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Model;
using System;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.BLL;
using System.Collections.Generic;
using System.Text;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class PhysicianController : BaseController
    {
        // GET: PhysicianStatus
        private readonly PhysicianStatusService _physicianStatusService;
        private readonly PhysicianService _physician;
        private readonly PhysicianStatusLogService _physicianStatusLogService;
        private readonly PhysicianStatusSnoozeService _physicianStatusSnoozeService;
        private readonly CaseService _caseService;
        private readonly UCLService _uclService;

        public PhysicianController()
        {
            _physicianStatusService = new PhysicianStatusService();
            _physician = new PhysicianService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _physicianStatusSnoozeService = new PhysicianStatusSnoozeService();
            _caseService = new CaseService();
            _uclService = new UCLService();
        }

        public ActionResult Dashboard()
        {
            return GetViewResult();
        }
        public ActionResult Index(string id = "")
        {
            id = string.IsNullOrEmpty(id) ? User.Identity.GetUserId() : id;
            var physician = _physician.GetDetail(id);
            // excluding the current status from list
            var list = _physicianStatusService.GetAll().Where(m => m.phs_key != physician.status_key);

            #region Rules Added Define in Ticket TCARE-11 Physician Status Rules/Changes
            if (User.IsInRole(UserRoles.Physician.ToDescription()))
            {
                var notAvailable = PhysicianStatus.NotAvailable.ToInt();
                list = list.Where(m => m.phs_key != notAvailable);
            }

            if (physician?.physician_status?.phs_key != PhysicianStatus.Stroke.ToInt())
            {
                var tpa = PhysicianStatus.TPA.ToDescription();
                list = list.Where(m => m.phs_name.ToLower() != tpa);
            }

            #endregion

            ViewBag.physician = physician;
            ViewBag.Id = id;
            return GetViewResult(list.ToList());
        }
        public ActionResult ChangeStatusPopup(string id)
        {
            var physician = _physician.GetDetail(id);
            // excluding the current status from list
            var list = _physicianStatusService.GetAll().Where(m => m.phs_key != physician.status_key);

            /* Getting only those physicians who are eligible for stroke alter cases.*/
            #region TCARE-483, excluding stroke altert if physician is not stroke alert
            if (!physician.IsStrokeAlert)
            {
                var strokeAlert = PhysicianStatus.Stroke.ToInt();
                list = list.Where(m => m.phs_key != strokeAlert);
            }
            #endregion

            #region Rules Added Define in Ticket TCARE-11 Physician Status Rules/Changes
            if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()) || User.IsInRole(UserRoles.AOC.ToDescription()))
            {
                var statusList = new List<int> { PhysicianStatus.Available.ToInt(), PhysicianStatus.Rounding.ToInt() };

                list = list.Where(m => statusList.Contains(m.phs_key));
            }

            if (physician?.physician_status?.phs_key != PhysicianStatus.Stroke.ToInt() && physician?.physician_status?.phs_key != PhysicianStatus.TPA.ToInt())
            {
                var tpa = PhysicianStatus.TPA.ToDescription();
                list = list.Where(m => m.phs_name.ToLower() != tpa);
            }


            #endregion

            ViewBag.physician = physician;
            ViewBag.Id = id;
            return GetViewResult(list.ToList());
        }
        public ActionResult _CurrentStatus(string id = "")
        {
            if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
            {
                var physician = _physician.GetDetail(User.Identity.GetUserId());
                return PartialView(physician);
            }
            else if (!string.IsNullOrEmpty(id))
            {
                id = string.IsNullOrEmpty(id) ? User.Identity.GetUserId() : id;
                var physician = _physician.GetDetail(id);
                return PartialView(physician);
            }
            else
            {
                return PartialView();
            }
        }
        public ActionResult GetCurrentStatus()
        {
            var physician = _physician.GetDetail(User.Identity.GetUserId());
            string result = RenderPartialViewToString("_CurrentStatus", physician);
            return Json(new { data = result, status_key = physician.status_key }, JsonRequestBehavior.AllowGet);
        }
        [OutputCache(Duration = 1)]
        public ActionResult _ChangeStatus()
        {
            var physician = _physician.GetDetail(User.Identity.GetUserId());
            return PartialView(physician);
        }
        [AccessRoles(Roles = "Super Admin,Administrator,Navigator,Partner Physician,Regional Medical Director,Outsourced Navigator,RRC Manager,RRC Director,AOC,Medical Staff")]
        public ActionResult Status(string id)
        {
            ViewBag.Id = id;
            var statusInteral = ApplicationSetting?.aps_status_page_interval >= 0 ? ApplicationSetting?.aps_status_page_interval : 1000;
            ViewBag.statusInteral = statusInteral;
            return GetViewResult();
        }
        public ActionResult _StatusList(bool showOnlyLoggedInUsers, string sortOrder = "")
        {

            var list = _physician.GetPhysicianStatusDashboard(sortOrder);
            if (showOnlyLoggedInUsers)
            {

                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();
            return PartialView(list.ToList());
        }

        #region Husnain  Code Block
        public ActionResult _PacStatusList(bool showOnlyLoggedInUsers, string sortOrder = "")
        {

            var list = _physician.GetPacPhysicianStatusDashboard(sortOrder);
            var _li = list.ToList();
            if (showOnlyLoggedInUsers)
            {

                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();
            return PartialView(list.ToList());
        }

        public ActionResult _SleepStatusList(bool showOnlyLoggedInUsers, string sortOrder = "")
        {

            var list = _physician.GetSleepPhysicianStatusDashboard(sortOrder);
            if (showOnlyLoggedInUsers)
            {
                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();
            return PartialView(list.ToList());
        }

        public ActionResult _NHStatusList(bool showOnlyLoggedInUsers, string sortOrder = "")
        {

            var list = _physician.GetNHPhysicianStatusDashboard(sortOrder);
            if (showOnlyLoggedInUsers)
            {
                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();
            return PartialView(list.ToList());
        }

        #endregion

        public JsonResult SetStatus(int id, string userId = "")
        {
            try
            {
                userId = string.IsNullOrEmpty(userId) ? User.Identity.GetUserId() : userId;
                var physician = UserManager.FindById(userId);
                bool isUpdateStatusDate = true;
                var status = _physicianStatusService.GetDetails(id);
                if (physician.status_key != id)
                {
                    #region Applying Rule 3 Define in TCARE-11 -  Physician Status Rules/Changes


                    if (physician.status_key == PhysicianStatus.Stroke.ToInt() && status.phs_key == PhysicianStatus.TPA.ToInt())
                    {
                        isUpdateStatusDate = false;
                    }
                    #endregion

                    physician.status_key = id;
                    if (isUpdateStatusDate)
                    {
                        physician.status_change_date = DateTime.Now.ToEST();
                    }

                    physician.status_change_date_forAll = DateTime.Now.ToEST();

                    physician.status_change_cas_key = null;
                    UserManager.Update(physician);
                    LogStatusChange(physician.status_key.Value, physician.Id);
                    ClearPreivousSnoozeData(userId);
                }
                else
                {
                    return Json(new { success = false, isResetTimer = false, message = "physician is already in that status" });
                }

                // if the administor is updating the status of physician, then refresh the timer of physician also  if online
                if (userId != loggedInUser.Id)
                {
                    if (Settings.RPCMode == RPCMode.SignalR)
                    {
                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                        var physicianOnlineWindows = PhysicianCasePopupHub.ConnectedUsers
                                                                    .Where(m => m.UserId == userId)
                                                                    .ToList();
                        physicianOnlineWindows.ForEach(window =>
                        {
                            hubContext.Clients.Client(window.ConnectionId).syncPhysicianStatusTime(isUpdateStatusDate);
                        });
                    }
                    else
                    {
                        var dataList = new List<object>();
                        dataList.Add(isUpdateStatusDate.ToString());
                        new WebSocketEventHandler().CallJSMethod(userId, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus", Data = dataList });
                    }

                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        phs_color_code = status.phs_color_code,
                        phy_status_date = DateTime.Now.ToUniversalTime().ToString("MMM,dd,yyyy,HH:mm:ss"),
                        phs_name = status.phs_name,
                        isResetTimer = isUpdateStatusDate
                    }
                });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, isResetTimer = false, message = "An error occurred while updating the status, please try later." });
            }
        }
        public ActionResult StatusSnoozePopup(int phs_key)
        {
            var status = _physicianStatusService.GetDetails(phs_key);
            string html = RenderPartialViewToString("_StatusSnoozePopup", status);
            return Json(new { data = html, userId = loggedInUser.Id }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SnoozeStatus(int phs_key, int pso_key)
        {
            var selectedSnoozeOption = _physicianStatusService.GetStatusSnoozeOption(pso_key);
            if (selectedSnoozeOption != null)
            {
                _physicianStatusSnoozeService.UpdateExistingRecords(phs_key, loggedInUser.Id);

                var currentSnooze = _physicianStatusSnoozeService.GetCurrentSnooze(phs_key, loggedInUser.Id);
                currentSnooze.pss_snooze_time = selectedSnoozeOption.pso_snooze_time;
                currentSnooze.pss_is_latest_snooze = false;
                currentSnooze.pss_modified_by = loggedInUser.Id;
                currentSnooze.pss_modified_by_name = loggedInUser.FullName;
                currentSnooze.pss_modified_date = DateTime.Now.ToEST();
                // updating the currentSnooze record
                _physicianStatusSnoozeService.Edit(currentSnooze);

            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowNewCasePopup(int id)
        {
            var caseDetail = _caseService.GetDetailsWithoutTimeConversion(id);
            if (caseDetail != null)
            {
                if (caseDetail.cas_phy_key == User.Identity.GetUserId())
                {
                    ViewBag.CaseType = _uclService.GetDetails(caseDetail.cas_ctp_key)?.ucd_title;
                    ViewBag.strokeStamp = GetCaseCopyData(caseDetail);
                    return GetViewResult("_NewCasePopupPhysician", caseDetail);
                }
            }

            return Json(new { success = false, message = "Case Not Found" }, JsonRequestBehavior.AllowGet);
        }
        #region Stroke for internal blast
        public ActionResult ShowNewCasePopupInternalBlast(int id)
        {
            var caseDetail = _caseService.GetDetailsWithoutTimeConversion(id);
            if (caseDetail != null)
            {
                ViewBag.CaseType = _uclService.GetDetails(caseDetail.cas_ctp_key)?.ucd_title;
                ViewBag.strokeStamp = GetCaseCopyData(caseDetail);
                return GetViewResult("_NewCasePopupPhysicianBlast", caseDetail);
            }
            return Json(new { success = false, message = "Case Not Found" }, JsonRequestBehavior.AllowGet);
        }

        public string GetCaseCopyData(@case dbModel)
        {
            StringBuilder copytext = new StringBuilder();
            //var dbModel = _caseService.GetDetails(case_key);

            if (dbModel != null)
            {
                var timezone = "";
                if (dbModel.FacilityTimeZone != null)
                {
                    timezone = dbModel.FacilityTimeZone;
                }
                if (timezone == "")
                {
                    timezone = "EST";
                }

                if (dbModel.cas_response_ts_notification != null)
                {
                    var cas_response_ts_notification = dbModel.cas_response_ts_notification;

                    copytext.Append(cas_response_ts_notification + " " + timezone + " " + " Local Time");
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_response_ts_notification == null && dbModel.cas_metric_stamp_time_est != null)
                {
                    var cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time_est;

                    copytext.Append(cas_metric_stamp_time_est + " " + timezone + " " + " Local Time");
                    copytext.Append("##NewLine##");
                }


                if (dbModel.facility != null)
                {
                    var fac_name = dbModel.facility.fac_name;
                    copytext.Append(fac_name);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_cart != null)
                {
                    var cart = dbModel.cas_cart;
                    copytext.Append("Cart: " + cart);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_callback != null)
                {
                    var callback = dbModel.cas_callback;
                    callback = Functions.FormatAsPhoneNumber(callback);
                    copytext.Append("Callback Phone: " + callback);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_callback_extension != null)
                {
                    var extension = dbModel.cas_callback_extension;
                    copytext.Append("Extension: " + extension);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_patient != null)
                {
                    var patientname = dbModel.cas_patient;
                    copytext.Append("Patient: " + patientname);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_triage_notes != null)
                {
                    var triagenotes = dbModel.cas_triage_notes;
                    copytext.Append("Triage Notes: " + triagenotes);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_notes != null)
                {
                    var notes = dbModel.cas_notes;
                    copytext.Append("Notes: " + notes);
                    copytext.Append("##NewLine##");
                }
                if (dbModel.cas_eta != null)
                {
                    var eta = dbModel.cas_eta;
                    copytext.Append("##NewLine##");
                    copytext.Append("##NewLine##");
                    copytext.Append("ETA: " + eta);
                    copytext.Append("##NewLine##");
                }
            }

            return copytext.ToString();
        }
        #endregion

        public void LogStatusChange(int psl_phs_key, string phy_key)
        {
            var physician = _physician.GetDetail(phy_key);
            var physician_status_log = _physicianStatusLogService.GetExistingLog(phy_key);
            if (physician_status_log != null)
            {
                physician_status_log.psl_end_date = DateTime.Now.ToEST();
                physician_status_log.psl_modified_by = User.Identity.GetUserId();
                physician_status_log.psl_modified_date = DateTime.Now.ToEST();
                _physicianStatusLogService.Edit(physician_status_log);
            }
            var new_entry = new physician_status_log
            {
                psl_created_date = DateTime.Now.ToEST(),
                psl_created_by = User.Identity.GetUserId(),
                psl_user_key = phy_key,
                psl_phs_key = psl_phs_key,
                psl_start_date = DateTime.Now.ToEST(),
                psl_status_name = physician.physician_status.phs_name
            };
            _physicianStatusLogService.Create(new_entry);
        }
        public ActionResult GetCTACasesList()
        {
            var model = _caseService.GetCTACaseslist(loggedInUser.Id, ApplicationSetting);
            if (model != null)
            {
                return PartialView("_CTACasesList", model);
            }
            return PartialView("_CTACasesList", new PhysicianCaseListing());
        }

        public ActionResult UpdatePhysicianPendingCasesClearanceDate()
        {
            int result = 0;
            using (var _appSettingService = new AppSettingService())
            {
                result = _appSettingService.UpdatePhysicianPendingCasesClearanceDate();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePhysicianCTAPendingCasesClearanceDate()
        {
            int result = 0;
            using (var _appSettingService = new AppSettingService())
            {
                result = _appSettingService.UpdatePhysicianCTAPendingCasesClearanceDate();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


public ActionResult CanReadEEGs(string id)
        {
            var _canReadEEGs = false;
            try
            {
                var physician = _physician.GetDetail(id);
                if(physician != null)
                {
                    _canReadEEGs = physician.IsEEG;
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { iseeg = _canReadEEGs}, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        public void ClearPreivousSnoozeData(string userId)
        {
            var list = _physicianStatusSnoozeService.GetUnProcessedRecords(userId);
            list.ForEach(m =>
            {
                m.pss_is_latest_snooze = false;
                m.pss_processed_date = DateTime.Now.ToEST();
                m.pss_modified_by = loggedInUser.Id;
                m.pss_modified_by_name = loggedInUser.FullName;
            });

            _physicianStatusSnoozeService.SaveChanges();
        }

        #region ----- IDispose -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _physicianStatusService?.Dispose();
                    _physician?.Dispose();
                    _physicianStatusLogService?.Dispose();
                    _physicianStatusSnoozeService?.Dispose();
                    _caseService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

