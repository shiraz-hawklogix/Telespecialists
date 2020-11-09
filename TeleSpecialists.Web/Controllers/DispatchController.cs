using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.BLL.ViewModels.Dispatch;
using TeleSpecialists.Controllers;
using TeleSpecialists.Web.Hubs;


namespace TeleSpecialists.Web.Controllers
{
    [Authorize]
    public class DispatchController : BaseController
    {
        private readonly DispatchService _dispatchService; // code is up to date 01/10/2020
        private readonly UCLService _uclService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly PhysicianService _physician;
        private readonly CaseService _caseService;
        private readonly FacilityPhysicianService _facilityPhysicianService;
        private readonly CaseAssignHistoryService _caseAssignHistoryService;
        private readonly CaseRejectService _caseRejectService;
        private readonly AdminService _adminService;
        private readonly FireBaseUserMailService _fireBaseUserMailService;
        private readonly user_fcm_notification _user_Fcm_Notification;
        private readonly PhysicianStatusService _physicianStatusService;
        private readonly PhysicianStatusLogService _physicianStatusLogService;
        private FireBaseData _fireBaseData;
        private readonly CaseRejectService _casRejectService;


        public DispatchController() : base()
        {
            _dispatchService = new DispatchService();
            _uclService = new UCLService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _caseService = new CaseService();
            _facilityPhysicianService = new FacilityPhysicianService();
            _physician = new PhysicianService();
            _caseAssignHistoryService = new CaseAssignHistoryService();
            _caseRejectService = new CaseRejectService();
            _adminService = new AdminService();
            _fireBaseUserMailService = new FireBaseUserMailService();
            _user_Fcm_Notification = new user_fcm_notification();
            _physicianStatusService = new PhysicianStatusService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _fireBaseData = new FireBaseData();
            _casRejectService = new CaseRejectService();

        }

        public ActionResult Index()
        {
            #region Get Only navigators
            DataSourceResult request = new DataSourceResult();
            List<string> roleIDs = new List<string>();
            roleIDs.Add("43299308-3a6c-4385-acb4-4c354fd5758d");
            var res = _adminService.GetAllUsersIds(roleIDs);
            var list = _fireBaseUserMailService.GetAllSpecificUser(res);
            ViewBag.navigators = list;
            #endregion
            return GetViewResult();
        }

