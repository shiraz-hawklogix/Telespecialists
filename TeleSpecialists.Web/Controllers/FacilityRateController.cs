using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;

namespace TeleSpecialists.Controllers
{
    public class FacilityRateController : BaseController
    {

        private readonly FacilityBillingReportService _facilityBillingReportService;
        private readonly LookupService _lookUpService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        public FacilityRateController()
        {
            _facilityBillingReportService = new FacilityBillingReportService();
            _lookUpService = new LookupService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
        }

        public ActionResult Index()
        {
            return GetViewResult();
        }

        // GET: FacilityRate
        public ActionResult FacilityRate()
        {
            return GetViewResult();
        }
        public ActionResult AvailabilityRate()
        {
            return GetViewResult();
        }
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _facilityBillingReportService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllAvailability(DataSourceRequest request)
        {
            var res = _facilityBillingReportService.GetAllAvailable(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFacility(List<int?> id, string Fac_type)
        {
            try
            {
                if (Fac_type == "system")
                {
                    IQueryable<int?> System = id.AsQueryable();
                    var facList = _lookUpService.GetAllFacilityBySystem(null, System)
                                                            .Select(m => new { fac_key = m.fac_key, fac_name = m.fac_name });
                    return Json(facList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    IQueryable<int?> region = id.AsQueryable();
                    var fac_List = _lookUpService.GetAllFacilityByRegion(null, region)
                                                            .Select(m => new { fac_key = m.fac_key, fac_name = m.fac_name });
                    return Json(fac_List, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult CreateAvailabilty()
        {
            facility_availability_rate facility_Rate = new facility_availability_rate();

            ViewBag.Facilities = _lookUpService.GetAllFacility("")
                                               .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                               .ToList()
                                               .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.Status = false;
            return GetViewResult(facility_Rate);
        }
        [HttpPost]
        public ActionResult CreateAvailabilty(facility_availability_rate facility_Rate, List<Guid> Facilities)
        {
            if (Facilities != null && Facilities.Count > 0)
            {
                DateTime startdate = (DateTime)facility_Rate.far_start_date;
                int days = DateTime.DaysInMonth(startdate.Year,startdate.Month);
                string date = startdate.Month + "/" + days + "/" + startdate.Year;
                DateTime enddate = Convert.ToDateTime(date);
                if (ModelState.IsValid)
                {
                    for (int i = 0; i < Facilities.Count; i++)
                    {
                        facility_Rate.far_fac_key = Facilities[i];
                        facility_Rate.far_created_by = loggedInUser.Id;
                        facility_Rate.far_end_date = enddate;
                        facility_Rate.far_created_by_name = loggedInUser.FullName;
                        facility_Rate.far_created_date = DateTime.Now.ToEST();
                        _facilityBillingReportService.Create(facility_Rate);

                    }
                    return ShowSuccessMessageOnly("Availability Rate Successfully Added", facility_Rate);
                }
            }

            return GetErrorResult(facility_Rate);
        }

        [HttpGet]
        public ActionResult CreateFacility()
        {
            facility_rate facility_Rate = new facility_rate();

            ViewBag.Facilities = _lookUpService.GetAllFacility("")
                                               .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                               .ToList()
                                               .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
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
                        UclTypes.CallerSource.ToInt()
                    };

            var uclDataList = _lookUpService.GetUclData(types)
                                      .Where(m => m.ucd_is_active)
                                      .OrderBy(c => c.ucd_sort_order)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key })
                                      .ToList();
            ViewBag.UclData = uclDataList.OrderBy(o => o.ucd_description);
            ViewBag.Status = false;
            return GetViewResult(facility_Rate);
        }
        [HttpPost]
        public ActionResult CreateFacility(facility_rate facility_Rate, List<Guid> Facilities)
        {
            if (Facilities != null && Facilities.Count > 0)
            {
                if (ModelState.IsValid)
                {
                    for (int i = 0; i < Facilities.Count; i++)
                    {
                        facility_Rate.fct_facility_key = Facilities[i];
                        facility_Rate.fct_range = facility_Rate.fct_starting + " - " + facility_Rate.fct_ending;
                        facility_Rate.fct_created_by = loggedInUser.Id;
                        facility_Rate.fct_created_by_name = loggedInUser.FullName;
                        facility_Rate.fct_created_date = DateTime.Now.ToEST();
                        bool alreadyExist = _facilityBillingReportService.IsAlreadyExists(facility_Rate.fct_facility_key, Convert.ToDateTime(facility_Rate.fct_start_date), Convert.ToDateTime(facility_Rate.fct_end_date), facility_Rate.fct_billing_key.ToInt(), facility_Rate.fct_starting.ToInt(), facility_Rate.fct_ending.ToInt());
                        if (!alreadyExist)
                        {
                            var verifyRange = _facilityBillingReportService.IsAlreadyExistsRange(facility_Rate.fct_facility_key, Convert.ToDateTime(facility_Rate.fct_start_date), Convert.ToDateTime(facility_Rate.fct_end_date), facility_Rate.fct_billing_key.ToInt(), facility_Rate.fct_starting.ToInt(), facility_Rate.fct_ending.ToInt());
                            if (!verifyRange)
                            {
                                _facilityBillingReportService.Create(facility_Rate);
                            }
                        }
                    }
                    return ShowSuccessMessageOnly("Facility Rate Successfully Added", facility_Rate);
                }
            }
            else
                return ShowErrorMessageOnly("No Facility Selected", facility_Rate);

            return GetErrorResult(facility_Rate);

        }

        [HttpGet]
        public ActionResult EditAvailability(int? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            facility_availability_rate facility_Rate = _facilityBillingReportService.GetDetailsAvail(Convert.ToInt32(id));

            ViewBag.Facilities = _lookUpService.GetAllFacility("")
                                               .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                               .ToList()
                                               .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.Status = true;
            return GetViewResult(facility_Rate);
        }

        public ActionResult EditAvailability(facility_availability_rate facility_Rate)
        {
            if (ModelState.IsValid)
            {
                DateTime startdate = (DateTime)facility_Rate.far_start_date;
                int days = DateTime.DaysInMonth(startdate.Year, startdate.Month);
                string date = startdate.Month + "/" + days + "/" + startdate.Year;
                DateTime enddate = Convert.ToDateTime(date);
                facility_Rate.far_end_date = enddate;
                facility_Rate.far_modified_by = loggedInUser.Id;
                facility_Rate.far_modified_by_name = loggedInUser.FullName;
                facility_Rate.far_modified_date = DateTime.Now.ToEST();

                _facilityBillingReportService.Edit(facility_Rate);
                return ShowSuccessMessageOnly("Availability Rate Successfully Updated", facility_Rate);
            }
            return GetErrorResult(facility_Rate);
        }
        [HttpGet]
        public ActionResult EditFacility(int? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            facility_rate facility_Rate = _facilityBillingReportService.GetDetails(Convert.ToInt32(id));

            ViewBag.Facilities = _lookUpService.GetAllFacility("")
                                               .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                               .ToList()
                                               .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
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
                        UclTypes.CallerSource.ToInt()
                    };

            var uclDataList = _lookUpService.GetUclData(types)
                                      .Where(m => m.ucd_is_active)
                                      .OrderBy(c => c.ucd_sort_order)
                                      .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key })
                                      .ToList();
            ViewBag.UclData = uclDataList.OrderBy(o => o.ucd_description);
            ViewBag.Status = true;
            return GetViewResult(facility_Rate);
        }

        public ActionResult EditFacility(facility_rate facility_Rate)
        {
            if (ModelState.IsValid)
            {
                facility_Rate.fct_modified_by = loggedInUser.Id;
                facility_Rate.fct_modified_by_name = loggedInUser.FullName;
                facility_Rate.fct_modified_date = DateTime.Now.ToEST();
                facility_Rate.fct_range = facility_Rate.fct_starting + " - " + facility_Rate.fct_ending;

                _facilityBillingReportService.Edit(facility_Rate);
                return ShowSuccessMessageOnly("Facility Rate Successfully Updated", facility_Rate);
            }
            return GetErrorResult(facility_Rate);
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
                    _facilityBillingReportService?.Dispose();
                    _lookUpService?.Dispose();
                    _ealertFacilitiesService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}