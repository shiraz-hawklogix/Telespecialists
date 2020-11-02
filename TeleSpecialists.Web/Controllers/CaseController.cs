using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.BLL.ViewModels.FacilityUser;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.Web.Models;
using TeleSpecialists.BLL.Service.CaseServices;
using Kendo.DynamicLinq;

namespace TeleSpecialists.Controllers
{
    /// <summary>   
    /// Will Require Admin Authentication
    /// </summary>
    [Authorize]
    public class CaseController : BaseController
    {
        private readonly AdminService _adminService;
        private readonly CaseService _caseService;
        private readonly CaseGridService _caseGridService;
        private readonly FacilityService _facilityService;
        private readonly ContactService _contactService;
        private readonly UCLService _uclService;
        private readonly FacilityPhysicianService _facilityPhysicianService;
        private readonly EntityNoteService _entityNotesService;
        private readonly CallhistoryService _callHistoryService;
        private readonly CaseAssignHistoryService _caseAssignHistoryService;
        private readonly PhysicianStatusService _physicianStatusService;
        private readonly NIHStrokeScaleService _nihStrokeScaleService;
        private readonly LookupService _lookUpService;
        private readonly PhysicianService _physician;
        private readonly PhysicianStatusLogService _physicianStatusLogService;
        private readonly CaseTemplateService _templateService;
        private readonly CaseGeneratedTemplateService _generateTemplateService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly FacilityContractService _facilityContractService;
        private readonly RootCauseService _rootCauseService;
        private readonly CaseReviewTemplateService _casereviewTemplateService;
        private readonly PhysicianCaseTempService _physicianCaseTempService;
        private readonly PostAcuteCareService _homeHealthService;
        private readonly CasCancelledTypeService _casCancelledTypeService;
        private readonly DiagnosisCodesService _diagnosisCodesService;
        private readonly HospitalprotocolServices _Protocols;
        private readonly OnBoardedServices _OnBoardedServices;
        private readonly TokenService _tokenservice;
        private readonly user_fcm_notification _user_Fcm_Notification;
        private readonly FireBaseUserMailService _fireBaseUserMailService;

        private const int _min_dob_year = 1753;
        private static bool isCalculateBill { get; set; }

        /// <summary>
        /// Constructor, initialize instances here.
        /// </summary>
        public CaseController() : base()
        {
            _adminService = new AdminService();
            _caseService = new CaseService();
            _uclService = new UCLService();
            _facilityService = new FacilityService();
            _facilityPhysicianService = new FacilityPhysicianService();
            _entityNotesService = new EntityNoteService();
            _contactService = new ContactService();
            _callHistoryService = new CallhistoryService();
            _caseAssignHistoryService = new CaseAssignHistoryService();
            _physicianStatusService = new PhysicianStatusService();

            _physician = new PhysicianService();
            _physicianStatusLogService = new PhysicianStatusLogService();
            _nihStrokeScaleService = new NIHStrokeScaleService();
            _templateService = new CaseTemplateService();
            _lookUpService = new LookupService();
            _generateTemplateService = new CaseGeneratedTemplateService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _facilityContractService = new FacilityContractService();
            _rootCauseService = new RootCauseService();
            _caseGridService = new CaseGridService();
            _physicianCaseTempService = new PhysicianCaseTempService();
            _casereviewTemplateService = new CaseReviewTemplateService();
            _homeHealthService = new PostAcuteCareService();
            _casCancelledTypeService = new CasCancelledTypeService();
            _diagnosisCodesService = new DiagnosisCodesService();
            _Protocols = new HospitalprotocolServices();
            _tokenservice = new TokenService();
            _user_Fcm_Notification = new user_fcm_notification();
            _fireBaseUserMailService = new FireBaseUserMailService();
            _OnBoardedServices = new OnBoardedServices();
        }