        [HttpGet]
        public JsonResult FetchCase()
        {
            var ss = Session.Keys; 
            Session["LastCaseID"] = _dispatchService.GetLatestCase();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetRefreshCase()
        {
            var refBit = _dispatchService.RefreshCase();
            return Json(refBit, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SetRefreshCase()
        {
            try
            {
                var refBit = _dispatchService.restrictRefresh();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpPost]
        public ActionResult GetAll(Kendo.DynamicLinq.DataSourceRequest request)
        {

            string userId = "";
            List<Guid> facilities = null;
            if (User.IsInRole(UserRoles.Physician.ToDescription()))
            {
                userId = User.Identity.GetUserId();
            }
            else if (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()))
            {
                facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                         .Select(x => x.Facility).ToList();
            }
            var res = _dispatchService.GetCaseLisingPageData(request, userId, facilities); //Getting cases for listing

            if (res.Data != null)
            {
                List<DispatchListing> dispatchResponse = (List<DispatchListing>)res.Data;
                DispatchListing found = dispatchResponse.Where(d => d.cas_cst_key == BLL.Helpers.CaseStatus.WaitingToAccept.ToInt()).FirstOrDefault();
                if (found != null)
                {
                    var caseStatusDropDownItems = CaseStatus();
                    var caseReasonsDropDownItems = GetRejectionReasons();
                    foreach (var cas in dispatchResponse)
                    {
                        cas.PhysicianDD = GetPhysicians(cas.cas_fac_key, cas.cas_ctp_key);
                        var settingselected = cas.PhysicianDD.Where(p => p.Value == cas.cas_phy_key).FirstOrDefault();
                        if (settingselected != null) { settingselected.Selected = true; }

                        cas.CaseStatusDD = caseStatusDropDownItems;

                        cas.ReasonsDD = caseReasonsDropDownItems;

                        //cas.timeElapsed = CalculateWaitingToAcceptTime(cas.case_assign_history_waitingToAccept, cas.case_assign_history_accepted);

                        //var currentTime = Functions.ConvertToFacilityTimeZone(DateTime.Now, "");
                        //cas.timeElapsed = Convert.ToString(currentTime.ToDateTime() - cas.cas_status_assign_date.ToDateTime());

                        cas.timeElapsed = (DateTime.Now.ToEST() - cas.cas_status_assign_date.ToDateTime()).FormatTimeSpan();// ToString();
                        cas.dateTimeElapsed =  cas.cas_status_assign_date;
                        //cas.timeElapsed = (DateTime.Now.ToEST() - cas.case_assign_history_waitingToAccept).FormatTimeSpan();

                        #region Case Copy Stamp

                        cas.CombinedMessage = GetCaseCopyData(cas);

                        #endregion



                    }
                    res.Data = dispatchResponse;
                }
                DispatchListing foundAccepted = dispatchResponse.Where(d => d.cas_cst_key == BLL.Helpers.CaseStatus.Accepted.ToInt()).FirstOrDefault();
                if (foundAccepted != null)
                {
                    var caseStatusDropDownItems = CaseStatus();
                    var caseReasonsDropDownItems = GetRejectionReasons();
                    foreach (var cas in dispatchResponse)
                    {
                        cas.PhysicianDD = GetPhysicians(cas.cas_fac_key, cas.cas_ctp_key);
                        var settingselected = cas.PhysicianDD.Where(p => p.Value == cas.cas_phy_key).FirstOrDefault();
                        if (settingselected != null) { settingselected.Selected = true; }
                        cas.CaseStatusDD = caseStatusDropDownItems;
                        cas.ReasonsDD = caseReasonsDropDownItems;
                        cas.crr_reason = "-Select-";
                        var selectedReason = cas.ReasonsDD.FirstOrDefault();
                        if (selectedReason != null) { selectedReason.Selected = true; }
                        //cas.timeElapsed = CalculateWaitingToAcceptTime(cas.case_assign_history_waitingToAccept, cas.case_assign_history_accepted);

                        //var currentTime = Functions.ConvertToFacilityTimeZone(DateTime.Now, "");
                        //cas.timeElapsed = Convert.ToString(currentTime.ToDateTime() - cas.cas_status_assign_date.ToDateTime());

                        cas.timeElapsed = (DateTime.Now.ToEST() - cas.cas_status_assign_date.ToDateTime()).FormatTimeSpan();// ToString();

                        //cas.timeElapsed = (DateTime.Now.ToEST() - cas.case_assign_history_waitingToAccept).FormatTimeSpan();

                    }
                    res.Data = dispatchResponse;
                }
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult RejectCase(int casKey, string casReasonId, string caseRejectionType)
        {
            var model = _dispatchService.GetCaseDetails(casKey);
            string prvPhysicianKey = model.cas_phy_key;
            string prvPhysician = model.AspNetUser2.FirstName + " " + model.AspNetUser2.LastName;
            string prvPhysicianInitials = model.AspNetUser2.UserInitial;
            string users = "";
            List<FireBaseData> resultList = new List<FireBaseData>();

            if (!string.IsNullOrEmpty(casReasonId))
            {
                case_rejection_reason reason = _caseRejectService.GetDetails(casReasonId.ToInt());

                if (reason.crr_troubleshoot)
                {
                    // Rejection Reason is Troubleshoot Reason
                    #region Husnain code for firebase chat grp

                    if (!string.IsNullOrEmpty(reason.crr_users))
                    {
                        users = reason.crr_users + "," + model.cas_phy_key;
                        var list = users.Split(',');
                        var phy_ids = new HashSet<string>(list);
                        resultList = _fireBaseUserMailService.GetAllSpecificUser(phy_ids);



                        #region Case Re-Assigning and Blast Functionality
                        try
                        {
                            var phyList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).Where(x => x.FinalSorted).ToList();

                            if (phyList != null && phyList.Count > 0)
                            {
                                List<string> PreviousAssignedPhysicians = model.case_assign_history.Select(cs => cs.cah_phy_key).ToList();
                                if (PreviousAssignedPhysicians.Count() > 0)
                                {
                                    // Skip Previous Assigned Physician BEFORE Re Assigning
                                    var filtered = phyList.Where(x => !PreviousAssignedPhysicians.Any(y => y == x.AspNetUser_Id.ToString())).ToList();
                                 // if (filtered.Where(p => p.phs_name == "Available").ToList().Count() > 0)       // Removed Available Status Check
                                    if (filtered.ToList().Count() > 0)
                                    {
                                        model.cas_phy_key = filtered.FirstOrDefault().AspNetUser_Id.ToString();
                                        model.cas_physician_assign_date = DateTime.Now.ToEST();

                                        model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                        model.cas_status_assign_date = DateTime.Now.ToEST();
                                        var status = "Waiting to Accept";

                                        HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                        LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                        model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                        model.cas_modified_by = loggedInUser.Id;
                                        model.cas_modified_by_name = loggedInUser.FullName;
                                        model.cas_modified_date = DateTime.Now.ToEST();

                                    }
                                    else
                                    {
                                        #region CASE REASSIGN EVEN ON BLAST

                                        model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                        model.cas_physician_assign_date = DateTime.Now.ToEST();

                                        model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                        model.cas_status_assign_date = DateTime.Now.ToEST();
                                        var status = "Waiting to Accept";

                                        HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                        LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                        model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                        model.cas_modified_by = loggedInUser.Id;
                                        model.cas_modified_by_name = loggedInUser.FullName;
                                        model.cas_modified_date = DateTime.Now.ToEST();

                                        #endregion
                                        
                                        // Internal Blast
                                        #region Husnain code for firebase
                                        var physicians = phyList.ToList();
                                        HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                                        var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                        var phyids = firebaseUsers.Select(x => x.user_id).ToList();
                                        var paramData = new List<object>();
                                        paramData.Add(JsonConvert.SerializeObject(phyids));
                                        bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phyids, caseType: "InternalBlast", Data: paramData);
                                        #endregion
                                    }
                                }
                                else
                                {
                                    // No Previous Assigned Physician

                                    // if (phyList.Where(p => p.phs_name == "Available").ToList().Count() > 0) // Removed Available Status Check
                                    if (phyList.ToList().Count() > 0)
                                    {
                                        model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                        model.cas_physician_assign_date = DateTime.Now.ToEST();
                                        model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                        model.cas_status_assign_date = DateTime.Now.ToEST();
                                        var status = "Waiting to Accept";

                                        HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                        LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                        model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                        model.cas_modified_by = loggedInUser.Id;
                                        model.cas_modified_by_name = loggedInUser.FullName;
                                        model.cas_modified_date = DateTime.Now.ToEST();
                                    }
                                    //else
                                    //{
                                    //    // Internal Blast
                                    //    #region Husnain code for firebase
                                    //    var physicians = phyList.Where(p => p.phs_name != "Available").ToList();//.Select(x => x.AspNetUser_Id).ToList();
                                    //    HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                                    //    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                    //    var phyids = firebaseUsers.Select(x => x.user_id).ToList();
                                    //    var paramData = new List<object>();
                                    //    paramData.Add(JsonConvert.SerializeObject(phyids));
                                    //    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phyids, caseType: "InternalBlast", Data: paramData);
                                    //    #endregion
                                    //}
                                }

                            }
                            else
                            {
                                var credentialedPhysiciansList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).ToList();

                                if (credentialedPhysiciansList != null && credentialedPhysiciansList.Count > 0)
                                {

                                    #region CASE REASSIGN EVEN ON BLAST

                                    model.cas_phy_key = credentialedPhysiciansList.FirstOrDefault().AspNetUser_Id.ToString();
                                    model.cas_physician_assign_date = DateTime.Now.ToEST();

                                    model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                    model.cas_status_assign_date = DateTime.Now.ToEST();
                                    var status = "Waiting to Accept";

                                    HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                    LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                    model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                    model.cas_modified_by = loggedInUser.Id;
                                    model.cas_modified_by_name = loggedInUser.FullName;
                                    model.cas_modified_date = DateTime.Now.ToEST();

                                    #endregion

                                    // External Blast
                                    #region Husnain code for firebase
                                    HashSet<string> hash_ids = new HashSet<string>(credentialedPhysiciansList.Select(s => s.AspNetUser_Id.ToString()));
                                    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                    var phyids = firebaseUsers.Select(x => x.user_id).ToList();
                                    var paramData = new List<object>();
                                    paramData.Add(JsonConvert.SerializeObject(phyids));
                                    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phyids, caseType: "ExternalBlast", Data: paramData);
                                    #endregion
                                }

                            }


                            if (!string.IsNullOrEmpty(caseRejectionType))
                                model.cas_rejection_type = caseRejectionType;

                            _dispatchService.EditCase(model, false);
                            _dispatchService.Save();
                            _dispatchService.Commit();
                            _dispatchService.UpdateTimeStamps(model.cas_key.ToString());


                            //return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }
                        #endregion


                        return Json(new { list = resultList, caseNumber = model.cas_case_number, initials = prvPhysicianInitials, physicianName = prvPhysician, phy_key  = prvPhysicianKey }, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                }

                if (reason.crr_troubleshoot == false)
                {
                    // Rejection Reason is NOT Troubleshoot Reason
                    try
                    {
                        var phyList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).Where(x => x.FinalSorted).ToList();

                        if (phyList != null && phyList.Count > 0)
                        {
                            List<string> PreviousAssignedPhysicians = model.case_assign_history.Select(cs => cs.cah_phy_key).ToList();
                            if (PreviousAssignedPhysicians.Count() > 0)
                            {
                                // Skip Previous Assigned Physician BEFORE Re Assigning
                                var filtered = phyList.Where(x => !PreviousAssignedPhysicians.Any(y => y == x.AspNetUser_Id.ToString())).ToList();

                                if (filtered.ToList().Count() > 0)
                                {
                                    model.cas_phy_key = filtered.FirstOrDefault().AspNetUser_Id.ToString();
                                    model.cas_physician_assign_date = DateTime.Now.ToEST();

                                    model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                    model.cas_status_assign_date = DateTime.Now.ToEST();
                                    var status = "Waiting to Accept";

                                    HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                    LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                    model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                    model.cas_modified_by = loggedInUser.Id;
                                    model.cas_modified_by_name = loggedInUser.FullName;
                                    model.cas_modified_date = DateTime.Now.ToEST();

                                }
                                else
                                {
                                    #region CASE REASSIGN EVEN ON BLAST

                                    model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                    model.cas_physician_assign_date = DateTime.Now.ToEST();

                                    model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                    model.cas_status_assign_date = DateTime.Now.ToEST();
                                    var status = "Waiting to Accept";

                                    HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                    LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                    model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                    model.cas_modified_by = loggedInUser.Id;
                                    model.cas_modified_by_name = loggedInUser.FullName;
                                    model.cas_modified_date = DateTime.Now.ToEST();

                                    #endregion

                                    // Internal Blast
                                    #region Husnain code for firebase
                                    var physicians = phyList.ToList();
                                    HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                                    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                    var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                                    var paramData = new List<object>();
                                    paramData.Add(JsonConvert.SerializeObject(phy_ids));
                                    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "InternalBlast", Data: paramData);
                                    #endregion
                                }
                            }
                            else
                            {
                                // No Previous Assigned Physician

                                if (phyList.ToList().Count() > 0)
                                {
                                    model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                    model.cas_physician_assign_date = DateTime.Now.ToEST();
                                    model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                    model.cas_status_assign_date = DateTime.Now.ToEST();
                                    var status = "Waiting to Accept";

                                    HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                    LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                    model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                    model.cas_modified_by = loggedInUser.Id;
                                    model.cas_modified_by_name = loggedInUser.FullName;
                                    model.cas_modified_date = DateTime.Now.ToEST();
                                }
                                //else
                                //{
                                //    // Internal Blast
                                //    #region Husnain code for firebase
                                //    var physicians = phyList.Where(p => p.phs_name != "Available").ToList();//.Select(x => x.AspNetUser_Id).ToList();
                                //    HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                                //    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                //    var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                                //    var paramData = new List<object>();
                                //    paramData.Add(JsonConvert.SerializeObject(phy_ids));
                                //    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "InternalBlast", Data: paramData);
                                //    #endregion
                                //}
                            }

                        }
                        else
                        {
                            var credentialedPhysiciansList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).ToList();

                            if (credentialedPhysiciansList != null && credentialedPhysiciansList.Count > 0)
                            {

                                #region CASE REASSIGN EVEN ON BLAST

                                model.cas_phy_key = credentialedPhysiciansList.FirstOrDefault().AspNetUser_Id.ToString();
                                model.cas_physician_assign_date = DateTime.Now.ToEST();

                                model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                model.cas_status_assign_date = DateTime.Now.ToEST();
                                var status = "Waiting to Accept";

                                HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                model.cas_modified_by = loggedInUser.Id;
                                model.cas_modified_by_name = loggedInUser.FullName;
                                model.cas_modified_date = DateTime.Now.ToEST();

                                #endregion

                                // External Blast
                                #region Husnain code for firebase
                                HashSet<string> hash_ids = new HashSet<string>(credentialedPhysiciansList.Select(s => s.AspNetUser_Id.ToString()));
                                var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                                var paramData = new List<object>();
                                paramData.Add(JsonConvert.SerializeObject(phy_ids));
                                bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "ExternalBlast", Data: paramData);
                                #endregion
                            }

                        }


                        if (!string.IsNullOrEmpty(caseRejectionType))
                            model.cas_rejection_type = caseRejectionType;

                        _dispatchService.EditCase(model, false);
                        _dispatchService.Save();
                        _dispatchService.Commit();
                        _dispatchService.UpdateTimeStamps(model.cas_key.ToString());


                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                }
            }
            else
            {
                try
                {
                    var phyList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).Where(x => x.FinalSorted).ToList();

                    if (phyList != null && phyList.Count > 0)
                    {
                        List<string> PreviousAssignedPhysicians = model.case_assign_history.Select(cs => cs.cah_phy_key).ToList();
                        if (PreviousAssignedPhysicians.Count() > 0)
                        {
                            // Skip Previous Assigned Physician BEFORE Re Assigning
                            var filtered = phyList.Where(x => !PreviousAssignedPhysicians.Any(y => y == x.AspNetUser_Id.ToString())).ToList();

                            if (filtered.ToList().Count() > 0)
                            {
                                model.cas_phy_key = filtered.FirstOrDefault().AspNetUser_Id.ToString();
                                model.cas_physician_assign_date = DateTime.Now.ToEST();

                                model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                model.cas_status_assign_date = DateTime.Now.ToEST();
                                var status = "Waiting to Accept";

                                HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                model.cas_modified_by = loggedInUser.Id;
                                model.cas_modified_by_name = loggedInUser.FullName;
                                model.cas_modified_date = DateTime.Now.ToEST();

                            }
                            else
                            {

                                #region CASE REASSIGN EVEN ON BLAST

                                model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                model.cas_physician_assign_date = DateTime.Now.ToEST();

                                model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                model.cas_status_assign_date = DateTime.Now.ToEST();
                                var status = "Waiting to Accept";

                                HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                model.cas_modified_by = loggedInUser.Id;
                                model.cas_modified_by_name = loggedInUser.FullName;
                                model.cas_modified_date = DateTime.Now.ToEST();

                                #endregion


                                // Internal Blast
                                #region Husnain code for firebase
                                var physicians = phyList.ToList();//.Select(x => x.AspNetUser_Id).ToList();
                                if(physicians.Count > 0)
                                {
                                    //These Physcians have not rejected this case yet.
                                    HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                                    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                    var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                                    var paramData = new List<object>();
                                    paramData.Add(JsonConvert.SerializeObject(phy_ids));
                                    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "InternalBlast", Data: paramData);
                                }
                                else
                                {
                                    var credentialedPhysiciansList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).ToList();

                                    if (credentialedPhysiciansList != null && credentialedPhysiciansList.Count > 0)
                                    {
                                        // External Blast
                                        #region Husnain code for firebase
                                        HashSet<string> hash_ids = new HashSet<string>(credentialedPhysiciansList.Select(s => s.AspNetUser_Id.ToString()));
                                        var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                                        var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                                        var paramData = new List<object>();
                                        paramData.Add(JsonConvert.SerializeObject(phy_ids));
                                        bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "ExternalBlast", Data: paramData);
                                        #endregion
                                    }
                                }
                                
                                #endregion
                            }
                        }
                        else
                        {
                            // No Previous Assigned Physician

                            if (phyList.ToList().Count() > 0)
                            {
                                model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                                model.cas_physician_assign_date = DateTime.Now.ToEST();
                                model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                                model.cas_status_assign_date = DateTime.Now.ToEST();
                                var status = "Waiting to Accept";

                                HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                                LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                                model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                                model.cas_modified_by = loggedInUser.Id;
                                model.cas_modified_by_name = loggedInUser.FullName;
                                model.cas_modified_date = DateTime.Now.ToEST();
                            }
                            //else
                            //{
                            //    // Internal Blast
                            //    #region Husnain code for firebase
                            //    var physicians = phyList.Where(p => p.phs_name != "Available").ToList();//.Select(x => x.AspNetUser_Id).ToList();
                            //    HashSet<string> hash_ids = new HashSet<string>(physicians.Select(s => s.AspNetUser_Id.ToString()));
                            //    var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                            //    var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                            //    var paramData = new List<object>();
                            //    paramData.Add(JsonConvert.SerializeObject(phy_ids));
                            //    bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "InternalBlast", Data: paramData);
                            //    #endregion
                            //}
                        }

                    }
                    else
                    {
                        var credentialedPhysiciansList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).ToList();

