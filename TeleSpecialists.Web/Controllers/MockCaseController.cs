using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Service.CaseServices;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class MockCaseController : BaseController
    {
        // GET: MockCase
        private readonly MockCaseService _mockCaseService;
        private readonly ContactService _contactService;
        private readonly UCLService _uclService;
        private readonly AdminService _adminService;
        private readonly EntityNoteService _entityNotesService;
        private FacilityPhysicianService _facilityPhysicianService;
        private FacilityService _facilityService;

        private readonly LookupService _lookUpService;

        private const int _min_dob_year = 1753;
        private static bool isCalculateBill { get; set; }
        public MockCaseController() : base()
        {
            _mockCaseService = new MockCaseService();
            _contactService = new ContactService();
            _uclService = new UCLService();
            _lookUpService = new LookupService();
            _adminService = new AdminService();
            _entityNotesService = new EntityNoteService();
            _facilityPhysicianService = new FacilityPhysicianService();
            _facilityService = new FacilityService();

        }

        public ActionResult Index()
        {
            return View();
        }
        [AccessRoles(Roles = "Mock Physician,RRC Manager,Super Admin,RRC Director")]
        public ActionResult Create()
        {
            try
            {
                try
                {
                    var selectedFacility = Guid.Empty;
                    ViewBag.SaveNumber = false;
                    string five9CaseType = "";
                    var caseObj = new mock_case();

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


                       //caseObj = new mock_case { mcas_ani = number, mcas_dnis = dnis, mcas_time_stamp = five9StartDate, mcas_campaign_id = campaign, mcas_call_id = callid, FacilityTimeZone = BLL.Settings.DefaultTimeZone, mcas_five9_original_stamp_time = timestamp };

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
                        UclTypes.CallerSource.ToInt(),
                        UclTypes.IdentificationType.ToInt(),
                        UclTypes.CartLocation.ToInt(),
                        UclTypes.MockCaseType.ToInt()
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

                    caseObj.mcas_ctp_key = defaultTypeUCL != null ? defaultTypeUCL.ucd_key : 0;
                    caseObj.mcas_cst_key = defaultStatusUCL != null ? defaultStatusUCL.ucd_key : 0;
                    caseObj.mcas_fac_key = selectedFacility;
                    caseObj.mcas_identification_type = GetDefaultTypeFromList(uclDataList, UclTypes.IdentificationType)?.ucd_key; //_uclService.GetDefault(UclTypes.IdentificationType)?.ucd_key;
                    var caller_source = GetDefaultTypeFromList(uclDataList, UclTypes.CallerSource);
                    caseObj.mcas_caller_source_key = caller_source?.ucd_key;
                    //caseObj.mcas_caller_source_key_title = caller_source?.ucd_title;

                    #endregion

                    ViewBag.UclData = uclDataList.Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).OrderBy(o => o.ucd_sort_order);

                    return GetViewResult(caseObj);
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AccessRoles(Roles = "Mock Physician,RRC Manager,Super Admin,RRC Director")]
        public ActionResult Create(mock_case model)
        {
            try
            {
                var isRedirect = Request.Params["mRedirectPage"] == "0" ? false : true;
                var isSaveAndSend = Request.Params["mhdnSaveAndSend"] == "1" ? true : false;
                var showPhyOfflinePopup = "0";
    
                if (ModelState.IsValid)
                {
                    model = PrepareNewCaseData(model);
                    _mockCaseService.Create(model);

                    #region showing the time in popup in facility time zone

                    string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                    var facility = _facilityService.GetDetails(model.mcas_fac_key);

                    if (!string.IsNullOrEmpty(facility?.fac_timezone))
                    {
                        facilityTimeZone = facility.fac_timezone;
                    }

                    var abbrivation = Functions.GetTimeZoneAbbreviation(facilityTimeZone);
                    //var displaytime = model.mcas_metric_stamp_time?.ToTimezoneFromUtc(facilityTimeZone).FormatDateTime();
                    //model.mcas_metric_stamp_time_formated = displaytime + " " + abbrivation;

                    #endregion

                    #region handling logging in case of physician updated
                    //if (model.cas_cst_key > 0)
                    //{
                    //    LogCaseAssignHistory(model.cas_key, model.cas_phy_key, _uclService.GetDetails(model.cas_cst_key)?.ucd_title, true);
                    //    _caseService.UpdateCaseInitials(model.cas_key, _caseService.GetCaseInitials(model.cas_key));
                    //}
                    #endregion

                    TempData["Message"] = "Mock drill has been added.";
                    //_MockcaseService.UpdateTimeStamps(model.mcas_key.ToString());

                    var isAutoSave = Request.Params["IsAutoSave"] == "0" ? false : true;
                    if (isAutoSave)
                    {

                        return ShowSuccessMessageOnly("Mock drill has been added.", model);
                    }
                    else
                    {
                        if (isRedirect)
                            return GetSuccessResult("/case/Index", "Mock drill has been added.");
                        else
                        {
                            return GetSuccessResult(Url.Action("Edit", new { Id = model.mcas_key, showPopupOnLoad = (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()) ? false : true), showPhyOfflinePopup = showPhyOfflinePopup, isInitialSave = true })); /* commented due to #411 - settings.aps_cas_facility_popup_on_load */
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

        private mock_case PrepareNewCaseData(mock_case model)
        {
            try
            {
                // Assign default physician in case of EAlert cases

                if (model.mcas_ctp_key == CaseType.StrokeAlert.ToInt())
                {
                    var phyList = _facilityPhysicianService.GetAllPhysiciansByFacility(ApplicationSetting, model.mcas_fac_key, null, model.mcas_ctp_key).Where(x => x.FinalSorted).ToList();
                    if (phyList != null && phyList.Count > 0)
                    {
                        model.mcas_phy_key = phyList.FirstOrDefault().AspNetUser_Id.ToString();
                        model.mcas_cst_key = CaseStatus.WaitingToAccept.ToInt();
                    }
                }


                // Get current facility timezone and set to local variable.
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                if (!string.IsNullOrEmpty(model.FacilityTimeZone))
                {
                    facilityTimeZone = model.FacilityTimeZone;
                }

                //int? contactKey = SaveNumber(model);

                model.mcas_callback = Functions.ClearPhoneFormat(model.mcas_callback);
                model.mcas_created_by = loggedInUser.Id;
                model.mcas_associate_id = loggedInUser.Id; // Id of the Navigator creating a record.
                model.mcas_created_by_name = loggedInUser.FullName;

                //// setting default value
               // model.mcas_response_sa_ts_md = MetricResponseStatus.NA.ToInt();
                model.mcas_response_technical_issues = MetricResponseStatus.NA.ToInt();
                model.mcas_physician_concurrent_alerts = StatusOptions.No.ToInt();
               // model.mcas_navigator_concurrent_alerts = StatusOptions.No.ToInt();
                model.mcas_response_nav_to_ts = MetricResponseStatus.NA.ToInt();
                model.mcas_response_miscommunication = MetricResponseStatus.NA.ToInt();
                model.mcas_response_pulled_rounding = MetricResponseStatus.NA.ToInt();
                model.mcas_response_inaccurate_eta = MetricResponseStatus.NA.ToInt();
                model.mcas_response_physician_blast = MetricResponseStatus.NA.ToInt();
                model.mcas_response_rca_tracker = MetricResponseStatus.NA.ToInt();
                model.mcas_response_review_initiated = MetricResponseStatus.NA.ToInt();
                model.mcas_response_tpa_to_minute = MetricResponseStatus.NA.ToInt();
                model.mcas_response_door_to_needle = MetricResponseStatus.NA.ToInt();
                model.mcas_is_active = true; // default as active

                //// Remove default value - ticket-364
                //// Default values for metric tab 
                //model.mcas_metric_ct_head_is_not_reviewed = true; // default value

                //// default value
                //// Remove default value - ticket-220
                ////* model.cas_metric_advance_imaging_is_reviewed = true; // default value    
                //model.mcas_metric_discussed_with_neurointerventionalist = true;

               // model.mcas_metric_physician_recommented_consult_neurointerventionalist = false;

                //// remove default - TCARE-409 
                //model.mcas_metric_physician_notified_of_thrombolytics = false;

                //model.mcas_metric_last_seen_normal = LB2S2CriteriaOptions.UNK.ToInt();
                //model.mcas_metric_has_hemorrhgic_history = LB2S2CriteriaOptions.UNK.ToInt();
                //model.mcas_metric_has_recent_anticoagulants = LB2S2CriteriaOptions.UNK.ToInt();
                //model.mcas_metric_has_major_surgery_history = LB2S2CriteriaOptions.UNK.ToInt();
                //model.mcas_metric_has_stroke_history = LB2S2CriteriaOptions.UNK.ToInt();
                //model.mcas_metric_non_tpa_reason_key = _uclService.GetDefault(UclTypes.NonTPACandidate)?.ucd_key;
                //model.mcas_metric_tpaDelay_key = _uclService.GetDefault(UclTypes.TpaDelay)?.ucd_key;
                //// Remove default value - ticket-364
                model.mcas_patient_type = PatientType.EMS.ToInt();

                if (model.mcas_cst_key == CaseStatus.Accepted.ToInt())
                {
                    model.mcas_response_time_physician = DateTime.UtcNow;
                }
                // Set arrival (door) time value according to facility time zone.
                //if (model.mcas_metric_door_time_est.HasValue)
                //{
                //    model.mcas_metric_door_time = model.mcas_metric_door_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                //    model.mcas_metric_door_time_est = model.mcas_metric_door_time?.ToTimezoneFromUtc("Eastern Standard Time");
                //}
                //else
                //{
                //    model.mcas_metric_door_time = null;
                //}

                //if (model.mcas_metric_symptom_onset_during_ed_stay_time_est.HasValue)
                //{
                //    model.mcas_metric_symptom_onset_during_ed_stay_time = model.mcas_metric_symptom_onset_during_ed_stay_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                //    model.mcas_metric_symptom_onset_during_ed_stay_time_est = model.mcas_metric_symptom_onset_during_ed_stay_time?.ToTimezoneFromUtc("Eastern Standard Time");
                //}

                //model.mcas_billing_patient_name = model.mcas_patient;

                //default follow up data for all case type for [TCARE - 425]
                //if (model.mcas_ctp_key == CaseType.StrokeAlert.ToInt())
                //        {
                //            model.mcas_billing_date_of_consult = model.mcas_billing_date_of_consult == null ?
                //                                                            DateTime.Now.ToEST() :
                //                                                            model.mcas_billing_date_of_consult;
                //        }

                if (model.mcas_time_stamp.HasValue)
                {
                    model.mcas_response_ts_notification = model.mcas_time_stamp;
                }
                else
                {
                    model.mcas_response_ts_notification = DateTime.UtcNow;//start time
                }

                if (model.mcas_phy_key != null)
                {
                    model.mcas_physician_assign_date = DateTime.Now.ToEST();
                }

                if (model.mcas_phy_technical_issue_date_est.HasValue)
                {
                    model.mcas_phy_technical_issue_date = model.mcas_phy_technical_issue_date_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.mcas_phy_technical_issue_date_est = model.mcas_phy_technical_issue_date?.ToTimezoneFromUtc("Eastern Standard Time");

                    if (model.mcas_response_first_atempt == null)
                    {
                        model.mcas_response_first_atempt = model.mcas_phy_technical_issue_date;

                    }
                }

                if (model.mcas_callback_response_time_est.HasValue)
                {
                    model.mcas_callback_response_time = model.mcas_callback_response_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                    model.mcas_callback_response_time_est = model.mcas_callback_response_time?.ToTimezoneFromUtc("Eastern Standard Time");

                }

                //model.mcas_metric_stamp_time = DateTime.UtcNow;
                //model.mcas_metric_stamp_time_est = DateTime.Now.ToEST();
                model.mcas_created_date = DateTime.Now.ToEST();

                if (model.mcas_cst_key > 0)
                    model.mcas_status_assign_date = model.mcas_response_ts_notification?.ToTimezoneFromUtc("Eastern Standard Time");

                #region TCARE-482 
                model.mcas_notes = GetNotes(model.mcas_phy_key, model.mcas_fac_key.ToString(), model.mcas_notes);
                #endregion

                return model;
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
                if (notes != null && notes.Count() > 0)
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

        private int SaveNumber(mock_case model)
        {
            var contact = new contact();
            try
            {
                if (Request.Params["AddNumber"] != null && Request.Params["AddNumber"].ToString() == "1" && !string.IsNullOrEmpty(model.mcas_ani))
                {
                    contact.cnt_fac_key = model.mcas_fac_key;
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

        private ucl_data GetDefaultTypeFromList(List<ucl_data> list, UclTypes type)
        {
            return list.Where(m => m.ucd_ucl_key == ((int)type) && m.ucd_is_active && m.ucd_is_default).FirstOrDefault();
        }

        [HttpGet]
        [AccessRoles(Roles = "Mock Physician,RRC Manager,Super Admin,RRC Director")]
        public ActionResult Edit(int id, bool isReadOnly = false)
        {
            isCalculateBill = false;

            ViewBag.IsReadOnlyCase = isReadOnly;
            if (User.IsInRole(UserRoles.Finance.ToDescription()))
            {
                ViewBag.IsReadOnlyCase = true;
            }
            var model = new mock_case();
            ViewBag.EnableAutoSave = ApplicationSetting.aps_enable_case_auto_save;
            ViewBag.ShowNotesPopup = ApplicationSetting.aps_cas_facility_popup_on_load;
            //  var viewModel = new CaseViewModel();
            try
            {
                model = _mockCaseService.GetDetails(id);

                #region  [QPS NAME]
                //if (model.facility != null)
                //{
                //    if (!string.IsNullOrWhiteSpace(model.facility.qps_number))
                //    {
                //        var GetQPSName = _adminService.GetAspNetUsers().Where(m => m.Id == model.facility.qps_number).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                //        if (GetQPSName != null)
                //        {
                //            ViewBag.QPSName = GetQPSName.FirstName + " " + GetQPSName.LastName;
                //        }
                //        else
                //        {
                //            ViewBag.QPSName = "";
                //        }

                //    }

                //}
                //else
                //{
                //    ViewBag.QPSName = "";
                //}
                #endregion

                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    // Check case is assigned to user or not.
                    if (model.mcas_phy_key != User.Identity.GetUserId())
                    {
                        ViewBag.IsReadOnlyCase = true;
                    }
                }

                ViewBag.CaseModel = model; // binding the related data from the ViewBag so the lazy loading work after the post back.
                if (model.mcas_key == 0)
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
                        UclTypes.CaseType.ToInt(),
                        UclTypes.IdentificationType.ToInt(),
                        UclTypes.CallerSource.ToInt(),
                        UclTypes.CartLocation.ToInt(),
                        UclTypes.MockCaseType.ToInt()
                    };

                    var uclDataList = _lookUpService.GetUclData(types)
                                      .Where(m => m.ucd_is_active)
                                      .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).ToList();

                    if (model.mcas_caller_source_key != null)
                    {
                        var caller_source = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.CallerSource.ToInt() && m.ucd_key == model.mcas_caller_source_key).FirstOrDefault();
                        model.mcas_caller_source_key_title = caller_source?.ucd_title;
                    }

                    /*TCARE - 472 */

                    if (model.mcas_cart_location_key != null)
                    {
                        var cart_location = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.CartLocation.ToInt() && m.ucd_key == model.mcas_cart_location_key).FirstOrDefault();
                        model.mcas_cart_location_key_Title = cart_location?.ucd_title;
                    }

                    if (model.mcas_identification_type != null)
                    {
                        var ucddata = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.IdentificationType.ToInt() && m.ucd_key == model.mcas_identification_type).FirstOrDefault();
                        model.mcas_identification_Title = ucddata?.ucd_title;
                    }

                    //model.mCaseInterval = new CaseInterval(model);

                    var _uclData = uclDataList;

                    #region Code for remove EEG records by husnain
                    var billingCodes = _uclData.Where(x => x.ucd_ucl_key == 10).ToList();
                    if (model.mcas_ctp_key != 13 && model.mcas_ctp_key != 14 && model.mcas_ctp_key != 15)
                    {
                        var listForRemove = billingCodes.Where(x => x.ucd_key == 5 || x.ucd_key == 6 || x.ucd_key == 324 || x.ucd_key == 325 || x.ucd_key == 326).ToList();
                        _uclData = _uclData.Except(listForRemove).ToList();
                    }
                    #endregion

                    ViewBag.UclData = _uclData;

                    // Sorting according to the ticket no. 439. 
                    // if there isnt a sort order set then it should go in alpha order yes but why would we give users the option to set a sort order if we are setting in alpha order? that doesnt make much sense ' From Darcy

                    ViewBag.CaseTypes = uclDataList.Where(m => m.ucd_ucl_key == UclTypes.MockCaseType.ToInt())
                                                 .Select(m => new { m.ucd_key, m.ucd_title })
                                                 .ToDictionary(m => m.ucd_key, m => m.ucd_title);

                    if (model.mcas_identification_type.HasValue)
                    {
                        ViewBag.casIdentityTypeName = _uclService.GetUclData(UclTypes.IdentificationType).Where(c => c.ucd_key == model.mcas_identification_type).FirstOrDefault().ucd_title;
                    }

                    var entityId = model.facility.fac_key.ToString();
                    var entityType = EntityTypes.Facility;
                    var notes = _entityNotesService.GetEnityNotes(entityId, entityType).ToList();
                    ViewBag.FacilityNotes = notes;
                    model.FacilityTimeZone = model.facility.fac_timezone;
                }


                //ViewBag.CaseReviewers = _adminService.GetAspNetUsers().Where(x => x.CaseReviewer == true).ToList();
                ViewBag.CaseStatus = model.mcas_cst_key;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            //if (model.mcas_billing_bic_key_initial != null)
            //{
            //    var record = _uclService.GetDetails((int)model.mcas_billing_bic_key_initial);
            //    ViewBag.revised = record.ucd_title;
            //}
            return GetViewResult(model);
        }

        [HttpPost]
        [AccessRoles(Roles = "Mock Physician,RRC Manager,Super Admin,RRC Director")]
        public ActionResult Edit(mock_case model)
        {
            try
            {
                var redirectToEdit = false;
                if (Request.Params["isReadOnly"] != null)
                {
                    redirectToEdit = Request.Params["isReadOnly"] == "true" ? true : false;
                    //@case followUpCase = null;
                    bool isFollowUpCase = false;
                    var isSaveAndSend = Request.Params["mhdnSaveAndSend"] == "1" ? true : false;
                    var showPhyOfflinePopup = "0";

                    ViewBag.EnableAutoSave = ApplicationSetting.aps_enable_case_auto_save;
                    var showSuccessMessageOnly = Request.Params["ShowSuccessOnly"] == "1";
                    var isRedirect = Request.Params["mRedirectPage"] == "0" ? false : true;

                    var dbModel = _mockCaseService.GetDetails(model.mcas_key);


                    // As there is not complete option for users other then admins in case status, So checking it through non-persisted propperty "IsCaseCompleted"
                    if (model.IsCaseCompleted && (!User.IsInRole(UserRoles.Administrator.ToDescription()) && !User.IsInRole(UserRoles.SuperAdmin.ToDescription())))
                    {
                        model.mcas_cst_key = CaseStatus.Complete.ToInt();
                    }

                    if (ModelState.IsValid)
                    {

                        model.mcas_is_flagged = true;

                        if (model.mcas_is_flagged && model.mcas_cst_key == CaseStatus.Cancelled.ToInt() && dbModel.mcas_cst_key != model.mcas_cst_key)
                        {
                            model.mcas_is_flagged = false;
                        }

                        model.mcas_callback = Functions.ClearPhoneFormat(model.mcas_callback);
                        model.mcas_modified_by = loggedInUser.Id;
                        model.mcas_modified_by_name = loggedInUser.FullName;
                        model.mcas_modified_date = DateTime.Now.ToEST();


                        #region updating the case metric time if facility updated
                        string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                        if (!string.IsNullOrEmpty(model.FacilityTimeZone))
                        {
                            facilityTimeZone = model.FacilityTimeZone;
                        }
                        #endregion

                        #region ----- Converting all non eastern fields to facility time zone - TCARE-187 -----

                        if (model.mcas_response_ts_notification.HasValue)
                        {
                            model.mcas_response_ts_notification = model.mcas_response_ts_notification.Value.ToUniversalTimeZone(facilityTimeZone);
                        }
                        if (model.mcas_phy_technical_issue_date_est.HasValue)
                        {
                            model.mcas_phy_technical_issue_date = model.mcas_phy_technical_issue_date_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                            model.mcas_phy_technical_issue_date_est = model.mcas_phy_technical_issue_date?.ToTimezoneFromUtc("Eastern Standard Time");
                        }

                        if (model.mcas_callback_response_time_est.HasValue)
                        {
                            model.mcas_callback_response_time = model.mcas_callback_response_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                            model.mcas_callback_response_time_est = model.mcas_callback_response_time?.ToTimezoneFromUtc("Eastern Standard Time");

                        }

                        if (model.mcas_cst_key == CaseStatus.Accepted.ToInt())
                        {
                            if (!model.mcas_response_time_physician.HasValue)
                                model.mcas_response_time_physician = DateTime.UtcNow;
                        }
                        #endregion

                        if (model.mcas_cst_key != dbModel.mcas_cst_key)
                        {
                            model.mcas_status_assign_date = DateTime.Now.ToEST();
                        }

                        if (model.mcas_phy_key != dbModel.mcas_phy_key && model.mcas_phy_key != null)
                            model.mcas_physician_assign_date = DateTime.Now.ToEST();
                        else if (model.mcas_phy_key == null)
                            model.mcas_physician_assign_date = null;

                        #region TCARE-540
                        if (model.mcas_phy_key != null && model.mcas_fac_key != null)
                        {
                            //If physician is changed or facility is changed we need to grab the notes.
                            if (model.mcas_fac_key != dbModel.mcas_fac_key || model.mcas_phy_key != dbModel.mcas_phy_key)
                            {
                                model.mcas_notes = GetNotes(model.mcas_phy_key, model.mcas_fac_key.ToString(), null);
                            }
                        }
                        #endregion

                        model.mcas_is_partial_update = false;

                        if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                        {
                            if (model.mcas_cst_key == 20)
                                isCalculateBill = true;
                            else
                                isCalculateBill = false;
                        }
                        _mockCaseService.Edit(model, false);
                        _mockCaseService.Save();
                        _mockCaseService.Commit();


                        if (showSuccessMessageOnly)
                        {
                            return ShowSuccessMessageOnly("Mock drill has been updated successfully", null);
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
                                  return GetSuccessResult("/case/Index", "Mock drill has been updated.");
                            else if (redirectToEdit)
                            {
                                return GetSuccessResult(Url.Action("Edit", new { id = model.mcas_key, isReadOnly = true }), "Mock drill has been updated successfully.");
                            }
                            else
                            {
                                return GetSuccessResult(
                                    Url.Action(
                                      "Edit",
                                       new
                                       {
                                           id = model.mcas_key,
                                           showPhyOfflinePopup = showPhyOfflinePopup
                                       }
                                    ), "Mock drill successfully updated"
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
            }
            catch (Exception ex)
            {
                ViewBag.CaseModel = _mockCaseService.GetDetails(model.mcas_key);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
                return GetErrorResult(model);
            }

            return GetErrorResult(model);
        }

        public JsonResult GetFacilityCart(Guid key)
        {
            var GetRecord = _mockCaseService.GetCart(key);
            return Json(GetRecord, JsonRequestBehavior.AllowGet);
        }
    }
}