        public ActionResult Index()
        {
            try
            {

                if (TempData.ContainsKey("Message"))
                    TempData.Remove("Message");

                bool allowListing = true;
                if (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                            .Select(x => x.Facility).ToList();
                    if (facilities.Count < 1)
                    {
                        allowListing = false;
                        ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                    }
                }
                if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                            .Select(x => x.Facility).ToList();
                    if (facilities.Count < 1)
                    {
                        allowListing = false;
                        ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                    }
                }
                //  Commented Temporay
                //if (User.IsInRole(UserRoles.QPS.ToDescription()))
                //{
                //    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                //                            .Select(x => x.Facility).ToList();
                //    if (facilities.Count < 1)
                //    {
                //        allowListing = false;
                //        ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                //    }
                //}
                ViewBag.isSleepAllow = loggedInUser.IsSleep;
                ViewBag.AllowListing = allowListing;
                ViewBag.Error = (TempData["Error"] as bool?) ?? false;
                ViewBag.Message = TempData["StatusMessage"] as string;
                ViewBag.StatusFilter = _uclService.GetUclData(UclTypes.CaseStatus).ToList();
                if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                    ViewBag.iscalculateBill = isCalculateBill;
                else
                    ViewBag.iscalculateBill = false;
                return GetViewResult();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [AccessRoles(Roles = "Administrator,Super Admin,Quality Team,Regional Medical Director,Navigator,QPS,RRC Manager,RRC Director,VP Quality, Quality Director,AOC")]
        public ActionResult Dashboard()
        {
            if (TempData.ContainsKey("Message"))
                TempData.Remove("Message");

            bool allowListing = true;
            if (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()))
            {
                var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                                         .Select(x => x.Facility)
                                                         .ToList();
                if (facilities.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }
            if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
            {
                var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                        .Select(x => x.Facility).ToList();
                if (facilities.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }
            #region Can be used in future for qps facility purpose
            //if (User.IsInRole(UserRoles.QPS.ToDescription()))
            //{
            //    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //                            .Select(x => x.Facility).ToList();
            //    if (facilities.Count < 1)
            //    {
            //        allowListing = false;
            //        ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
            //    }
            //}
            #endregion
            if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
            {
                var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                        .Select(x => x.Facility).ToList();
                if (facilities.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }
            ViewBag.AllowListing = allowListing;
            ViewBag.Error = (TempData["Error"] as bool?) ?? false;
            ViewBag.Message = TempData["StatusMessage"] as string;
            ViewBag.StatusFilter = _uclService.GetUclData(UclTypes.CaseStatus).ToList();
            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                 .Select(m => new
                                 {
                                     Key = Convert.ToInt32(m).ToString(),
                                     Value = m.ToDescription()
                                 }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            ViewBag.CallerSource = _uclService.GetUclData(UclTypes.CallerSource)
                                     .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                     .ToList()
                                     .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;

            return GetViewResult();
        }

        [AccessRoles(NotInRoles = "Facility Admin,Regional Medical Director")]
        public ActionResult CreateMultipleCases()
        {

            return GetViewResult();
        }

        [AccessRoles(NotInRoles = "Facility Admin,Regional Medical Director")]
        public ActionResult SaveMultipleCases(MultipleCaseViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isFacilityTeleneuro = false;
                    var contract = _facilityContractService.GetDetails(new Guid(model.cas_fac_key));
                    if (contract != null)
                    {
                        if (contract.fct_service_calc.Contains(ContractServiceTypes.TeleNeuro.ToString()))
                        {
                            isFacilityTeleneuro = true;
                        }
                    }

                    var casesList = new List<@case>();
                    foreach (var caseModel in model.Cases)
                    {
                        var caseObj = new @case
                        {

                            cas_ctp_key = caseModel.CaseType.ucd_key,
                            cas_fac_key = new Guid(model.cas_fac_key),
                            cas_billing_date_of_consult = model.cas_billing_date_of_consult,
                            cas_phy_key = model.cas_phy_key,
                            cas_patient = Functions.DecodeFrom64(caseModel.cas_patient_name),
                            cas_identification_number = Functions.DecodeFrom64(caseModel.cas_identification_number),
                            cas_billing_diagnosis = Functions.DecodeFrom64(caseModel.cas_billing_diagnosis),
                            IsCaseCompleted = caseModel.isMarkCompleted,
                            cas_billing_comments = Functions.DecodeFrom64(caseModel.comments)

                        };
                        caseObj.cas_commnets_off = model.cas_commnets_off;
                        if (caseModel.BillingCode.ucd_key > 0)
                        {
                            caseObj.cas_billing_bic_key = caseModel.BillingCode.ucd_key;
                        }

                        if (caseModel.IdentificationType.ucd_key > 0)
                        {
                            caseObj.cas_identification_type = caseModel.IdentificationType.ucd_key;
                        }

                        if (isFacilityTeleneuro)
                        {
                            if (!string.IsNullOrEmpty(caseModel.VisitType.ucd_key))
                            {
                                caseObj.cas_billing_visit_type = caseModel.VisitType.ucd_title;
                            }
                        }

                        if (caseModel.cas_billing_dob.HasValue)
                            caseObj.cas_billing_dob = caseModel.cas_billing_dob;


                        if (model.cas_phy_key != "")
                        {
                            if (caseModel.isMarkCompleted)
                            {
                                caseObj.cas_cst_key = CaseStatus.Complete.ToInt();
                            }
                            else
                            {
                                if (caseObj.cas_phy_key != loggedInUser.Id)
                                    caseObj.cas_cst_key = CaseStatus.Open.ToInt();
                                else
                                    caseObj.cas_cst_key = CaseStatus.Accepted.ToInt();
                            }

                        }
                        else
                        {
                            caseObj.cas_cst_key = CaseStatus.Open.ToInt();
                        }

                        caseObj = PrepareNewCaseData(caseObj);

                        if (caseModel.VisitType.ucd_key == FollowUpTypes.FollowUp.ToString())
                        {
                            caseObj.cas_follow_up_date = DateTime.Now.ToEST().AddDays(1).Date;
                        }
                        casesList.Add(caseObj);

                    }

                    _caseService.SaveMultipleCases(casesList);
                    if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                    {
                        var isComplete = casesList.Where(x => x.cas_cst_key == 20).FirstOrDefault();
                        if (isComplete != null)
                            isCalculateBill = true;
                        else
                            isCalculateBill = false;
                    }

                    return GetSuccessResult(Url.Action("Index"));
                }
                else
                {
                    return GetErrorResult(model);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }

        }
        public ActionResult _AdvanceSearch(PageSource source, bool showFollowUp = true)
        {
            ViewBag.cas_fac_key = _lookUpService.GetAllFacility("").Select(m => new SelectListItem
            {
                Text = m.fac_name,
                Value = m.fac_key.ToString()
            }).ToList();

            var _case_types = _uclService.GetUclData(UclTypes.CaseType).Where(c => c.ucd_is_active); ;
            var _case_Status = _uclService.GetUclData(UclTypes.CaseStatus).Where(c => c.ucd_is_active);
            //if(source== PageSource.SignOutListing || source== PageSource.CaseListing)
            //{
            //    _case_types = _case_types.Where(c => c.ucd_is_active);
            //}

            ViewBag.UserType = _case_types.Select(m => new SelectListItem
            {
                Text = m.ucd_title,
                Value = m.ucd_key.ToString()
            }).ToList();

            ViewBag.CaseStatus = _case_Status.Select(m => new SelectListItem
            {
                Text = m.ucd_title,
                Value = m.ucd_key.ToString()
            }).ToList();

            ViewBag.user_Initials = _lookUpService.GetPhysicians().
                                                OrderBy(m => m.UserInitial)
                                                .Select(m => new SelectListItem
                                                {
                                                    Text = m.UserInitial,
                                                    Value = m.Id
                                                }).ToList();

            ViewBag.showFollowUp = showFollowUp;
            ViewBag.source = source;
            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;
            return PartialView();
        }
        public JsonResult GetPhysiciansForCases(Guid fac_key, int? cType_key, Guid? softSaveGuid)
        {
            try
            {
                if (cType_key == null)
                    cType_key = CaseType.StrokeAlert.ToInt();

                var result = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, fac_key, softSaveGuid, cType_key).ToList();
                var scheduleUser = result.Where(x => x.FinalSorted).ToList();

                ViewBag.OnlineUsers = OnlineUserIds;

                var phyGridHtml = RenderPartialViewToString("_PhysicianStatusList", scheduleUser);
                return Json(new
                {
                    data = result != null && result.Count > 0 ? result : new List<PhysicianStatusViewModel>()
                    ,
                    htmlData = phyGridHtml
                }
                , JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckPhysicianForCase(string Id)
        {
            try
            {
                var lastCase = _caseService.GetPhysicianLastCase(Id)
                                           .FirstOrDefault();

                object response = new { IsBusy = false };
                if (lastCase?.cas_cst_key == CaseStatus.WaitingToAccept.ToInt())
                {
                    var waitingToAccept = CaseStatus.WaitingToAccept.ToDescription().ToLower();
                    var history = _caseAssignHistoryService.GetAll().Where(m => m.cah_cas_key == lastCase.cas_key)
                                                                    .Where(m => m.cah_action.ToLower() == waitingToAccept)
                                                                    .OrderByDescending(m => m.cah_key)
                                                                    .FirstOrDefault();
                    if (history != null)
                    {
                        TimeSpan? timeSpan = (DateTime.Now.ToEST() - history.cah_created_date);
                        response = new { IsBusy = (timeSpan?.TotalMinutes > 2 ? false : true), WaitingToAcceptTime = timeSpan.FormatTimeSpan(), CaseNumber = lastCase.cas_case_number };
                    }

                }

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }
        public ActionResult _CasePopupAlertPhysician(int id)
        {
            try
            {
                var @case = _caseService.GetDetailsWithoutTimeConversion(id);
                ViewBag.CaseType = _uclService.GetDetails(@case.cas_ctp_key)?.ucd_title;
                return PartialView(@case);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }
        public ActionResult _CasePopupAlertNavigator(int id)
        {
            try
            {
                var @case = _caseService.GetDetailsWithoutTimeConversion(id);
                ViewBag.CaseType = _uclService.GetDetails(@case.cas_ctp_key)?.ucd_title;
                return PartialView(@case);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        public ActionResult _CasePopupAlertNavigatorForManualAssign(int id, PhysicianCaseAssignQueue action = PhysicianCaseAssignQueue.Rejected)
        {
            try
            {
                var @case = _caseService.GetDetailsWithoutTimeConversion(id);
                ViewBag.CaseType = _uclService.GetDetails(@case.cas_ctp_key)?.ucd_title;
                ViewBag.status = action;
                return PartialView(@case);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        public ActionResult AcceptCase(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var model = _caseService.GetDetailsWithoutTimeConversion(id);
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();

                PhysicianCasePopupHub.CleanCaseData(model.cas_key);

                var physicianReq = _caseAssignHistoryService.GetAll().Where(m => m.cah_phy_key == userId)
                                                                          .Where(m => m.cah_cas_key == id)
                                                                          .FirstOrDefault();

                #region updating the case assign history columns which should be modified in each case                  
                physicianReq.cah_action_time = DateTime.Now.ToEST();
                physicianReq.cah_action_time_utc = DateTime.UtcNow;
                physicianReq.cah_modified_by = User.Identity.GetUserId();
                physicianReq.cah_modified_date = DateTime.Now.ToEST();
                physicianReq.cah_action = PhysicianCaseAssignQueue.Expired.ToString();
                _caseAssignHistoryService.Edit(physicianReq);
                #endregion

                if (model.cas_phy_key == null)
                {
                    var IsNotExpired = (DateTime.Now.ToEST() - (physicianReq.cah_request_sent_time != null ? physicianReq.cah_request_sent_time.Value : DateTime.Now.ToEST().AddDays(-1))).TotalMinutes < 2;
                    if (IsNotExpired)
                    {
                        model.cas_phy_key = userId;
                        model.cas_modified_by = userId;
                        model.cas_modified_date = DateTime.Now.ToEST();
                        model.cas_cst_key = CaseStatus.Accepted.ToInt(); // can be changed to accepted, after the requirements are clear
                        model.cas_status_assign_date = DateTime.Now.ToEST();
                        model.cas_response_time_physician = DateTime.UtcNow;
                        model.cas_physician_assign_date = DateTime.Now.ToEST();
                        model.cas_history_physician_initial = _caseService.GetCaseInitials(model.cas_key);
                        _caseService.EditCaseOnly(model);

                        _caseService.UpdateTimeStamps(model.cas_key.ToString());
                        //#region updating the case assign history queue  
                        //physicianReq.cah_action = PhysicianCaseAssignQueue.Accepted.ToString();
                        //_caseAssignHistoryService.Edit(physicianReq);
                        //#endregion

                        #region updating physician status
                        var case_type = _uclService.GetDetails(model.cas_ctp_key);
                        var status = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == case_type.ucd_title.ToLower());

                        if (status != null)
                        {

                            SetStatus(status.phs_key, model.cas_key, userId, "Case Accepted by physician itself");
                        }

                        #endregion

                        #region updating the case edit page info, if opened by the navigator
                        var navigators = PhysicianCasePopupHub.ConnectedUsers
                                                              .Where(m => m.UserId == model.cas_created_by)
                                                              .ToList();
                        navigators.ForEach(navigator =>
                        {
                            hubContext.Clients.Client(navigator.ConnectionId).syncCaseInfo(model.cas_key, model.cas_phy_key, model.cas_cst_key, model.cas_response_time_physician);
                        });

                        #endregion

                        return Json(new { success = true, message = "Case #" + model.cas_case_number + " has been assigned to you successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Request Timeout" });
                    }

                }
                else
                {

                    return Json(new { success = false, showInfoPopup = true, message = "Case has been already assigned to another physician" });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        public ActionResult AcceptCaseWithNoQueue(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var model = _caseService.GetDetailsWithoutTimeConversion(id);
                var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();

                PhysicianCasePopupHub.CleanCaseData(model.cas_key);

                var physicianReq = _caseAssignHistoryService.GetAll().Where(m => m.cah_phy_key == userId)
                                                                          .Where(m => m.cah_cas_key == id)
                                                                          .OrderByDescending(m => m.cah_key)
                                                                          .FirstOrDefault();



                if (model.cas_phy_key != null && model.cas_cst_key == CaseStatus.WaitingToAccept.ToInt())
                {
                    var IsNotExpired = (DateTime.Now.ToEST() - (physicianReq.cah_request_sent_time != null ? physicianReq.cah_request_sent_time.Value : DateTime.Now.ToEST().AddDays(-1))).TotalMinutes < 2;
                    if (IsNotExpired)
                    {
                        model.cas_phy_key = userId;
                        model.cas_modified_by = userId;
                        model.cas_modified_date = DateTime.Now.ToEST();
                        model.cas_cst_key = CaseStatus.Accepted.ToInt(); // can be changed to accepted, after the requirements are clear
                        model.cas_status_assign_date = DateTime.Now.ToEST();
                        model.cas_response_time_physician = DateTime.UtcNow;
                        model.cas_physician_assign_date = DateTime.Now.ToEST();
                        model.cas_history_physician_initial = _caseService.GetCaseInitials(model.cas_key);
                        _caseService.EditCaseOnly(model);


                        #region updating the case assign history queue  
                        physicianReq.cah_action = PhysicianCaseAssignQueue.Accepted.ToString();
                        LogCaseAssignHistory(model.cas_key, model.cas_phy_key, CaseStatus.Accepted.ToDescription(), false);
                        #endregion

                        #region updating physician status
                        var case_type = _uclService.GetDetails(model.cas_ctp_key);
                        var status = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == case_type.ucd_title.ToLower());

                        if (status != null)
                        {

                            SetStatus(status.phs_key, model.cas_key, userId, "Case accepted by physician itself");
                        }

                        #endregion

                        _caseService.UpdateTimeStamps(model.cas_key.ToString());

                        ShowNavigatorAcceptCasePopup(model, physicianReq.cah_created_by);



                        return Json(new { success = true, message = "Case #" + model.cas_case_number + " has been assigned to you successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Request Timeout" });
                    }

                }
                else
                {

                    return Json(new { success = false, showInfoPopup = true, message = "Case has been already assigned to another physician" });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        public ActionResult RejectCaseWithNoQueue(int id, bool hasExpired = false)
        {
            try
            {
                var currentUserId = User.Identity.GetUserId();
                var currentUser = _caseAssignHistoryService.GetRequests(id, PhysicianCaseAssignQueue.WaitingForAction)
                                                           .Where(m => m.cah_phy_key == currentUserId)
                                                           .OrderByDescending(m => m.cah_key)
                                                .FirstOrDefault();


                if (currentUser != null)
                {
                    var model = _caseService.GetQueryable().Where(m => m.cas_key == id).FirstOrDefault();

                    if (model.cas_phy_key == currentUserId && model.cas_cst_key == CaseStatus.WaitingToAccept.ToInt())
                    {

                        model.cas_cst_key = CaseStatus.Open.ToInt();
                        model.cas_phy_key = null;
                        model.cas_modified_by = currentUserId;
                        model.cas_modified_date = DateTime.Now.ToEST();
                        model.cas_modified_by_name = loggedInUser.FullName;
                        _caseService.EditCaseOnly(model);


                        var currentQueueItem = _caseAssignHistoryService.GetDetails(currentUser.cah_key);
                        currentQueueItem.cah_action_time = DateTime.Now.ToEST();
                        currentQueueItem.cah_action_time_utc = DateTime.UtcNow;
                        var action = PhysicianCaseAssignQueue.Rejected;
                        if (hasExpired)
                            action = PhysicianCaseAssignQueue.Expired;

                        currentQueueItem.cah_action = action.ToString();



                        currentQueueItem.cah_modified_date = DateTime.Now.ToEST();
                        currentQueueItem.cah_modified_by = currentUserId;
                        _caseAssignHistoryService.Edit(currentQueueItem);



                        ShowNavigatorRejectCasePopup(model, currentQueueItem.cah_created_by, action);
                        _caseService.UpdateTimeStamps(model.cas_key.ToString());

                    }

                }

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }



        public ActionResult _CheckDuplicateCaseByFacility(Guid facilityId)
        {
            try
            {
                ViewBag.CaseTypes = _uclService.GetUclData(UclTypes.CaseType)
                                    .Select(m => new { m.ucd_key, m.ucd_title })
                                    .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                var casesList = _caseService.GetCaseListByFacility(facilityId, ApplicationSetting).ToList();

                return GetViewResult(casesList);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetViewResult(null);
            }
        }

        [AccessRoles(NotInRoles = "Facility Admin,Regional Medical Director")]
        public ActionResult Create()
        {
            try
            {
                var selectedFacility = Guid.Empty;
                ViewBag.SaveNumber = false;
                string five9CaseType = "";
                var caseObj = new @case();

                if (Request.QueryString["ani"] != null) // In case of Five9 call
                {
                    var number = Request.QueryString["ani"] != null ? Request.QueryString["ani"].ToString() : "";
                    var dnis = Request.QueryString["dnis"] != null ? Request.QueryString["dnis"].ToString() : "";
                    var cid = Request.QueryString["cid"] != null ? Request.QueryString["cid"].ToString() : "";
                    var timestamp = Request.QueryString["start_timestamp"] != null ? Request.QueryString["start_timestamp"].ToString() : "";
                    var campaign = Request.QueryString["Campaign"] != null ? Request.QueryString["Campaign"].ToString() : "";
                    var callid = Request.QueryString["call_id"] != null ? Request.QueryString["call_id"].ToString() : "";
                    five9CaseType = Request.QueryString["case_type"] != null ? Request.QueryString["case_type"].ToString() : "";

                    var facilities = _contactService.GetAllContactFacilities(number).ToList();
                    if (facilities != null && facilities.Count() == 1)
                        selectedFacility = facilities.FirstOrDefault().fac_key;

                    var five9StartDate = Functions.TimeStampToDateTime(Convert.ToDouble(timestamp));


                    caseObj = new @case { cas_ani = number, cas_dnis = dnis, cas_time_stamp = five9StartDate, cas_campaign_id = campaign, cas_call_id = callid, FacilityTimeZone = BLL.Settings.DefaultTimeZone, cas_five9_original_stamp_time = timestamp };

                    ViewBag.SaveNumber = facilities.Count() > 0 ? false : true;
                }

                if (TempData.ContainsKey("Message"))
                {
                    ViewBag.Message = TempData["Message"];
                }

                #region preloading Ucls types for all 
                var types = new List<int>()
                    {
                        UclTypes.CaseStatus.ToInt(),
                        UclTypes.CaseType.ToInt(),
                        UclTypes.CallerSource.ToInt(),
                        UclTypes.IdentificationType.ToInt(),
                        UclTypes.CartLocation.ToInt()
                    };

                var uclDataList = _lookUpService.GetUclData(types)
                                  .Where(m => m.ucd_is_active)
                                   .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                                  .ToList();
                #endregion

                #region Setting Default UCL Ids
                var defaultStatusUCL = GetDefaultTypeFromList(uclDataList, UclTypes.CaseStatus); //_uclService.GetDefault(UclTypes.CaseStatus);
                ucl_data defaultTypeUCL = null;

                if (!string.IsNullOrEmpty(five9CaseType))
                    defaultTypeUCL = _uclService.GetDetails(five9CaseType, UclTypes.CaseType);
                else
                {
                    defaultTypeUCL = GetDefaultTypeFromList(uclDataList, UclTypes.CaseType);//_uclService.GetDefault(UclTypes.CaseType);
                }

                caseObj.cas_ctp_key = defaultTypeUCL != null ? defaultTypeUCL.ucd_key : 0;
                caseObj.cas_cst_key = defaultStatusUCL != null ? defaultStatusUCL.ucd_key : 0;
                caseObj.cas_fac_key = selectedFacility;
                caseObj.cas_identification_type = GetDefaultTypeFromList(uclDataList, UclTypes.IdentificationType)?.ucd_key; //_uclService.GetDefault(UclTypes.IdentificationType)?.ucd_key;
                var caller_source = GetDefaultTypeFromList(uclDataList, UclTypes.CallerSource);
                caseObj.cas_caller_source_key = caller_source?.ucd_key;
                caseObj.cas_caller_source_key_title = caller_source?.ucd_title;

                #endregion

                ViewBag.UclData = uclDataList.Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).OrderBy(o => o.ucd_sort_order);

                #region Get CasCancelledType
                // ViewBag.CancelledType = _casCancelledTypeService.GetAll().Select(c => new { c.cct_key, c.cct_name });
                #endregion

                return GetViewResult(caseObj);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }

        }
        private ucl_data GetDefaultTypeFromList(List<ucl_data> list, UclTypes type)
        {
            return list.Where(m => m.ucd_ucl_key == ((int)type) && m.ucd_is_active && m.ucd_is_default).FirstOrDefault();
        }
        public void LogStatusChange(int psl_phs_key, string phy_key, int? cas_key, string comments)
        {
            try
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
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public ActionResult Edit(int id, bool isReadOnly = false)
        {
            isCalculateBill = false;

            ViewBag.IsReadOnlyCase = isReadOnly;
            if (User.IsInRole(UserRoles.Finance.ToDescription()))
            {
                ViewBag.IsReadOnlyCase = true;
            }
            var model = new @case();
            ViewBag.EnableAutoSave = ApplicationSetting.aps_enable_case_auto_save;
            ViewBag.ShowNotesPopup = ApplicationSetting.aps_cas_facility_popup_on_load;
            //  var viewModel = new CaseViewModel();
            //Added by Axim 29-10-2020
            string selected = "";
            List<string> roless = new List<string>();

            var QPS = UserRoles.QPS.ToDescription();
            var QualityDirector = UserRoles.QualityDirector.ToDescription();
            var VPQuality = UserRoles.VPQuality.ToDescription();

            var QPSId = RoleManager.Roles.Where(x => x.Description == QPS).Select(x => x.Id).FirstOrDefault();
            var QualityDirectorId = RoleManager.Roles.Where(x => x.Description == QualityDirector).Select(x => x.Id).FirstOrDefault();
            var VPQualityId = RoleManager.Roles.Where(x => x.Description == VPQuality).Select(x => x.Id).FirstOrDefault();

            roless.Add(QPSId);
            roless.Add(QualityDirectorId);
            roless.Add(VPQualityId);
            ViewBag.QPSList = _facilityService.GetUserByRole(roless, selected);

            //Ended By Axim 29-10-2020

            try
            {
                model = _caseService.GetDetails(id);
                if (model.facility != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.facility.qps_number))
                    {
                        var GetQPSName = _adminService.GetAspNetUsers().Where(m => m.Id == model.facility.qps_number).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                        if (GetQPSName != null)
                        {
                            ViewBag.QPSName = GetQPSName.FirstName + " " + GetQPSName.LastName;
                        }
                        else
                        {
                            ViewBag.QPSName = "";
                        }

                    }

                }
                else
                {
                    ViewBag.QPSName = "";
                }
                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    // Check case is assigned to user or not.
                    if (model.cas_phy_key != User.Identity.GetUserId())
                    {

                        //TCARE-487
                        //ViewBag.Title = "Access Denied";
                        //ViewBag.Message = "You are not authorized to open this case. This case is not assigned to you.";
                        //return GetViewResult("Error");
                        ViewBag.IsReadOnlyCase = true;
                        //TCARE-487

                    }
                }

                ViewBag.CaseModel = model; // binding the related data from the ViewBag so the lazy loading work after the post back.
                if (model.cas_key == 0)
                {
                    ViewBag.Error = true;
                    ViewBag.Message = "Case not found.";
                    return GetViewResult();
                }
                else
                {

                    var types = new List<int>()
                    {
                        UclTypes.ServiceType.ToInt(),
                        UclTypes.CoverageType.ToInt(),
                        UclTypes.CaseType.ToInt(),
                        UclTypes.IdentificationType.ToInt(),
                        UclTypes.TpaDelay.ToInt(),
                        UclTypes.NonTPACandidate.ToInt(),
                        UclTypes.LoginDelay.ToInt(),
                        UclTypes.BillingCode.ToInt(),
                        UclTypes.CallerSource.ToInt(),
                        UclTypes.CartLocation.ToInt()
                    };

                    var uclDataList = _lookUpService.GetUclData(types)
                                      .Where(m => m.ucd_is_active)
                                      .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).ToList();

                    if (model.cas_caller_source_key != null)
                    {
                        var caller_source = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.CallerSource.ToInt() && m.ucd_key == model.cas_caller_source_key).FirstOrDefault();
                        model.cas_caller_source_key_title = caller_source?.ucd_title;
                    }

                    /*TCARE - 472 */

                    if (model.cas_cart_location_key != null)
                    {
                        var cart_location = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.CartLocation.ToInt() && m.ucd_key == model.cas_cart_location_key).FirstOrDefault();
                        model.cas_cart_location_key_Title = cart_location?.ucd_title;
                    }

                    if (model.cas_identification_type != null)
                    {
                        var ucddata = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.IdentificationType.ToInt() && m.ucd_key == model.cas_identification_type).FirstOrDefault();
                        model.cas_identification_Title = ucddata?.ucd_title;
                    }

                    model.CaseInterval = new CaseInterval(model);

                    var _uclData = uclDataList;
                    var excluededList = new List<string>();
                    if (model.PhysicianUser != null)
                    {
                        if (!model.PhysicianUser.IsEEG)
                        {
                            excluededList.Add(CaseBillingCode.EEG.ToDescription());
                            excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                        }
                    }
                    var isTeleNeuroFacility = false;
                    if (model.facility != null)
                    {
                        if (model.facility.facility_contract != null)
                        {
                            if (model.facility.facility_contract.fct_service_calc != null
                                && model.facility.facility_contract.fct_service_calc != ""
                                && model.facility.facility_contract.fct_service_calc.Contains(ContractServiceTypes.TeleNeuro.ToString()))
                            {
                                isTeleNeuroFacility = true;
                            }
                        }
                    }

                    if (!isTeleNeuroFacility)
                    {
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.RoutineConsultNew.ToInt() || model.cas_ctp_key == CaseType.RoutineConsultFollowUp.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.CC1_StrokeAlert.ToDescription());
                        //excluededList.Add(CaseBillingCode.CC1_STAT.ToDescription());
                        //excluededList.Add(CaseBillingCode.New.ToDescription());
                        //excluededList.Add(CaseBillingCode.FU.ToDescription());
                        excluededList.Add(CaseBillingCode.EEG.ToDescription());
                        excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                        //excluededList.Add(CaseBillingCode.TC.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.StatConsult.ToInt() || model.cas_ctp_key == CaseType.TransferAlert.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.CC1_StrokeAlert.ToDescription());
                        excluededList.Add(CaseBillingCode.EEG.ToDescription());
                        excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());

                        excluededList.Add(CaseBillingCode.CC1_STAT.ToDescription());
                        excluededList.Add(CaseBillingCode.EEG.ToDescription());
                        excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.NursetoDr.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.CC1_StrokeAlert.ToDescription());
                        excluededList.Add(CaseBillingCode.CC1_STAT.ToDescription());
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());
                        excluededList.Add(CaseBillingCode.EEG.ToDescription());
                        excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                        excluededList.Add(CaseBillingCode.TC.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.DrtoDr.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.CC1_StrokeAlert.ToDescription());
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());
                        excluededList.Add(CaseBillingCode.EEG.ToDescription());
                        excluededList.Add(CaseBillingCode.LTMEEG.ToDescription());
                    }
                    if (model.cas_ctp_key == CaseType.LongTermEEG.ToInt() || model.cas_ctp_key == CaseType.RoutineEEG.ToInt() || model.cas_ctp_key == CaseType.StatEEG.ToInt())
                    {
                        excluededList.Add(CaseBillingCode.CC1_StrokeAlert.ToDescription());
                        excluededList.Add(CaseBillingCode.CC1_STAT.ToDescription());
                        excluededList.Add(CaseBillingCode.New.ToDescription());
                        excluededList.Add(CaseBillingCode.FU.ToDescription());
                        excluededList.Add(CaseBillingCode.TC.ToDescription());
                    }

                    if (excluededList.Count > 0)
                    {
                        _uclData = _uclData.Where(bc => !excluededList.Contains(bc.ucd_title)).ToList();
                    }
                    #region Code for remove EEG records by husnain
                    var billingCodes = _uclData.Where(x => x.ucd_ucl_key == 10).ToList();
                    if (model.cas_ctp_key != 13 && model.cas_ctp_key != 14 && model.cas_ctp_key != 15)
                    {
                        var listForRemove = billingCodes.Where(x => x.ucd_key == 5 || x.ucd_key == 6 || x.ucd_key == 324 || x.ucd_key == 325 || x.ucd_key == 326).ToList();
                        _uclData = _uclData.Except(listForRemove).ToList();
                    }
                    #endregion

                    ViewBag.UclData = _uclData;

                    // Sorting according to the ticket no. 439. 
                    // if there isnt a sort order set then it should go in alpha order yes but why would we give users the option to set a sort order if we are setting in alpha order? that doesnt make much sense ' From Darcy



                    ViewBag.CaseTypes = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.CaseType.ToInt())
                                                 .Select(m => new { m.ucd_key, m.ucd_title })
                                                 .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                    if (model.cas_identification_type.HasValue)
                    {
                        ViewBag.casIdentityTypeName = _uclService.GetUclData(UclTypes.IdentificationType).Where(c => c.ucd_key == model.cas_identification_type).FirstOrDefault().ucd_title;
                    }

                    if (model.cas_metric_non_tpa_reason_key.HasValue)
                    {
                        ViewBag.IsOther = _uclService.GetDetails(model.cas_metric_non_tpa_reason_key.Value).ucd_title.ToLower() == "other" ? true : false;
                    }

                    var templateTypes = new List<int>()
                    {
                        EntityTypes.StrokeAlertTemplateTpa.ToInt(),
                        EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt(),
                        EntityTypes.StrokeAlertTemplateNoTpa.ToInt(),
                        EntityTypes.StrokeAlertTemplateNoTpaTeleStroke.ToInt(),
                        EntityTypes.StateAlertTemplate.ToInt()
                    };

                    if (model.TemplateEntityType.HasValue && templateTypes.Contains(model.TemplateEntityType.Value))
                        SetTemplateData(model);
                    else
                        CreateTemplateData(model);

                    if (model.cas_billing_bic_key.HasValue)
                        ViewBag.bic_code = _uclService.GetDetails(model.cas_billing_bic_key.Value).ucd_title;

                    if (model.cas_metric_total_dose.HasValue)
                    {
                        model.cas_metric_total_dose = Math.Round(model.cas_metric_total_dose.Value, 1);
                    }

                    if (model.cas_metric_bolus.HasValue)
                    {
                        model.cas_metric_bolus = Math.Round(model.cas_metric_bolus.Value, 1);
                    }

                    if (model.cas_metric_infusion.HasValue)
                    {
                        model.cas_metric_infusion = Math.Round(model.cas_metric_infusion.Value, 1);
                    }

                    if (model.cas_metric_discard_quantity.HasValue)
                    {
                        model.cas_metric_discard_quantity = Math.Round(model.cas_metric_discard_quantity.Value, 1);
                    }

                    var entityId = model.facility.fac_key.ToString();
                    var entityType = EntityTypes.Facility;
                    var notes = _entityNotesService.GetEnityNotes(entityId, entityType).ToList();
                    ViewBag.FacilityNotes = notes;
                    model.FacilityTimeZone = model.facility.fac_timezone;
                }

                model.FromWaitingToAcceptToAcceptTime = CalculateWaitingToAcceptTime(model);

                ViewBag.CaseReviewers = _adminService.GetAspNetUsers().Where(x => x.CaseReviewer == true).ToList();
                ViewBag.CaseStatus = model.cas_cst_key;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            if (model.cas_billing_bic_key_initial != null)
            {
                var record = _uclService.GetDetails((int)model.cas_billing_bic_key_initial);
                ViewBag.revised = record.ucd_title;
            }
            return GetViewResult(model);
        }

        private string CalculateWaitingToAcceptTime(@case model)
        {
            try
            {
                var waitingTime = string.Empty;
                //if (model.cas_cst_key == CaseStatus.Accepted.ToInt())
                //{
                string waitingToAcceptStatus = CaseStatus.WaitingToAccept.ToDescription().ToLower();
                string acceptedStatus = CaseStatus.Accepted.ToDescription().ToLower();
                var waitingToAcceptTime = model.case_assign_history.Where(m => m.cah_action.ToLower().Trim() == waitingToAcceptStatus).OrderByDescending(m => m.cah_key).FirstOrDefault();
                var acceptedTime = model.case_assign_history.Where(m => m.cah_action.ToLower().Trim() == acceptedStatus).OrderByDescending(m => m.cah_key).FirstOrDefault();
                if (waitingToAcceptTime != null && acceptedTime != null)
                {
                    TimeSpan? d = (acceptedTime.cah_created_date - waitingToAcceptTime.cah_created_date);
                    waitingTime = d.FormatTimeSpan();
                }
                //}

                if (string.IsNullOrEmpty(waitingTime))
                {
                    waitingTime = "00:00:00";
                }

                return waitingTime;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public ActionResult _caseAssignmentHistory(int id)
        {
            try
            {
                var accepted = PhysicianCaseAssignQueue.Accepted.ToString();
                var rejected = PhysicianCaseAssignQueue.Rejected.ToString();

                var model = _caseAssignHistoryService.GetAll()
                    .Where(m => m.cah_is_active)
                    .Where(m => m.cah_cas_key == id)
                    .Where(m => m.cah_is_manuall_assign || m.cah_action == accepted || m.cah_action == rejected)
                    .OrderByDescending(x => x.cah_created_date).ThenBy(x => x.cah_key)
                    .ToList();
                return GetViewResult(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(null);
            }

        }
        public ActionResult DisplayNotes(int id)
        {
            try
            {
                var casedetail = _caseService.GetDetailsWithoutTimeConversion(id);
                var physicianNotes = _entityNotesService.GetEnityNotes(casedetail.cas_phy_key, EntityTypes.User).Where(x => x.etn_display_on_open).ToList();
                var facilityNotes = _entityNotesService.GetEnityNotes(casedetail.cas_fac_key.ToString(), EntityTypes.Facility).Where(x => x.etn_display_on_open).ToList();
                physicianNotes.AddRange(facilityNotes);
                physicianNotes.OrderByDescending(x => x.etn_created_date);
                if (physicianNotes.Count() > 0)
                    return GetViewResult(physicianNotes);
                else
                    return new EmptyResult();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(null);
            }

        }
        public JsonResult Check15MinuteRule(Guid physicianKey, int cas_key = 0)
        {
            try
            {
                var timeNotPassed = _caseAssignHistoryService.Check15MinuteRule(physicianKey.ToString(), cas_key);
                return Json(new { success = timeNotPassed }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFacilityTimeZone(Guid id, string inputDateTime)
        {
            try
            {
                var facility = _facilityService.GetDetails(id);
                var timeZone = string.IsNullOrEmpty(facility.fac_timezone) ? BLL.Settings.DefaultTimeZone : facility.fac_timezone;
                var facilityCurrentTime = DateTime.UtcNow.ToTimezoneFromUtc(timeZone);
                var abbrivation = Functions.GetTimeZoneAbbreviation(timeZone);

                if (inputDateTime != "")
                {
                    var convertedTime = inputDateTime.ToDateTime().Value.ToTimezoneFromUtc(timeZone);
                    return Json(new { success = true, timeZoneOffset = Functions.GetTimeZoneOffset(timeZone), timeZone = timeZone, convertedTime = convertedTime.FormatDateTime(), abbrivation = abbrivation }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, timeZoneOffset = Functions.GetTimeZoneOffset(timeZone), timeZone = timeZone, abbrivation = abbrivation }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AccessRoles(NotInRoles = "Facility Admin,Regional Medical Director")]
        public ActionResult Create(@case model)
        {
            try
            {
                var isRedirect = Request.Params["RedirectPage"] == "0" ? false : true;
                var isSaveAndSend = Request.Params["hdnSaveAndSend"] == "1" ? true : false;
                var showPhyOfflinePopup = "0";

                if (model.cas_cst_key != CaseStatus.Cancelled.ToInt())
                {
                    if (isRedirect && model.cas_time_stamp.HasValue == false && (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatConsult.ToInt()))
                    {
                        if (string.IsNullOrEmpty(model.cas_cart))
                            ModelState.AddModelError("cas_cart", "The Cart field is required.");
                    }
                    if (isRedirect && model.cas_time_stamp.HasValue == false)
                    {
                        if (string.IsNullOrEmpty(model.cas_callback)
                            && !(model.cas_ctp_key == CaseType.RoundingNew.ToInt()
                                || model.cas_ctp_key == CaseType.RoundingFollowUp.ToInt()
                                || model.cas_ctp_key == CaseType.RoutineConsult.ToInt()
                                || model.cas_ctp_key == CaseType.RoutineConsultNew.ToInt()
                                || model.cas_ctp_key == CaseType.RoutineConsultFollowUp.ToInt()
                                 || model.cas_ctp_key == CaseType.RoutineEEG.ToInt()
                                  || model.cas_ctp_key == CaseType.StatEEG.ToInt()
                                   || model.cas_ctp_key == CaseType.LongTermEEG.ToInt()
                                ))
                            ModelState.AddModelError("cas_callback", "The Callback field is required.");
                    }
                }

                if (ModelState.IsValid)
                {
                    model = PrepareNewCaseData(model);
                    _caseService.Create(model);
                    var guid_key = Request.Params["cas_pct_key"];
                    if (!string.IsNullOrEmpty(guid_key))
                    {
                        _physicianCaseTempService.DeleteById(new Guid(guid_key));
                    }




                    if (model.cas_phy_key == null && ApplicationSetting.aps_enable_auto_assign_process)
                        InitPhyscianPopupAlert(model);

                    if (model.cas_cst_key == CaseStatus.Accepted.ToInt() && !string.IsNullOrEmpty(model.cas_phy_key))
                    {
                        #region updating physician status
                        var case_type = _uclService.GetDetails(model.cas_ctp_key);
                        var status = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == case_type.ucd_title.ToLower());
                        if (status != null)
                        {
                            SetStatus(status.phs_key, model.cas_key, model.cas_phy_key, "Case Assigned to Physician through Case Create Page");
                        }
                        #endregion
                    }

                    #region showing the time in popup in facility time zone

                    string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                    var facility = _facilityService.GetDetails(model.cas_fac_key);

                    if (!string.IsNullOrEmpty(facility?.fac_timezone))
                    {
                        facilityTimeZone = facility.fac_timezone;
                    }

                    var abbrivation = Functions.GetTimeZoneAbbreviation(facilityTimeZone);
                    var displaytime = model.cas_metric_stamp_time?.ToTimezoneFromUtc(facilityTimeZone).FormatDateTime();
                    model.cas_metric_stamp_time_formated = displaytime + " " + abbrivation;

                    #endregion

                    #region handling logging in case of physician updated
                    if (model.cas_cst_key > 0)
                    {
                        LogCaseAssignHistory(model.cas_key, model.cas_phy_key, _uclService.GetDetails(model.cas_cst_key)?.ucd_title, true);
                        _caseService.UpdateCaseInitials(model.cas_key, _caseService.GetCaseInitials(model.cas_key));
                    }
                    #endregion

                    TempData["Message"] = "Case has been added.";
                    _caseService.UpdateTimeStamps(model.cas_key.ToString());

                    var isAutoSave = Request.Params["IsAutoSave"] == "0" ? false : true;
                    if (isAutoSave)
                    {

                        return ShowSuccessMessageOnly("Case has been added.", model);
                    }
                    else
                    {
                        if (isSaveAndSend)
                        {

                            showPhyOfflinePopup = IsUserOnline(model.cas_phy_key) ? "0" : "1";
                            if (showPhyOfflinePopup == "0")
                            {
                                SendCaseToPhysician(model);
                            }
                        }

                        if (isRedirect)
                            return GetSuccessResult();
                        else
                        {
                            return GetSuccessResult(Url.Action("Edit", new { Id = model.cas_key, showPopupOnLoad = (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()) ? false : true), showPhyOfflinePopup = showPhyOfflinePopup, isInitialSave = true })); /* commented due to #411 - settings.aps_cas_facility_popup_on_load */
                        }
                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Error! Please try again.");
                    return GetErrorResult(model);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
                return GetErrorResult(model);
            }
        }
        #region Changes for tele

        public JsonResult DeleteRCARootCause(int id)
        {
            var GetRecord = _rootCauseService.GetDetailById(id);
            if (GetRecord != null)
            {
                _rootCauseService.DeleteRootCause(id);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRootRecord(int id)
        {
            var GetRecord = _rootCauseService.GetDetail(id);

            return Json(GetRecord.Select(x => new
            {
                x.rca_rootcause_id,
                x.rca_root_cause,
                x.rca_proposed_countermeasure,
                x.rca_responsible_party,
                x.rca_proposed_due_date,
                x.rca_completed_date,
                x.rca_Id
            }), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFacilityCart(Guid key)
        {
            var GetRecord = _caseService.GetCart(key);
            return Json(GetRecord, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetUserByRoleId()
        {
            try
            {
                List<string> roles = new List<string>();
                var RRCDirector = UserRoles.RRCDirector.ToDescription();
                var RRCManager = UserRoles.RRCManager.ToDescription();
                var RRCDirectorId = RoleManager.Roles.Where(x => x.Description == RRCDirector).Select(x => x.Id).FirstOrDefault();
                var RRCManagerId = RoleManager.Roles.Where(x => x.Description == RRCManager).Select(x => x.Id).FirstOrDefault();
                roles.Add(RRCDirectorId);
                roles.Add(RRCManagerId);

                var Physicians = _lookUpService.GetUserByRole(roles).Prepend(new SelectListItem()
                {
                    Text = "Not Available",
                    Value = "notavailable"
                }).ToList();                
                return Json(Physicians, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AccessRoles(NotInRoles = "Facility Admin")]
        [ValidateInput(false)]
        public ActionResult Edit(@case model, string[] rootcausevalue, string[] propcountermsureval, string[] responsibleprtyval, string[] propduedateval, String[] copddateval, string[] idsrootvalue, string[] cas_datetime_of_contact, string[] cas_contact_comments)
        {
            try
            {
                var redirectToEdit = false;
                if (Request.Params["isReadOnly"] != null)
                {
                    redirectToEdit = Request.Params["isReadOnly"] == "true" ? true : false;
                }
                var GetRecord = _rootCauseService.GetDetail(model.cas_key);
                var count = GetRecord.Count();
                if (GetRecord.Count() != 0)
                {
                    if (rootcausevalue != null || propcountermsureval != null || responsibleprtyval != null || propduedateval != null || copddateval != null)
                    {
                        List<rca_counter_measure> Newlist = new List<rca_counter_measure>();
                        List<rca_counter_measure> Oldlist = new List<rca_counter_measure>();
                        for (int i = 0; i < rootcausevalue.Length; i++)
                        {
                            count--;
                            rca_counter_measure counter_Measure = new rca_counter_measure();
                            counter_Measure.rca_key_id = model.cas_key;
                            counter_Measure.rca_rootcause_id = idsrootvalue[i].ToInt();
                            counter_Measure.rca_proposed_countermeasure = propcountermsureval[i];
                            counter_Measure.rca_root_cause = rootcausevalue[i];
                            counter_Measure.rca_responsible_party = responsibleprtyval[i];
                            DateTime dDate;
                            if (DateTime.TryParse(propduedateval[i], out dDate))
                            {
                                counter_Measure.rca_proposed_due_date = Convert.ToDateTime(propduedateval[i]);
                            }

                            if (DateTime.TryParse(copddateval[i], out dDate))
                            {
                                counter_Measure.rca_completed_date = Convert.ToDateTime(copddateval[i]);
                            }

                            if (count < 0)
                            {
                                Newlist.Add(counter_Measure);
                            }
                            else
                            {
                                Oldlist.Add(counter_Measure);
                            }

                        }
                        _rootCauseService.CreateRange(Newlist);
                        for (int i = 0; i < Oldlist.Count(); i++)
                        {
                            for (int j = i; j == i; j++)
                            {
                                GetRecord[j].rca_rootcause_id = Oldlist[i].rca_rootcause_id;
                                GetRecord[j].rca_root_cause = Oldlist[i].rca_root_cause;
                                GetRecord[j].rca_proposed_countermeasure = Oldlist[i].rca_proposed_countermeasure;
                                GetRecord[j].rca_responsible_party = Oldlist[i].rca_responsible_party;
                                GetRecord[j].rca_proposed_due_date = Oldlist[i].rca_proposed_due_date;
                                GetRecord[j].rca_completed_date = Oldlist[i].rca_completed_date;
                                _rootCauseService.Edit(GetRecord[j]);
                            }
                        }
                    }
                }
                else
                {
                    if (rootcausevalue != null || propcountermsureval != null || responsibleprtyval != null || propduedateval != null || copddateval != null)
                    {
                        for (int i = 0; i < rootcausevalue.Length; i++)
                        {
                            rca_counter_measure counter_Measure = new rca_counter_measure();
                            counter_Measure.rca_key_id = model.cas_key;
                            counter_Measure.rca_rootcause_id = idsrootvalue[i].ToInt();
                            counter_Measure.rca_proposed_countermeasure = propcountermsureval[i];
                            counter_Measure.rca_root_cause = rootcausevalue[i];
                            counter_Measure.rca_responsible_party = responsibleprtyval[i];
                            DateTime dDate;
                            if (DateTime.TryParse(propduedateval[i], out dDate))
                            {
                                counter_Measure.rca_proposed_due_date = Convert.ToDateTime(propduedateval[i]);
                            }

                            if (DateTime.TryParse(copddateval[i], out dDate))
                            {
                                counter_Measure.rca_completed_date = Convert.ToDateTime(copddateval[i]);
                            }

                            _rootCauseService.Create(counter_Measure);
                        }
                    }
                }
                if (cas_datetime_of_contact != null && cas_datetime_of_contact.Length > 0 || cas_contact_comments != null && cas_contact_comments.Length > 0)
                {
                    string cas_comments = "";
                    string cas_datetime_contact = "";
                    for (var i = 0; i < cas_datetime_of_contact.Length; i++)
                    {
                        cas_comments += cas_contact_comments[i] + "{/!/}";
                        cas_datetime_contact += cas_datetime_of_contact[i] + "{/!/}";
                    }
                    cas_comments = cas_comments.TrimEnd(' ', '{', '/', '!', '/', '}');
                    cas_datetime_contact = cas_datetime_contact.TrimEnd(' ', '{', '/', '!', '/', '}');
                    //    '/', '!', '/'
                    model.cas_typeof_correspondence = model.cas_typeof_correspondence.TrimEnd(',');
                    model.cas_datetime_of_contact = cas_datetime_contact;
                    model.cas_contact_comments = cas_comments;
                }
                //@case followUpCase = null;
                bool isFollowUpCase = false;
                var isSaveAndSend = Request.Params["hdnSaveAndSend"] == "1" ? true : false;
                var showPhyOfflinePopup = "0";

                ViewBag.EnableAutoSave = ApplicationSetting.aps_enable_case_auto_save;
                var showSuccessMessageOnly = Request.Params["ShowSuccessOnly"] == "1";
                var isRedirect = Request.Params["RedirectPage"] == "0" ? false : true;
                if (model.cas_cst_key != CaseStatus.Cancelled.ToInt())
                {
                    if (model.cas_billing_dob.HasValue)
                    {
                        if (model.cas_billing_dob.Value.Year <= _min_dob_year)
                        {
                            ModelState.AddModelError("cas_billing_dob", "The Date of Birth is not valid.");
                        }
                    }
                    if (User.IsInRole(UserRoles.QualityTeam.ToDescription()))
                    {
                        if (model.cas_response_date_consult == null)
                            ModelState.AddModelError("cas_response_date_consult", "The Date of Consult field on Metric Response Tab is required");

                        #region TCARE-547
                        // cas_response_phy_key field has no reference on client side, ie in any cshtml file. and it is always returning null.
                        // So disabling it.
                        //if (string.IsNullOrEmpty(model.cas_response_phy_key))
                        //    ModelState.AddModelError("cas_response_phy_key", "The Physician field on Metric Response Tab is required");
                        #endregion
                    }
                }

                if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
                {
                    var physician = _physician.GetDetail(model.cas_phy_key);

                    if (!physician.IsStrokeAlert)
                    {
                        ModelState.AddModelError("cas_ctp_key", "This physician cannot take stroke alerts.");
                    }
                }


                var dbModel = _caseService.GetDetails(model.cas_key);

                model = MapRoleBaseData(model, dbModel);

                var signalRPostModel = GetSignalRPostData(model);

                #region Check authorized user to save case
                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    if (dbModel.cas_phy_key != User.Identity.GetUserId())
                    {
                        ModelState.AddModelError("", "Access Denied! <br/>");
                        ModelState.AddModelError("", "You are not authorized to save this case. This case is reassigned.");
                        return GetErrorResult(model);
                    }
                }
                #endregion

                // As there is not complete option for users other then admins in case status, So checking it through non-persisted propperty "IsCaseCompleted"
                if (model.IsCaseCompleted && (!User.IsInRole(UserRoles.Administrator.ToDescription()) && !User.IsInRole(UserRoles.SuperAdmin.ToDescription())))
                {
                    model.cas_cst_key = CaseStatus.Complete.ToInt();
                }

                if (ModelState.IsValid)
                {

                    if (model.cas_is_ealert && model.cas_is_flagged && model.cas_cst_key == CaseStatus.Accepted.ToInt() && dbModel.cas_cst_key != model.cas_cst_key)
                    {
                        model.cas_is_flagged = false;
                    }

                    else if (model.cas_is_flagged && model.cas_cst_key == CaseStatus.Cancelled.ToInt() && dbModel.cas_cst_key != model.cas_cst_key)
                    {
                        model.cas_is_flagged = false;
                    }

                    model.cas_callback = Functions.ClearPhoneFormat(model.cas_callback);
                    model.cas_modified_by = loggedInUser.Id;
                    model.cas_modified_by_name = loggedInUser.FullName;
                    model.cas_modified_date = DateTime.Now.ToEST();
                    //if (model.cas_billing_bic_key == null)
                    //    model.cas_billing_bic_key = model.cas_billing_bic_key_initial;
                    #region TCARE-484 Advance Imaging Checkboxes

                    if (model.cas_metric_thrombectomy_medical_decision_making == null ||
                        model.cas_metric_thrombectomy_medical_decision_making ==
                        ThrombectomyMedicalDecisionMaking.ClinicalPresentationIsNotSuggestiveOfLargeVesselOcclusiveDisease_PatientIsNotACandidateForThrombectomy.ToInt())
                    {
                        model.cas_metric_advance_imaging_cta_head_and_neck =
                        model.cas_metric_advance_imaging_ct_perfusion =
                        model.cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir =
                        model.cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion =
                        model.cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus = false;
                    }

                    #endregion 

                    #region follow-up field
                    if (model.cas_billing_visit_type != null)
                    {
                        if (model.cas_billing_visit_type.Equals(FollowUpTypes.FollowUp.ToDescription()))
                        {
                            var isAlreadyFollowUp = _caseService.GetQueryable().Where(x => x.cas_followup_case_key == model.cas_key).Any();
                            isFollowUpCase = isAlreadyFollowUp ? false : model.cas_cst_key == CaseStatus.Complete.ToInt();
                        }
                        else
                            model.cas_follow_up_date = null;
                    }
                    #endregion

                    #region ---- handling consult end time field ----
                    if (model.cas_metric_video_end_time_est != null && model.cas_metric_documentation_end_time_est != null)
                        model.cas_metric_consult_endtime_est = model.cas_metric_video_end_time_est > model.cas_metric_documentation_end_time_est ? model.cas_metric_video_end_time_est : model.cas_metric_documentation_end_time_est;
                    else if (model.cas_metric_video_end_time_est != null)
                        model.cas_metric_consult_endtime_est = model.cas_metric_video_end_time_est;
                    else if (model.cas_metric_documentation_end_time_est != null)
                        model.cas_metric_consult_endtime_est = model.cas_metric_documentation_end_time_est;
                    #endregion

                    #region updating the case metric time if facility updated
                    string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                    if (!string.IsNullOrEmpty(model.FacilityTimeZone))
                    {
                        facilityTimeZone = model.FacilityTimeZone;
                    }
                    #endregion

                    #region ----- Converting all non eastern fields to facility time zone - TCARE-187 -----
                    //Do not remove this commented code until Adnan review it and remove it by him self.
                    //if (model.cas_response_ts_notification.HasValue && dbModel.cas_response_ts_notification.HasValue == false)
                    //{
                    //    model.cas_response_ts_notification = model.cas_response_ts_notification?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    //}
                    //else
                    //{
                    //    model.cas_response_ts_notification = dbModel.cas_response_ts_notification_utc;
                    //} 


                    if (model.cas_response_ts_notification.HasValue)
                    {
                        model.cas_response_ts_notification = model.cas_response_ts_notification.Value.ToUniversalTimeZone(facilityTimeZone);
                    }

                    if (model.cas_metric_lastwell_date_est.HasValue)
                    {
                        model.cas_metric_lastwell_date = model.cas_metric_lastwell_date_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_lastwell_date_est = model.cas_metric_lastwell_date?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_door_time_est.HasValue)
                    {
                        model.cas_metric_door_time = model.cas_metric_door_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_door_time_est = model.cas_metric_door_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    else
                        model.cas_metric_door_time = null;

                    if (model.cas_metric_symptom_onset_during_ed_stay_time_est.HasValue)
                    {
                        model.cas_metric_symptom_onset_during_ed_stay_time = model.cas_metric_symptom_onset_during_ed_stay_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_symptom_onset_during_ed_stay_time_est = model.cas_metric_symptom_onset_during_ed_stay_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }


                    if (model.cas_metric_assesment_time_est.HasValue)
                    {
                        model.cas_metric_assesment_time = model.cas_metric_assesment_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_assesment_time_est = model.cas_metric_assesment_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_pa_ordertime_est.HasValue)
                    {
                        model.cas_metric_pa_ordertime = model.cas_metric_pa_ordertime_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_pa_ordertime_est = model.cas_metric_pa_ordertime?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_needle_time_est.HasValue)
                    {
                        model.cas_metric_needle_time = model.cas_metric_needle_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_needle_time_est = model.cas_metric_needle_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_consult_endtime_est.HasValue)
                    {
                        model.cas_metric_consult_endtime = model.cas_metric_consult_endtime_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_consult_endtime_est = model.cas_metric_consult_endtime?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_documentation_end_time_est.HasValue)
                    {
                        model.cas_metric_documentation_end_time = model.cas_metric_documentation_end_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_documentation_end_time_est = model.cas_metric_documentation_end_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_video_end_time_est.HasValue)
                    {
                        model.cas_metric_video_end_time = model.cas_metric_video_end_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_video_end_time_est = model.cas_metric_video_end_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_video_start_time_est.HasValue)
                    {
                        model.cas_metric_video_start_time = model.cas_metric_video_start_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_video_start_time_est = model.cas_metric_video_start_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_metric_tpa_verbal_order_time_est.HasValue)
                    {
                        model.cas_metric_tpa_verbal_order_time = model.cas_metric_tpa_verbal_order_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_tpa_verbal_order_time_est = model.cas_metric_tpa_verbal_order_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (model.cas_response_first_atempt.HasValue)
                    {
                        model.cas_response_first_atempt = model.cas_response_first_atempt?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    }
                    if (model.cas_response_time_physician.HasValue)
                    {
                        model.cas_response_time_physician = model.cas_response_time_physician?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    }
                    if (model.cas_metric_stamp_time_est.HasValue)
                    {
                        model.cas_metric_stamp_time = model.cas_metric_stamp_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_metric_stamp_time_est = model.cas_metric_stamp_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }

                    if (model.cas_phy_technical_issue_date_est.HasValue)
                    {
                        model.cas_phy_technical_issue_date = model.cas_phy_technical_issue_date_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_phy_technical_issue_date_est = model.cas_phy_technical_issue_date?.ToTimezoneFromUtc("Eastern Standard Time");
                    }

                    if (model.cas_callback_response_time_est.HasValue)
                    {
                        model.cas_callback_response_time = model.cas_callback_response_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        model.cas_callback_response_time_est = model.cas_callback_response_time?.ToTimezoneFromUtc("Eastern Standard Time");

                    }

                    if (model.cas_cst_key == CaseStatus.Accepted.ToInt())
                    {
                        if (!model.cas_response_time_physician.HasValue)
                            model.cas_response_time_physician = DateTime.UtcNow;
                    }
                    #endregion

                    if (model.cas_cst_key != dbModel.cas_cst_key)
                    {
                        model.cas_status_assign_date = DateTime.Now.ToEST();
                    }

                    #region handling case template nihss changes
                    if (!string.IsNullOrEmpty(model.SelectedNIHSQuestionResponse))
                    {
                        //if (model.case_template_stroke_neuro_tpa != null && !model.case_template_stroke_neuro_tpa.csn_ignore_nihss)
                        if (model.case_template_stroke_neuro_tpa != null && !model.cas_nihss_cannot_completed)
                            SavenihssData(model.cas_key, model.SelectedNIHSQuestionResponse);
                        else if (model.case_template_stroke_notpa != null && !model.cas_nihss_cannot_completed)
                            //else if (model.case_template_stroke_notpa != null && !model.case_template_stroke_notpa.ctn_ignore_nihss)
                            SavenihssData(model.cas_key, model.SelectedNIHSQuestionResponse);
                        //else if (model.case_template_stroke_tpa != null && !model.case_template_stroke_tpa.cts_ignore_nihss)
                        else if (model.case_template_stroke_tpa != null && !model.cas_nihss_cannot_completed)
                            SavenihssData(model.cas_key, model.SelectedNIHSQuestionResponse);
                        //else if (model.case_template_telestroke_notpa != null && !model.case_template_telestroke_notpa.ctt_ignore_nihss)
                        else if (model.case_template_telestroke_notpa != null && !model.cas_nihss_cannot_completed)
                            SavenihssData(model.cas_key, model.SelectedNIHSQuestionResponse);
                        else if (model.case_template_statconsult != null && !model.cas_nihss_cannot_completed)
                            SavenihssData(model.cas_key, model.SelectedNIHSQuestionResponse);
                    }
                    #endregion

                    if (model.cas_phy_key != dbModel.cas_phy_key && model.cas_phy_key != null)
                        model.cas_physician_assign_date = DateTime.Now.ToEST();
                    else if (model.cas_phy_key == null)
                        model.cas_physician_assign_date = null;

                    #region TCARE-540
                    if (model.cas_phy_key != null && model.cas_fac_key != null)
                    {
                        //If physician is changed or facility is changed we need to grab the notes.
                        if (model.cas_fac_key != dbModel.cas_fac_key || model.cas_phy_key != dbModel.cas_phy_key)
                        {
                            model.cas_notes = GetNotes(model.cas_phy_key, model.cas_fac_key.ToString(), null);
                        }
                    }
                    #endregion
                    #region Commented Code Temporary to exclude 335 changes from production deployment

                    //if (isFollowUpCase)
                    //{
                    //    followUpCase = new @case()
                    //    {
                    //        cas_followup_case_key = model.cas_key,
                    //        cas_ctp_key = (int)CaseType.RoundingFollowUp,
                    //        cas_fac_key = model.cas_fac_key,
                    //        cas_callback = model.cas_callback,
                    //        cas_callback_extension = model.cas_callback_extension,
                    //        cas_patient = model.cas_patient,
                    //        cas_billing_dob = model.cas_billing_dob,
                    //        cas_caller = model.cas_caller,
                    //        cas_metric_door_time = model.cas_metric_door_time,
                    //        cas_metric_door_time_est = model.cas_metric_door_time_est,
                    //        cas_identification_type = model.cas_identification_type,
                    //        cas_identification_number = model.cas_identification_number,
                    //        cas_last_4_of_ssn = model.cas_last_4_of_ssn,
                    //        cas_referring_physician = model.cas_referring_physician,
                    //        cas_notes = model.cas_notes,
                    //        cas_eta = model.cas_eta,
                    //        cas_pulled_from_rounding = model.cas_pulled_from_rounding,
                    //        cas_is_nav_blast = model.cas_is_nav_blast,
                    //        cas_cst_key = (int)CaseStatus.Open
                    //    };
                    //    followUpCase = PrepareNewCaseData(followUpCase);
                    //    _caseService.HandleFollowUpCase(model, followUpCase);
                    //}
                    //else
                    #endregion

                    #region handling logging in case of physician updated

                    // handling status update time of physician
                    HandleCaseStatusCode(model, dbModel);

                    if (model.cas_cst_key > 0)
                    {
                        if ((model.cas_cst_key != dbModel.cas_cst_key || model.cas_phy_key != dbModel.cas_phy_key))
                        {
                            LogCaseAssignHistory(model.cas_key, model.cas_phy_key, _uclService.GetDetails(model.cas_cst_key)?.ucd_title, true);
                            model.cas_history_physician_initial = _caseService.GetCaseInitials(model.cas_key);
                        }
                    }

                    #endregion

                    model.cas_is_partial_update = false;
                    if (model.cas_ctp_key == 10)
                        model.TemplateEntityType = 11;
                    else
                    {
                        if (model.TemplateEntityType == 11)
                        {
                            model.TemplateEntityType = 0;
                        }
                    }
                    if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                    {
                        if (model.cas_cst_key == 20)
                            isCalculateBill = true;
                        else
                            isCalculateBill = false;
                    }
                    _caseService.Edit(model, false);
                    LogCaseChanges(model.cas_key.ToString());
                    _caseService.Save();
                    _caseService.Commit();
                    _caseService.UpdateTimeStamps(model.cas_key.ToString());

                    SyncCaseDataFromAdmin(signalRPostModel);


                    if (showSuccessMessageOnly)
                    {
                        return ShowSuccessMessageOnly("Case has been updated successfully", null);
                    }
                    else
                    {
                        if (isRedirect && redirectToEdit)
                        {
                            if (User.IsInRole(UserRoles.Finance.ToDescription()))
                            {
                                return GetSuccessResult(Url.Action("Index"));
                            }
                            else
                            {
                                return GetSuccessResult(Url.Action("DashBoard"));
                            }
                        }
                        if (isRedirect)
                            return GetSuccessResult();
                        else if (redirectToEdit)
                        {
                            return GetSuccessResult(Url.Action("Edit", new { id = model.cas_key, isReadOnly = true }), "Case has been updated successfully");
                        }
                        else
                        {
                            if (isSaveAndSend)
                            {
                                showPhyOfflinePopup = IsUserOnline(model.cas_phy_key) ? "0" : "1";
                                if (showPhyOfflinePopup == "0")
                                {
                                    SendCaseToPhysician(model);
                                }
                            }

                            return GetSuccessResult(
                                Url.Action(
                                  "Edit",
                                   new
                                   {
                                       id = model.cas_key,
                                       //TCARE-482
                                       //showPopupOnLoad = User.IsInRole(UserRoles.FacilityAdmin.ToDescription())
                                       //                    ? false
                                       //                    : settings.aps_cas_facility_popup_on_load,
                                       showPhyOfflinePopup = showPhyOfflinePopup
                                   }
                                ), "Case successfully updated"
                            );
                        }
                    }
                }
                else
                {
                    ViewBag.CaseModel = dbModel;  // using view bag for loading related entities. 
                    return GetErrorResult(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.CaseModel = _caseService.GetDetails(model.cas_key);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
                return GetErrorResult(model);
            }
        }

        public ActionResult checkIfCaseUpdatedBefore(int cas_key, int cas_cst_key, DateTime? cas_modified_date, string VisibleTabs)
        {
            try
            {
                bool result = false;
                bool tabMatched = false;


                var dbModel = _caseService.GetDetailsWithoutTimeConversion(cas_key);
                if (dbModel != null)
                {
                    var modifiedBy = User.Identity.GetUserId();
                    if (!string.IsNullOrEmpty(dbModel.cas_modified_by))
                    {
                        if (modifiedBy != dbModel.cas_modified_by && (!dbModel.cas_is_partial_update || dbModel.cas_cst_key != cas_cst_key))
                        {
                            var userTabs = GetVisibleTabsForUser(dbModel.cas_modified_by, dbModel);
                            userTabs.ForEach(m =>
                            {
                                if (VisibleTabs.Contains(m))
                                {
                                    tabMatched = true;
                                    return;
                                }
                            });
                        }

                        if (tabMatched)
                        {
                            var timeSpan = dbModel.cas_modified_date - cas_modified_date;
                            if (timeSpan?.TotalSeconds > 1)
                            {
                                result = true;
                            }
                        }
                    }
                }

                return Json(new { AlreadyUpdated = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        private void HandleCaseStatusCode(@case model, @case dbModel)
        {
            try
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

                if (model.cas_cst_key == CaseStatus.Accepted.ToInt()
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
                    if (model.cas_cst_key == CaseStatus.Accepted.ToInt() && !string.IsNullOrEmpty(model.cas_phy_key))
                    {
                        UpdateStatusOfPreviousPhy(model, dbModel);
                        var case_type = _uclService.GetDetails(model.cas_ctp_key);
                        var status = _physicianStatusService.GetAll().FirstOrDefault(m => m.phs_name.ToLower() == case_type.ucd_title.ToLower());
                        if (status != null)
                        {
                            SetStatus(status.phs_key, model.cas_key, model.cas_phy_key, $"Case {model.cas_key} Assigned to Physician from Case Edit Page");
                        }
                    }
                    else if ((model.cas_cst_key == CaseStatus.Complete.ToInt() || model.cas_cst_key == CaseStatus.Cancelled.ToInt())
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
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GetAll(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
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
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.QPS.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                    .Select(x => x.Facility).ToList();
                }
                var res = _caseGridService.GetCaseLisingPageData(request, userId, facilities); //Getting cases for listing
                var jsonResult = Json(res, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
                //return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [HttpPost]
        public ActionResult GetAllForDashboard(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
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
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.QPS.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                var res = _caseGridService.GetCaseDashboardPageData(request, userId, facilities);
                var jsonResult = Json(res, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
                //return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [HttpPost]
        public ActionResult ExportDashboard(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
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
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.QPS.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                var list = _caseGridService.GetCaseDashboardPage_Export(request, userId, facilities);

                for (int i = 0; i < list.Count(); i++)
                {
                    list[i].phy_name = _caseService.FormatPhysicianInitials(list[i].phy_name);
                }

                var folderName = "ReportsToPrint";
                var directoryInfo = new DirectoryInfo(Server.MapPath("~/" + folderName));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                var csvItems = list.Select(m => new
                {
                    Start_Time = m.cas_response_ts_notification,
                    Stamp_Time = m.cas_metric_stamp_time_est,
                    Call_Type = m.callType,
                    Caller_Source = m.callerSource,
                    Type = m.ctp_name,
                    Facility = m.fac_name,
                    Patient_Name = m.cas_patient,
                    Physician = m.phy_name,
                    ResponseTime = m.ResponseTime,
                    Status = m.cst_name,
                    TPA = m.TPACandidate,
                    Navigator = m.Navigator,
                    StartToStamp = m.StartToStamp,
                    StartToAccepted = m.StartToAccept,


                }).ToCSV();

                var filePath = Server.MapPath("~/" + folderName) + "/CasesReport.csv";
                System.IO.File.WriteAllText(filePath, csvItems);

                return Json(new { exportedFileUrl = $"/{folderName}/CasesReport.csv", success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [HttpPost]
        public ActionResult Export(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
            {
                string userId = User.IsInRole(UserRoles.Physician.ToDescription()) ? User.Identity.GetUserId() : "";
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
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.QPS.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                else if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                    .Select(x => x.Facility).ToList();
                }
                var list = _caseGridService.GetCaseLisingPageData_Export(request, userId, facilities);



                #region Pre loading the Case Reviewers
                var uniqueCaseReviewsIds = new List<string>();
                list.Where(m => !string.IsNullOrEmpty(m.cas_response_reviewer)).ToList().ForEach(m =>
                {
                    m.cas_response_reviewer.Split(',').ToList().ForEach(r =>
                    {
                        if (!uniqueCaseReviewsIds.Contains(r))
                        {
                            uniqueCaseReviewsIds.Add(r);
                        }
                    });
                });

                var UniqueReviewers = new Dictionary<string, string>();
                if (uniqueCaseReviewsIds.Count() > 0)
                {
                    UniqueReviewers = _adminService.GetAspNetUsers()
                                                       .Where(m => uniqueCaseReviewsIds.Contains(m.Id))
                                                       .Select(m => new { m.Id, FullName = (m.FirstName + " " + m.LastName) })
                                                       .ToDictionary(m => m.Id, m => m.FullName);
                }

                string GetReviewerNames(string Ids)
                {
                    List<string> strList = new List<string>();
                    if (!string.IsNullOrEmpty(Ids))
                    {
                        Ids.Split(',').ToList().ForEach(m =>
                        {
                            strList.Add(UniqueReviewers[m]);
                        });
                        return string.Join(",", strList);
                    }
                    else
                    {
                        return "";
                    }
                }

                #endregion
                // Check current login user is facility admin or not.
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());

                string csvItems = "";
                if (isFacilityAdmin)
                {
                    var caseExportData = list.Select(m =>
                                           new
                                           {
                                           // General Tab
                                           Case_Number = m.cas_case_number,
                                               Timestamp = m.cas_created_date.ToString("MM/dd/yyyy HH:mm:ss"), //Functions.ConvertToFacilityTimeZone(m.cas_created_date, m.fac_timezone),
                                           Case_Date = m.cas_created_date.ToString("M/d/yyyy"),
                                               Case_Time = m.cas_created_date.ToString("HH:mm"),
                                               Case_Type = m.CaseType,
                                               Facility = m.FacityName,
                                               Assigned_Physician = m.AssignedPhysician,
                                               Arrival_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_door_time, m.fac_timezone),
                                           // Metric Tab
                                           Last_Known_Well = Functions.ConvertToFacilityTimeZone(m.cas_metric_lastwell_date, m.fac_timezone),
                                               Is_Last_Known_Well_Unknow = m.cas_metric_is_lastwell_unknown.ToYesNo(),
                                               Stamp_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_stamp_time, m.fac_timezone),
                                               Workflow_Type = m.cas_patient_type.ToInt() == 0 ? "" : ((PatientType)m.cas_patient_type).ToDescription(),
                                               Time_First_Login_Attempt = Functions.ConvertToFacilityTimeZone(m.cas_response_first_atempt, m.fac_timezone),
                                               Video_Start_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_video_start_time, m.fac_timezone),
                                               Symptoms = m.cas_metric_symptoms,
                                               NIHSS_Start_Assessment_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_assesment_time, m.fac_timezone),
                                           // Col - 2
                                           Last_Seen_Normal_outside_of_4_dot_5_hours = m.cas_metric_last_seen_normal == 0 ? "" : ((LB2S2CriteriaOptions)m.cas_metric_last_seen_normal).ToDescription(),
                                               History_of_hemorrhagic_complications_or_intracranial_hemorrhage = ((LB2S2CriteriaOptions)m.cas_metric_has_hemorrhgic_history).ToDescription(),
                                               Recent_Anticoagulants = ((LB2S2CriteriaOptions)m.cas_metric_has_recent_anticoagulants).ToDescription(),
                                               History_of_recent_major_surgery = ((LB2S2CriteriaOptions)m.cas_metric_has_major_surgery_history).ToDescription(),
                                               History_of_recent_stroke = ((LB2S2CriteriaOptions)m.cas_metric_has_stroke_history).ToDescription(),
                                               tPA_Verbal_Order_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_tpa_verbal_order_time, m.fac_timezone),
                                               tPA_Candidate = m.cas_metric_tpa_consult.ToYesNo(),
                                               tPA_CPOE_Order_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_pa_ordertime, m.fac_timezone),//m.cas_metric_pa_ordertime_est
                                           Needle_Time = m.cas_metric_needle_time,
                                               Weight_Noted_By_Staff = m.cas_metric_weight,
                                               Weight_Unit = m.cas_metric_weight_unit,
                                               Total_Dose = m.cas_metric_total_dose,
                                               Bolus = m.cas_metric_bolus,
                                               Infusion = m.cas_metric_infusion,
                                               Discard_Quantity = m.cas_metric_discard_quantity,
                                               Video_End_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_video_end_time, m.fac_timezone),
                                               Reason_for_tpa_Login_Delay = m.Reason_for_tpa_Login_Delay,
                                               Patient_is_not_Candidate_for_tPA_Administration_because = m.non_tpa_reason,
                                               tPA_Delay_Notes = m.cas_billing_tpa_delay_notes,
                                           // Col - 3
                                           CT_Head_Showed_no_acute_hemorrhage_or_acute_core_infarct = m.cas_metric_ct_head_has_no_acture_hemorrhage.ToYesNo(),
                                               CT_Head_Reviewed = m.cas_metric_ct_head_is_reviewed.ToYesNo(),
                                               CT_Not_Reviewed = m.cas_metric_ct_head_is_not_reviewed.ToYesNo(),

                                               Neuro_Interventional_Case = m.cas_metric_is_neuro_interventional.HasValue ? m.cas_metric_is_neuro_interventional.Value.ToYesNo() : "",
                                               Discussed_with_Neurointerventionalist = m.cas_metric_discussed_with_neurointerventionalist.HasValue ? m.cas_metric_discussed_with_neurointerventionalist.Value.ToYesNo() : "",

                                               ER_physician_notified_of_the_decision_on_thrombolytics_management = m.cas_metric_physician_notified_of_thrombolytics.HasValue ? m.cas_metric_physician_notified_of_thrombolytics.Value.ToYesNo() : "",
                                               ER_physician_recommended_to_consult_neurointerventionalist_physician_if_the_advanced_imaging_suggestive_of_Large_Vessel_Occlusive_Thrombotic_Disease = m.cas_metric_physician_recommented_consult_neurointerventionalist?.ToYesNo(),

                                           // Billing Tab
                                           Billing_Code = m.Billing_Code,
                                               Date_Of_Consult = m.cas_billing_date_of_consult?.FormatDate(),
                                               Sign_Off_or_Follow_Up = m.cas_billing_visit_type,
                                               Follow_Up_Date = m.cas_follow_up_date?.FormatDate(),
                                               DOB = m.cas_billing_dob?.Date.FormatDate()
                                           });
                    csvItems = caseExportData.ToCSV();
                }
                else
                {
                    var caseExportData = list.Select(m =>
                                             new
                                             {
                                             // General Tab
                                             Case_Number = m.cas_case_number,
                                                 Timestamp = m.cas_created_date.ToString("MM/dd/yyyy HH:mm:ss"),//Functions.ConvertToFacilityTimeZone(m.cas_created_date, m.fac_timezone),//m.cas_created_date.FormatDateTime(),
                                             Case_Date = m.cas_created_date.ToString("M/d/yyyy"),
                                                 Case_Time = m.cas_created_date.ToString("HH:mm"),
                                                 Case_Type = m.CaseType,
                                                 Facility = m.FacityName,
                                                 Assigned_Physician = m.AssignedPhysician,
                                                 Status = m.Status,
                                                 Cart = m.cas_cart,
                                                 Callback_Phone = Functions.FormatAsPhoneNumber(m.cas_callback),
                                                 Extension = m.cas_callback_extension,
                                                 Patient_Name = m.cas_patient,
                                                 DOB = m.cas_billing_dob?.Date.FormatDate(),
                                                 Caller = m.cas_caller,
                                                 Arrival_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_door_time, m.fac_timezone),
                                                 Identification_Type = m.Identification_Type,
                                                 Identification_Number = m.cas_identification_number,
                                                 Last_4_Of_SSN = m.cas_last_4_of_ssn != null ? m.cas_last_4_of_ssn : "",
                                                 Referring_Physician = m.cas_referring_physician != null ? m.cas_referring_physician : "",
                                                 Notes = m.cas_notes,
                                                 ETA = m.cas_eta,
                                                 Pulled_From_Rounding = m.cas_pulled_from_rounding.ToYesNo(),
                                                 Navigator_Blast = m.cas_is_nav_blast.ToYesNo(),
                                             // Metric Tab
                                             Last_Known_Well = Functions.ConvertToFacilityTimeZone(m.cas_metric_lastwell_date, m.fac_timezone),
                                                 Is_Last_Known_Well_Unknow = m.cas_metric_is_lastwell_unknown.ToYesNo(),
                                                 Stamp_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_stamp_time, m.fac_timezone),
                                                 Workflow_Type = m.cas_patient_type.ToInt() == 0 ? "" : ((PatientType)m.cas_patient_type).ToDescription(),
                                                 Time_First_Login_Attempt = Functions.ConvertToFacilityTimeZone(m.cas_response_first_atempt, m.fac_timezone),
                                                 Video_Start_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_video_start_time, m.fac_timezone),
                                                 Symptoms = m.cas_metric_symptoms,
                                                 NIHSS_Start_Assessment_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_assesment_time, m.fac_timezone),
                                             // Col - 2
                                             Last_Seen_Normal_outside_of_4_dot_5_hours = m.cas_metric_last_seen_normal == 0 ? "" : ((LB2S2CriteriaOptions)m.cas_metric_last_seen_normal).ToDescription(),
                                                 History_of_hemorrhagic_complications_or_intracranial_hemorrhage = ((LB2S2CriteriaOptions)m.cas_metric_has_hemorrhgic_history).ToDescription(),
                                                 Recent_Anticoagulants = ((LB2S2CriteriaOptions)m.cas_metric_has_recent_anticoagulants).ToDescription(),
                                                 History_of_recent_major_surgery = ((LB2S2CriteriaOptions)m.cas_metric_has_major_surgery_history).ToDescription(),
                                                 History_of_recent_stroke = ((LB2S2CriteriaOptions)m.cas_metric_has_stroke_history).ToDescription(),
                                                 tPA_Verbal_Order_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_tpa_verbal_order_time, m.fac_timezone),
                                                 tPA_Candidate = m.cas_metric_tpa_consult.ToYesNo(),
                                                 Reason_for_Login_Delay = m.Reason_for_Login_Delay,
                                                 Login_Delay_Notes = m.cas_metric_notes,
                                                 tPA_CPOE_Order_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_pa_ordertime, m.fac_timezone),//m.cas_metric_pa_ordertime_est
                                             Needle_Time = m.cas_metric_needle_time,
                                                 Weight_Noted_By_Staff = m.cas_metric_weight,
                                                 Weight_Unit = m.cas_metric_weight_unit,
                                                 Total_Dose = m.cas_metric_total_dose,
                                                 Bolus = m.cas_metric_bolus,
                                                 Infusion = m.cas_metric_infusion,
                                                 Discard_Quantity = m.cas_metric_discard_quantity,
                                                 Video_End_Time = Functions.ConvertToFacilityTimeZone(m.cas_metric_video_end_time, m.fac_timezone),
                                                 Reason_for_tpa_Login_Delay = m.Reason_for_tpa_Login_Delay,
                                                 Patient_is_not_Candidate_for_tPA_Administration_because = m.non_tpa_reason,
                                                 tPA_Delay_Notes = m.cas_billing_tpa_delay_notes,
                                             // Col - 3
                                             CT_Head_Showed_no_acute_hemorrhage_or_acute_core_infarct = m.cas_metric_ct_head_has_no_acture_hemorrhage.ToYesNo(),
                                                 CT_Head_Reviewed = m.cas_metric_ct_head_is_reviewed.ToYesNo(),
                                                 CT_Not_Reviewed = m.cas_metric_ct_head_is_not_reviewed.ToYesNo(),

                                                 Neuro_Interventional_Case = m.cas_metric_is_neuro_interventional.HasValue ? m.cas_metric_is_neuro_interventional.Value.ToYesNo() : "",
                                                 Discussed_with_Neurointerventionalist = m.cas_metric_discussed_with_neurointerventionalist.HasValue ? m.cas_metric_discussed_with_neurointerventionalist.Value.ToYesNo() : "",

                                                 ER_physician_notified_of_the_decision_on_thrombolytics_management = m.cas_metric_physician_notified_of_thrombolytics.HasValue ? m.cas_metric_physician_notified_of_thrombolytics.Value.ToYesNo() : "",
                                                 ER_physician_recommended_to_consult_neurointerventionalist_physician_if_the_advanced_imaging_suggestive_of_Large_Vessel_Occlusive_Thrombotic_Disease = m.cas_metric_physician_recommented_consult_neurointerventionalist?.ToYesNo(),

                                             // Billing Tab
                                             Billing_Code = m.Billing_Code,
                                                 Date_Of_Consult = m.cas_billing_date_of_consult?.FormatDate(),
                                                 Diagnosis = m.cas_billing_diagnosis,
                                             //MRN_FIN = m.cas_billing_mrn_fin,
                                             Billing_Notes = m.cas_billing_notes,
                                                 Sign_Off_or_Follow_Up = m.cas_billing_visit_type,
                                                 Follow_Up_Date = m.cas_follow_up_date?.FormatDate(),
                                                 Physician_Blast = m.cas_billing_physician_blast.ToYesNo(),
                                             // Case Review Tab
                                             Date_of_Consult = m.cas_response_date_consult?.FormatDate(),
                                                 Start_Time = Functions.ConvertToFacilityTimeZone(m.cas_response_ts_notification, m.fac_timezone), //   start time                                     
                                             Final_Physician_Acceptance_Time = Functions.ConvertToFacilityTimeZone(m.cas_response_time_physician, m.fac_timezone),
                                                 SA_to_TS_MD = m.cas_response_sa_ts_md > 0 ? ((MetricResponseStatus)m.cas_response_sa_ts_md).ToDescription() : "",
                                                 Navigator_Concurrent_Alerts = m.cas_navigator_concurrent_alerts > 0 ? ((MetricResponseStatus)m.cas_navigator_concurrent_alerts).ToDescription() : "",
                                                 Physician_Concurrent_Alerts = m.cas_physician_concurrent_alerts > 0 ? ((MetricResponseStatus)m.cas_physician_concurrent_alerts).ToDescription() : "",
                                                 Miscommunication = m.cas_response_miscommunication > 0 ? ((MetricResponseStatus)m.cas_response_miscommunication).ToDescription() : "",
                                                 Technical_Issues = m.cas_response_technical_issues > 0 ? ((MetricResponseStatus)m.cas_response_technical_issues).ToDescription() : "",
                                                 tpa_60_minutes = m.cas_response_tpa_to_minute > 0 ? ((MetricResponseStatus)m.cas_response_tpa_to_minute).ToDescription() : "",
                                                 door_Time_to_Needle_Time_60_Minutes = m.cas_response_door_to_needle > 0 ? ((MetricResponseStatus)m.cas_response_door_to_needle).ToDescription() : "",
                                                 Start_Time_to_Needle_Time = Functions.GetSubtractedDateFormated(m.cas_metric_stamp_time_est, m.cas_metric_needle_time_est), // calculated field
                                             Time_First_Login_Attempt_to_Needle_Time = Functions.GetSubtractedDateFormated(m.cas_response_first_atempt, m.cas_metric_needle_time_est), // calculated field
                                             tPA_to_Needle_Time = Functions.GetSubtractedDateFormated(m.cas_metric_pa_ordertime_est, m.cas_metric_needle_time_est), // calculated field
                                                                                                                                                                    // Col - 2
                                             Response_Time = Functions.GetSubtractedDateFormated(m.cas_metric_stamp_time_est, m.cas_response_first_atempt), // calculated field                          
                                             Reviewer = m.cas_response_reviewer != "" ? GetReviewerNames(m.cas_response_reviewer) : "",
                                                 Case_Research = m.cas_response_case_research,
                                                 Physician_to_TS_Accept_3 = m.cas_response_nav_to_ts > 0 ? ((MetricResponseStatus)m.cas_response_nav_to_ts).ToDescription() : "",
                                                 Pulled_from_Rounding = m.cas_response_pulled_rounding > 0 ? ((MetricResponseStatus)m.cas_response_pulled_rounding).ToDescription() : "",
                                                 Physician_Having_Technical_Issue_Date = Functions.ConvertToFacilityTimeZone(m.cas_phy_technical_issue_date, m.fac_timezone),
                                                 Physician_Blast_8 = m.cas_response_physician_blast > 0 ? ((MetricResponseStatus)m.cas_response_physician_blast).ToDescription() : "",
                                                 RCA_Initiated = m.cas_response_review_initiated > 0 ? ((MetricResponseStatus)m.cas_response_review_initiated).ToDescription() : "",
                                                 RCA_Number = m.cas_response_case_number,
                                             });

                    csvItems = caseExportData.ToCSV();
                }

                var folderName = "ReportsToPrint";
                var directoryInfo = new DirectoryInfo(Server.MapPath("~/" + folderName));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                var filePath = Server.MapPath("~/" + folderName) + "/CasesReport.csv";
                System.IO.File.WriteAllText(filePath, csvItems);

                return Json(new { exportedFileUrl = $"/{folderName}/CasesReport.csv", success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }
        [HttpPost]
        public JsonResult CreateCallLog(InitializeCallHistoryViewModel model)
        {
            try
            {
                var callId = model.CallId;
                if (!string.IsNullOrEmpty(model.ANI))
                {
                    var prevCallHistory = _callHistoryService.GetCallInfo(callId).FirstOrDefault();
                    if (prevCallHistory == null || string.IsNullOrEmpty(prevCallHistory.chi_call_id))
                    {
                        var callHistory = new call_history
                        {
                            chi_ani = model.ANI ?? "",
                            chi_dnis = model.DNIS ?? null,
                            chi_start_time_stamp = model.TimeStamp != null ? Functions.TimeStampToDateTime(Convert.ToDouble(model.TimeStamp)) : null,
                            chi_call_id = callId,
                            chi_campaign_id = model.CampaignId ?? "",
                            chi_customer = model.Customer ?? "",
                            chi_created_by = User.Identity.GetUserId(),
                            chi_created_date = DateTime.Now.ToEST(),
                            chi_is_active = true,
                            chi_original_stamp_time = model.TimeStamp != null ? model.TimeStamp : ""
                        };
                        _callHistoryService.Create(callHistory);
                    }

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateCallLog(CallHistoryViewModel vm)
        {
            try
            {
                var callHistory = _callHistoryService.GetCallInfo(vm.callId).FirstOrDefault();
                if (callHistory != null && !string.IsNullOrEmpty(callHistory.chi_call_id))
                {
                    callHistory.chi_call_object_id = vm.callObjectId;
                    callHistory.chi_campaign_name = vm.campaign;
                    callHistory.chi_agent = vm.agent;
                    callHistory.chi_agent_extension = vm.agentExtension;
                    callHistory.chi_agent_name = vm.agentName;
                    callHistory.chi_call_result = vm.callResult;
                    callHistory.chi_call_type = vm.callType;
                    callHistory.chi_call_back_id = vm.callbackId;
                    callHistory.chi_call_back_number = vm.callbackNumber;
                    callHistory.chi_comments = vm.comments;
                    callHistory.chi_duration = vm.duration;
                    callHistory.chi_format_api_call_type = vm.formatAPICallType;
                    callHistory.chi_handle_time = vm.handleTime;
                    callHistory.chi_session_id = vm.sessionId;
                    callHistory.chi_subject = vm.subject;
                    callHistory.chi_talk_and_hold_duration = vm.talkAndHoldDuration;
                    callHistory.chi_wrap_time = vm.wrapTime;
                    callHistory.chi_modified_by = User.Identity.GetUserId();
                    callHistory.chi_modified_date = DateTime.Now.ToEST();

                    _callHistoryService.Edit(callHistory);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateCaseFlag(int caskey, bool isFlagged, PageSource source = PageSource.CaseListing)
        {
            try
            {
                bool isUpdated = false;
                if (caskey > 0)
                {
                    var model = _caseService.GetDetailsWithoutTimeConversion(caskey);

                    if (source == PageSource.CaseListing)
                        model.cas_is_flagged = !isFlagged;
                    else if (source == PageSource.Dashboard)
                        model.cas_is_flagged_dashboard = !isFlagged;

                    _caseService.EditCaseOnly(model);
                    isUpdated = true;
                }
                return Json(new { success = isUpdated });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occured while processing your request, please try later" });
            }
        }
        #region Added by Ahmad 11-09-2020
        [HttpPost]
        public ActionResult UpdateCasePhysicianFlag(int caskey, bool isFlagged, PageSource source = PageSource.CaseListing)
        {
            try
            {
                bool isUpdated = false;
                if (caskey > 0)
                {
                    var model = _caseService.GetDetailsWithoutTimeConversion(caskey);

                    //if (source == PageSource.CaseListing)
                    model.cas_is_flagged_physician = !isFlagged;
                    //else if (source == PageSource.Dashboard)
                    //model.cas_is_flagged_dashboard = !isFlagged;

                    _caseService.EditCaseOnly(model);
                    isUpdated = true;
                }
                return Json(new { success = isUpdated });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occured while processing your request, please try later" });
            }
        }
        #endregion

        [HttpPost]
        public ActionResult autoSaveCaseData(AutoSaveViewModel model)
        {
            try
            {
                if (ApplicationSetting.aps_enable_case_auto_save)
                {
                    var formData = model.FormData.Select(m => new { m.Key, m.Value }).Distinct().ToDictionary(m => m.Key, m => Functions.DecodeFrom64(m.Value));

                    var SelectedNIHSQuestionResponse = formData.Where(m => m.Key == "SelectedNIHSQuestionResponse");
                    if (SelectedNIHSQuestionResponse.Count() > 0)
                    {
                        var nihssData = SelectedNIHSQuestionResponse.First().Value;
                        SavenihssData(model.Id, nihssData);
                    }

                    var templatesData = formData.Where(m => m.Key.Contains("."));
                    if (templatesData.Count() > 0 && model.TemplateEntityType > 0)
                    {
                        var tables = templatesData.Select(m => m.Key.Split('.').First()).Distinct();
                        foreach (var tbl in tables)
                        {
                            var tableFields = templatesData.Where(m => m.Key.Contains(tbl)).Select(m => new { Key = m.Key.Split('.').Last(), Value = m.Value })
                                                                                           .ToDictionary(m => m.Key, m => m.Value);

                            if (model.TemplateKey > 0)
                            {
                                try
                                {
                                    DBHelper.UpdateSelectedColumns(model.TemplateKeyName, model.Id.ToString(), tbl, tableFields.ToDictionary(m => m.Key, m => m.Value));
                                }
                                catch (Exception ex)
                                {

                                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                                }

                            }
                            else
                            {
                                try
                                {
                                    var postedFields = tableFields.ToDictionary(m => m.Key, m => m.Value);
                                    postedFields.Add(model.TemplateKeyName, model.Id.ToString());
                                    var entityType = (EntityTypes)model.TemplateEntityType;
                                    postedFields = SetDefaultValuesForTemplate(entityType, postedFields);
                                    DBHelper.InsertSelectedColumns(tbl, postedFields);
                                }
                                catch
                                {
                                    DBHelper.UpdateSelectedColumns(model.TemplateKeyName, model.Id.ToString(), tbl, tableFields.ToDictionary(m => m.Key, m => m.Value));
                                }

                            }
                        }
                    }

                    var partialClassField = "TemplateKey,VisibleTabs,PhysicianUser,FacilityTimeZone,cas_response_ts_notification,cas_response_ts_notification_utc,SelectedNIHSQuestionResponse,TemplateEntityType,IsCaseCompleted".Split(',');
                    var caseMetaData = (typeof(@case)).GetProperties().Select(m => new { m.Name, PropertyType = m.PropertyType.FullName.ToLower() })
                                                                          .ToDictionary(m => m.Name, m => m.PropertyType);
                    var caseProperties = caseMetaData.Select(m => m.Key);

                    var fieldsToUpdated = model.FormData.Where(m => caseProperties.Contains(m.Key)).Where(m => !partialClassField.Contains(m.Key) && !m.Key.Contains(".")).Select(m => new { Key = m.Key, Value = Functions.DecodeFrom64(m.Value), SaveAsUTC = m.SaveAsUTC, PreviousValue = Functions.DecodeFrom64(m.PreviousValue) }).ToList();

                    Dictionary<string, string> result = new Dictionary<string, string>();

                    if (fieldsToUpdated.Count() > 0)
                    {

                        if (string.IsNullOrEmpty(model.FacilityTimeZone))
                        {
                            model.FacilityTimeZone = Settings.DefaultTimeZone;
                        }

                        foreach (var field in fieldsToUpdated)
                        {
                            #region handling eastern and utc conversion
                            if (field.Key.Contains("_est"))
                            {
                                var utc_field = field.Key.Replace("_est", "");
                                var isUtcFieldExists = caseProperties.Where(m => m.ToLower() == utc_field.ToLower()).Any();

                                if (!string.IsNullOrEmpty(field.Value))
                                {
                                    string prevESTValue = "";
                                    if (!string.IsNullOrEmpty(field.PreviousValue))
                                        prevESTValue = Convert.ToDateTime(field.PreviousValue).ToUniversalTimeZone(model.FacilityTimeZone).ToTimezoneFromUtc("Eastern Standard Time").ToString();

                                    var est_value = Convert.ToDateTime(field.Value).ToUniversalTimeZone(model.FacilityTimeZone).ToTimezoneFromUtc("Eastern Standard Time");
                                    result.Add(field.Key, est_value.ToString());
                                    _UpdateInList(new AutoSaveDictionary { Key = field.Key, Value = est_value.ToString(), PreviousValue = prevESTValue });

                                    if (isUtcFieldExists)
                                    {
                                        var utc_value = Convert.ToDateTime(field.Value).ToUniversalTimeZone(model.FacilityTimeZone).ToString();
                                        string prevUTCValue = "";
                                        if (!string.IsNullOrEmpty(field.PreviousValue))
                                            prevUTCValue = Convert.ToDateTime(field.PreviousValue).ToUniversalTimeZone(model.FacilityTimeZone).ToString();
                                        result.Add(utc_field, utc_value);

                                        _UpdateInList(new AutoSaveDictionary { Key = utc_field, Value = utc_value, PreviousValue = prevUTCValue });
                                    }
                                }
                                else
                                {
                                    result.Add(field.Key, null);
                                    if (isUtcFieldExists)
                                    {
                                        result.Add(utc_field, null);
                                    }
                                }

                            }
                            #endregion
                            else
                            {
                                if (field.SaveAsUTC)
                                {
                                    var utc_value = Convert.ToDateTime(field.Value).ToUniversalTimeZone(model.FacilityTimeZone).ToString();
                                    string prevUTCValue = "";
                                    if (!string.IsNullOrEmpty(field.PreviousValue))
                                        prevUTCValue = Convert.ToDateTime(field.PreviousValue).ToUniversalTimeZone(model.FacilityTimeZone).ToString();

                                    result.Add(field.Key, utc_value);
                                    _UpdateInList(new AutoSaveDictionary { Key = field.Key, Value = utc_value, PreviousValue = prevUTCValue });
                                }
                                else
                                {
                                    string elementValue = field.Value;
                                    #region handling null value for nullable types in case of empty string 
                                    if (string.IsNullOrEmpty(field.Value))
                                    {
                                        if (caseMetaData.ContainsKey(field.Key))
                                        {
                                            if (caseMetaData[field.Key].Contains("nullable"))
                                            {
                                                elementValue = null;
                                            }
                                        }
                                    }
                                    #endregion

                                    result.Add(field.Key, elementValue);
                                }

                            }


                        }

                        try
                        {
                            DBHelper.UpdateSelectedColumns("cas_key", model.Id.ToString(), "[case]", result);
                        }
                        catch (Exception ex)
                        {

                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        }


                        _caseService.UpdateTimeStamps(model.Id.ToString());
                    }

                    void _UpdateInList(AutoSaveDictionary dictItem)
                    {
                        var obj = model.FormData.FirstOrDefault(m => m.Key == dictItem.Key);
                        if (obj != null)
                            obj.Value = dictItem.Value;
                        else
                            model.FormData.Add(dictItem);
                    }


                    model.FormData = model.FormData.Where(m => !partialClassField.Contains(m.Key)).ToList();
                    // addding the data in change log              
                    if (model.FormData.Count() > 0)
                    {

                        LogAutoSaveChanges(model);
                    }

                    return Json(new { success = true, autosave = true });
                }
                else
                {
                    return Json(new { success = false, autosave = false });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, autosave = false });
            }
        }

        [HttpPost]
        public JsonResult CancelCase(int casKey, string caseType, string caseText)
        {
            try
            {
                var dbModel = _caseService.GetDetailsWithoutTimeConversion(casKey);

                #region Check authorized user to save case
                if (!(User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription())))
                {
                    if (dbModel.cas_phy_key != User.Identity.GetUserId())
                    {
                        ModelState.AddModelError("", "Access Denied! <br/>");
                        ModelState.AddModelError("", "You are not authorized to save this case. This case is reassigned.");

                        return Json(new { success = false, error = "Access Denied!" }, JsonRequestBehavior.AllowGet);
                    }
                }
                #endregion

                HideNavigatorCasePopup(dbModel.cas_key, dbModel.cas_phy_key);

                // handling logging in case of physician updated          
                LogCaseAssignHistory(casKey, dbModel.cas_phy_key, CaseStatus.Cancelled.ToDescription(), true);
                // handling status update time of physician

                var assignedPhysician = _adminService.GetAspNetUsers().Where(m => m.status_change_cas_key == casKey && m.Id == dbModel.cas_phy_key).FirstOrDefault();
                if (assignedPhysician != null)
                {
                    if ((dbModel.cas_ctp_key == CaseType.StrokeAlert.ToInt() && assignedPhysician.status_key == PhysicianStatus.Stroke.ToInt())
                         || (dbModel.cas_ctp_key == CaseType.StatConsult.ToInt() && assignedPhysician.status_key == PhysicianStatus.STATConsult.ToInt())
                        )
                        SetStatus(PhysicianStatus.Available.ToInt(), casKey, dbModel.cas_phy_key, $"Status changed to Available due to case {dbModel.cas_key} marked as Cancelled from CancelCase Action");
                }

                var currentUser = loggedInUser;
                dbModel.cas_modified_by = currentUser.Id;
                dbModel.cas_modified_by_name = currentUser.FullName;
                dbModel.cas_modified_date = DateTime.Now.ToEST();
                //  
                dbModel.cas_is_partial_update = true;
                // update case
                dbModel.cas_cst_key = (int)CaseStatus.Cancelled;
                dbModel.cas_history_physician_initial = _caseService.GetCaseInitials(dbModel.cas_key);

                if (dbModel.cas_is_flagged)
                {
                    dbModel.cas_is_flagged = false;
                }
                if (!string.IsNullOrEmpty(caseType))
                    dbModel.cas_cancelled_type = caseType;
                if (!string.IsNullOrEmpty(caseText))
                    dbModel.cas_cancelled_text = caseText;
                _caseService.EditCaseOnly(dbModel);

                _caseService.UpdateTimeStamps(dbModel.cas_key.ToString());

                var signalRPostModel = GetSignalRPostData(dbModel);
                SyncCaseDataFromAdmin(signalRPostModel);

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCaseToCTAQueue(int casKey)
        {
            try
            {
                var _updated = false;
                try
                {
                    _updated = OnCTAQueueChange(casKey, true);

                    return Json(new { updated = _updated }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }

                return Json(new { updated = _updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }

        }
        [HttpPost]
        public ActionResult RemoveCaseFromCTAQueue(int casKey)
        {
            var _updated = false;

            try
            {
                _updated = OnCTAQueueChange(casKey, false);

                return Json(new { updated = _updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { updated = _updated }, JsonRequestBehavior.AllowGet);

        }

        private bool OnCTAQueueChange(int casKey, bool status)
        {
            try
            {
                var _updated = false;
                var _case = _caseService.GetDetailsWithoutTimeConversion(casKey);
                var previousValue = _case.cas_metric_in_cta_queue;

                _case.cas_metric_in_cta_queue = status;
                LogCaseChanges(casKey.ToString());

                var currentUser = loggedInUser;
                _case.cas_modified_by = currentUser.Id;
                _case.cas_modified_by_name = currentUser.FullName;
                _case.cas_modified_date = DateTime.Now.ToEST();

                _caseService.EditCaseOnly(_case, true);
                _updated = true;

                var currentValue = _case.cas_metric_in_cta_queue;

                var list = new List<ChangeTrackEntityVM>()
                        {
                            new ChangeTrackEntityVM()
                            {
                                entity = EntityTypes.Case.ToDescription(),
                                field = "cas_metric_in_cta_queue",
                                current = currentValue,
                                previous = previousValue
                            }
                        };

                var formattedList = _caseService.GetFormattedList(list);

                if (formattedList.Count() > 0)
                {
                    AddChangeLog(formattedList, casKey.ToString(), EntityTypes.Case);
                }
                return _updated;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        public ActionResult GetFacilityEDMainContacts(Guid Id, string type)
        {
            try
            {
                var contacts = _contactService.GetContactsByType(Id, type)

                                              .Select(m => m.cnt_primary_phone)
                                              .Distinct()
                                              .ToList()
                                              .Select(m => new { phone_number = m, phone_number_formatted = Functions.FormatAsPhoneNumber(m) });

                return Json(new { contacts = contacts });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [HttpGet]
        [AccessRoles(Roles = "Administrator,Super Admin,Physician,Facility Physician,Partner Physician,Navigator,RRC Manager, RRC Director,AOC")]
        public ActionResult SignOutListing()
        {
            try
            {
                if (TempData.ContainsKey("Message"))
                    TempData.Remove("Message");

                bool allowListing = true;

                var facilities = new List<FacilityViewModel>().AsQueryable();

                if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                {
                    facilities = _facilityPhysicianService.GetPhsicianFacilities(loggedInUser.Id, "").Select(f => new FacilityViewModel { fac_key = f.fac_key, fac_name = f.fac_name });
                }
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId()).Select(f => new FacilityViewModel { fac_key = f.Facility, fac_name = f.FacilityName });
                }
                else
                {
                    facilities = _lookUpService.GetAllFacility(null).Select(f => new FacilityViewModel { fac_key = f.fac_key, fac_name = f.fac_name });
                }

                if (facilities.Count() < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
                else
                {
                    var facilitiesSelectList = facilities.Select(m => new SelectListItem
                    {
                        Text = m.fac_name,
                        Value = m.fac_key.ToString()
                    }).ToList();


                    facilitiesSelectList = facilitiesSelectList.Prepend(new SelectListItem()
                    {
                        Text = "--Select--",
                        Value = ""
                    }).ToList();

                    ViewBag.Facilities = facilitiesSelectList;
                }


                var types = new List<int>()
                    {
                        UclTypes.BillingCode.ToInt(),
                    };
                var uclDataList = _lookUpService.GetUclData(types)
                                        .Where(m => m.ucd_is_active)
                                        .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                                        .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).ToList();
                ViewBag.UclData = uclDataList;

                ViewBag.AllowListing = allowListing;
                ViewBag.Error = (TempData["Error"] as bool?) ?? false;
                ViewBag.Message = TempData["StatusMessage"] as string;

                return GetViewResult();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        [HttpPost]
        public ActionResult GerForSignOutListing(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
            {
                //Check incomming filter first.
                List<Guid> facilities = new List<Guid>();
                var cas_facility_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "signout_cas_fac_key");
                if (cas_facility_filter_field != null)
                {
                    var facility_ids = cas_facility_filter_field.Value?.ToString();
                    if (!string.IsNullOrEmpty(facility_ids))
                    {
                        facilities.AddRange(facility_ids.Split(',').Select(m => new Guid(m)));
                    }
                }

                /// If filter is empty then load only assigned facilities 
                //if (facilities.Count == 0)
                //{
                //    if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                //    {
                //        facilities = _facilityPhysicianService.GetPhsicianFacilities(loggedInUser.Id, "").Select(f => f.fac_key).ToList();
                //    }
                //    else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                //    {
                //        facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId()).Select(f =>f.Facility).ToList();
                //    } 
                //}

                var res = _caseService.GetAllForSignOutListing(request, "", facilities);
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        public ActionResult ExportSignOutListing(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
            {
                //Check incomming filter first.
                List<Guid> facilities = new List<Guid>();
                var cas_facility_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "signout_cas_fac_key");
                if (cas_facility_filter_field != null)
                {
                    var facility_ids = cas_facility_filter_field.Value?.ToString();
                    if (!string.IsNullOrEmpty(facility_ids))
                    {
                        facilities.AddRange(facility_ids.Split(',').Select(m => new Guid(m)));
                    }
                }
                /// If filter is empty then load only assigned facilities 
                if (facilities.Count == 0)
                {
                    if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                    {
                        facilities = _facilityPhysicianService.GetPhsicianFacilities(loggedInUser.Id, "").Select(f => f.fac_key).ToList();
                    }
                    else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                    {
                        facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId()).Select(f => f.Facility).ToList();
                    }
                }

                var list = _caseService.GetAllQueerableForSignOutListing(request, "", facilities, isExporting: true);

                var folderName = "ReportsToPrint";
                var directoryInfo = new DirectoryInfo(Server.MapPath("~/" + folderName));
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }

                var csvItems = list.Select(m => new
                {
                    Facility = m.fac_name,
                    Patient_Name = m.cas_patient,
                    Date_of_Birth = m.date_of_birth,
                    Date_of_Consult = m.date_of_consult,
                    Case_Type = m.case_type,
                    Billing_Code = m.billing_code,
                    Case_Status = m.case_status,
                    Sign_off_Follow_Up = m.sign_off_follow_up,
                    Comments_Sign_Out = m.comments
                }).ToCSV();

                var filePath = Server.MapPath("~/" + folderName) + "/CasesReport.csv";
                System.IO.File.WriteAllText(filePath, csvItems);

                return Json(new { exportedFileUrl = $"/{folderName}/CasesReport.csv", success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        #region Template Methods
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GenerateCaseTemplate(@case model)
        {
            try
            {
                //var inputTemplate = Server.MapPath("~/Templates/StrokeAlertTPATemplate.html");
                ViewBag.PhysicianMetrics = _templateService.GetPhysicianMetrics(model);
                ViewBag.NIHSSAssessmentsData = _templateService.GenerateNIHSSReport(model);

                if (model.cas_phy_key != null)
                {
                    ViewBag.Physician = _physician.GetDetail(model.cas_phy_key);
                }
                var result = string.Empty;
                if (model.TemplateEntityType.HasValue && (model.TemplateEntityType == EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt() || model.TemplateEntityType == EntityTypes.StrokeAlertTemplateTpa.ToInt()))
                {
                    if (model.TemplateEntityType == EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt())
                    {
                        ViewBag.IllnessHistory = _templateService.GetIllnessHistory(model, EntityTypes.NeuroStrokeAlertTemplateTpa);
                        ViewBag.StrokeTemplateRecommendations = _uclService.GetUclData(UclTypes.StrokeTemplateRecommendations)
                                                .Select(m => new { m.ucd_key, m.ucd_title })
                                                .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                        result = RenderPartialViewToString("Templates/Preview/_StrokeNeuroAlertTPATemplate", model);
                    }
                    else
                    {
                        ViewBag.IllnessHistory = _templateService.GetIllnessHistory(model, EntityTypes.StrokeAlertTemplateTpa);
                        result = RenderPartialViewToString("Templates/Preview/_StrokeAlertTPATemplate", model);
                    }

                    //   var result = _templateService.GenerateStrokeTpATemplate(model, inputTemplate);
                }
                else if (model.TemplateEntityType.HasValue && model.TemplateEntityType == EntityTypes.StrokeAlertTemplateNoTpa.ToInt())
                {
                    StrokeAlertNoTpaData(model);
                    ViewBag.IllnessHistory = _templateService.GetIllnessHistory(model, EntityTypes.StrokeAlertTemplateNoTpa);
                    result = RenderPartialViewToString("Templates/Preview/_StrokeAlertNoTPATemplate", model);
                }
                else if (model.TemplateEntityType.HasValue && model.TemplateEntityType == EntityTypes.StrokeAlertTemplateNoTpaTeleStroke.ToInt())
                {
                    ViewBag.IllnessHistory = _templateService.GetIllnessHistory(model, EntityTypes.StrokeAlertTemplateNoTpaTeleStroke);
                    ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.AntiplateletTherapyRecommendedNoTpa)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    //Added By Axim
                    ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutNoTpa)
                              .Select(m => new { m.ucd_key, m.ucd_title })
                              .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    result = RenderPartialViewToString("Templates/Preview/_TeleStrokeNoTPATemplate", model);
                }
                else if (model.TemplateEntityType.HasValue && model.TemplateEntityType == EntityTypes.StateAlertTemplate.ToInt())
                {
                    ViewBag.IllnessHistory = _templateService.GetIllnessHistory(model, EntityTypes.StateAlertTemplate);
                    ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.StateConsultTemplate)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutStat)
                              .Select(m => new { m.ucd_key, m.ucd_title })
                              .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.AntiplateletTherapyRecommendedStatConsult = _uclService.GetUclData(UclTypes.ImagingStudiesStatConsult)
                                     .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                     .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultTherapies = _uclService.GetUclData(UclTypes.TherapiesState)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultOtherWork = _uclService.GetUclData(UclTypes.OtherWorkUp)
                                     .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                     .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    //Added By Axim
                    ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    result = RenderPartialViewToString("Templates/Preview/_TeleStatConsultNoTPATemplate", model);
                }
                return Json(new { success = true, data = result, showEditor = true });

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public ActionResult LoadCaseTemplate(int caseKey, EntityTypes templateEntity)
        {
            try
            {
                var template = _generateTemplateService.GetDetails(caseKey, templateEntity.ToInt());
                if (template != null)
                {
                    var result = template.cgt_template_html;
                    return Json(new { success = true, data = result, showEditor = (template.cgt_finalize_date.HasValue ? false : true) });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveTemplate(int cas_key, int entityKey, bool is_finalized, string ptemplateData)
        {
            try
            {
                #region Check authorized user to save case
                if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                {
                    var caseModel = _caseService.GetDetailsWithoutTimeConversion(cas_key);
                    if (caseModel.cas_phy_key != User.Identity.GetUserId())
                    {
                        return Json(new { success = false, data = "Access Denied! <br/> You are not authorized to save or finalize this case template. This case is reassigned." });
                    }
                }
                #endregion
                var templateData = Functions.DecodeFrom64(ptemplateData);
                // var templateData = HttpUtility.HtmlDecode(encodeFromBase64);

                var template = _generateTemplateService.GetDetails(cas_key, entityKey);
                if (template == null)
                {
                    var obj = new case_generated_template
                    {
                        cgt_cas_key = cas_key,
                        cgt_ent_key = entityKey,
                        cgt_template_html = templateData,
                        cgt_created_by = loggedInUser.Id,
                        cgt_created_by_name = loggedInUser.FullName,
                        cgt_created_date = DateTime.Now.ToEST()
                    };
                    if (is_finalized)
                        obj.cgt_finalize_date = DateTime.Now.ToEST();

                    _generateTemplateService.Create(obj);
                }
                else
                {
                    template.cgt_modified_by = loggedInUser.Id;
                    template.cgt_modified_by_name = loggedInUser.FullName;
                    template.cgt_modified_date = DateTime.Now.ToEST();
                    template.cgt_template_html = templateData;
                    if (is_finalized)
                        template.cgt_finalize_date = DateTime.Now.ToEST();

                    _generateTemplateService.Edit(template);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occured while processing your request, please try later" });
            }
        }

        [HttpPost]
        public ActionResult ChangeTemplate(@case model)
        {
            try
            {
                if (model.cas_fac_key == null || model.cas_fac_key.ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    return Json(new { result = false, view = "" });
                }

                #region Swapping Templates Data

                case_template_stroke_tpa _case_template_stroke_tpa;
                case_template_stroke_neuro_tpa _case_template_stroke_neuro_tpa;
                case_template_stroke_notpa _case_template_stroke_notpa;
                case_template_telestroke_notpa _case_template_telestroke_notpa;
                case_template_statconsult _case_Template_Statconsult;

                var cas_key = model.cas_key;
                var _template_created_by = model.cas_modified_by;
                var _template_created_by_name = model.cas_modified_by_name;
                var _template_created_or_modified_Date = DateTime.Now.ToEST();
                var _template_modified_by = _template_created_by;
                var _template_modified_by_name = _template_created_by_name;

                #region Creating Templates 

                _case_template_stroke_tpa = new case_template_stroke_tpa()
                {
                    cts_cas_key = cas_key,
                    cts_created_by = _template_created_by,
                    cts_created_by_name = _template_created_by_name,
                    cts_created_date = _template_created_or_modified_Date,
                };
                _case_template_stroke_neuro_tpa = new case_template_stroke_neuro_tpa()
                {
                    csn_cas_key = cas_key,
                    csn_created_by = _template_created_by,
                    csn_created_by_name = _template_created_by_name,
                    csn_created_date = _template_created_or_modified_Date,
                };
                _case_template_stroke_notpa = new case_template_stroke_notpa()
                {
                    ctn_cas_key = cas_key,
                    ctn_created_by = _template_created_by,
                    ctn_created_by_name = _template_created_by_name,
                    ctn_created_date = _template_created_or_modified_Date
                };
                _case_template_telestroke_notpa = new case_template_telestroke_notpa()
                {
                    ctt_cas_key = cas_key,
                    ctt_created_by = _template_created_by,
                    ctt_created_by_name = _template_created_by_name,
                    ctt_created_date = _template_created_or_modified_Date,
                };
                _case_Template_Statconsult = new case_template_statconsult()
                {
                    ctt_cas_key = cas_key,
                    ctt_created_by = _template_created_by,
                    ctt_created_by_name = _template_created_by_name,
                    ctt_created_date = _template_created_or_modified_Date,
                };

                #endregion

                if (model.case_template_stroke_tpa != null)
                {
                    var template = model.case_template_stroke_tpa;

                    #region Fields 

                    //Added By Axim
                    _case_template_stroke_neuro_tpa.csn_family_consent_available
                       = _case_template_stroke_notpa.ctn_family_consent_available
                  = _case_template_stroke_neuro_tpa.csn_family_consent_available
                       = _case_template_stroke_tpa.cts_family_consent_available
                       = template.cts_family_consent_available;

                    _case_template_stroke_neuro_tpa.csn_PMH
                       = _case_template_stroke_notpa.ctn_PMH
                  = _case_template_stroke_neuro_tpa.csn_PMH
                       = _case_template_stroke_tpa.cts_PMH
                       = template.cts_PMH;

                    _case_template_stroke_neuro_tpa.csn_anticoagulant_use
                      = _case_template_stroke_notpa.ctn_anticoagulant_use
                 = _case_template_stroke_neuro_tpa.csn_anticoagulant_use
                      = _case_template_stroke_tpa.cts_anticoagulant_use
                      = template.cts_anticoagulant_use;

                    _case_template_stroke_neuro_tpa.csn_antiplatelet_use
                     = _case_template_stroke_notpa.ctn_antiplatelet_use
                = _case_template_stroke_neuro_tpa.csn_antiplatelet_use
                     = _case_template_stroke_tpa.cts_antiplatelet_use
                     = template.cts_antiplatelet_use;

                    _case_template_stroke_notpa.ctn_anticoagulant_use_text
                  = _case_template_stroke_neuro_tpa.csn_anticoagulant_use_text
                  = _case_template_telestroke_notpa.ctt_anticoagulant_use_text
                  = _case_template_stroke_tpa.cts_anticoagulant_use_text
                  = template.cts_anticoagulant_use_text;

                    _case_template_stroke_notpa.ctn_antiplatelet_use_text
                 = _case_template_stroke_neuro_tpa.csn_antiplatelet_use_text
                 = _case_template_telestroke_notpa.ctt_antiplatelet_use_text
                 = _case_template_stroke_tpa.cts_antiplatelet_use_text
                 = template.cts_antiplatelet_use_text;

                    //ended By Axim

                    _case_template_stroke_notpa.ctn_impression_text
                   = _case_template_stroke_neuro_tpa.csn_impression
                    = _case_template_telestroke_notpa.ctt_impression_text
                    = _case_template_stroke_tpa.cts_impression
                    = template.cts_impression;

                    _case_template_stroke_notpa.ctn_impression
                   = _case_template_telestroke_notpa.ctt_impression
                   = template.cts_acute_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke
                       = _case_template_stroke_neuro_tpa.csn_mechanism_stroke
                       = _case_template_telestroke_notpa.ctt_mechanism_stroke
                       = _case_template_stroke_tpa.cts_mechanism_stroke
                       = template.cts_mechanism_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke_text
                      = _case_template_stroke_neuro_tpa.csn_mechanism_stroke_text
                      = _case_template_telestroke_notpa.ctt_mechanism_stroke_text
                      = _case_template_stroke_tpa.cts_mechanism_stroke_text
                      = template.cts_mechanism_stroke_text;

                    _case_template_stroke_notpa.ctn_vitals_bp
                     = _case_template_stroke_neuro_tpa.csn_vitals_bp
                     = _case_template_telestroke_notpa.ctt_vitals_bp
                     = _case_template_stroke_tpa.cts_vitals_bp
                     = template.cts_vitals_bp;

                    _case_template_stroke_notpa.ctn_vitals_pulse
                     = _case_template_stroke_neuro_tpa.csn_vitals_pulse
                     = _case_template_telestroke_notpa.ctt_vitals_pulse
                     = _case_template_stroke_tpa.cts_vitals_pulse
                     = template.cts_vitals_pulse;

                    _case_template_stroke_notpa.ctn_vitals_blood_glucose
                     = _case_template_stroke_neuro_tpa.csn_vitals_blood_glucose
                     = _case_template_telestroke_notpa.ctt_vitals_blood_glucose
                     = _case_template_stroke_tpa.cts_vitals_blood_glucose
                     = template.cts_vitals_blood_glucose;

                    _case_template_stroke_neuro_tpa.csn_acute_stroke
                        = _case_template_stroke_notpa.ctn_impression
                   = _case_template_stroke_neuro_tpa.csn_impression
                        = _case_template_stroke_tpa.cts_acute_stroke
                        = template.cts_acute_stroke;

                    _case_template_stroke_neuro_tpa.csn_tpa_bolus_complications
                       = _case_template_stroke_tpa.cts_tpa_bolus_complications
                       = template.cts_tpa_bolus_complications;

                    _case_template_stroke_neuro_tpa.csn_tpa_bolus_complications_text
                       = _case_template_stroke_tpa.cts_tpa_bolus_complications_text
                       = template.cts_tpa_bolus_complications_text;

                    _case_template_stroke_neuro_tpa.csn_verbal_consent
                       = _case_template_stroke_tpa.cts_verbal_consent
                       = template.cts_verbal_consent;

                    _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                        = _case_template_stroke_notpa.ctn_patient_family_cosulted
                   = _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                        = _case_template_stroke_tpa.cts_patient_family_cosulted
                        = template.cts_patient_family_cosulted;

                    //TCARE-550
                    _case_template_stroke_notpa.ctn_critical_care_was_provided
                    = _case_template_stroke_neuro_tpa.csn_critical_care_was_provided
                    = _case_template_telestroke_notpa.ctt_critical_care_was_provided
                    = _case_template_stroke_tpa.cts_critical_care_was_provided
                    = template.cts_critical_care_was_provided;

                    _case_template_stroke_notpa.ctn_critical_care_minutes
                   = _case_template_stroke_neuro_tpa.csn_critical_care_minutes
                   = _case_template_telestroke_notpa.ctt_critical_care_minutes
                   = _case_template_stroke_tpa.cts_critical_care_minutes
                   = template.cts_critical_care_minutes;
                    #endregion

                    #region Dates

                    _case_template_stroke_tpa.cts_modified_by = _template_modified_by;
                    _case_template_stroke_tpa.cts_modfied_by_name = _template_modified_by_name;
                    _case_template_stroke_tpa.cts_modified_date = _template_created_or_modified_Date;
                    #endregion
                }
                else if (model.case_template_stroke_neuro_tpa != null)
                {
                    var template = model.case_template_stroke_neuro_tpa;

                    #region  Fields in template only
                    _case_template_stroke_neuro_tpa.csn_additional_recomendations = template.csn_additional_recomendations;
                    #endregion

                    #region Fields
                    //Added By Axim
                    _case_template_stroke_neuro_tpa.csn_family_consent_available
                      = _case_template_stroke_notpa.ctn_family_consent_available
                      = _case_template_telestroke_notpa.ctt_family_consent_available
                     = _case_template_stroke_tpa.cts_family_consent_available
                     = template.csn_family_consent_available;

                    _case_template_stroke_neuro_tpa.csn_PMH
                      = _case_template_stroke_notpa.ctn_PMH
                      = _case_template_telestroke_notpa.ctt_PMH
                     = _case_template_stroke_tpa.cts_PMH
                     = template.csn_PMH;

                    _case_template_stroke_neuro_tpa.csn_anticoagulant_use
                      = _case_template_stroke_notpa.ctn_anticoagulant_use
                      = _case_template_telestroke_notpa.ctt_anticoagulant_use
                     = _case_template_stroke_tpa.cts_anticoagulant_use
                     = template.csn_anticoagulant_use;

                    _case_template_stroke_neuro_tpa.csn_anticoagulant_use_text
                     = _case_template_stroke_notpa.ctn_anticoagulant_use_text
                     = _case_template_telestroke_notpa.ctt_anticoagulant_use_text
                    = _case_template_stroke_tpa.cts_anticoagulant_use_text
                    = template.csn_anticoagulant_use_text;

                    _case_template_stroke_neuro_tpa.csn_antiplatelet_use
                     = _case_template_stroke_notpa.ctn_antiplatelet_use
                     = _case_template_telestroke_notpa.ctt_antiplatelet_use
                    = _case_template_stroke_tpa.cts_antiplatelet_use
                    = template.csn_antiplatelet_use;

                    _case_template_stroke_neuro_tpa.csn_antiplatelet_use_text
                     = _case_template_stroke_notpa.ctn_antiplatelet_use_text
                     = _case_template_telestroke_notpa.ctt_antiplatelet_use_text
                    = _case_template_stroke_tpa.cts_antiplatelet_use_text
                    = template.csn_antiplatelet_use_text;

                    //Ended By AXIM

                    _case_template_stroke_notpa.ctn_impression_text
                    = _case_template_stroke_neuro_tpa.csn_impression
                    = _case_template_telestroke_notpa.ctt_impression_text
                    = _case_template_stroke_tpa.cts_impression
                    = template.csn_impression;

                    _case_template_stroke_notpa.ctn_impression
                    = _case_template_telestroke_notpa.ctt_impression
                    = _case_template_stroke_tpa.cts_impression
                    = template.csn_acute_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke
                       = _case_template_stroke_neuro_tpa.csn_mechanism_stroke
                       = _case_template_telestroke_notpa.ctt_mechanism_stroke
                       = _case_template_stroke_tpa.cts_mechanism_stroke
                       = template.csn_mechanism_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke_text
                      = _case_template_stroke_neuro_tpa.csn_mechanism_stroke_text
                      = _case_template_telestroke_notpa.ctt_mechanism_stroke_text
                      = _case_template_stroke_tpa.cts_mechanism_stroke_text
                       = template.csn_mechanism_stroke_text;

                    _case_template_stroke_notpa.ctn_vitals_bp
                     = _case_template_stroke_neuro_tpa.csn_vitals_bp
                     = _case_template_telestroke_notpa.ctt_vitals_bp
                     = _case_template_stroke_tpa.cts_vitals_bp
                      = template.csn_vitals_bp;

                    _case_template_stroke_notpa.ctn_vitals_pulse
                     = _case_template_stroke_neuro_tpa.csn_vitals_pulse
                     = _case_template_telestroke_notpa.ctt_vitals_pulse
                     = _case_template_stroke_tpa.cts_vitals_pulse
                      = template.csn_vitals_pulse;

                    _case_template_stroke_notpa.ctn_vitals_blood_glucose
                     = _case_template_stroke_neuro_tpa.csn_vitals_blood_glucose
                     = _case_template_telestroke_notpa.ctt_vitals_blood_glucose
                     = _case_template_stroke_tpa.cts_vitals_blood_glucose
                     = template.csn_vitals_blood_glucose;


                    _case_template_stroke_neuro_tpa.csn_acute_stroke
                        = _case_template_stroke_neuro_tpa.csn_impression
                        = _case_template_stroke_notpa.ctn_impression
                        = _case_template_telestroke_notpa.ctt_impression
                       = _case_template_stroke_tpa.cts_acute_stroke
                       = template.csn_acute_stroke;

                    _case_template_stroke_neuro_tpa.csn_tpa_bolus_complications
                       = _case_template_stroke_tpa.cts_tpa_bolus_complications
                       = template.csn_tpa_bolus_complications;

                    _case_template_stroke_neuro_tpa.csn_tpa_bolus_complications_text
                       = _case_template_stroke_tpa.cts_tpa_bolus_complications_text
                       = template.csn_tpa_bolus_complications_text;



                    _case_template_stroke_neuro_tpa.csn_verbal_consent
                       = _case_template_stroke_tpa.cts_verbal_consent
                       = template.csn_verbal_consent;


                    _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                   = _case_template_stroke_notpa.ctn_patient_family_cosulted
                   = _case_template_telestroke_notpa.ctt_patient_family_cosulted
                  = _case_template_stroke_tpa.cts_patient_family_cosulted
                  = template.csn_patient_family_cosulted;


                    //TCARE-550
                    _case_template_stroke_notpa.ctn_critical_care_was_provided
                    = _case_template_stroke_neuro_tpa.csn_critical_care_was_provided
                    = _case_template_telestroke_notpa.ctt_critical_care_was_provided
                    = _case_template_stroke_tpa.cts_critical_care_was_provided
                    = template.csn_critical_care_was_provided;

                    _case_template_stroke_notpa.ctn_critical_care_minutes
                   = _case_template_stroke_neuro_tpa.csn_critical_care_minutes
                   = _case_template_telestroke_notpa.ctt_critical_care_minutes
                   = _case_template_stroke_tpa.cts_critical_care_minutes
                   = template.csn_critical_care_minutes;

                    #endregion

                    #region Dates

                    _case_template_stroke_neuro_tpa.csn_modified_by = _template_modified_by;
                    _case_template_stroke_neuro_tpa.csn_modfied_by_name = _template_modified_by_name;
                    _case_template_stroke_neuro_tpa.csn_modified_date = _template_created_or_modified_Date;

                    #endregion
                }
                else if (model.case_template_stroke_notpa != null)
                {
                    var template = model.case_template_stroke_notpa;
                    #region Fields

                    #region Fields in template only

                    _case_template_stroke_notpa.ctn_imaging_studies_recommedned = template.ctn_imaging_studies_recommedned;
                    _case_template_stroke_notpa.ctn_imaging_studies_recommedned_text = template.ctn_imaging_studies_recommedned_text;
                    _case_template_stroke_notpa.ctn_therapies = template.ctn_therapies;
                    _case_template_stroke_notpa.ctn_dysphaghia_screen = template.ctn_dysphaghia_screen;
                    _case_template_stroke_notpa.ctn_dvt_prophylaxis = template.ctn_dvt_prophylaxis;
                    _case_template_stroke_notpa.ctn_dvt_prophylaxis_text = template.ctn_dvt_prophylaxis_text;
                    _case_template_stroke_notpa.ctn_lipid_panel_obtained = template.ctn_lipid_panel_obtained;
                    _case_template_stroke_notpa.ctn_disposition = template.ctn_disposition;

                    #endregion

                    //Added By Axim
                    _case_template_stroke_notpa.ctn_family_consent_available
                        = _case_template_stroke_neuro_tpa.csn_family_consent_available
                        = _case_template_stroke_tpa.cts_family_consent_available
                        = _case_template_telestroke_notpa.ctt_family_consent_available
                        = template.ctn_family_consent_available;

                    _case_template_stroke_notpa.ctn_PMH
                        = _case_template_stroke_neuro_tpa.csn_PMH
                        = _case_template_stroke_tpa.cts_PMH
                        = _case_template_telestroke_notpa.ctt_PMH
                        = template.ctn_PMH;

                    _case_template_stroke_notpa.ctn_antiplatelet_use
                        = _case_template_stroke_neuro_tpa.csn_antiplatelet_use
                        = _case_template_stroke_tpa.cts_antiplatelet_use
                        = _case_template_telestroke_notpa.ctt_antiplatelet_use
                        = template.ctn_antiplatelet_use;

                    _case_template_stroke_notpa.ctn_antiplatelet_use_text
                        = _case_template_stroke_neuro_tpa.csn_antiplatelet_use_text
                        = _case_template_stroke_tpa.cts_antiplatelet_use_text
                        = _case_template_telestroke_notpa.ctt_antiplatelet_use_text
                        = template.ctn_antiplatelet_use_text;

                    _case_template_stroke_notpa.ctn_anticoagulant_use
                        = _case_template_stroke_neuro_tpa.csn_anticoagulant_use
                        = _case_template_stroke_tpa.cts_anticoagulant_use
                        = _case_template_telestroke_notpa.ctt_anticoagulant_use
                        = template.ctn_anticoagulant_use;

                    _case_template_stroke_notpa.ctn_anticoagulant_use_text
                        = _case_template_stroke_neuro_tpa.csn_anticoagulant_use_text
                        = _case_template_stroke_tpa.cts_anticoagulant_use_text
                        = _case_template_telestroke_notpa.ctt_anticoagulant_use_text
                        = template.ctn_anticoagulant_use_text;

                    //ended By Axim
                    _case_template_stroke_notpa.ctn_impression
                                    = _case_template_stroke_neuro_tpa.csn_impression
                                    = _case_template_telestroke_notpa.ctt_impression
                                    = _case_template_stroke_tpa.cts_impression
                                    = template.ctn_impression;


                    _case_template_stroke_neuro_tpa.csn_acute_stroke
                                 = _case_template_stroke_tpa.cts_acute_stroke
                                 = template.ctn_impression;

                    _case_template_stroke_notpa.ctn_mechanism_stroke
                       = _case_template_stroke_neuro_tpa.csn_mechanism_stroke
                       = _case_template_telestroke_notpa.ctt_mechanism_stroke
                       = _case_template_stroke_tpa.cts_mechanism_stroke
                       = template.ctn_mechanism_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke_text
                      = _case_template_stroke_neuro_tpa.csn_mechanism_stroke_text
                      = _case_template_telestroke_notpa.ctt_mechanism_stroke_text
                      = _case_template_stroke_tpa.cts_mechanism_stroke_text
                       = template.ctn_mechanism_stroke_text;

                    _case_template_stroke_notpa.ctn_vitals_bp
                     = _case_template_stroke_neuro_tpa.csn_vitals_bp
                     = _case_template_telestroke_notpa.ctt_vitals_bp
                     = _case_template_stroke_tpa.cts_vitals_bp
                      = template.ctn_vitals_bp;

                    _case_template_stroke_notpa.ctn_vitals_pulse
                     = _case_template_stroke_neuro_tpa.csn_vitals_pulse
                     = _case_template_telestroke_notpa.ctt_vitals_pulse
                     = _case_template_stroke_tpa.cts_vitals_pulse
                      = template.ctn_vitals_pulse;

                    _case_template_stroke_notpa.ctn_vitals_blood_glucose
                     = _case_template_stroke_neuro_tpa.csn_vitals_blood_glucose
                     = _case_template_telestroke_notpa.ctt_vitals_blood_glucose
                     = _case_template_stroke_tpa.cts_vitals_blood_glucose
                     = template.ctn_vitals_blood_glucose;

                    _case_template_stroke_notpa.ctn_impression_text
                        = _case_template_stroke_neuro_tpa.csn_impression
                        = _case_template_stroke_tpa.cts_impression
                        = _case_template_telestroke_notpa.ctt_impression_text
                        = template.ctn_impression_text;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned
                       = _case_template_telestroke_notpa.ctt_antiplatelet_therapy_recommedned
                       = template.ctn_antiplatelet_therapy_recommedned;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned_text
                       = _case_template_telestroke_notpa.ctt_antiplatelet_therapy_recommedned_text
                       = template.ctn_antiplatelet_therapy_recommedned_text;

                    _case_template_stroke_notpa.ctn_sign_out
                       = _case_template_telestroke_notpa.ctt_sign_out
                       = template.ctn_sign_out;

                    _case_template_stroke_notpa.ctn_patient_family_cosulted
                        = _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                        = _case_template_stroke_tpa.cts_patient_family_cosulted
                        = _case_template_telestroke_notpa.ctt_patient_family_cosulted
                        = template.ctn_patient_family_cosulted;


                    //TCARE-550
                    _case_template_stroke_notpa.ctn_critical_care_was_provided
                    = _case_template_stroke_neuro_tpa.csn_critical_care_was_provided
                    = _case_template_telestroke_notpa.ctt_critical_care_was_provided
                    = _case_template_stroke_tpa.cts_critical_care_was_provided
                    = template.ctn_critical_care_was_provided;

                    _case_template_stroke_notpa.ctn_critical_care_minutes
                   = _case_template_stroke_neuro_tpa.csn_critical_care_minutes
                   = _case_template_telestroke_notpa.ctt_critical_care_minutes
                   = _case_template_stroke_tpa.cts_critical_care_minutes
                   = template.ctn_critical_care_minutes;
                    #endregion

                    #region Dates

                    _case_template_stroke_notpa.ctn_modified_by = _template_modified_by;
                    _case_template_stroke_notpa.ctn_modfied_by_name = _template_modified_by_name;
                    _case_template_stroke_notpa.ctn_modified_date = _template_created_or_modified_Date;
                    #endregion
                }
                else if (model.case_template_telestroke_notpa != null)
                {
                    var template = model.case_template_telestroke_notpa;
                    #region Fields

                    #region  Fields in templates 

                    _case_template_telestroke_notpa.ctt_routine_consultation = template.ctt_routine_consultation;

                    #endregion

                    //Added By Axim
                    _case_template_stroke_notpa.ctn_family_consent_available
                     = _case_template_stroke_neuro_tpa.csn_family_consent_available
                     = _case_template_telestroke_notpa.ctt_family_consent_available
                     = _case_template_stroke_tpa.cts_family_consent_available
                      = template.ctt_family_consent_available;

                    _case_template_stroke_notpa.ctn_PMH
                     = _case_template_stroke_neuro_tpa.csn_PMH
                     = _case_template_telestroke_notpa.ctt_PMH
                     = _case_template_stroke_tpa.cts_PMH
                      = template.ctt_PMH;

                    _case_template_stroke_notpa.ctn_anticoagulant_use
                     = _case_template_stroke_neuro_tpa.csn_anticoagulant_use
                     = _case_template_telestroke_notpa.ctt_anticoagulant_use
                     = _case_template_stroke_tpa.cts_anticoagulant_use
                      = template.ctt_anticoagulant_use;

                    _case_template_stroke_notpa.ctn_anticoagulant_use_text
                     = _case_template_stroke_neuro_tpa.csn_anticoagulant_use_text
                     = _case_template_telestroke_notpa.ctt_anticoagulant_use_text
                     = _case_template_stroke_tpa.cts_anticoagulant_use_text
                      = template.ctt_anticoagulant_use_text;

                    _case_template_stroke_notpa.ctn_antiplatelet_use
                     = _case_template_stroke_neuro_tpa.csn_antiplatelet_use
                     = _case_template_telestroke_notpa.ctt_antiplatelet_use
                     = _case_template_stroke_tpa.cts_antiplatelet_use
                      = template.ctt_antiplatelet_use;

                    _case_template_stroke_notpa.ctn_antiplatelet_use_text
                     = _case_template_stroke_neuro_tpa.csn_antiplatelet_use_text
                     = _case_template_telestroke_notpa.ctt_antiplatelet_use_text
                     = _case_template_stroke_tpa.cts_antiplatelet_use_text
                      = template.ctt_antiplatelet_use_text;

                    //Ended By Axim
                    _case_template_stroke_notpa.ctn_impression
                                    = _case_template_stroke_neuro_tpa.csn_impression
                                    = _case_template_stroke_tpa.cts_acute_stroke
                                    = _case_template_telestroke_notpa.ctt_impression
                                     = _case_template_stroke_tpa.cts_impression
                                    = template.ctt_impression;

                    _case_template_stroke_notpa.ctn_mechanism_stroke
                       = _case_template_stroke_neuro_tpa.csn_mechanism_stroke
                       = _case_template_telestroke_notpa.ctt_mechanism_stroke
                       = _case_template_stroke_tpa.cts_mechanism_stroke
                       = template.ctt_mechanism_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke_text
                      = _case_template_stroke_neuro_tpa.csn_mechanism_stroke_text
                      = _case_template_telestroke_notpa.ctt_mechanism_stroke_text
                      = _case_template_stroke_tpa.cts_mechanism_stroke_text
                       = template.ctt_mechanism_stroke_text;

                    _case_template_stroke_notpa.ctn_vitals_bp
                     = _case_template_stroke_neuro_tpa.csn_vitals_bp
                     = _case_template_telestroke_notpa.ctt_vitals_bp
                     = _case_template_stroke_tpa.cts_vitals_bp
                      = template.ctt_vitals_bp;

                    _case_template_stroke_notpa.ctn_vitals_pulse
                     = _case_template_stroke_neuro_tpa.csn_vitals_pulse
                     = _case_template_telestroke_notpa.ctt_vitals_pulse
                     = _case_template_stroke_tpa.cts_vitals_pulse
                      = template.ctt_vitals_pulse;

                    _case_template_stroke_notpa.ctn_vitals_blood_glucose
                     = _case_template_stroke_neuro_tpa.csn_vitals_blood_glucose
                     = _case_template_telestroke_notpa.ctt_vitals_blood_glucose
                     = _case_template_stroke_tpa.cts_vitals_blood_glucose
                     = template.ctt_vitals_blood_glucose;


                    _case_template_stroke_notpa.ctn_impression_text
                        = _case_template_stroke_neuro_tpa.csn_impression
                        = _case_template_stroke_tpa.cts_impression
                       = _case_template_telestroke_notpa.ctt_impression_text
                       = template.ctt_impression_text;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned
                       = _case_template_telestroke_notpa.ctt_antiplatelet_therapy_recommedned
                       = template.ctt_antiplatelet_therapy_recommedned;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned_text
                       = _case_template_telestroke_notpa.ctt_antiplatelet_therapy_recommedned_text
                       = template.ctt_antiplatelet_therapy_recommedned_text;

                    _case_template_stroke_notpa.ctn_sign_out
                       = _case_template_telestroke_notpa.ctt_sign_out
                       = template.ctt_sign_out;

                    _case_template_stroke_notpa.ctn_patient_family_cosulted
                     = _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                     = _case_template_telestroke_notpa.ctt_patient_family_cosulted
                     = _case_template_stroke_tpa.cts_patient_family_cosulted
                      = template.ctt_patient_family_cosulted;

                    //TCARE-550
                    _case_template_stroke_notpa.ctn_critical_care_was_provided
                    = _case_template_stroke_neuro_tpa.csn_critical_care_was_provided
                    = _case_template_telestroke_notpa.ctt_critical_care_was_provided
                    = _case_template_stroke_tpa.cts_critical_care_was_provided
                    = template.ctt_critical_care_was_provided;

                    _case_template_stroke_notpa.ctn_critical_care_minutes
                   = _case_template_stroke_neuro_tpa.csn_critical_care_minutes
                   = _case_template_telestroke_notpa.ctt_critical_care_minutes
                   = _case_template_stroke_tpa.cts_critical_care_minutes
                   = template.ctt_critical_care_minutes;
                    #endregion

                    #region Dates

                    _case_template_telestroke_notpa.ctt_modified_by = _template_modified_by;
                    _case_template_telestroke_notpa.ctt_modfied_by_name = _template_modified_by_name;
                    _case_template_telestroke_notpa.ctt_modified_date = _template_created_or_modified_Date;

                    #endregion
                }
                else if (model.case_template_statconsult != null)
                {
                    var template = model.case_template_statconsult;
                    #region Fields

                    #region  Fields in templates 

                    _case_Template_Statconsult.ctt_routine_consultation = template.ctt_routine_consultation;

                    #endregion

                    _case_template_stroke_notpa.ctn_impression
                                    = _case_template_stroke_neuro_tpa.csn_impression
                                    = _case_template_stroke_tpa.cts_acute_stroke
                                    = _case_Template_Statconsult.ctt_impression
                                     = _case_template_stroke_tpa.cts_impression
                                    = template.ctt_impression;

                    _case_template_stroke_notpa.ctn_mechanism_stroke
                       = _case_template_stroke_neuro_tpa.csn_mechanism_stroke
                       = _case_Template_Statconsult.ctt_mechanism_stroke
                       = _case_template_stroke_tpa.cts_mechanism_stroke
                       = template.ctt_mechanism_stroke;

                    _case_template_stroke_notpa.ctn_mechanism_stroke_text
                      = _case_template_stroke_neuro_tpa.csn_mechanism_stroke_text
                      = _case_Template_Statconsult.ctt_mechanism_stroke_text
                      = _case_template_stroke_tpa.cts_mechanism_stroke_text
                       = template.ctt_mechanism_stroke_text;

                    _case_template_stroke_notpa.ctn_vitals_bp
                     = _case_template_stroke_neuro_tpa.csn_vitals_bp
                     = _case_Template_Statconsult.ctt_vitals_bp
                     = _case_template_stroke_tpa.cts_vitals_bp
                      = template.ctt_vitals_bp;

                    _case_template_stroke_notpa.ctn_vitals_pulse
                     = _case_template_stroke_neuro_tpa.csn_vitals_pulse
                     = _case_Template_Statconsult.ctt_vitals_pulse
                     = _case_template_stroke_tpa.cts_vitals_pulse
                      = template.ctt_vitals_pulse;

                    _case_template_stroke_notpa.ctn_vitals_blood_glucose
                     = _case_template_stroke_neuro_tpa.csn_vitals_blood_glucose
                     = _case_Template_Statconsult.ctt_vitals_blood_glucose
                     = _case_template_stroke_tpa.cts_vitals_blood_glucose
                     = template.ctt_vitals_blood_glucose;


                    _case_template_stroke_notpa.ctn_impression_text
                        = _case_template_stroke_neuro_tpa.csn_impression
                        = _case_template_stroke_tpa.cts_impression
                       = _case_Template_Statconsult.ctt_impression_text
                       = template.ctt_impression_text;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned
                       = _case_Template_Statconsult.ctt_antiplatelet_therapy_recommedned
                       = template.ctt_antiplatelet_therapy_recommedned;

                    _case_template_stroke_notpa.ctn_antiplatelet_therapy_recommedned_text
                       = _case_Template_Statconsult.ctt_antiplatelet_therapy_recommedned_text
                       = template.ctt_antiplatelet_therapy_recommedned_text;

                    _case_template_stroke_notpa.ctn_sign_out
                       = _case_Template_Statconsult.ctt_sign_out
                       = template.ctt_sign_out;

                    _case_template_stroke_notpa.ctn_patient_family_cosulted
                     = _case_template_stroke_neuro_tpa.csn_patient_family_cosulted
                     = _case_Template_Statconsult.ctt_patient_family_cosulted
                     = _case_template_stroke_tpa.cts_patient_family_cosulted
                      = template.ctt_patient_family_cosulted;

                    //TCARE-550
                    _case_template_stroke_notpa.ctn_critical_care_was_provided
                    = _case_template_stroke_neuro_tpa.csn_critical_care_was_provided
                    = _case_Template_Statconsult.ctt_critical_care_was_provided
                    = _case_template_stroke_tpa.cts_critical_care_was_provided
                    = template.ctt_critical_care_was_provided;

                    _case_template_stroke_notpa.ctn_critical_care_minutes
                   = _case_template_stroke_neuro_tpa.csn_critical_care_minutes
                   = _case_Template_Statconsult.ctt_critical_care_minutes
                   = _case_template_stroke_tpa.cts_critical_care_minutes
                   = template.ctt_critical_care_minutes;
                    #endregion

                    #region Dates

                    _case_Template_Statconsult.ctt_modified_by = _template_modified_by;
                    _case_Template_Statconsult.ctt_modfied_by_name = _template_modified_by_name;
                    _case_Template_Statconsult.ctt_modified_date = _template_created_or_modified_Date;

                    #endregion
                }

                model.case_template_telestroke_notpa = _case_template_telestroke_notpa;
                model.case_template_stroke_notpa = _case_template_stroke_notpa;
                model.case_template_stroke_neuro_tpa = _case_template_stroke_neuro_tpa;
                model.case_template_stroke_tpa = _case_template_stroke_tpa;
                model.case_template_statconsult = _case_Template_Statconsult;


                #endregion
                var _type = _caseService.CheckTemplateType(model.cas_key, model.cas_fac_key, model.cas_metric_tpa_consult, model.cas_ctp_key == CaseType.StrokeAlert.ToInt());

                model.TemplateEntityType = _type.ToInt();

                SetTemplateData(model);


                string view = null;
                if (_type == EntityTypes.StrokeAlertTemplateTpa)
                {
                    view = RenderPartialViewToString("Templates/Forms/_StrokeAlertTPATemplateForm", model);
                }
                else if (_type == EntityTypes.NeuroStrokeAlertTemplateTpa)
                {
                    view = RenderPartialViewToString("Templates/Forms/_StrokeNeuroTPATemplateForm", model);
                }
                else if (_type == EntityTypes.StrokeAlertTemplateNoTpa)
                {
                    view = RenderPartialViewToString("Templates/Forms/_StrokeAlertNoTPATemplateForm", model);
                }
                else if (_type == EntityTypes.StrokeAlertTemplateNoTpaTeleStroke)
                {
                    view = RenderPartialViewToString("Templates/Forms/_TeleStrokeNoTPATemplateForm", model);
                }
                else if (_type == EntityTypes.StateAlertTemplate)
                {
                    view = RenderPartialViewToString("Templates/Forms/_TeleStateTemplate", model);
                }
                return Json(new { result = true, view = view, template = model.TemplateEntityType });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }

        }
        #endregion

        #region Case Review Template

        [HttpPost]
        public ActionResult GenerateCaseReviewTemplate(int id)
        {
            try
            {
                var model = new @case();
                model = _caseService.GetDetails(id);
                if (model.facility != null)
                {
                    if (!string.IsNullOrWhiteSpace(model.facility.qps_number))
                    {
                        var GetQPSName = _adminService.GetAspNetUsers().Where(m => m.Id == model.facility.qps_number).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                        if (GetQPSName != null)
                        {
                            ViewBag.QPSName = GetQPSName.FirstName + " " + GetQPSName.LastName;
                        }
                        else
                        {
                            ViewBag.QPSName = "";
                        }

                    }
                }
                else
                {
                    ViewBag.QPSName = "";
                }
                var rcamodel = new List<rca_counter_measure>();
                rcamodel = _rootCauseService.GetDetail(id);
                var types = new List<int>()
                    {
                        UclTypes.TpaDelay.ToInt(),
                    };
                var uclDataList = _lookUpService.GetUclData(types)
                                    .Where(m => m.ucd_is_active)
                                    .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                                    .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).ToList();
                string tpadelaysreason = uclDataList.Where(x => x.ucd_key == model.cas_metric_tpaDelay_key).Select(y => y.ucd_description).FirstOrDefault();
                ViewBag.tpareason = tpadelaysreason;
                //ViewBag.Demographics = _caseReviewService.GetDemographics(model);
                //result = RenderPartialViewToString("_CaseMetricResponse", model);

                var result = RenderPartialViewToString("Templates/CaseReview/_caseReview", model);
                return Json(new { success = true, data = result, showEditor = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult LoadCaseReviewTemplate(int caseKey)
        {
            try
            {
                var reviewtemplate = _casereviewTemplateService.GetDetails(caseKey);
                if (reviewtemplate != null)
                {
                    var result = reviewtemplate.crt_template_html;
                    return Json(new { success = true, data = result, showEditor = (reviewtemplate.crt_finalize_date.HasValue ? false : true) });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveReviewTemplate(int cas_key, bool is_finalized, string ptemplateData)
        {
            try
            {
                #region Check authorized user to save case
                if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                {
                    var caseModel = _caseService.GetDetailsWithoutTimeConversion(cas_key);
                    if (caseModel.cas_phy_key != User.Identity.GetUserId())
                    {
                        return Json(new { success = false, data = "Access Denied! <br/> You are not authorized to save or finalize this case template. This case is reassigned." });
                    }
                }
                #endregion
                var templateData = Functions.DecodeFrom64(ptemplateData);
                var template = _casereviewTemplateService.GetDetails(cas_key);
                if (template == null)
                {
                    var obj = new case_review_template
                    {
                        crt_cas_key = cas_key,
                        crt_template_html = templateData,
                        crt_created_by = loggedInUser.Id,
                        crt_created_by_name = loggedInUser.FullName,
                        crt_created_date = DateTime.Now.ToEST()
                    };
                    if (is_finalized)
                        obj.crt_finalize_date = DateTime.Now.ToEST();
                    _casereviewTemplateService.Create(obj);
                }
                else
                {
                    template.crt_modified_by = loggedInUser.Id;
                    template.crt_modified_by_name = loggedInUser.FullName;
                    template.crt_modified_date = DateTime.Now.ToEST();
                    template.crt_template_html = templateData;
                    if (is_finalized)
                        template.crt_finalize_date = DateTime.Now.ToEST();
                    _casereviewTemplateService.Edit(template);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occured while processing your request, please try later" });
            }
        }

        #endregion


        private void SetTemplateData(@case model)
        {
            try
            {
                ViewBag.NIHSSList = _nihStrokeScaleService.GetAllQuestions().ToList();

                if (model.TemplateEntityType == EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt())
                {
                    ViewBag.StrokeTemplateRecommendations = _uclService.GetUclData(UclTypes.StrokeTemplateRecommendations).ToList();
                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)model.TemplateEntityType).Select(m => m.nsa_nss_key).ToList();
                }
                else if (model.TemplateEntityType == EntityTypes.StrokeAlertTemplateTpa.ToInt())
                {
                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)model.TemplateEntityType).Select(m => m.nsa_nss_key).ToList();
                }
                else if (model.TemplateEntityType == EntityTypes.StrokeAlertTemplateNoTpa.ToInt())
                {
                    StrokeAlertNoTpaData(model);
                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StrokeAlertTemplateNoTpa).Select(m => m.nsa_nss_key).ToList();

                }
                else if (model.TemplateEntityType == EntityTypes.StrokeAlertTemplateNoTpaTeleStroke.ToInt())
                {
                    ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.AntiplateletTherapyRecommendedNoTpa)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    //Added By Axim
                    ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutNoTpa)
                                  .Select(m => new { m.ucd_key, m.ucd_title })
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StrokeAlertTemplateNoTpaTeleStroke).Select(m => m.nsa_nss_key).ToList();
                }
                else if (model.TemplateEntityType == EntityTypes.StateAlertTemplate.ToInt())
                {
                    //Added By Axim
                    ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.StateConsultTemplate)
                                  .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.AntiplateletTherapyRecommendedStatConsult = _uclService.GetUclData(UclTypes.ImagingStudiesStatConsult)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultTherapies = _uclService.GetUclData(UclTypes.TherapiesState)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutStat)
                              .Select(m => new { m.ucd_key, m.ucd_title })
                              .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultOtherWork = _uclService.GetUclData(UclTypes.OtherWorkUp)
                                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    model.TemplateEntityType = EntityTypes.StateAlertTemplate.ToInt();
                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StateAlertTemplate).Select(m => m.nsa_nss_key).ToList();
                    //ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.StateConsultTemplate)
                    //              .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                    //              .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    //ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutStat)
                    //          .Select(m => new { m.ucd_key, m.ucd_title })
                    //          .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                    //ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StateAlertTemplate).Select(m => m.nsa_nss_key).ToList();            
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private async Task SendCaseToPhysician(@case model)
        {
            try
            {
                if (model.cas_cst_key == CaseStatus.WaitingToAccept.ToInt() && model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
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
                    new WebSocketEventHandler().CallJSMethod(model.cas_phy_key, new SocketResponseModel { MethodName = "showPhysicianNewCasePopup_def", Data = new List<object> { model.cas_key, true } });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

        }

        private bool IsUserOnline(string phy_key)
        {
            return OnlineUserIds.Where(m => m == phy_key).Any();

        }

        #region private-methods
        private void InitPhyscianPopupAlert(@case model)
        {
            try
            {
                var busyUsers = _caseAssignHistoryService.GetLog(PhysicianCaseAssignQueue.WaitingForAction)
                                                       .Where(m => m.cah_request_sent_time != null && m.cah_phy_key != null)
                                                       .ToList()
                                                       .Where(m => (DateTime.Now.ToEST() - m.cah_request_sent_time.Value).TotalMinutes <= 2)
                                                       .Select(m => m.cah_phy_key)
                                                       .ToList();

                // extracting the list of currently on line users from signal r hub
                var onlineUsers = PhysicianCasePopupHub.ConnectedUsers
                                                       .Where(m => busyUsers.Contains(m.UserId) == false)
                                                       .Select(m => m.UserId)
                                                       .Distinct()
                                                       .ToList();


                // getting the list of physician and filtering the on line users based of the mentioned criteria
                var physicians = _facilityPhysicianService.GetPhysiciansByFacility(model.cas_fac_key, true)
                                                          .Where(m => onlineUsers.Contains(m.Id))
                                                          .Where(m => m.Id != model.cas_created_by)
                                                          .ToList();
                if (physicians.Count() > 0)
                {

                    // adding the references of cas_key so we can group the physician pop up request based on 
                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                    int order = 1;
                    physicians.ForEach(phy =>
                    {

                        _caseAssignHistoryService.Create(new case_assign_history
                        {
                            cah_cas_key = model.cas_key,
                            cah_phy_key = phy.Id,
                            cah_action = PhysicianCaseAssignQueue.InQueue.ToString(),
                            cah_request_sent_time = null,
                            cah_created_date = DateTime.Now.ToEST(),
                            cah_created_date_utc = DateTime.UtcNow,
                            cah_created_by = User.Identity.GetUserId(),
                            cah_sort_order = order,
                            cah_is_active = true,
                            cah_request_sent_time_utc = null

                        });

                        order++;

                    });


                    #region Adding Entry for Navigator 

                    _caseAssignHistoryService.Create(new case_assign_history
                    {
                        cah_cas_key = model.cas_key,
                        cah_phy_key = model.cas_created_by,
                        cah_action = PhysicianCaseAssignQueue.InQueue.ToString(),
                        cah_request_sent_time = null,
                        cah_created_date = DateTime.Now.ToEST(),
                        cah_created_date_utc = DateTime.UtcNow,
                        cah_created_by = User.Identity.GetUserId(),
                        cah_sort_order = order,
                        cah_is_active = true,
                        cah_request_sent_time_utc = null,

                    });

                    #endregion

                    // sending the pop up to first on line physician based on the defined sort order
                    var initialPhysicians = PhysicianCasePopupHub.ConnectedUsers
                                                               .Where(m => m.UserId == physicians.FirstOrDefault().Id)
                                                               .ToList();
                    initialPhysicians.ForEach(initialPhysician =>
                    {
                        hubContext.Clients.Client(initialPhysician.ConnectionId).showPhysicianCasePopup(model.cas_key);
                    });

                    if (initialPhysicians.Count() > 0)
                    {
                        var firstPhysician = _caseAssignHistoryService.GetInQueuePhysicians(model.cas_key).FirstOrDefault();
                        firstPhysician.cah_request_sent_time = DateTime.Now.ToEST();
                        firstPhysician.cah_request_sent_time_utc = DateTime.UtcNow;
                        firstPhysician.cah_action = PhysicianCaseAssignQueue.WaitingForAction.ToString();
                        _caseAssignHistoryService.Edit(firstPhysician);
                        PhysicianCasePopupHub.LinkCaseWithUser(model.cas_key, firstPhysician.cah_phy_key); // added to handle disconnection scenario

                        model.cas_cst_key = CaseStatus.WaitingToAccept.ToInt();
                        model.cas_status_assign_date = DateTime.Now.ToEST();
                        //model.cas_history_physician_initial = 
                        model.cas_history_physician_initial = _caseService.GetCaseInitials(model.cas_key);
                        _caseService.EditCaseOnly(model);

                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

        }
        private void UpdateStatusOfPreviousPhy(@case model, @case dbModel)
        {
            try
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
                                        new WebSocketEventHandler().CallJSMethod(physician.Id, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus" });
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
        private void SetStatus(int id, int? cas_key, string userId, string comments)
        {
            try
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
                    new WebSocketEventHandler().CallJSMethod(userId, new SocketResponseModel { MethodName = "refreshCurrentPhyStatus", Data = dataList });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
        public ActionResult _homehealth()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult GetAllHomeHealth(Kendo.DynamicLinq.DataSourceRequest request)
        {
            try
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
                else if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
                {
                    facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                             .Select(x => x.Facility).ToList();
                }
                //if (!(User.IsInRole(UserRoles.Administrator.ToDescription()) || User.IsInRole(UserRoles.SuperAdmin.ToDescription())))
                //    userId = User.Identity.GetUserId();

                var res = _homeHealthService.GetAll(request, userId, facilities);
                return Json(res, JsonRequestBehavior.AllowGet);

                //string userId = User.Identity.GetUserId();
                //bool isPac = false;
                //if (User.IsInRole(UserRoles.AOC.ToDescription()))
                //    isPac = true;

                //var res = _homeHealthService.GetAll(request, userId, isPac);
                //return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }

        private int SaveNumber(@case model)
        {
            var contact = new contact();
            try
            {
                if (Request.Params["AddNumber"] != null && Request.Params["AddNumber"].ToString() == "1" && !string.IsNullOrEmpty(model.cas_ani))
                {
                    contact.cnt_fac_key = model.cas_fac_key;
                    contact.cnt_primary_phone = model.cas_ani;
                    contact.cnt_is_active = true;
                    contact.cnt_created_by = User.Identity.GetUserId();
                    contact.cnt_created_date = DateTime.Now.ToEST();
                    _contactService.Create(contact);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Phone number is invalid.");
            }
            return contact.cnt_key;
        }
        private void CreateTemplateData(@case model)
        {
            try
            {
                ViewBag.NIHSSList = _nihStrokeScaleService.GetAllQuestions().ToList();
                if (model.cas_ctp_key == CaseType.StatConsult.ToInt() || model.cas_ctp_key == CaseType.TransferAlert.ToInt())
                {
                    //Added By Axim
                    ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.StateConsultTemplate)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.AntiplateletTherapyRecommendedStatConsult = _uclService.GetUclData(UclTypes.ImagingStudiesStatConsult)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultTherapies = _uclService.GetUclData(UclTypes.TherapiesState)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutStat)
                                  .Select(m => new { m.ucd_key, m.ucd_title })
                                  .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    ViewBag.StatConsultOtherWork = _uclService.GetUclData(UclTypes.OtherWorkUp)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                    model.TemplateEntityType = EntityTypes.StateAlertTemplate.ToInt();
                    ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StateAlertTemplate).Select(m => m.nsa_nss_key).ToList();
                }

                // For tpa template
                else if (model.cas_metric_tpa_consult && model.cas_ctp_key == CaseType.StrokeAlert.ToInt() && model.facility != null &&
                           model.facility.fac_not_templated_used &&
                           model.facility.facility_contract != null &&
                           (
                           model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                           || model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower())))
                {
                    if (model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                            && model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower()))
                    {
                        ViewBag.StrokeTemplateRecommendations = _uclService.GetUclData(UclTypes.StrokeTemplateRecommendations).ToList();


                        model.TemplateEntityType = EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt();
                        ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)model.TemplateEntityType).Select(m => m.nsa_nss_key).ToList();
                    }
                    else if (model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower()))
                    {
                        model.TemplateEntityType = EntityTypes.StrokeAlertTemplateTpa.ToInt();
                        ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)model.TemplateEntityType).Select(m => m.nsa_nss_key).ToList();
                    }


                }
                // For non tpa template
                else if (!model.cas_metric_tpa_consult && model.cas_ctp_key == CaseType.StrokeAlert.ToInt()
                                //// ticket - 364 point- 11 (below code is commented)
                                //&& (model.cas_patient_type == PatientType.EMS.ToInt() || model.cas_patient_type == PatientType.Inpatient.ToInt()
                                //   || model.cas_patient_type == PatientType.Triage.ToInt()) 
                                && model.facility != null
                             && model.facility.fac_not_templated_used && model.facility.facility_contract != null)
                {
                    if (model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                        && model.facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower()))
                    {
                        StrokeAlertNoTpaData(model);
                        model.TemplateEntityType = EntityTypes.StrokeAlertTemplateNoTpa.ToInt();
                        ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StrokeAlertTemplateNoTpa).Select(m => m.nsa_nss_key).ToList();

                    }
                    else if (model.facility.facility_contract.fct_service_calc.ToLower().Equals(ContractTypes.TeleStroke.ToDescription().ToLower()))
                    {
                        ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.AntiplateletTherapyRecommendedNoTpa)
                                          .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                          .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                        //Added By Axim
                        ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                          .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order }).OrderBy(m => m.ucd_sort_order)
                                          .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                        ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutNoTpa)
                                      .Select(m => new { m.ucd_key, m.ucd_title })
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                        model.TemplateEntityType = EntityTypes.StrokeAlertTemplateNoTpaTeleStroke.ToInt();
                        ViewBag.NIHSSSelectedOptions = _nihStrokeScaleService.GetAnswers(model.cas_key, (int)EntityTypes.StrokeAlertTemplateNoTpaTeleStroke).Select(m => m.nsa_nss_key).ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
        private void StrokeAlertNoTpaData(@case model)
        {
            //Added by Axim
            try
            {
                ViewBag.PMH = _uclService.GetUclData(UclTypes.PMH)
                                       .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                       .OrderBy(m => m.ucd_sort_order)
                                       .ToDictionary(m => m.ucd_key, m => m.ucd_title);
                ViewBag.AntiplateletTherapyRecommended = _uclService.GetUclData(UclTypes.AntiplateletTherapyRecommendedNoTpa)
                                           .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                           .OrderBy(m => m.ucd_sort_order)
                                           .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                ViewBag.ImagingStudiesRecommendedNoTpa = _uclService.GetUclData(UclTypes.ImagingStudiesRecommendedNoTpa)
                                        .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                           .OrderBy(m => m.ucd_sort_order)
                                        .ToDictionary(m => m.ucd_key, m => m.ucd_title);


                ViewBag.DVTProphylaxisNoTpa = _uclService.GetUclData(UclTypes.DVTProphylaxisNoTpa)
                                        .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                           .OrderBy(m => m.ucd_sort_order)
                                        .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                ViewBag.DispositionNoTpa = _uclService.GetUclData(UclTypes.DispositionNoTpa)
                                       .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                           .OrderBy(m => m.ucd_sort_order)
                                       .ToDictionary(m => Convert.ToString(m.ucd_key), m => m.ucd_title);

                ViewBag.SignOutNoTpa = _uclService.GetUclData(UclTypes.SignOutNoTpa)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order })
                                           .OrderBy(m => m.ucd_sort_order)
                                      .ToDictionary(m => m.ucd_key, m => m.ucd_title);



                var tempTherapiesNoTpa = _uclService.GetUclData(UclTypes.TherapiesNoTpa)
                                                    .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order, m.ucd_is_default })
                                                    .OrderBy(m => m.ucd_sort_order)
                                                    .ToList();

                ViewBag.DefaultTherapiesNoTpa = tempTherapiesNoTpa.Where(x => x.ucd_is_default)
                                                                  .Select(x => x.ucd_key)
                                                                  .ToList();

                ViewBag.TherapiesNoTpa = tempTherapiesNoTpa.ToDictionary(m => m.ucd_key, m => m.ucd_title);


                var tempDysphaghiaScreenNoTpa = _uclService.GetUclData(UclTypes.DysphaghiaScreenNoTpa)
                                                           .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_sort_order, m.ucd_is_default })
                                                           .OrderBy(m => m.ucd_sort_order)
                                                           .ToList();

                ViewBag.DefaultDysphaghiaScreenNoTpa = tempDysphaghiaScreenNoTpa.Where(x => x.ucd_is_default)
                                                                                .Select(x => x.ucd_key)
                                                                                .ToList();

                ViewBag.DysphaghiaScreenNoTpa = tempDysphaghiaScreenNoTpa.ToDictionary(m => m.ucd_key, m => m.ucd_title);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

        }

        private void SavenihssData(int cas_key, string SelectedNIHSQuestionResponse)
        {
            try
            {
                if (!string.IsNullOrEmpty(SelectedNIHSQuestionResponse))
                {
                    var responseList = SelectedNIHSQuestionResponse.Split(',')
                                            .Select(m => m.ToInt());

                    //_nihStrokeScaleService.SubmitAnswers(model.cas_key, responseList.ToList(), model.TemplateEntityType.ToInt(), loggedInUser.Id, loggedInUser.FullName);
                    // changes for ticket - 364
                    _nihStrokeScaleService.SubmitAnswers(cas_key, responseList.ToList(), loggedInUser.Id, loggedInUser.FullName);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
        private @case PrepareNewCaseData(@case model)
        {
            try
            {
                // Assign default physician in case of EAlert cases
                if (model.cas_is_ealert)
                {
                    model.cas_is_flagged = true;
                    if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
                    {
                        var phyList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.cas_fac_key, null, model.cas_ctp_key).Where(x => x.FinalSorted).ToList();
                        if (phyList != null && phyList.Count > 0)
                        {
                            model.cas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                            model.cas_cst_key = CaseStatus.WaitingToAccept.ToInt();
                        }
                    }
                }
                // Assign default billing code if it is true 
                if (model.cas_billing_bic_key == null)
                {
                    var cas_billingCode = _uclService.GetDefault(UclTypes.BillingCode);
                    if (cas_billingCode != null)
                    {
                        model.cas_billing_bic_key = cas_billingCode.ucd_key;
                    }
                }

                // Get current facility timezone and set to local variable.
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                if (!string.IsNullOrEmpty(model.FacilityTimeZone))
                {
                    facilityTimeZone = model.FacilityTimeZone;
                }

                int? contactKey = SaveNumber(model);

                model.cas_callback = Functions.ClearPhoneFormat(model.cas_callback);
                model.cas_created_by = loggedInUser.Id;
                model.cas_associate_id = loggedInUser.Id; // Id of the Navigator creating a record.
                model.cas_created_by_name = loggedInUser.FullName;

                // setting default value
                model.cas_response_sa_ts_md = MetricResponseStatus.NA.ToInt();
                model.cas_response_technical_issues = MetricResponseStatus.NA.ToInt();
                model.cas_physician_concurrent_alerts = StatusOptions.No.ToInt();
                model.cas_navigator_concurrent_alerts = StatusOptions.No.ToInt();
                model.cas_response_nav_to_ts = MetricResponseStatus.NA.ToInt();
                model.cas_response_miscommunication = MetricResponseStatus.NA.ToInt();
                model.cas_response_pulled_rounding = MetricResponseStatus.NA.ToInt();
                model.cas_response_inaccurate_eta = MetricResponseStatus.NA.ToInt();
                model.cas_response_physician_blast = MetricResponseStatus.NA.ToInt();
                model.cas_response_rca_tracker = MetricResponseStatus.NA.ToInt();
                model.cas_response_review_initiated = MetricResponseStatus.NA.ToInt();
                model.cas_response_tpa_to_minute = MetricResponseStatus.NA.ToInt();
                model.cas_response_door_to_needle = MetricResponseStatus.NA.ToInt();
                model.cas_is_active = true; // default as active

                // Remove default value - ticket-364
                // Default values for metric tab 
                //model.cas_metric_ct_head_is_not_reviewed = true; // default value

                // default value
                // Remove default value - ticket-220
                //* model.cas_metric_advance_imaging_is_reviewed = true; // default value    
                //model.cas_metric_discussed_with_neurointerventionalist = true;

                model.cas_metric_physician_recommented_consult_neurointerventionalist = false;

                // remove default - TCARE-409 
                //model.cas_metric_physician_notified_of_thrombolytics = false;

                model.cas_metric_last_seen_normal = LB2S2CriteriaOptions.UNK.ToInt();
                model.cas_metric_has_hemorrhgic_history = LB2S2CriteriaOptions.UNK.ToInt();
                model.cas_metric_has_recent_anticoagulants = LB2S2CriteriaOptions.UNK.ToInt();
                model.cas_metric_has_major_surgery_history = LB2S2CriteriaOptions.UNK.ToInt();
                model.cas_metric_has_stroke_history = LB2S2CriteriaOptions.UNK.ToInt();
                model.cas_metric_non_tpa_reason_key = _uclService.GetDefault(UclTypes.NonTPACandidate)?.ucd_key;
                model.cas_metric_tpaDelay_key = _uclService.GetDefault(UclTypes.TpaDelay)?.ucd_key;
                // Remove default value - ticket-364
                //model.cas_patient_type = PatientType.EMS.ToInt();

                if (model.cas_cst_key == CaseStatus.Accepted.ToInt())
                {
                    model.cas_response_time_physician = DateTime.UtcNow;
                }
                // Set arrival (door) time value according to facility time zone.
                if (model.cas_metric_door_time_est.HasValue)
                {
                    model.cas_metric_door_time = model.cas_metric_door_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.cas_metric_door_time_est = model.cas_metric_door_time?.ToTimezoneFromUtc("Eastern Standard Time");
                }
                else
                {
                    model.cas_metric_door_time = null;
                }

                if (model.cas_metric_symptom_onset_during_ed_stay_time_est.HasValue)
                {
                    model.cas_metric_symptom_onset_during_ed_stay_time = model.cas_metric_symptom_onset_during_ed_stay_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.cas_metric_symptom_onset_during_ed_stay_time_est = model.cas_metric_symptom_onset_during_ed_stay_time?.ToTimezoneFromUtc("Eastern Standard Time");
                }

                model.cas_billing_patient_name = model.cas_patient;

                //default follow up data for all case type for [TCARE-425]  
                //if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt())
                //{
                model.cas_billing_date_of_consult = model.cas_billing_date_of_consult == null ?
                                                                DateTime.Now.ToEST() :
                                                                model.cas_billing_date_of_consult;
                //}

                if (model.cas_time_stamp.HasValue)
                {
                    model.cas_response_ts_notification = model.cas_time_stamp;
                }
                else
                {
                    model.cas_response_ts_notification = DateTime.UtcNow;//start time
                }

                if (model.cas_phy_key != null)
                {
                    model.cas_physician_assign_date = DateTime.Now.ToEST();
                }

                if (model.cas_phy_technical_issue_date_est.HasValue)
                {
                    model.cas_phy_technical_issue_date = model.cas_phy_technical_issue_date_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.cas_phy_technical_issue_date_est = model.cas_phy_technical_issue_date?.ToTimezoneFromUtc("Eastern Standard Time");

                    if (model.cas_response_first_atempt == null)
                    {
                        model.cas_response_first_atempt = model.cas_phy_technical_issue_date;

                    }
                }

                if (model.cas_callback_response_time_est.HasValue)
                {
                    model.cas_callback_response_time = model.cas_callback_response_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.cas_callback_response_time_est = model.cas_callback_response_time?.ToTimezoneFromUtc("Eastern Standard Time");

                }

                model.cas_metric_stamp_time = DateTime.UtcNow;
                model.cas_metric_stamp_time_est = DateTime.Now.ToEST();
                model.cas_created_date = DateTime.Now.ToEST();

                if (model.cas_cst_key > 0)
                    model.cas_status_assign_date = model.cas_response_ts_notification?.ToTimezoneFromUtc("Eastern Standard Time");

                #region TCARE-482 
                model.cas_notes = GetNotes(model.cas_phy_key, model.cas_fac_key.ToString(), model.cas_notes);
                #endregion

                return model;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private @case MapRoleBaseData(@case model, @case dbModel)
        {
            try
            {
                model.cas_history_physician_initial = dbModel.cas_history_physician_initial;
                var VisibleTabs = model.VisibleTabs.Split(',').Select(m => m).ToList();
                #region handling disalbed/readonly fields in case of different roles
                if (User.IsInRole(UserRoles.Navigator.ToDescription()))
                {
                    model.cas_triage_notes = dbModel.cas_triage_notes;
                }

                if (!User.IsInRole(UserRoles.SuperAdmin.ToDescription()))
                {
                    model.cas_response_ts_notification = dbModel.cas_response_ts_notification;
                    model.cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time_est;
                    model.cas_metric_stamp_time = dbModel.cas_metric_stamp_time;
                }

                #endregion

                if (model.cas_ctp_key != CaseType.StrokeAlert.ToInt())
                {
                    model.cas_caller_source_key = null;
                    model.cas_caller_source_text = null;
                    model.cas_call_type = null;

                    model.cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time_est;
                    model.cas_metric_stamp_time = dbModel.cas_metric_stamp_time;
                }


                if (VisibleTabs.FirstOrDefault(m => m == "basic") == null)
                {
                    // col 1 fields
                    model.cas_ctp_key = dbModel.cas_ctp_key;
                    model.cas_fac_key = dbModel.cas_fac_key;
                    model.cas_cart = dbModel.cas_cart;
                    model.cas_callback = dbModel.cas_callback;
                    model.cas_callback_extension = dbModel.cas_callback_extension;
                    // model.cas_billing_patient_name = dbModel.cas_patient;              
                    model.cas_caller = dbModel.cas_caller;
                    // arrival time


                    // col2 fields
                    // model.cas_identification_type = dbModel.cas_identification_type;
                    // model.cas_identification_number = dbModel.cas_identification_number;
                    model.cas_last_4_of_ssn = dbModel.cas_last_4_of_ssn;
                    model.cas_referring_physician = dbModel.cas_referring_physician;
                    model.cas_notes = dbModel.cas_notes;
                    model.cas_eta = dbModel.cas_eta;
                    model.cas_pulled_from_rounding = dbModel.cas_pulled_from_rounding;
                    model.cas_phy_technical_issue_date_est = dbModel.cas_phy_technical_issue_date_est;

                    //557 Amir.j
                    //model.cas_callback_response_time_est = dbModel.cas_callback_response_time_est;
                    //model.cas_callback_response_time = dbModel.cas_callback_response_time;

                    // col 3 fields
                    model.cas_phy_key = dbModel.cas_phy_key;
                    //  model.cas_billing_physician_blast = dbModel.cas_billing_physician_blast;
                    model.cas_cst_key = dbModel.cas_cst_key;
                    model.cas_phy_has_technical_issue = dbModel.cas_phy_has_technical_issue;
                    model.cas_triage_notes = dbModel.cas_triage_notes;


                    if (model.cas_phy_has_technical_issue)
                    {
                        model.cas_response_first_atempt = dbModel.cas_phy_technical_issue_date_est;
                    }

                }
                else
                {
                    if (model.cas_caller_source_key.HasValue)
                    {
                        var callerSource = _uclService.GetDetails(model.cas_caller_source_key.Value);
                        if (callerSource.ucd_title.ToLower() != "other")
                            model.cas_caller_source_text = null;
                    }

                    /*TCARE - 472 */
                    if (model.cas_cart_location_key.HasValue)
                    {
                        var carLocation = _uclService.GetDetails(model.cas_cart_location_key.Value);
                        if (carLocation.ucd_title.Trim().ToLower() != "other")
                            model.cas_cart_location_text = null;
                    }
                }

                if ((VisibleTabs.FirstOrDefault(m => m == "metric") == null))
                {
                    // col 1
                    model.cas_metric_lastwell_date_est = dbModel.cas_metric_lastwell_date_est;
                    model.cas_metric_lastwell_date = dbModel.cas_metric_lastwell_date;

                    model.cas_metric_is_lastwell_unknown = dbModel.cas_metric_is_lastwell_unknown;
                    model.cas_patient_type = dbModel.cas_patient_type;

                    // model.cas_metric_door_time_est = dbModel.cas_metric_door_time_est;
                    // model.cas_metric_door_time = dbModel.cas_metric_door_time;

                    model.cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time_est;
                    model.cas_metric_stamp_time = dbModel.cas_metric_stamp_time;
                    model.cas_response_ts_notification = dbModel.cas_response_ts_notification;


                    model.cas_response_first_atempt = dbModel.cas_response_first_atempt;

                    model.cas_metric_video_start_time_est = dbModel.cas_metric_video_start_time_est;
                    model.cas_metric_video_start_time = dbModel.cas_metric_video_start_time;

                    model.cas_metric_patient_gender = dbModel.cas_metric_patient_gender;
                    model.cas_metric_symptoms = dbModel.cas_metric_symptoms;
                    model.cas_metric_hpi = dbModel.cas_metric_hpi;
                    model.cas_metric_assesment_time = dbModel.cas_metric_assesment_time;
                    model.cas_billing_lod_key = dbModel.cas_billing_lod_key;
                    model.cas_metric_notes = dbModel.cas_metric_notes;

                    // col 2
                    model.cas_metric_last_seen_normal = dbModel.cas_metric_last_seen_normal;
                    model.cas_metric_has_hemorrhgic_history = dbModel.cas_metric_has_hemorrhgic_history;
                    model.cas_metric_has_recent_anticoagulants = dbModel.cas_metric_has_recent_anticoagulants;
                    model.cas_metric_has_major_surgery_history = dbModel.cas_metric_has_major_surgery_history;
                    model.cas_metric_has_stroke_history = dbModel.cas_metric_has_stroke_history;

                    model.cas_metric_tpa_verbal_order_time_est = dbModel.cas_metric_tpa_verbal_order_time_est;
                    model.cas_metric_tpa_verbal_order_time = dbModel.cas_metric_tpa_verbal_order_time;

                    model.cas_metric_tpa_consult = dbModel.cas_metric_tpa_consult;

                    model.cas_metric_pa_ordertime_est = dbModel.cas_metric_pa_ordertime_est;
                    model.cas_metric_pa_ordertime = dbModel.cas_metric_pa_ordertime;

                    model.cas_metric_needle_time_est = dbModel.cas_metric_needle_time_est;
                    model.cas_metric_needle_time = dbModel.cas_metric_needle_time;

                    model.cas_metric_weight = dbModel.cas_metric_weight;
                    model.cas_metric_weight_unit = dbModel.cas_metric_weight_unit;

                    model.cas_metric_total_dose = dbModel.cas_metric_total_dose;
                    model.cas_metric_bolus = dbModel.cas_metric_bolus;
                    model.cas_metric_infusion = dbModel.cas_metric_infusion;
                    model.cas_metric_discard_quantity = dbModel.cas_metric_discard_quantity;

                    model.cas_metric_video_end_time_est = dbModel.cas_metric_video_end_time_est;
                    model.cas_metric_video_end_time = dbModel.cas_metric_video_end_time;

                    model.cas_metric_non_tpa_reason_key = dbModel.cas_metric_non_tpa_reason_key;


                    // col - 3

                    model.cas_metric_ct_head_has_no_acture_hemorrhage = dbModel.cas_metric_ct_head_has_no_acture_hemorrhage;
                    model.cas_metric_ct_head_is_reviewed = dbModel.cas_metric_ct_head_is_reviewed;
                    model.cas_metric_ct_head_is_not_reviewed = dbModel.cas_metric_ct_head_is_not_reviewed;
                    model.cas_metric_ct_head_reviewed_text = dbModel.cas_metric_ct_head_reviewed_text;

                    model.cas_metric_radiologist_callback_for_review_of_advance_imaging = dbModel.cas_metric_radiologist_callback_for_review_of_advance_imaging;
                    model.cas_metric_radiologist_callback_for_review_of_advance_imaging_date = dbModel.cas_metric_radiologist_callback_for_review_of_advance_imaging_date;
                    model.cas_metric_radiologist_callback_for_review_of_advance_imaging_notes = dbModel.cas_metric_radiologist_callback_for_review_of_advance_imaging_notes;

                    model.cas_metric_discussed_with_neurointerventionalist_date = dbModel.cas_metric_discussed_with_neurointerventionalist_date;

                    model.cas_metric_in_cta_queue = dbModel.cas_metric_in_cta_queue;

                    model.cas_metric_door_time_est = dbModel.cas_metric_door_time_est;
                    model.cas_metric_door_time = dbModel.cas_metric_door_time;
                }

                if ((VisibleTabs.FirstOrDefault(m => m == "billing") == null))
                {
                    // col1
                    model.cas_billing_bic_key = dbModel.cas_billing_bic_key;
                    model.cas_billing_visit_type = dbModel.cas_billing_visit_type;

                    // col2
                    model.cas_billing_diagnosis = dbModel.cas_billing_diagnosis;
                    model.cas_billing_notes = dbModel.cas_billing_notes;

                }

                if (VisibleTabs.FirstOrDefault(m => m == "metricResponse") == null)
                {

                    // col 1
                    model.cas_response_date_consult = dbModel.cas_response_date_consult;
                    model.cas_response_reviewer = dbModel.cas_response_reviewer;
                    model.cas_response_case_research = dbModel.cas_response_case_research;
                    model.cas_response_sa_ts_md = dbModel.cas_response_sa_ts_md;
                    model.cas_navigator_concurrent_alerts = dbModel.cas_navigator_concurrent_alerts;
                    model.cas_physician_concurrent_alerts = dbModel.cas_physician_concurrent_alerts;
                    model.cas_response_miscommunication = dbModel.cas_response_miscommunication;
                    model.cas_response_technical_issues = dbModel.cas_response_technical_issues;
                    model.cas_response_tpa_to_minute = dbModel.cas_response_tpa_to_minute;
                    model.cas_response_door_to_needle = dbModel.cas_response_door_to_needle;

                    // col 2

                    model.cas_response_nav_to_ts = dbModel.cas_response_nav_to_ts;
                    model.cas_response_pulled_rounding = dbModel.cas_response_pulled_rounding;
                    model.cas_response_physician_blast = dbModel.cas_response_physician_blast;
                    model.cas_response_review_initiated = dbModel.cas_response_review_initiated;
                    model.cas_response_case_number = dbModel.cas_response_case_number;

                }


                if (VisibleTabs.FirstOrDefault(m => m == "basic") == null && VisibleTabs.FirstOrDefault(m => m == "billing") == null)
                {
                    model.cas_billing_patient_name = dbModel.cas_billing_patient_name;
                    model.cas_identification_type = dbModel.cas_identification_type;
                    model.cas_identification_number = dbModel.cas_identification_number;
                    model.cas_billing_physician_blast = dbModel.cas_billing_physician_blast;
                    model.cas_billing_date_of_consult = dbModel.cas_billing_date_of_consult;


                }

                if (VisibleTabs.FirstOrDefault(m => m == "basic") == null && VisibleTabs.FirstOrDefault(m => m == "metric") == null && VisibleTabs.FirstOrDefault(m => m == "billing") == null)
                {
                    model.cas_billing_dob = dbModel.cas_billing_dob;
                }


                // handling technical issue date for roles except admin/super admin

                if (model.cas_phy_has_technical_issue && !User.IsInRole("Administrator") && !User.IsInRole("Super Admin"))
                {
                    model.cas_response_first_atempt = model.cas_phy_technical_issue_date_est;
                }

                return model;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private Dictionary<string, string> SetDefaultValuesForTemplate(EntityTypes entityType, Dictionary<string, string> dictFormData)
        {
            try
            {
                if (entityType == EntityTypes.StrokeAlertTemplateTpa)
                {

                    //case_template_stroke_tpa
                    dictFormData.Add("cts_created_by", loggedInUser.Id);
                    dictFormData.Add("cts_created_by_name", loggedInUser.FullName);
                    dictFormData.Add("cts_created_date", DateTime.Now.ToEST().ToString());
                    //@Html.Partial("Templates/Forms/_StrokeAlertTPATemplateForm", Model)
                }
                else if (entityType == EntityTypes.NeuroStrokeAlertTemplateTpa)
                {
                    //case_template_stroke_neuro_tpa

                    dictFormData.Add("csn_created_by", loggedInUser.Id);
                    dictFormData.Add("csn_created_by_name", loggedInUser.FullName);
                    dictFormData.Add("csn_created_date", DateTime.Now.ToEST().ToString());
                    //@Html.Partial("Templates/Forms/_StrokeNeuroTPATemplateForm", Model)
                }
                else if (entityType == EntityTypes.StrokeAlertTemplateNoTpa)
                {
                    // @Html.Partial("Templates/Forms/_StrokeAlertNoTPATemplateForm", Model)
                    //case_template_stroke_notpa


                    dictFormData.Add("ctn_lipid_panel_obtained", "0");
                    dictFormData.Add("ctn_created_by", loggedInUser.Id);
                    dictFormData.Add("ctn_created_by_name", loggedInUser.FullName);
                    dictFormData.Add("ctn_created_date", DateTime.Now.ToEST().ToString());

                }
                else if (entityType == EntityTypes.StrokeAlertTemplateNoTpaTeleStroke)
                {
                    //  @Html.Partial("Templates/Forms/_TeleStrokeNoTPATemplateForm", Model)
                    //case_template_telestroke_notpa                
                    dictFormData.Add("ctt_routine_consultation", "0");
                    dictFormData.Add("ctt_created_by", loggedInUser.Id);
                    dictFormData.Add("ctt_created_by_name", loggedInUser.FullName);
                    dictFormData.Add("ctt_created_date", DateTime.Now.ToEST().ToString());
                }

                return dictFormData;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }
        private void LogCaseChanges(string key)
        {
            try
            {
                var partialClassFields = "VisibleTabs,PhysicianUser,FacilityTimeZone,cas_response_ts_notification_utc,SelectedNIHSQuestionResponse,TemplateEntityType,IsCaseCompleted";
                var changes = _caseService.GetChangeTrackset(partialClassFields);
                if (changes.Count() > 0)
                {
                    AddChangeLog(changes, key, EntityTypes.Case);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private void LogAutoSaveChanges(AutoSaveViewModel model)
        {
            try
            {
                var list = model.FormData.Select(m => new ChangeTrackEntityVM
                {
                    entity = m.Key.Split('.').Count() > 0 ? m.Key.Split('.').First() : "case",
                    field = m.Key.Split('.').Count() > 0 ? m.Key.Split('.').Last() : m.Key,
                    current = m.Value,
                    previous = m.PreviousValue
                }).ToList();

                var formattedList = _caseService.GetFormattedList(list);

                if (formattedList.Count() > 0)
                {
                    AddChangeLog(formattedList, model.Id.ToString(), EntityTypes.AutoSaveCase);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private void LogCaseAssignHistory(int cas_key, string cas_phy_key, string cah_action, bool isManualAssign)
        {
            try
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
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        #endregion

        private SignalRCaseDataModel GetSignalRPostData(@case model)
        {
            // if (model.cas_phy_key == User.Identity.GetUserId())
            //   return null;
            try
            {

                var caseData = new SignalRCaseDataModel
                {
                    cas_key = model.cas_key,
                    cas_response_first_atempt = model.cas_response_first_atempt?.FormatDateTime(),
                    cas_phy_has_technical_issue = model.cas_phy_has_technical_issue,
                    IsLoginDelayRequired = false,
                    cas_phy_key = model.cas_phy_key,
                    cas_cst_key = model.cas_cst_key
                };


                TimeSpan? b = model.cas_metric_stamp_time_est.HasValue && model.cas_response_first_atempt.HasValue ? (model.cas_response_first_atempt.Value - model.cas_metric_stamp_time_est.Value) : new TimeSpan();
                if (b?.TotalMinutes > 10.0)
                {
                    caseData.IsLoginDelayRequired = true;
                }

                return caseData;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

        }
        private bool SyncCaseDataFromAdmin(SignalRCaseDataModel model)
        {
            try
            {
                if (model == null)
                    return false;

                #region updating the case edit page info, if opened by the navigator
                if (Settings.RPCMode == RPCMode.SignalR)
                {

                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                    var physicianOnlineWindows = PhysicianCasePopupHub.ConnectedUsers
                                                                .Where(m => m.UserId == model.cas_phy_key)
                                                                .ToList();

                    string jsonData = JsonConvert.SerializeObject(model);


                    physicianOnlineWindows.ForEach(window =>
                    {
                        hubContext.Clients.Client(window.ConnectionId).syncCaseInfoFromAdmin(model);
                    });
                }
                else
                {
                    var paramData = new List<object>();
                    paramData.Add(JsonConvert.SerializeObject(model));

                    new WebSocketEventHandler().CallJSMethod(new SocketResponseModel { MethodName = "syncCaseInfoFromAdmin_def", Data = paramData });
                }

                #endregion
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private void ShowNavigatorRejectCasePopup(@case model, string popupInitiatedUserId, PhysicianCaseAssignQueue action)
        {
            try
            {
                // checking the entry in log so the in any case the popup is not displayed to navigator multiple times

                if (Settings.RPCMode == RPCMode.SignalR)
                {
                    #region Sending Alert to Physician in case of All Physicians reject the case

                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                    var navigatorList = PhysicianCasePopupHub.ConnectedUsers.Where(m => m.UserId == popupInitiatedUserId)
                                                            .ToList();

                    navigatorList.ForEach(navigator =>
                    {
                        hubContext.Clients.Client(navigator.ConnectionId).showNavigatorRejectCasePopupWithNoQueue(model.cas_key, action);

                    });


                    #endregion
                }
                else if (Settings.RPCMode == RPCMode.WebSocket)
                {
                    new WebSocketEventHandler().CallJSMethod(popupInitiatedUserId, new SocketResponseModel { MethodName = "showNavigatorRejectCasePopupWithNoQueue_def", Data = new List<object> { model.cas_key, action.ToInt() } });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private List<string> GetVisibleTabsForUser(string userId, @case model)
        {
            try
            {
                var tabs = new List<string>();
                var userRoles = UserManager.GetRoles(userId);

                if (!(userRoles.Contains(UserRoles.FacilityAdmin.ToDescription()) || userRoles.Contains(UserRoles.Physician.ToDescription()) || userRoles.Contains(UserRoles.PartnerPhysician.ToDescription())))
                {
                    tabs.Add("basic");
                }
                if (!(userRoles.Contains(UserRoles.Navigator.ToDescription()) || userRoles.Contains(UserRoles.OutsourcedNavigator.ToDescription())))
                {
                    if (model.cas_ctp_key == CaseType.StrokeAlert.ToInt() || model.cas_ctp_key == CaseType.StatEEG.ToInt())
                    {
                        tabs.Add("metric");
                    }
                }
                if (!(userRoles.Contains(UserRoles.Navigator.ToDescription()) || userRoles.Contains(UserRoles.OutsourcedNavigator.ToDescription())) && model.TemplateEntityType != null
                    && !(User.IsInRole(UserRoles.FacilityAdmin.ToDescription())))
                {
                    tabs.Add("templates");
                }

                if (!(userRoles.Contains(UserRoles.Navigator.ToDescription()) || userRoles.Contains(UserRoles.OutsourcedNavigator.ToDescription())))
                {
                    tabs.Add("billing");
                }

                if (!userRoles.Contains(UserRoles.Physician.ToDescription()) && !userRoles.Contains(UserRoles.PartnerPhysician.ToDescription()) &&
                                                               !(userRoles.Contains(UserRoles.Navigator.ToDescription()) || userRoles.Contains(UserRoles.OutsourcedNavigator.ToDescription()))
                                                               && !(userRoles.Contains(UserRoles.FacilityAdmin.ToDescription())))
                {
                    tabs.Add("metricResponse");
                }

                return tabs;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private void ShowNavigatorAcceptCasePopup(@case model, string popupInitiatedByUserId)
        {
            try
            {
                var facilityTimeZone = Settings.DefaultTimeZone;
                string cas_response_time_physician = "";
                if (!string.IsNullOrEmpty(model.facility?.fac_timezone))
                {
                    facilityTimeZone = model.facility?.fac_timezone;
                }

                if (model.cas_response_time_physician.HasValue)
                    cas_response_time_physician = model.cas_response_time_physician.Value.ToTimezoneFromUtc(facilityTimeZone).FormatDateTime();

                model.FromWaitingToAcceptToAcceptTime = CalculateWaitingToAcceptTime(model);

                string jsonData = JsonConvert.SerializeObject(new { model.cas_key, model.cas_phy_key, model.cas_cst_key, cas_response_time_physician, model.cas_case_number, model.FromWaitingToAcceptToAcceptTime });

                if (Settings.RPCMode == RPCMode.SignalR)
                {
                    #region Sending Alert to Physician in case of All Physicians reject the case

                    var hubContext = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<PhysicianCasePopupHub>();
                    var navigatorList = PhysicianCasePopupHub.ConnectedUsers.Where(m => m.UserId == popupInitiatedByUserId)
                                                            .ToList();

                    navigatorList.ForEach(navigator =>
                    {
                        hubContext.Clients.Client(navigator.ConnectionId).showNavigatorAcceptCasePopupWithNoQueue(jsonData);

                    });


                    #endregion
                }
                else if (Settings.RPCMode == RPCMode.WebSocket)
                {
                    new WebSocketEventHandler().CallJSMethod(popupInitiatedByUserId, new SocketResponseModel { MethodName = "showNavigatorAcceptCasePopupWithNoQueue_def", Data = new List<object> { jsonData } });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        private async Task HideNavigatorCasePopup(int cas_key, string phy_key)
        {
            try
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
                            new WebSocketEventHandler().CallJSMethod(phy_key, new SocketResponseModel { MethodName = "closeNavigatorCasePopupWithNoQueue_def" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

        }

        private string GetNotes(string phy_key, string fac_key, string existingNotes)
        {
            try
            {
                var physicianNotes = _entityNotesService.GetEnityNotes(phy_key, EntityTypes.User).Where(x => x.etn_display_on_open).ToList();
                var facilityNotes = _entityNotesService.GetEnityNotes(fac_key, EntityTypes.Facility).Where(x => x.etn_display_on_open).ToList();
                physicianNotes.AddRange(facilityNotes);


                if (existingNotes == null)
                {
                    existingNotes = string.Empty;
                }
                else
                {
                    existingNotes += "\r\n\r\n";
                }

                var notes = physicianNotes;
                if (notes != null && notes.Count > 0)
                {
                    foreach (var note in notes)
                    {
                        existingNotes += note.etn_notes;

                        //Adding some spance to separate each note according to TCARE-482.
                        existingNotes += "\r\n\r\n";
                    }
                }

                return existingNotes;  
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }
        }

        #region diagnosis codes
        [HttpGet]
        public ActionResult SearchDiagnosisCodes(string Id = "", bool isImpressionChecked = false, string Name = "")
        {
            if (string.IsNullOrEmpty(Id) && Id != "123")
            {
                return RedirectToAction("Home", "Index");
            }
            var result = _diagnosisCodesService.SearchDiagnosisCodes(Name, isImpressionChecked);
            return Json(new { status = true, codes = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult SearchRecentDiagnosisCodes(string Id = "", string UserId = "")
        {
            if (string.IsNullOrEmpty(Id) && Id != "123")
            {
                return RedirectToAction("Home", "Index");
            }
            var result = _diagnosisCodesService.SearchRecentDiagnosisCodes(UserId);
            return Json(new { status = true, codes = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult getBillingDiagnosisCode()
        {
            var result = _diagnosisCodesService.getBillingDiagnosisCode();

            var result1 = result.Select(x => new Tuple<string, string, string>(x.title, x.title, x.title)).ToList();

            return Json(result1, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Token work
        public JsonResult SendToken(string phy_key, string token)
        {
            //phy_key = User.Identity.GetUserId();
            var GetDetail = _tokenservice.GetAll(phy_key);
            user_fcm_notification user_Fcm_Notification = new user_fcm_notification();
            if (GetDetail.Count > 0)
            {
                var first = GetDetail.FirstOrDefault();
                var tokens = GetDetail.Select(m => m.tok_phy_token).ToList();
                user_Fcm_Notification.phy_key = first.tok_phy_key;
                user_Fcm_Notification.msg_title = "Stroke Alert";
                user_Fcm_Notification.msg_body = first.AspNetUser.FirstName + " " + first.AspNetUser.LastName + " You have New Stroke!";
                user_Fcm_Notification.notify_img = "https://media.graytvinc.com/images/690*387/Stroke+MGN+graphic.JPG";
                user_Fcm_Notification.phy_tokens = tokens;
            }
            return Json(user_Fcm_Notification, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateTokens(token model, string phy_token_key)
        {
            string phy_key = User.Identity.GetUserId();
            var GetDetail = _tokenservice.GetDetailById(phy_key, phy_token_key);
            if (GetDetail == null)
            {
                model.tok_phy_key = phy_key;
                model.tok_phy_token = phy_token_key;
                _tokenservice.Create(model);
            }
            var detail = _fireBaseUserMailService.GetDetails(phy_key);
            return Json(detail, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteToken(string phy_token_key)
        {
            string phy_key = User.Identity.GetUserId();
            var GetDetail = _tokenservice.GetDetailById(phy_key, phy_token_key);
            if (GetDetail != null)
            {
                int id = GetDetail.tok_key;
                _tokenservice.Delete(id);
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult GetProtocol(string fac_key)
        {
            var model = _Protocols.GetDetailsForPOpUP(fac_key);
            var result = RenderPartialViewToString("ViewProtocols", model);
            return Json(new { success = true, data = result });
        }
        public ActionResult GetOnboard(string fac_key)
        {
            var model = _OnBoardedServices.GetDetailsForPOpUP(fac_key);
            var result = RenderPartialViewToString("ViewOnBoarding", model);
            return Json(new { success = true, data = result });
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
                    _adminService?.Dispose();
                    _caseGridService?.Dispose();
                    _caseService?.Dispose();
                    _physicianCaseTempService.Dispose();
                    _lookUpService?.Dispose();
                    _uclService?.Dispose();
                    _facilityService?.Dispose();
                    _facilityPhysicianService?.Dispose();
                    _entityNotesService?.Dispose();
                    _contactService?.Dispose();
                    _callHistoryService?.Dispose();
                    _caseAssignHistoryService?.Dispose();
                    _physicianStatusService?.Dispose();
                    _physician?.Dispose();
                    _physicianStatusLogService?.Dispose();
                    _nihStrokeScaleService?.Dispose();
                    _templateService?.Dispose();
                    _generateTemplateService?.Dispose();
                    _ealertFacilitiesService?.Dispose();
                    _facilityContractService?.Dispose();
                    _homeHealthService?.Dispose();
                    _casCancelledTypeService?.Dispose();
                    _rootCauseService?.Dispose();
                    _casereviewTemplateService?.Dispose();
                    _Protocols?.Dispose();
                    _OnBoardedServices?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