                        if (credentialedPhysiciansList != null && credentialedPhysiciansList.Count > 0)
                        {

                            #region CASE REASSIGN EVEN ON BLAST

                            model.cas_phy_key = credentialedPhysiciansList.FirstOrDefault().AspNetUser_Id.ToString();
                            model.cas_physician_assign_date = DateTime.Now.ToEST();

                            model.cas_cst_key = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt();
                            model.cas_status_assign_date = DateTime.Now.ToEST();
                            var status = "Waiting to Accept";

                            HandleCaseStatusCodeForCaseRejection(model, _caseService.GetDetails(casKey));

                            LogCaseAssignHistory(casKey, model.cas_phy_key, status, true);
                            model.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                            model.cas_modified_by = loggedInUser.Id;
                            model.cas_modified_by_name = loggedInUser.FullName;
                            model.cas_modified_date = DateTime.Now.ToEST();

                            #endregion

                            // External Blast
                            #region Husnain code for firebase
                            HashSet<string> hash_ids = new HashSet<string>(credentialedPhysiciansList.Select(s => s.AspNetUser_Id.ToString()));
                            var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
                            var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();
                            var paramData = new List<object>();
                            paramData.Add(JsonConvert.SerializeObject(phy_ids));
                            bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: casKey, phy_ids: phy_ids, caseType: "ExternalBlast", Data: paramData);
                            #endregion
                        }

                    }


                    if (!string.IsNullOrEmpty(caseRejectionType))
                        model.cas_rejection_type = caseRejectionType;

                    _dispatchService.EditCase(model, false);
                    _dispatchService.Save();
                    _dispatchService.Commit();
                    _dispatchService.UpdateTimeStamps(model.cas_key.ToString());


                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                } 
            }
             
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetRejectionReasonsMenuString(string cas_key)
        {
            var response = _caseRejectService.GetRejectionReasonsForDispatch();
            var main = response.Where(rs => rs.crr_parent_key == null).ToList();

            StringBuilder builder = new StringBuilder();

            builder.Append("<ul class='main-navigation' id='styleReasonsUL'>");

            foreach (var m in main)
            {
                builder.Append("<li id='styleReasonsDD'>");
                builder.Append("<a href='#' onclick = 'caseRejected(\"" + cas_key + "\",\"" + m.crr_key + "\",\"" + m.crr_reason + "\")'> " + m.crr_reason + "<span style='float:right' unselectable='on' class='k-select' aria-label='select'><span class='k-icon k-i-arrow-60-down'></span></span> </a>");
                
               
                var subReason = response.Where(rs => rs.crr_parent_key == m.crr_key).ToList();
                if (subReason.Count() > 0)
                {
                    builder.Append("<ul>");
                    foreach (var sr in subReason)
                    {
                        builder.Append("<li id='styleReasonsDD'>");
                        builder.Append("<a href='#' onclick = 'caseRejected(\"" + cas_key + "\",\"" + sr.crr_key + "\",\"" + sr.crr_reason + "\")'>" + sr.crr_reason + "</a>");
                        builder.Append("</li>");

                    }
                    builder.Append("</ul>");
                }
                builder.Append("</li>");
            }

            builder.Append("</ul>");

            return Json(builder.ToString(),JsonRequestBehavior.AllowGet); 
        }

        [HttpPost]
        public ActionResult Save(int casKey, string physician, string status)
        { 

            var dbModel = _caseService.GetDetails(casKey);

            #region Check authorized user to save case
            if (User.IsInRole(UserRoles.Physician.ToDescription()))
            {
                if (dbModel.cas_phy_key != User.Identity.GetUserId())
                {
                    ModelState.AddModelError("", "Access Denied! <br/>");
                    ModelState.AddModelError("", "You are not authorized to save this case. This case is reassigned.");
                    return GetErrorResult(dbModel);
                }
            }
            #endregion

            try
            {
                if (!string.IsNullOrEmpty(status))
                    status = status.ToLower();
                Guid? softSaveGuid = null;
                //string physicianId = "";
                string physicianId = physician;
                var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, dbModel.cas_fac_key, softSaveGuid, dbModel.cas_ctp_key).ToList();
                //if (result != null)
                //{
                //    physicianId = result.Where(rp => rp.AspNetUser_FirstName + " " + rp.AspNetUser_LastName == physician).Select(x => x.AspNetUser_Id.ToString()).FirstOrDefault();
                //}
                if (dbModel.cas_phy_key != physicianId && physicianId != "") // Physician Changed

                {
                    dbModel.cas_phy_key = physicianId;
                    dbModel.AspNetUser2 = _physician.GetDetailForDispatch(physicianId);
                    dbModel.cas_physician_assign_date = DateTime.Now.ToEST();

                    HandleCaseStatusCode(dbModel, _caseService.GetDetails(casKey));

                    LogCaseAssignHistory(casKey, physicianId, status, true);
                    dbModel.cas_history_physician_initial = _caseService.GetCaseInitials(casKey);

                    dbModel.cas_modified_by = loggedInUser.Id;
                    dbModel.cas_modified_by_name = loggedInUser.FullName;
                    dbModel.cas_modified_date = DateTime.Now.ToEST();
                }

                int statusint = 0;
                if (status == "accepted")
                { statusint = BLL.Helpers.CaseStatus.Accepted.ToInt(); }
                if (status == "cancelled")
                { statusint = BLL.Helpers.CaseStatus.Cancelled.ToInt(); }
                if (status == "complete")
                { statusint = BLL.Helpers.CaseStatus.Complete.ToInt(); }
                if (status == "open")
                { statusint = BLL.Helpers.CaseStatus.Open.ToInt(); }
                if (status == "waiting to accept")
                { statusint = BLL.Helpers.CaseStatus.WaitingToAccept.ToInt(); }


                if (dbModel.cas_cst_key != statusint) // Case Status Changed
                {
                    if (statusint == BLL.Helpers.CaseStatus.Accepted.ToInt())
                    {
                        if (!dbModel.cas_response_time_physician.HasValue)
                            dbModel.cas_response_time_physician = DateTime.UtcNow;
                    }

                    dbModel.cas_cst_key = statusint;
                    dbModel.cas_status_assign_date = DateTime.Now.ToEST();

                    dbModel.cas_modified_by = loggedInUser.Id;
                    dbModel.cas_modified_by_name = loggedInUser.FullName;
                    dbModel.cas_modified_date = DateTime.Now.ToEST();
                    HandleCaseStatusCode(dbModel, _caseService.GetDetails(casKey));
                }
                try
                {
                    if (status == "waiting to accept")
                    {
                        SendCaseToPhysician(dbModel);
                    }
                }
                catch (Exception e) { }

                _dispatchService.EditCase(dbModel, false);
                _dispatchService.Save();
                _dispatchService.Commit();
                _dispatchService.UpdateTimeStamps(dbModel.cas_key.ToString());

            }
            catch (Exception ex)
            {
                ViewBag.CaseModel = _caseService.GetDetails(casKey);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
                return GetErrorResult(dbModel);
            }

            return Json("Updated", JsonRequestBehavior.AllowGet);
        }
        private void HandleCaseStatusCode(@case model, @case dbModel)
        {
            bool updatePhysicianStatus = true;
            bool isPhysicianCurrentCase = false;
            var previousPhysician = _adminService.GetAspNetUsers().Where(m => m.status_change_cas_key == model.cas_key && m.Id == model.cas_phy_key).FirstOrDefault();
            if (previousPhysician != null)
            {
                if (previousPhysician.status_key == PhysicianStatus.Stroke.ToInt() || previousPhysician.status_key == PhysicianStatus.STATConsult.ToInt())
                    isPhysicianCurrentCase = true;
            }


            HideNavigatorCasePopup(dbModel.cas_key, dbModel.cas_phy_key);




            if (model.cas_cst_key == BLL.Helpers.CaseStatus.Accepted.ToInt()
                && !string.IsNullOrEmpty(model.cas_phy_key)
                && (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatConsult.ToInt())
                && isPhysicianCurrentCase

                )
            {
                var assignedPhysician = UserManager.FindById(model.cas_phy_key);
                if (assignedPhysician.status_key != PhysicianStatus.Available.ToInt())
                {

                    if (model.cas_billing_bic_key == CaseBillingCode.NotSeen.ToInt() && (model.cas_ctp_key != dbModel.cas_ctp_key || model.cas_cst_key != dbModel.cas_cst_key || model.cas_phy_key != dbModel.cas_phy_key || dbModel.cas_billing_bic_key != model.cas_billing_bic_key))
                    {
                        SetStatus(PhysicianStatus.Available.ToInt(), null, model.cas_phy_key, $"Status changed to Available due to Billing Code for {model.cas_key}");
                        updatePhysicianStatus = false;
                    }
                    else if (model.cas_metric_video_end_time_est != null && (model.cas_ctp_key != dbModel.cas_ctp_key || model.cas_cst_key != dbModel.cas_cst_key || model.cas_phy_key != dbModel.cas_phy_key || dbModel.cas_billing_bic_key != model.cas_billing_bic_key || dbModel.cas_metric_video_end_time_est == null))
                    {
                        SetStatus(PhysicianStatus.Available.ToInt(), null, model.cas_phy_key, $"Status changed to Available due to video end time for {model.cas_key}");
                        updatePhysicianStatus = false;
                    }
                }

            }


            if (updatePhysicianStatus && (model.cas_ctp_key != dbModel.cas_ctp_key || model.cas_cst_key != dbModel.cas_cst_key || model.cas_phy_key != dbModel.cas_phy_key))
            {
                #region updating physician status
                if (model.cas_cst_key == BLL.Helpers.CaseStatus.Accepted.ToInt() && !string.IsNullOrEmpty(model.cas_phy_key))
                {
                    UpdateStatusOfPreviousPhy(model, dbModel);
                    var case_type = _uclService.GetDetails(model.cas_ctp_key);
                    var status = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == case_type.ucd_title.ToLower());
                    if (status != null)
                    {
                        SetStatus(status.phs_key, model.cas_key, model.cas_phy_key, $"Case {model.cas_key} Assigned to Physician from Case Edit Page");
                    }
                }
                else if ((model.cas_cst_key == BLL.Helpers.CaseStatus.Complete.ToInt() || model.cas_cst_key == BLL.Helpers.CaseStatus.Cancelled.ToInt())
                                    && !string.IsNullOrEmpty(model.cas_phy_key)
                                    && (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatConsult.ToInt())
                                    )
                {

                    UpdateStatusOfPreviousPhy(model, dbModel);

                    if (isPhysicianCurrentCase) // if it is current
                    {
                        SetStatus(PhysicianStatus.Available.ToInt(), null, model.cas_phy_key, $"Status changed to Available due to case {model.cas_key} updated to status {model.cas_cst_key}");
                    }
                }
                #endregion
            }
        }

        private async Task HideNavigatorCasePopup(int cas_key, string phy_key)
        {

            string strWaitingForAction = PhysicianCaseAssignQueue.WaitingForAction.ToString();
            var physicianReq = _caseAssignHistoryService.GetAll().Where(m => m.cah_cas_key == cas_key && m.cah_phy_key == phy_key)
                                                                 .Where(m => m.cah_action == strWaitingForAction)
                                                                 .OrderByDescending(m => m.cah_key)
                                                                 .FirstOrDefault();






            if (physicianReq != null)
            {
                var IsNotExpired = (DateTime.Now.ToEST() - (physicianReq.cah_request_sent_time != null ? physicianReq.cah_request_sent_time.Value : DateTime.Now.ToEST().AddDays(-1))).TotalMinutes < 2;
                if (IsNotExpired)
                {


                    if (Settings.RPCMode == RPCMode.SignalR)
                    {
                        #region Sending Alert to Physician in case of All Physicians reject the case

                        var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                        var navigatorList = PhysicianCasePopupHub.ConnectedUsers.Where(m => m.UserId == phy_key)
                                                                .ToList();

                        navigatorList.ForEach(navigator =>
                        {
                            hubContext.Clients.Client(navigator.ConnectionId).showNavigatorAcceptCasePopupWithNoQueue();

                        });


                        #endregion
                    }
                    else if (Settings.RPCMode == RPCMode.WebSocket)
                    {
                        //new WebSocketEventHandler().CallJSMethod(phy_key, new SocketResponseModel { MethodName = "closeNavigatorCasePopupWithNoQueue_def" });
                    }
                    bool status = _user_Fcm_Notification.SendNotification(phy_key: User.Identity.GetUserId(), caseType: "closeNavigatorCase");
                }
            }
        }

        private void SetStatus(int id, int? cas_key, string userId, string comments)
        {

            userId = string.IsNullOrEmpty(userId) ? User.Identity.GetUserId() : userId;
            var physician = UserManager.FindById(userId);
            bool isUpdateStatusDate = true;

            var status = _physicianStatusService.GetDetails(id);

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
            physician.status_change_cas_key = cas_key;
            physician.status_change_date_forAll = DateTime.Now.ToEST();

            UserManager.Update(physician);
            LogStatusChange(physician.status_key.Value, physician.Id, cas_key, comments);

            if (BLL.Settings.RPCMode == RPCMode.SignalR)
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
            else if (BLL.Settings.RPCMode == RPCMode.WebSocket)
            {
                var dataList = new List<object>();
                dataList.Add(isUpdateStatusDate);
                //new WebSocketEventHandler().CallJSMethod(userId, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus", Data = dataList });
            }
            bool _status = _user_Fcm_Notification.SendNotification(phy_key: userId, caseType: "refreshPhyStatus");
        }

        private void UpdateStatusOfPreviousPhy(@case model, @case dbModel)
        {

            if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatConsult.ToInt())
            {
                var previousPhysician = _adminService.GetAspNetUsers().Where(m => m.status_change_cas_key == model.cas_key).FirstOrDefault();
                // currently the status will be only reversed in case of physician is removed from the case or physician is changed
                if (previousPhysician != null)
                {
                    if (previousPhysician.Id != model.cas_phy_key) //|| model.cas_cst_key == CaseStatus.Cancelled.ToInt() || model.cas_cst_key == CaseStatus.Complete.ToInt())
                    {
                        var dbCaseType = _uclService.GetDetails(dbModel.cas_ctp_key);
                        var dbCaseStatus = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == dbCaseType.ucd_title.ToLower());

                        //int? phy_status_key = null;
                        if (dbCaseStatus != null)
                        {
                            var physician = UserManager.FindById(previousPhysician.Id);
                            if (physician.status_change_cas_key == dbModel.cas_key) // if it is current
                            {
                                // reset the physician status to available
                                physician.status_change_date = DateTime.Now.ToEST();
                                physician.status_change_date_forAll = DateTime.Now.ToEST();
                                physician.status_key = PhysicianStatus.Available.ToInt();
                                physician.status_change_cas_key = null;
                                UserManager.Update(physician);

                                HideNavigatorCasePopup(dbModel.cas_key, dbModel.cas_phy_key);

                                LogStatusChange(PhysicianStatus.Available.ToInt(), physician.Id, dbModel.cas_key, $"Case Assigned to {model.cas_phy_key}");

                                if (Settings.RPCMode == RPCMode.SignalR)
                                {
                                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                                    var physicianOnlineWindows = PhysicianCasePopupHub.ConnectedUsers
                                                                                .Where(m => m.UserId == physician.Id)
                                                                                .ToList();
                                    physicianOnlineWindows.ForEach(window =>
                                    {
                                        hubContext.Clients.Client(window.ConnectionId).syncPhysicianStatusTime();
                                    });
                                }
                                else if (Settings.RPCMode == RPCMode.WebSocket)
                                {
                                    //new WebSocketEventHandler().CallJSMethod(physician.Id, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus" });
                                }
                                bool status = _user_Fcm_Notification.SendNotification(phy_key: physician.Id, caseType: "refreshPhyStatus");
                            }
                        }
                    }

                }
            }

        }

        public void LogStatusChange(int psl_phs_key, string phy_key, int? cas_key, string comments)
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
                psl_cas_key = cas_key,
                psl_created_date = DateTime.Now.ToEST(),
                psl_created_by = User.Identity.GetUserId(),
                psl_user_key = phy_key,
                psl_phs_key = psl_phs_key,
                psl_start_date = DateTime.Now.ToEST(),
                psl_comments = comments,
                psl_status_name = physician.physician_status.phs_name
            };

            _physicianStatusLogService.Create(new_entry);
        }

        private void HandleCaseStatusCodeForCaseRejection(@case model, @case dbModel)
        {
            bool updatePhysicianStatus = true;
            bool isPhysicianCurrentCase = false;
            var previousPhysician = _adminService.GetAspNetUsers().Where(m => m.status_change_cas_key == model.cas_key && m.Id == dbModel.cas_phy_key).FirstOrDefault();
            if (previousPhysician != null)
            {
                if (previousPhysician.status_key == PhysicianStatus.Stroke.ToInt() || previousPhysician.status_key == PhysicianStatus.STATConsult.ToInt())
                    isPhysicianCurrentCase = true;
                SetStatus(PhysicianStatus.Available.ToInt(), null, dbModel.cas_phy_key, $"Status changed to Available due to Case Rejection for {model.cas_key}");
                UpdateStatusOfPreviousPhy(model, dbModel);
            }

            HideNavigatorCasePopup(dbModel.cas_key, dbModel.cas_phy_key);

            if (model.cas_cst_key == BLL.Helpers.CaseStatus.WaitingToAccept.ToInt()
                && !string.IsNullOrEmpty(model.cas_phy_key)
                && (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatConsult.ToInt())
                && isPhysicianCurrentCase

                )
            {
                var assignedPhysician = UserManager.FindById(model.cas_phy_key);
                if (assignedPhysician.status_key == PhysicianStatus.Available.ToInt())
                {

                    if (model.cas_cst_key != dbModel.cas_cst_key && model.cas_phy_key != dbModel.cas_phy_key)
                    {
                        //SetStatus(PhysicianStatus.Stroke.ToInt(), null, model.cas_phy_key, $"Status changed to Stroke due to Case Reassigning through Case Rejection for {model.cas_key}");
                        updatePhysicianStatus = false;
                    }
                }
            }
        }
        private void LogCaseAssignHistory(int cas_key, string cas_phy_key, string cah_action, bool isManualAssign)
        {
            _caseAssignHistoryService.Create(new case_assign_history
            {
                cah_cas_key = cas_key,
                cah_phy_key = cas_phy_key,
                cah_action = cah_action,
                cah_created_date = DateTime.Now.ToEST(),
                cah_created_date_utc = DateTime.UtcNow,
                cah_created_by = User.Identity.GetUserId(),
                cah_is_active = true,
                cah_request_sent_time = DateTime.Now.ToEST(),
                cah_action_time = DateTime.Now.ToEST(),
                cah_is_manuall_assign = isManualAssign,
                cah_action_time_utc = DateTime.UtcNow,
                cah_request_sent_time_utc = DateTime.UtcNow

            });
        }
        [NonAction]
        public List<SelectListItem> CaseStatus()
        {
            var statuslist = _uclService.GetUclData(UclTypes.CaseStatus).ToList();
            List<SelectListItem> caseStatuslist = statuslist.Where(s => s.ucd_title != "Complete").Select(d => new SelectListItem
            {
                Text = d.ucd_title,
                Value = Convert.ToString(d.ucd_key)
            }).ToList();
            caseStatuslist.Where(p => p.Value == Convert.ToString(BLL.Helpers.CaseStatus.WaitingToAccept.ToInt())).First().Selected = true;

            return caseStatuslist;
        }
        [HttpPost]
        public JsonResult GetPhysiciansForInternalBlast(Guid fac_key, int? cas_ctp_key, Guid? softSaveGuid,  int cas_key, string strokeStamp)
        {
            if (cas_ctp_key == null)
                cas_ctp_key = CaseType.StrokeAlert.ToInt();
            var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();
            var scheduleUser = result.Where(x => x.FinalSorted).ToList();
            List<string> phy_ids = new List<string>();
            foreach(var item in scheduleUser)
            {
                string id = Convert.ToString( item.AspNetUser_Id);
                var userDetail = _fireBaseUserMailService.GetDetails(id);
                if(userDetail != null)
                {
                    phy_ids.Add(id);
                    item.firebaseId = userDetail.fre_firebase_uid;
                    item.firebaseEmail = userDetail.fre_firebase_email;
                    item.img = "/Content/images/M.png";
                }
            }

            #region Husnain code for firebase
            var paramData = new List<object>();
            paramData.Add(JsonConvert.SerializeObject(phy_ids));
            bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: cas_key, phy_ids: phy_ids , caseType: "InternalBlast",  Data:paramData, strokeStamp: strokeStamp);
            #endregion

            //Session["InternalBlast"] = scheduleUser;

            return Json(scheduleUser, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetPhysiciansForExternalBlast(Guid fac_key, int? cas_ctp_key, Guid? softSaveGuid, int cas_key)
        {
            if (cas_ctp_key == null)
                cas_ctp_key = CaseType.StrokeAlert.ToInt();
            var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();
            //var _idsList = result.Select(x => x.AspNetUser_Id).ToList();
            //
            HashSet<string> hash_ids = new HashSet<string>(result.Select(s => s.AspNetUser_Id.ToString()));
            var firebaseUsers = _fireBaseUserMailService.GetAllSpecificUserForAuto(hash_ids);
            var phy_ids = firebaseUsers.Select(x => x.user_id).ToList();

            #region Husnain code for firebase
            var paramData = new List<object>();
            paramData.Add(JsonConvert.SerializeObject(phy_ids));
            bool sentStatus = _user_Fcm_Notification.SendNotification(caseId: cas_key, phy_ids: phy_ids, caseType: "ExternalBlast", Data: paramData);
            #endregion

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [NonAction]
        public List<SelectListItem> GetPhysicians(Guid fac_key, int? cas_ctp_key)
        {
            if (cas_ctp_key == null)
                cas_ctp_key = CaseType.StrokeAlert.ToInt();

            Guid? softSaveGuid = null;

            var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();

            List<SelectListItem> physiciansList = result.Select(d => new SelectListItem
            {
                Text = d.FullName,
                Value = d.AspNetUser_Id.ToString()
            }).ToList();

            return physiciansList;
        }

        [NonAction]
        public List<SelectListItem> GetRejectionReasons()
        {
            

            var result = _caseRejectService.GetAllRecords().ToList();

            List<SelectListItem> reasonsList = result.Select(d => new SelectListItem
            {
                Text = d.crr_reason,
                Value = d.crr_key.ToString()
            }).ToList();

            return reasonsList;
        }

        [HttpPost]
        public ActionResult GetPhysiciansForDispatch(Guid fac_key, int? cas_ctp_key, Guid? softSaveGuid)
        {
            try
            {
                if (cas_ctp_key == null)
                    cas_ctp_key = CaseType.StrokeAlert.ToInt();

                var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();
                var scheduleUser = result.Where(x => x.FinalSorted).ToList();
                ViewBag.OnlineUsers = OnlineUserIds;

                var phyGridHtml = RenderPartialViewToString("_dispatchPhysicianStatusList", scheduleUser);
                return Json(new { htmlData = phyGridHtml }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public string GetCaseCopyData(int case_key)
        {
            StringBuilder copytext = new StringBuilder();
            var dbModel = _caseService.GetDetails(case_key);

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


                if (dbModel.cas_cst_key != 0)
                {
                    var casType = (CaseStatus)dbModel.cas_cst_key;
                    copytext.Append(casType);
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

        [NonAction]
        public string GetCaseCopyData(DispatchListing dbModel)
        {
            StringBuilder copytext = new StringBuilder();

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

                 
                if (dbModel.fac_name != null)
                {
                    var fac_name = dbModel.fac_name;
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

        [NonAction]
        private string CalculateWaitingToAcceptTime(DateTime? waitingToAcceptTime, DateTime? acceptedTime)
        {
            var waitingTime = string.Empty;
            string waitingToAcceptStatus = BLL.Helpers.CaseStatus.WaitingToAccept.ToDescription().ToLower();
            string acceptedStatus = BLL.Helpers.CaseStatus.Accepted.ToDescription().ToLower();
            if (waitingToAcceptTime != null && acceptedTime != null)
            {
                TimeSpan? d = (acceptedTime - waitingToAcceptTime);
                waitingTime = d.FormatTimeSpan();
            }

            if (string.IsNullOrEmpty(waitingTime))
            {
                waitingTime = "00:00:00";
            }

            return waitingTime;
        }


        [HttpGet]
        public JsonResult GetBtnStatusList()
        {
            var res = _dispatchService.GetButtonStatus();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DisableButton(string cas_key)
        {
            var resp = _dispatchService.disableSaveButton(cas_key.ToInt());

            return Json(resp.ToString() , JsonRequestBehavior.AllowGet);
        }





            //    [NonAction]
            //    private string UpTime(string countTo)
            //    {
            //        //if (element.length == 0)
            //        //    return "";

            //        DateTime countToDate = Convert.ToDateTime(countTo);
            //        try
            //        {
            //            DateTime now = DateTime.Now;  //new Date(new Date().toUTCString().replace(" GMT", ""));
            //            //countTo = new DateTime(countTo);
            //            var difference = now - countToDate;
            //            var days = difference.Days; // / (60 * 60 * 1000 * 24) * 1;
            //            var years = days / 365;
            //            if (years > 1)
            //            {
            //                days = days - (years * 365);
            //            }
            //            var hours = difference.Hours;  // (difference % (60 * 60 * 1000 * 24)) / (60 * 60 * 1000) * 1;
            //            var mins = difference.Minutes; // ((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) / (60 * 1000) * 1;
            //            var secs = difference.Seconds; // (((difference % (60 * 60 * 1000 * 24)) % (60 * 60 * 1000)) % (60 * 1000)) / 1000 * 1;
            //            //document.getElementById('years').firstChild.nodeValue = years;
            //            // document.getElementById('days').firstChild.nodeValue = days;
            //            return "";
            //        }
            ////            if (secs.formatNumber() >= 0)
            ////            {

            ////                var dd = days * 24;
            ////                var hrs = (hours + (days >= 1 ? dd : 0));

            ////                element.find("#hours").html(hrs);
            ////                element.find('#minutes').html(mins.formatNumber(2));
            ////                element.find('#seconds').html(secs.formatNumber(2));

            ////                var caseStatus = $("#cas_cst_key").val();
            ////                if (hrs > 0 && caseStatus != caseStatusEnum.Accepted)
            ////                    element.find("#custom").addClass("elapsed-time");
            ////                else if (mins.formatNumber(2) > 7 && caseStatus != caseStatusEnum.Accepted)
            ////                    element.find("#custom").addClass("elapsed-time");
            ////                else
            ////                    element.find("#custom").removeClass("elapsed-time");

            ////                if (typeof(callBackFunction) == "function")
            ////                {
            ////                    callBackFunction();
            ////                }
            ////            }
            ////            else
            ////            {
            ////                element.find("#hours").html("00");
            ////                element.find('#minutes').html("00");
            ////                element.find('#seconds').html("00");

            ////            }

            ////            var elementId = element.attr("id");

            ////            clearTimeout(upTime[elementId]);

            ////            element.show();
            ////            if (callBackFunction != undefined)
            ////                upTime[elementId] = setTimeout(function() { upTime(countTo, element, callBackFunction); }, 1000);
            ////else
            ////    upTime[elementId] = setTimeout(function() { upTime(countTo, element); }, 1000);
            ////        }
            //       catch { return ""; }
            //    }


            #region Dispatch Without Kendo


            //[HttpGet]
            //public ActionResult GetAllWaitingToAccept()
            //{
            //    List<DispatchListing> model = new List<DispatchListing>();
            //    var caseStatusDropDownItems = CaseStatus();
            //    List<@case> caseslist = _dispatchService.GetWaitingToAcceptCases();
            //    if (caseslist != null && caseslist.Count() > 0)
            //    {
            //        foreach (var cs in caseslist)
            //        {
            //            DispatchListing temp = new DispatchListing();
            //            temp.caseModel = cs;
            //            temp.PhysicianDD = GetPhysicians(cs.cas_fac_key, cs.cas_ctp_key);
            //            var settingselected = temp.PhysicianDD.Where(p => p.Value == cs.cas_phy_key).FirstOrDefault();
            //            if (settingselected != null) { settingselected.Selected = true; }
            //            temp.CaseStatusDD = caseStatusDropDownItems;
            //            model.Add(temp);
            //        }
            //    }

            //    return PartialView("_dispatchWaitingToAccept", model);
            //}

            //[HttpGet]
            //public ActionResult GetAllAccepted()
            //{
            //    List<@case> caseslist = _dispatchService.GetAcceptedCases();
            //    return PartialView("_dispatchAcceptedCases", caseslist);
            //}

            //[HttpGet]
            //public string GetCaseCopyData(int case_key)
            //{
            //    StringBuilder copytext = new StringBuilder();
            //    var dbModel = _caseService.GetDetails(case_key);

            //    if (dbModel != null)
            //    {
            //        var timezone = "";
            //        if (dbModel.FacilityTimeZone != null)
            //        {
            //            timezone = dbModel.FacilityTimeZone;
            //        }
            //        if (timezone == "")
            //        {
            //            timezone = "EST";
            //        }

            //        if (dbModel.cas_response_ts_notification != null)
            //        {
            //            var cas_response_ts_notification = dbModel.cas_response_ts_notification;

            //            copytext.Append(cas_response_ts_notification + " " + timezone + " " + " Local Time");
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_response_ts_notification == null && dbModel.cas_metric_stamp_time_est != null)
            //        {
            //            var cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time_est;

            //            copytext.Append(cas_metric_stamp_time_est + " " + timezone + " " + " Local Time");
            //            copytext.Append("##NewLine##");
            //        }


            //        if (dbModel.cas_cst_key != 0)
            //        {
            //            var casType = (CaseStatus)dbModel.cas_cst_key;
            //            copytext.Append(casType);
            //            copytext.Append("##NewLine##");
            //        }

            //        if (dbModel.facility != null)
            //        {
            //            var fac_name = dbModel.facility.fac_name;
            //            copytext.Append(fac_name);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_cart != null)
            //        {
            //            var cart = dbModel.cas_cart;
            //            copytext.Append("Cart: " + cart);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_callback != null)
            //        {
            //            var callback = dbModel.cas_callback;
            //            callback = Functions.FormatAsPhoneNumber(callback);
            //            copytext.Append("Callback Phone: " + callback);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_callback_extension != null)
            //        {
            //            var extension = dbModel.cas_callback_extension;
            //            copytext.Append("Extension: " + extension);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_patient != null)
            //        {
            //            var patientname = dbModel.cas_patient;
            //            copytext.Append("Patient: " + patientname);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_triage_notes != null)
            //        {
            //            var triagenotes = dbModel.cas_triage_notes;
            //            copytext.Append("Triage Notes: " + triagenotes);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_notes != null)
            //        {
            //            var notes = dbModel.cas_notes;
            //            copytext.Append("Notes: " + notes);
            //            copytext.Append("##NewLine##");
            //        }
            //        if (dbModel.cas_eta != null)
            //        {
            //            var eta = dbModel.cas_eta;
            //            copytext.Append("##NewLine##");
            //            copytext.Append("##NewLine##");
            //            copytext.Append("ETA: " + eta);
            //            copytext.Append("##NewLine##");
            //        }
            //    }

            //    return copytext.ToString();
            //}


            //[HttpPost]
            //public ActionResult Save(int casKey, string physician, int status)
            //{
            //    var dbModel = _caseService.GetDetails(casKey);

            //    #region Check authorized user to save case
            //    if (User.IsInRole(UserRoles.Physician.ToDescription()))
            //    {
            //        if (dbModel.cas_phy_key != User.Identity.GetUserId())
            //        {
            //            ModelState.AddModelError("", "Access Denied! <br/>");
            //            ModelState.AddModelError("", "You are not authorized to save this case. This case is reassigned.");
            //            return GetErrorResult(dbModel);
            //        }
            //    }
            //    #endregion

            //    try
            //    {
            //        if (dbModel.cas_phy_key != physician) // Physician Changed
            //        {
            //            dbModel.cas_phy_key = physician;
            //            dbModel.cas_physician_assign_date = DateTime.Now.ToEST();


            //            dbModel.cas_modified_by = loggedInUser.Id;
            //            dbModel.cas_modified_by_name = loggedInUser.FullName;
            //            dbModel.cas_modified_date = DateTime.Now.ToEST();
            //        }

            //        if (dbModel.cas_cst_key != status) // Case Status Changed
            //        {
            //            if (status == TeleSpecialists.BLL.Helpers.CaseStatus.Accepted.ToInt())
            //            {
            //                if (!dbModel.cas_response_time_physician.HasValue)
            //                    dbModel.cas_response_time_physician = DateTime.UtcNow;
            //            }

            //            dbModel.cas_cst_key = status;
            //            dbModel.cas_status_assign_date = DateTime.Now.ToEST();

            //            dbModel.cas_modified_by = loggedInUser.Id;
            //            dbModel.cas_modified_by_name = loggedInUser.FullName;
            //            dbModel.cas_modified_date = DateTime.Now.ToEST();
            //        }

            //        _caseService.Edit(dbModel, false);
            //        _caseService.Save();
            //        _caseService.Commit();
            //        _caseService.UpdateTimeStamps(dbModel.cas_key.ToString());

            //    }
            //    catch (Exception ex)
            //    {
            //        ViewBag.CaseModel = _caseService.GetDetails(casKey);
            //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            //        ModelState.AddModelError("", "Error! Please try again.");
            //        return GetErrorResult(dbModel);
            //    }

            //    return Json("Updated", JsonRequestBehavior.AllowGet);
            //}


            //[NonAction]
            //public List<SelectListItem> GetPhysicians(Guid fac_key, int? cas_ctp_key)
            //{
            //    if (cas_ctp_key == null)
            //        cas_ctp_key = CaseType.StrokeAlert.ToInt();

            //    Guid? softSaveGuid = null;

            //    var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();

            //    List<SelectListItem> physiciansList = result.Select(d => new SelectListItem
            //    {
            //        Text = d.Name,
            //        Value = d.Id
            //    }).ToList();

            //    return physiciansList;
            //}


            //[HttpPost]
            //public ActionResult GetPhysiciansForDispatch(Guid fac_key, int? cas_ctp_key, Guid? softSaveGuid)
            //{
            //    try
            //    {
            //        if (cas_ctp_key == null)
            //            cas_ctp_key = CaseType.StrokeAlert.ToInt();

            //        var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cas_ctp_key).ToList();
            //        var scheduleUser = result.Where(x => x.isScheduled).ToList();
            //        ViewBag.OnlineUsers = OnlineUserIds;

            //        var phyGridHtml = RenderPartialViewToString("_dispatchPhysicianStatusList", scheduleUser);
            //        return Json(new { htmlData = phyGridHtml }, JsonRequestBehavior.AllowGet);

            //    }
            //    catch (Exception ex)
            //    {
            //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            //        return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            //    }
            //}




            ////private string CalculateWaitingToAcceptTime(DispatchListing model)
            ////{
            ////    var waitingTime = string.Empty;
            ////    //if (model.cas_cst_key == CaseStatus.Accepted.ToInt())
            ////    //{
            ////    string waitingToAcceptStatus = TeleSpecialists.BLL.Helpers.CaseStatus.WaitingToAccept.ToDescription().ToLower();
            ////    string acceptedStatus = TeleSpecialists.BLL.Helpers.CaseStatus.Accepted.ToDescription().ToLower();
            ////    var waitingToAcceptTime = model. Where(m => m.cah_action.ToLower().Trim() == waitingToAcceptStatus).OrderByDescending(m => m.cah_key).FirstOrDefault();
            ////    var acceptedTime = model.case_assign_history.Where(m => m.cah_action.ToLower().Trim() == acceptedStatus).OrderByDescending(m => m.cah_key).FirstOrDefault();
            ////    if (waitingToAcceptTime != null && acceptedTime != null)
            ////    {
            ////        TimeSpan? d = (acceptedTime.cah_created_date - waitingToAcceptTime.cah_created_date);
            ////        waitingTime = d.FormatTimeSpan();
            ////    }
            ////    //}

            ////    if (string.IsNullOrEmpty(waitingTime))
            ////    {
            ////        waitingTime = "00:00:00";
            ////    }

            ////    return waitingTime;
            ////}

            //[NonAction]
            //public List<SelectListItem> CaseStatus()
            //{
            //    var statuslist = _uclService.GetUclData(UclTypes.CaseStatus).ToList();
            //    List<SelectListItem> caseStatuslist = statuslist.Select(d => new SelectListItem
            //    {
            //        Text = d.ucd_title,
            //        Value = Convert.ToString(d.ucd_key)
            //    }).ToList();
            //    caseStatuslist.Where(p => p.Value == Convert.ToString(BLL.Helpers.CaseStatus.WaitingToAccept.ToInt())).First().Selected = true;

            //    return caseStatuslist;
            //}


            #endregion

            #region Husnain Code for firebase notifications
            private async Task SendCaseToPhysician(@case model)
        {

            if (model.cas_cst_key == BLL.Helpers.CaseStatus.WaitingToAccept.ToInt() && model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
            {
                _caseAssignHistoryService.Create(new case_assign_history
                {
                    cah_cas_key = model.cas_key,
                    cah_phy_key = model.cas_phy_key,
                    cah_action = PhysicianCaseAssignQueue.WaitingForAction.ToString(),
                    cah_request_sent_time = DateTime.Now.ToEST(),
                    cah_created_date = DateTime.Now.ToEST(),
                    cah_created_date_utc = DateTime.UtcNow,
                    cah_created_by = User.Identity.GetUserId(),
                    cah_sort_order = 1,
                    cah_is_active = true,
                    cah_request_sent_time_utc = DateTime.UtcNow,

                });
            }

            if (Settings.RPCMode == RPCMode.SignalR)
            {
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                var physicianOnlineWindows = PhysicianCasePopupHub.ConnectedUsers
                                                            .Where(m => m.UserId == model.cas_phy_key)
                                                            .ToList();

                physicianOnlineWindows.ForEach(window =>
                {
                    hubContext.Clients.Client(window.ConnectionId).showPhysicianNewCasePopup(model.cas_phy_key);
                });


            }
            else if (Settings.RPCMode == RPCMode.WebSocket)
            {
                //new WebSocketEventHandler().CallJSMethod(model.cas_phy_key, new SocketResponseModel { MethodName = "showPhysicianNewCasePopup_def", Data = new List<object> { model.cas_key, true } });
            }
            #region Husnain code for firebase
            bool sentStatus = _user_Fcm_Notification.SendNotification(model.cas_phy_key, model.cas_key);
            #endregion

        }
        #endregion

        public ActionResult _firebaseChat()
        {
            var result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
            if (result != null)
            {
                _fireBaseData.user_id = loggedInUser.Id;
                _fireBaseData.teleid = 1;
                _fireBaseData.name = result.fre_firstname;
                _fireBaseData.ImgPath = result.fre_profileimg;
                _fireBaseData.email = result.fre_firebase_email;
                _fireBaseData.password = result.fre_firebase_email;
                _fireBaseData.UserName = result.fre_email;

                //var file = RenderImage("");
            }
            return PartialView(_fireBaseData);
        }
    }
}