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
    [Authorize]
    public class RateController : BaseController
    {
        // GET: Rate
        private readonly PhysicianRateService _physicianRateService;
        private readonly RateService _rateService;
      //  private readonly AlarmService _alarmService;
        private readonly LookupService _lookUpService;
        private readonly PhysicianPercentageRateService _physicianPercentageRateService;
        private readonly Physician_billing_rates _physician_Billing_Rates;
        private readonly PhysicianHolidayRateService _physicianHolidayRateService;

        public RateController()
        {
            _physicianRateService = new PhysicianRateService();
            _rateService = new RateService();
            //_alarmService = new AlarmService();
            _lookUpService = new LookupService();
            _physicianPercentageRateService = new PhysicianPercentageRateService();
            _physician_Billing_Rates = new Physician_billing_rates();
            _physicianHolidayRateService = new PhysicianHolidayRateService();
        }

        #region  Get doctor shift rate
        public ActionResult Physicians()
        {
            return GetViewResult();
        }
        public ActionResult Users()
        {
            ViewBag.Role = RoleManager.Roles
                 .Select(m => new SelectListItem
                 {
                     Text = m.Name,
                     Value = m.Id
                 }).ToList();

            ViewBag.Error = (TempData["Error"] as bool?) ?? false;
            ViewBag.Message = TempData["StatusMessage"] as string;
            return GetViewResult();
        }
        public ActionResult GetPhysician(string id)
        {
            ViewBag.name = _physicianRateService.GetPhysicianName(id);
            _physician_Billing_Rates.phy_key = id;
            return GetViewResult(_physician_Billing_Rates);
        }
        [HttpPost]
        public ActionResult PhysicianFloorRate(DataSourceRequest request, string phy_key)
        {
            var res = _physicianRateService.GetAllRecordByPhyscian(request, phy_key);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PhysicianProductivityRate(DataSourceRequest request, string phy_key)
        {
            var res = _rateService.GetAllProductivity(request, phy_key);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PhysicianIncentiveRate(DataSourceRequest request, string phy_key)
        {
            var res = _physicianPercentageRateService.GetAllIncentive(request, phy_key);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Physician Shift/Floor Rate

        public ActionResult Index()
        {
            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _physicianRateService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create(string phy_key)
        {
            physician_shift_rate physicianRate = new physician_shift_rate();
            physicianRate.psr_phy_key = phy_key;
            return GetViewResult(physicianRate);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(physician_shift_rate physicianRate)
        {
            if (ModelState.IsValid)
            {
                //string phy_key = physicianRate.psr_phy_key;
                physicianRate.psr_created_by = loggedInUser.Id;
                physicianRate.psr_created_by_name = loggedInUser.FullName;
                physicianRate.psr_created_date = DateTime.Now.ToEST();
                bool alreadyExist = _physicianRateService.IsAlreadyExists(physicianRate.psr_phy_key, Convert.ToDateTime(physicianRate.psr_start_date), Convert.ToDateTime(physicianRate.psr_end_date), physicianRate.psr_shift.ToInt());
                if (!alreadyExist)
                {
                    _physicianRateService.Create(physicianRate);
                    return ShowSuccessMessageOnly("Physician Floor Rate Successfully Added", physicianRate);
                }
                else
                {
                    return ShowErrorMessageOnly("Record Already Exist. Please try an other dates/Shifts", physicianRate);
                }
            }
            return GetErrorResult(physicianRate);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            physician_shift_rate rateService = _physicianRateService.GetDetails(Convert.ToInt32(id));
            // get physician name 
            ViewBag.name = _physicianRateService.GetPhysicianName(rateService.psr_phy_key);
            if (rateService == null)
            {
                return HttpNotFound();
            }
            return GetViewResult(rateService);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(physician_shift_rate physicianRate)
        {
            if (ModelState.IsValid)
            {
                physicianRate.psr_modified_by = loggedInUser.Id;
                physicianRate.psr_modified_by_name = loggedInUser.FullName;
                physicianRate.psr_modified_date = DateTime.Now.ToEST();
                _physicianRateService.Edit(physicianRate);
                return ShowSuccessMessageOnly("Physician Floor Rate Successfully Updated.", physicianRate);
            }
            return GetErrorResult(physicianRate);
        }

        #endregion
        #region Physician Productivity Rate
        public ActionResult _rate()
        {
            return GetViewResult();
        }
        public ActionResult CreateRate(string phy_key)
        {
            physician_rate physicianRate = new physician_rate();
            physicianRate.rat_phy_key = phy_key;
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

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.Status = false;
            return GetViewResult(physicianRate);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRate(physician_rate physicianRate, string[] rat_shift_id, List<string> Physicians)
        {
            if (ModelState.IsValid)
            {
                for (int k = 0; k < Physicians.Count; k++)
                {
                    for (int i = 0; i < rat_shift_id.Length; i++)
                    {
                        physicianRate.rat_phy_key = Physicians[k];
                        physicianRate.rat_created_by = loggedInUser.Id;
                        physicianRate.rat_created_by_name = loggedInUser.FullName;
                        physicianRate.rat_created_date = DateTime.Now;
                        physicianRate.rat_range = physicianRate.rat_starting + " - " + physicianRate.rat_ending;
                        physicianRate.rat_shift_id = rat_shift_id[i].ToInt();
                        physicianRate.rat_shift_name = Enum.GetName(typeof(PhysicianShifts), rat_shift_id[i].ToInt());
                        bool alreadyExist = _rateService.IsAlreadyExists(physicianRate.rat_phy_key, Convert.ToDateTime(physicianRate.rat_start_date), Convert.ToDateTime(physicianRate.rat_end_date), physicianRate.rat_cas_id.ToInt(), physicianRate.rat_starting.ToInt(), physicianRate.rat_ending.ToInt(), physicianRate.rat_shift_id.ToInt());
                        if (!alreadyExist)
                        {
                            var verifyRange = _rateService.IsAlreadyExistsRange(physicianRate.rat_phy_key, Convert.ToDateTime(physicianRate.rat_start_date), Convert.ToDateTime(physicianRate.rat_end_date), physicianRate.rat_cas_id.ToInt(), physicianRate.rat_starting.ToInt(), physicianRate.rat_ending.ToInt(), (int)physicianRate.rat_shift_id);
                            if (!verifyRange)
                            {
                                _rateService.Create(physicianRate);
                            }
                        }
                    }
                }
                return ShowSuccessMessageOnly("Physician Productivity Rate Successfully Added", physicianRate);
            }
            return GetErrorResult(physicianRate);
        }

        [HttpPost]
        public ActionResult GetAllIndexRate(DataSourceRequest request)
        {
            var res = _rateService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditRate(int? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            physician_rate rateService = _rateService.GetDetails(Convert.ToInt32(id));
            // get physician name 
            ViewBag.name = _rateService.GetPhysicianName(rateService.rat_phy_key);
            if (rateService == null)
            {
                return HttpNotFound();
            }
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
            return GetViewResult(rateService);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRate(physician_rate physicianRate)
        {
            if (ModelState.IsValid)
            {
                physicianRate.rat_modified_by = loggedInUser.Id;
                physicianRate.rat_modified_by_name = loggedInUser.FullName;
                physicianRate.rat_modified_date = DateTime.Now;
                physicianRate.rat_range = physicianRate.rat_starting + " - " + physicianRate.rat_ending;
                physicianRate.rat_shift_name = Enum.GetName(typeof(PhysicianShifts), physicianRate.rat_shift_id);

                _rateService.Edit(physicianRate);
                return ShowSuccessMessageOnly("Physician Productivity Rate Successfully Updated", physicianRate);

            }
            return GetErrorResult(physicianRate);
        }


        #endregion
        #region  Physician Percentage/Incentive Rate
        public ActionResult PercentageRate()
        {
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetAllRate(DataSourceRequest request)
        {
            var res = _physicianPercentageRateService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreatePercentage(string phy_key)
        {
            physician_percentage_rate physicianPercentage = new physician_percentage_rate();
            physicianPercentage.ppr_phy_key = phy_key;
            return GetViewResult(physicianPercentage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePercentage(physician_percentage_rate physicianPercentage)
        {
            if (ModelState.IsValid)
            {
                physicianPercentage.ppr_created_by = loggedInUser.Id;
                physicianPercentage.ppr_created_by_name = loggedInUser.FullName;
                physicianPercentage.ppr_created_date = DateTime.Now.ToEST();
                bool alreadyExist = _physicianPercentageRateService.IsAlreadyExists(physicianPercentage.ppr_phy_key, Convert.ToDateTime(physicianPercentage.ppr_start_date), Convert.ToDateTime(physicianPercentage.ppr_end_date), physicianPercentage.ppr_shift_id.ToInt());
                if (!alreadyExist)
                {
                    _physicianPercentageRateService.Create(physicianPercentage);
                    return ShowSuccessMessageOnly("Physician Incentive Rate Successfully Added", physicianPercentage);
                }
                else
                {
                    return ShowErrorMessageOnly("Record Already Exist. Please try an other dates/Shifts", physicianPercentage);
                }
            }
            return GetErrorResult(physicianPercentage);
        }

        public ActionResult EditPercentage(int? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            physician_percentage_rate rateService = _physicianPercentageRateService.GetDetails(Convert.ToInt32(id));
            // get physician name 
            ViewBag.name = _physicianPercentageRateService.GetPhysicianName(rateService.ppr_phy_key);
            if (rateService == null)
            {
                return HttpNotFound();
            }
            return GetViewResult(rateService);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPercentage(physician_percentage_rate physicianPercentage)
        {
            if (ModelState.IsValid)
            {
                physicianPercentage.ppr_modified_by = loggedInUser.Id;
                physicianPercentage.ppr_modified_by_name = loggedInUser.FullName;
                physicianPercentage.ppr_modified_date = DateTime.Now.ToEST();
                _physicianPercentageRateService.Edit(physicianPercentage);
                return ShowSuccessMessageOnly("Physician Incentive Rate Successfully Updated", physicianPercentage);
            }
            return GetErrorResult(physicianPercentage);
        }

        #endregion
        #region Physician Holiday/Custom Rate
        public bool PhysicianHoliday(ScheduleRecordViewModel _UserSchedule, string user_name, string user_key, long uss_key)
        {
            bool status = false;
            var obj = _physicianHolidayRateService.GetCustomRate(uss_key);
            if (ModelState.IsValid)
            {
                if (obj == null)
                {
                    status = insert(_UserSchedule, user_key, user_name, uss_key);
                }
                else
                {
                    status = update(_UserSchedule, user_key, user_name, obj, uss_key);
                }
            }
            return status;
        }
        private bool insert(ScheduleRecordViewModel _UserSchedule, string user_key, string user_name, long uss_key)
        {
            physician_holiday_rate _holidayRate = new physician_holiday_rate();
            _holidayRate.phr_phy_key = _UserSchedule.UserId;
            _holidayRate.phr_rate = _UserSchedule.Rate;
            _holidayRate.phr_created_by = user_key;
            _holidayRate.phr_created_by_name = user_name;
            _holidayRate.phr_date = _UserSchedule.ScheduleDate.Date;
            _holidayRate.phr_created_date = DateTime.Now.ToEST();
            _holidayRate.phr_uss_key = uss_key;
            _holidayRate.phr_shift_key = _UserSchedule.ShiftId;
            _physicianHolidayRateService.Create(_holidayRate);
            return true;
        }
        private bool update(ScheduleRecordViewModel _UserSchedule, string user_key, string user_name, physician_holiday_rate obj, long uss_key)
        {
            obj.phr_phy_key = _UserSchedule.UserId;
            obj.phr_rate = _UserSchedule.Rate;
            obj.phr_modified_by = user_key;
            obj.phr_shift_key = _UserSchedule.ShiftId;
            obj.phr_modified_by_name = user_name;
            obj.phr_date = _UserSchedule.ScheduleDate.Date;
            obj.phr_modified_date = DateTime.Now.ToEST();
            _physicianHolidayRateService.Edit(obj);
            return true;
        }
        #endregion
        #region Get Custom Rate
        [HttpPost]
        public JsonResult getCustomRate(string uss_key)
        {
            long id = Convert.ToInt64(uss_key);
            var getRecord = _physicianHolidayRateService.GetCustomRate(id);
            if (getRecord != null)
            {
                return Json(getRecord.phr_rate, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }


        }
        #endregion
        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                   // _alarmService.Dispose();
                    _physicianHolidayRateService?.Dispose();
                    _physicianPercentageRateService?.Dispose();
                    _physicianRateService?.Dispose();
                    _rateService?.Dispose();
                    _lookUpService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}