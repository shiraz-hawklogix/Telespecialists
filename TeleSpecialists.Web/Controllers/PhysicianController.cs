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
        private readonly FacilityService _facilityService;

        public PhysicianController()
        {
            _physicianStatusService = new PhysicianStatusService();
            _physician = new PhysicianService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _physicianStatusSnoozeService = new PhysicianStatusSnoozeService();
            _caseService = new CaseService();
            _uclService = new UCLService();
            _facilityService = new FacilityService();
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

            var list = _physician.GetPhysicianStatusDashboard(sortOrder);//.ToList();
            if (showOnlyLoggedInUsers)
            {

                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));//.ToList();
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();


            //if (list.Count() > 0)
            //{
            //    foreach (var l in list)
            //    {
            //        if (l.physician.physician_status.phs_key != 1        // Available
            //            && l.physician.physician_status.phs_key != 4     // Rounding
            //            && l.physician.physician_status.phs_key != 19    // Rounding Prep
            //            && l.physician.physician_status.phs_key != 5     // Not Available
            //            && l.physician.physician_status.phs_key != 16)   // Break
            //        {
            //            int casKey = (int)l.physician.physician_status_log.Where(x => x.psl_user_key == l.physician.Id && x.psl_cas_key != null).OrderByDescending(or => or.psl_created_date).Select(s => s.psl_cas_key).FirstOrDefault();

            //            var casedata = _caseService.GetDetails(casKey);
            //            if (casedata != null)
            //            {
            //                l.CaseDetails = "Case #: " + casedata.cas_case_number + ", " + "Cart: " + casedata.cas_cart + ", " + "Facility: " + casedata.facility.fac_name;
            //            }
            //        }


            //        if (l.physician.physician_status.phs_key == 4    // Rounding
            //            || l.physician.physician_status.phs_key == 19)    // Rounding Prep
            //        {
            //            Guid facKey = (Guid)l.physician.physician_status_log.Where(x => x.psl_user_key == l.physician.Id && x.psl_fac_key != null).OrderByDescending(or => or.psl_created_date).Select(s => s.psl_fac_key).FirstOrDefault();
            //            if (facKey != null && facKey != Guid.Empty)
            //            {
            //                var facility = _facilityService.GetDetails(facKey);
            //                l.FacilityName = "Facility: " + facility.fac_name;
            //            }
            //        }
            //    }
            //}
            return PartialView(list); 
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

            var list = _physician.GetNHPhysicianStatusDashboard(sortOrder);//.ToList();
            if (showOnlyLoggedInUsers)
            {
                list = list.Where(m => OnlineUserIds.Contains(m.physician.Id));//.ToList();
            }
            var statusList = list.Where(m => m.physician.physician_status != null).Select(m => m.physician.physician_status).Distinct();
            ViewBag.OnlineUsers = OnlineUserIds;
            ViewBag.statusList = statusList.ToList();
            ViewBag.defaultStatus = _physicianStatusService.GetDefault();
            //if (list.Count() > 0)
            //{
            //    foreach (var l in list)
            //    {
            //        if (l.physician.physician_status.phs_key != 1        // Available
            //            && l.physician.physician_status.phs_key != 4     // Rounding
            //            && l.physician.physician_status.phs_key != 19    // Rounding Prep
            //            && l.physician.physician_status.phs_key != 5     // Not Available
            //            && l.physician.physician_status.phs_key != 16)   // Break
            //        {
            //            int casKey = (int)l.physician.physician_status_log.Where(x => x.psl_user_key == l.physician.Id && x.psl_cas_key != null).OrderByDescending(or => or.psl_created_date).Select(s => s.psl_cas_key).FirstOrDefault();

            //            var casedata = _caseService.GetDetails(casKey);
            //            if (casedata != null)
            //            {
            //                l.CaseDetails = "Case #: " + casedata.cas_case_number + ", " + "Cart: " + casedata.cas_cart + ", " + "Facility: " + casedata.facility.fac_name;
            //            }
            //        }


            //        if (l.physician.physician_status.phs_key == 4    // Rounding
            //            || l.physician.physician_status.phs_key == 19)    // Rounding Prep
            //        {
            //            Guid facKey = (Guid)l.physician.physician_status_log.Where(x => x.psl_user_key == l.physician.Id && x.psl_fac_key != null).OrderByDescending(or => or.psl_created_date).Select(s => s.psl_fac_key).FirstOrDefault();
            //            if (facKey != null && facKey != Guid.Empty)
            //            {
            //                var facility = _facilityService.GetDetails(facKey);
            //                l.FacilityName = "Facility: " + facility.fac_name;
            //            }
            //        }
            //    }
            //}
            return PartialView(list);
        }

        #endregion

        public JsonResult SetStatus(int id, string userId = "", string casKey = "", string facKey = "")
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
                    LogStatusChange(physician.status_key.Value, physician.Id, casKey, facKey);
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
                    return GetViewResult("_NewCasePopupPhysician", caseDetail);
                }
            }

            return Json(new { success = false, message = "Case Not Found" }, JsonRequestBehavior.AllowGet);
        }


        public void LogStatusChange(int psl_phs_key, string phy_key, string casKey = "", string facKey = "")
        {
            var physician = _physician.GetDetail(phy_key);
            var physician_status_log = _physicianStatusLogService.GetExistingLog(phy_key);

            BLL.Model.@case casedata = new @case();
            BLL.Model.facility Foundfacility = new BLL.Model.facility();

            if (physician_status_log != null)
            {
                if (casKey != "")
                {
                    physician_status_log.psl_cas_key = Convert.ToInt32(casKey);
                    casedata = _caseService.GetDetails(Convert.ToInt32(casKey));
                    if (casedata != null)
                    {
                        physician_status_log.psl_case_details = "Case #: " + casedata.cas_case_number + ", " + "Cart: " + casedata.cas_cart + ", " + "Facility: " + casedata.facility.fac_name;
                    }
                }
                if (facKey != "")
                {
                    physician_status_log.psl_fac_key = new Guid(facKey);
                    if (facKey != null && new Guid(facKey) != Guid.Empty)
                    {
                        Foundfacility = _facilityService.GetDetails(new Guid(facKey));
                        physician_status_log.psl_facility_name = "Facility: " + Foundfacility.fac_name;
                    }
                }
                physician_status_log.psl_end_date = DateTime.Now.ToEST();
                physician_status_log.psl_modified_by = User.Identity.GetUserId();
                physician_status_log.psl_modified_date = DateTime.Now.ToEST();
                _physicianStatusLogService.Edit(physician_status_log);
            }

            Nullable<Guid> facility;
            Nullable<int> casenum;
            string casedetails;
            string facilityName;

            if (facKey != "")
            {
                facility = new Guid(facKey);
                facilityName = Foundfacility.fac_name;
            }
            else
            {
                facility = null;
                facilityName = null;
            }

            if (casKey != "")
            {
                casenum = Convert.ToInt32(casKey);
                casedetails = "Case #: " + casedata.cas_case_number + ", " + "Cart: " + casedata.cas_cart + ", " + "Facility: " + casedata.facility.fac_name;
            }
            else
            {
                casenum = null;
                casedetails = null;
            }
            


                var new_entry = new physician_status_log
            {
                psl_cas_key = casenum,
                psl_fac_key = facility,
                psl_case_details = casedetails,
                psl_facility_name = facilityName,
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
        #region  Nabeel's Code

        [HttpGet]
        public JsonResult GetCaseNumberDropDown(string physicianKey)
        {
            var dropdown = _caseService.GetCaseNumberDropDown(physicianKey);
            return Json(dropdown, JsonRequestBehavior.AllowGet);
        }


        #endregion



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
                    _facilityService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

