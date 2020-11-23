using System;
using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.FacilityUser;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Facility Navigator, Facility Admin, Super Admin, PAC Navigator")]
    public class FacilityUserController : BaseController
    {
        #region private-members
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly EAlertCaseTypesService _ealertCaseTypesService;
        private readonly FacilityService _facilityService;
        private readonly UCLService _uCLService;
        private readonly CaseService _caseService;

        #endregion

        #region constructor
        public FacilityUserController()
        {
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _ealertCaseTypesService = new EAlertCaseTypesService();
            _facilityService = new FacilityService();
            _caseService = new CaseService();
            _uCLService = new UCLService();
        }
        #endregion

        #region Facility Navigator Dashboard
        public ActionResult FacilityNavigator()
        {
            return GetViewResult("FacilityNavigatorIndex");
        }
        public ActionResult DashBoard()
        {
            try
            {
                var currentUserId = User.Identity.GetUserId();
                var checkUser = _facilityService.GetUserRole(currentUserId);
                bool isUserHomeHealth = _facilityService.IsHomeHealth(checkUser.AspNetRole.Name);
                if(isUserHomeHealth)
                {
                    string name = checkUser.AspNetUser.FirstName + " " + checkUser.AspNetUser.LastName;
                    //var _types = _ealertCaseTypesService.GetAllHomeHealthType(currentUserId, name);
                    var _types = _uCLService.GetDefault(UclTypes.PacCaseType);
                    //_types = _types.Where(x => x.CaseTypeKey == 5).ToList();
                    ViewBag._types = _types;
                    //return View("_PacDashboard", _types);
                    //return View("_PacDashboard");

                }

                var allConsultTypes = _ealertCaseTypesService.GetAllAssignedCaseTypes(currentUserId)
                                                            .OrderByDescending(m => m.CaseTypeKey.Equals((int)CaseType.StrokeAlert))
                                                            .ToList();
                if (allConsultTypes != null && allConsultTypes.Count > 0)
                {
                    if (allConsultTypes.Count == 1)
                    {
                        var currentCaseType = allConsultTypes.FirstOrDefault();
                        ViewBag.CaseTypeKey = currentCaseType.CaseTypeKey;
                        ViewBag.CaseType = currentCaseType.CaseTypeName;
                        ViewBag.ClassName = currentCaseType.CaseTypeName.Equals(CaseType.StrokeAlert.ToDescription())
                                            || currentCaseType.CaseTypeName.Equals(CaseType.StatConsult.ToDescription())
                                            ? "text-danger" : "color-lightgray";
                        ViewBag.IsItDashboard = true;

                        if (ApplicationSetting != null)
                        {
                            ViewBag.f9Domain = ApplicationSetting.aps_five9_domain;
                            ViewBag.f9CallNumber = Functions.ClearPhoneFormat(ApplicationSetting.aps_five9_number_to_dial);
                            ViewBag.f9List = ApplicationSetting.aps_five9_list;
                        }
                        var model = new @case();
                        model.cas_identification_type = _uCLService.GetDefault(UclTypes.IdentificationType)?.ucd_key;
                        return View("_FacilityInfo", model);
                    }
                    else
                    {
                        return View("_Dashboard", allConsultTypes);
                    }
                }
                else
                {
                    return View("_Dashboard", allConsultTypes);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Error";
                ViewBag.Message = "Unexpected error occurred. Please try again.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetViewResult("Error");
            }
        }
        public ActionResult FacilityInfo(string caseType, int caseTypeKey, string className = "")
        {
            try
            {
                ViewBag.CaseTypeKey = caseTypeKey;
                ViewBag.CaseType = caseType;
                ViewBag.ClassName = className;
                ViewBag.IsItDashboard = false;

                if (ApplicationSetting != null)
                {
                    ViewBag.f9Domain = ApplicationSetting.aps_five9_domain;
                    ViewBag.f9CallNumber = Functions.ClearPhoneFormat(ApplicationSetting.aps_five9_number_to_dial);
                    ViewBag.f9List = ApplicationSetting.aps_five9_list;
                }
                var model = new @case();
                model.cas_identification_type = _uCLService.GetDefault(UclTypes.IdentificationType)?.ucd_key;
                return View("_FacilityInfo", model);
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Error";
                ViewBag.Message = "Unexpected error occurred. Please try again.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetViewResult("Error");
            }
        }
        #region Husnain Code For PAC Home Health 
        public ActionResult FacilityInfoHomeHealth(string caseType, int caseTypeKey, string className = "")
        {
            try
            {
                ViewBag.CaseTypeKey = caseTypeKey;
                ViewBag.CaseType = caseType;
                ViewBag.ClassName = className;
                ViewBag.IsItDashboard = false;
                
                var model = new post_acute_care();
                model.pac_identification_type = _uCLService.GetDefault(UclTypes.IdentificationType)?.ucd_key;
                return View("_FacilityInfoHomeHealth", model);
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Error";
                ViewBag.Message = "Unexpected error occurred. Please try again.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetViewResult("Error");
            }
        }
        #endregion
        public ActionResult PatientInfo(int caseKey)
        {
            try
            {
                if (caseKey > 0)
                {
                    var model = _caseService.GetDetailsWithoutTimeConversion(caseKey);
                    model.cas_callback = Functions.FormatAsPhoneNumber(model.cas_callback);
                    return View("_PatientInfo", model);
                }
                else
                {
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Error";
                ViewBag.Message = "Unexpected error occurred. Please try again.";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetViewResult("Error");
            }
        }
        public JsonResult GetAllAssignedFacilities()
        {
            try
            {
                var list = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                      .Select(x => new { x.Facility, x.FacilityName });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllAssignedFacilitiesSleep()
        {
            try
            {
                var list = _ealertFacilitiesService.GetAllAssignedFacilitiesSleep(User.Identity.GetUserId())
                                      .Select(x => new { x.Facility, x.FacilityName });
                var li = list.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SetCaseStatus(int caseKey)
        {
            var caseDetails = _caseService.GetDetailsWithoutTimeConversion(caseKey);
            try
            {
                if (caseDetails != null)
                {
                    caseDetails.cas_cst_key = (int)CaseStatus.Cancelled;
                    _caseService.EditCaseOnly(caseDetails);
                    return Json(new { success = true, message = "Case successfully cancelled." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Error! Case key is empty." }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "Error! Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditFacilityNavigatorCase(@case model)
        {
            try
            {
                var dbModel = _caseService.GetDetails(model.cas_key);
                if (ModelState.IsValid)
                {
                    model.cas_modified_by = loggedInUser.Id;
                    model.cas_modified_by_name = loggedInUser.FullName;
                    model.cas_modified_date = DateTime.Now.ToEST();
                    dbModel.cas_patient = model.cas_patient;
                    dbModel.cas_billing_dob = model.cas_billing_dob;
                    dbModel.cas_last_4_of_ssn = model.cas_last_4_of_ssn;
                    dbModel.cas_identification_type = model.cas_identification_type;
                    dbModel.cas_identification_number = model.cas_identification_number;
                    dbModel.cas_eta = model.cas_eta;
                    dbModel.cas_notes = model.cas_notes;

                    string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                    if (!string.IsNullOrEmpty(model.FacilityTimeZone))
                    {
                        facilityTimeZone = model.FacilityTimeZone;
                    }

                    #region ----- Converting non eastern fields to facility time zone

                    if (dbModel.cas_response_ts_notification.HasValue)
                    {
                        dbModel.cas_response_ts_notification = dbModel.cas_response_ts_notification_utc;
                    }
                    if (model.cas_metric_door_time_est.HasValue)
                    {
                        dbModel.cas_metric_door_time = model.cas_metric_door_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        dbModel.cas_metric_door_time_est = dbModel.cas_metric_door_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    if (dbModel.cas_metric_stamp_time_est.HasValue)
                    {
                        dbModel.cas_metric_stamp_time = dbModel.cas_metric_stamp_time_est?.ToUniversalTimeZone(facilityTimeZone); //.ToTimezoneFromUtc(facilityTimeZone);
                        dbModel.cas_metric_stamp_time_est = dbModel.cas_metric_stamp_time?.ToTimezoneFromUtc("Eastern Standard Time");
                    }
                    #endregion
                    _caseService.Edit(dbModel);
                    var casNumber = dbModel.cas_case_number;
                    return ShowSuccessMessageOnly("Case has been updated.", casNumber);
                }
                else
                {
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
        #endregion

        #region User Edit - Facilities Tabs
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult _FacilityUserFacilities(string userKey, string userFullName)
        {
            ViewBag.UserKey = userKey;
            ViewBag.UserFullName = userFullName;
            return PartialView();
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult GetAllFacilitiesForNavigator(DataSourceRequest request)
        {
            var facilities = _ealertFacilitiesService.GetAllFacilities(request);
            return Json(facilities, JsonRequestBehavior.AllowGet);
        }
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult AddFacilities(string userKey, string userFullName)
        {
            var assignedFacilitiesList = _ealertFacilitiesService.GetAllAssignedFacilities(userKey).Select(x => x.Facility).ToList();
            var model = new PostFacilityViewModel()
            {
                Facilities = assignedFacilitiesList,
                UserKey = userKey
            };
            var role = _facilityService.GetUserRole(userKey);
            if (role.AspNetRole.Name == "PAC Physician" || role.AspNetRole.Name == "PAC Navigator")
                return PartialView("_AddPACFacility", model);
            else
                return PartialView("_AddFacility", model);
        }
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult EditFacility(int efa_key)
        {
            var model = new PutFacilityViewModel();
            var response = _ealertFacilitiesService.GetDetails(efa_key);
            if (response != null)
            {
                model.Id = response.efa_key;
                model.UserFullName = response.AspNetUser.FirstName + " " + response.AspNetUser.LastName;
                model.UserKey = response.efa_user_key;
                model.Facility = response.efa_fac_key; //.facility.fac_key;
            }
            return PartialView("_EditFacility", model);
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult AddFacilities(PostFacilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                var list = model.Facilities.Select(x => new ealert_user_facility()
                {
                    efa_user_key = model.UserKey,
                    efa_fac_key = x,
                    efa_created_by = User.Identity.GetUserId(),
                    efa_is_active = true,
                    efa_created_date = DateTime.Now.ToEST()
                }).ToList();
                _ealertFacilitiesService.AssginFacilities(model.UserKey, list);
                return Json(new { success = true });
            }
            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult EditFacility(PutFacilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_ealertFacilitiesService.IsAlreadyExists(model.UserKey, model.Facility, model.Id))
                {
                    ModelState.AddModelError("", "Facility (" + model.FacilityName + ") for this user already exist");
                }
                else
                {
                    var modelDetail = _ealertFacilitiesService.GetDetails(model.Id);
                    modelDetail.efa_user_key = model.UserKey;
                    modelDetail.efa_fac_key = model.Facility;
                    modelDetail.efa_modified_by = User.Identity.GetUserId();
                    modelDetail.efa_is_active = true;
                    modelDetail.efa_modified_date = DateTime.Now.ToEST();
                    _ealertFacilitiesService.Edit(modelDetail);
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
        }
      //  [AccessRoles(Roles = "Super Admin")]
        public ActionResult RemvoeAssignedFacility(int id)
        {
            try
            {
                if (id > 0)
                {
                    var model = _ealertFacilitiesService.GetDetails(id);
                    var isDeleted = _ealertFacilitiesService.Delete(model);
                    return Json(new { success = isDeleted }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
        }
        #endregion

        #region Usre Edit = Case Types
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult _FacilityUserCaseTypes(string userKey, string userFullName)
        {
            ViewBag.UserKey = userKey;
            ViewBag.UserFullName = userFullName;
            return PartialView();
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult GetAllCaseTypesForNavigator(DataSourceRequest request)
        {
            var cases = _ealertCaseTypesService.GetAllCaseTypes(request);
            return Json(cases, JsonRequestBehavior.AllowGet);
        }
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult AddCaseTypes(string userKey, string userFullName)
        {
            var assignedCaseTypeList = _ealertCaseTypesService.GetAllAssignedCaseTypes(userKey).Select(x => x.CaseTypeKey);
            var model = new PostCaeTypeViewModel()
            {
                CaseTypes = assignedCaseTypeList.ToList(),
                UserKey = userKey
            };
            return PartialView("_AddCaseTypes", model);
        }
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult EditCaseType(int ect_key)
        {
            var model = new PutCaeTypeViewModel();
            var response = _ealertCaseTypesService.GetDetails(ect_key);
            if (response != null)
            {
                model.Id = response.ect_key;
                model.UserFullName = response.AspNetUser.FirstName + " " + response.AspNetUser.LastName;
                model.UserKey = response.ect_user_key;
                model.CaseTypeKey = response.ect_case_type_key;
            }
            return PartialView("_EditCaseType", model);
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult AddCaseTypes(PostCaeTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var list = model.CaseTypes.Select(x => new ealert_user_case_type()
                    {
                        ect_user_key = model.UserKey,
                        ect_case_type_key = x,
                        ect_created_by = User.Identity.GetUserId(),
                        ect_is_active = true,
                        ect_created_date = DateTime.Now.ToEST()
                    }).ToList();
                    _ealertCaseTypesService.AssginCaseTypes(model.UserKey, list);
                    return Json(new { success = true });
                }
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        [HttpPost]
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult EditCaseType(PutCaeTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_ealertCaseTypesService.IsAlreadyExists(model.UserKey, model.CaseTypeKey, model.Id))
                    {
                        ModelState.AddModelError("", "Case Type (" + model.CaseTypeName + ") for this user already exist");
                    }
                    else
                    {
                        var modelDetail = _ealertCaseTypesService.GetDetails(model.Id);
                        modelDetail.ect_user_key = model.UserKey;
                        modelDetail.ect_case_type_key = model.CaseTypeKey;
                        modelDetail.ect_modified_by = User.Identity.GetUserId();
                        modelDetail.ect_is_active = true;
                        modelDetail.ect_modified_date = DateTime.Now.ToEST();
                        _ealertCaseTypesService.Edit(modelDetail);
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        [AccessRoles(Roles = "Super Admin")]
        public ActionResult RemvoeAssignedCaseType(int id)
        {
            try
            {
                if (id > 0)
                {
                    var model = _ealertCaseTypesService.GetDetails(id);
                    var isDeleted = _ealertCaseTypesService.Delete(model);
                    return Json(new { success = isDeleted }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
        }
        #endregion

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _ealertFacilitiesService?.Dispose();
                    _ealertCaseTypesService?.Dispose();
                    _facilityService?.Dispose();
                    _caseService?.Dispose();
                    _uCLService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}