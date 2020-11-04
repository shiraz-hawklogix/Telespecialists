using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.BLL.Model;
using System.Net;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels.Schedule;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(NotInRoles = "Facility Admin")]
    public class ScheduleController : BaseController
    {
        #region private-members
        private readonly SchedulerService _schedulerService;
        private readonly AdminService _adminService;
        private readonly RateController _RateController;
        private readonly PhysicianHolidayRateService _physicianHolidayRateService;
        private readonly PhysicianDictionary _physicianDictionary;
        private readonly SleepService _sleepService;
        private readonly NHService _nHService;

        private static string physicianType { get; set; }
        #endregion

        #region constructor
        public ScheduleController()
        {
            this._schedulerService = new SchedulerService();
            this._adminService = new AdminService();
            this._RateController = new RateController();
            this._physicianHolidayRateService = new PhysicianHolidayRateService();
            this._physicianDictionary = new PhysicianDictionary();
            this._sleepService = new SleepService();
            this._nHService = new NHService();
        }
        #endregion

        #region get-methods

        public ActionResult Main()
        {
            ViewBag.isSleep = loggedInUser.IsSleep;
            ViewBag.nhAlert = loggedInUser.NHAlert;
            ViewBag.isStrokeAlert = loggedInUser.IsStrokeAlert;
            return GetViewResult();
        }

        public ActionResult Index(string phy_type)
        {
            physicianType = phy_type;
            ViewBag.typeforPhy = phy_type;
            ViewBag.SuperAdmin = User.IsInRole(UserRoles.SuperAdmin.ToDescription()) ? "SuperAdmin" : "";
            bool isAdmin = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
               || User.IsInRole(UserRoles.Administrator.ToDescription())
               || User.IsInRole(UserRoles.RRCManager.ToDescription())
               || User.IsInRole(UserRoles.RRCDirector.ToDescription())
               || User.IsInRole(UserRoles.Finance.ToDescription())
               || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
               || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
               || User.IsInRole(UserRoles.QPS.ToDescription())
               || User.IsInRole(UserRoles.QualityDirector.ToDescription())
               || User.IsInRole(UserRoles.QualityTeam.ToDescription())
               || User.IsInRole(UserRoles.VPQuality.ToDescription())
               || User.IsInRole(UserRoles.CredentialingTeam.ToDescription()));
            ViewBag.Physicians = _schedulerService.GetScheduledPhysicians(isAdmin, loggedInUser.Id,  phy_type : phy_type);
            return GetViewResult();
        }
      
        public ActionResult Import()
        {
            return GetViewResult(new SchedulerResponseViewModel());
        }
        public ActionResult GetAll(DataSourceRequest request, DateTime startDate, DateTime endDate, List<string> Physicians, string SchType)
        {
            try
            {
                bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
                    || User.IsInRole(UserRoles.Administrator.ToDescription())
                    || User.IsInRole(UserRoles.Navigator.ToDescription())
                    || User.IsInRole(UserRoles.RRCManager.ToDescription())
                    || User.IsInRole(UserRoles.RRCDirector.ToDescription())
                    || User.IsInRole(UserRoles.Finance.ToDescription())
                    || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                    || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
                    || User.IsInRole(UserRoles.QPS.ToDescription())
                    || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                    || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                    || User.IsInRole(UserRoles.VPQuality.ToDescription())
                    || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
                    );
                bool isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin.ToDescription());

                var res = _schedulerService.GetAllPhyScheduals(isGetAllRequeset, loggedInUser.Id, startDate, endDate, Physicians, physicianType, isSuperAdmin, SchType);

                #region Get Only Required Physician
                var ids = _physicianDictionary.GetRecordAsList(physicianType);
                // request.Take = 0;
                var foundIds = _adminService.GetAllUsersIds(ids);
                HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
                var matchIds = resIds.Intersect(foundIds).ToList();
                var _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
                double? daysDiff = (endDate - startDate).TotalDays;
                #endregion

                bool publishStatus = false;

                #region flagging
                if (User.IsInRole(UserRoles.SuperAdmin.ToDescription()) && SchType == "Physician")
                {
                    #region check fewer physcian on month/work/workweek schedual
                    if (daysDiff != null && daysDiff > 3 && startDate != endDate)
                    {
                        var groupedSecdualedList = _res.GroupBy(u => u.ScheduleDate).ToList();
                        foreach (var item in groupedSecdualedList)
                        {
                            bool flag = _schedulerService.getAllPhyscianListDay(item.Key.ToString());
                            if (flag)
                            {
                                var result = _res.Where(x => x.ScheduleDate == item.Key).FirstOrDefault();
                                //result.TitleBig = result.TitleBig + "#FLAG#" + result.ScheduleDate.Month + "#" + result.ScheduleDate.Day + "#";
                                result.isFlag = true;
                                result.isFlagMonth = result.ScheduleDate.Month;
                                result.isFlagDay = result.ScheduleDate.Day;
                            }
                        }
                    }
                    #endregion

                    #region check schedule is publish
                    if (startDate != endDate)
                    {
                        //DateTime dddd = startDate.AddDays(endDate.Day);
                        //DateTime middleDate = startDate.AddDays((endDate.Day) / 2);
                        //int month = (startDate.Month + endDate.Month) / 2;
                        int month = startDate.AddDays(20).Month;
                        var MonthList = _res.Where(u => u.ScheduleDate.Month == month).ToList();
                        if (MonthList.Count > 0)
                        {
                            if (MonthList.Any(x => x.IsPublish == false))
                            {
                                publishStatus = false;
                            }
                            else
                            {
                                publishStatus = true;
                            }
                        }
                        else
                        {
                            publishStatus = true;
                        }
                    }
                    #endregion
                }
                #endregion

                #region  Physician Today index Rate 
                if (startDate == endDate)
                {
                    _res = CallForindexCount(_res);
                }
                #endregion

                var newres = _res.Select(m => new
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Title = m.Title,
                    TitleBig = m.TitleBig,
                    Description = m.Description,
                    ScheduleDate = m.ScheduleDate.FormatDateTime(),
                    Start = m.Start.FormatDateTime(),
                    End = m.End.FormatDateTime(),
                    IsActive = m.IsActive,
                    FullName = m.FullName,
                    IsAllDay = m.IsAllDay,
                    Rate = m.Rate,
                    ShiftId = m.ShiftId,
                    PhyIndexRate = m.PhyIndexRate,
                    PublishStatus = publishStatus,
                    scheduleGetAll = true,
                    isFlag = m.isFlag,
                    isFlagMonth = m.isFlagMonth,
                    isFlagDay = m.isFlagDay
                });

                return Json(newres, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(null);
            }

        }
        //public ActionResult GetAll(DataSourceRequest request, DateTime startDate, DateTime endDate, List<string> Physicians)
        //{
        //    try
        //    {
        //        bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
        //            || User.IsInRole(UserRoles.Administrator.ToDescription())
        //            || User.IsInRole(UserRoles.Navigator.ToDescription())
        //            || User.IsInRole(UserRoles.RRCManager.ToDescription())
        //            || User.IsInRole(UserRoles.RRCDirector.ToDescription())
        //            || User.IsInRole(UserRoles.Finance.ToDescription())
        //            || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
        //            || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
        //            || User.IsInRole(UserRoles.QPS.ToDescription())
        //            || User.IsInRole(UserRoles.QualityDirector.ToDescription())
        //            || User.IsInRole(UserRoles.QualityTeam.ToDescription())
        //            || User.IsInRole(UserRoles.VPQuality.ToDescription())
        //            );
        //        var res = _schedulerService.GetAll(isGetAllRequeset, loggedInUser.Id, startDate, endDate, Physicians, physicianType)
        //                                   .Select(m => new
        //                                   {
        //                                       Id = m.uss_key,
        //                                       UserId = m.uss_user_id,
        //                                       Title = m.AspNetUser.UserInitial,
        //                                       //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
        //                                       TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), ("#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
        //                                       Description = string.IsNullOrEmpty(m.uss_description) ? "" : m.uss_description,
        //                                       ScheduleDate = m.uss_date.FormatDateTime(),
        //                                       Start = m.uss_time_from_calc.Value.FormatDateTime(),
        //                                       End = m.uss_time_to_calc.Value.FormatDateTime(),
        //                                       IsActive = m.uss_is_active,
        //                                       FullName = string.Format("{0} {1}", m.AspNetUser.FirstName, m.AspNetUser.LastName),
        //                                       IsAllDay = (m.uss_time_to - TimeSpan.TicksPerDay) > 0,
        //                                       Rate = m.uss_custome_rate,
        //                                       ShiftId = m.uss_shift_key,
        //                                       PhyIndexRate  = m.AspNetUser.CredentialIndex
        //                                   });

        //        #region Get Only Required Physician
        //        var ids = _physicianDictionary.GetRecordAsList(physicianType);
        //       // request.Take = 0;
        //        var foundIds = _adminService.GetAllUsersIds(ids);
        //        HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
        //        var matchIds = resIds.Intersect(foundIds).ToList();
        //        var _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
        //        #endregion
        //        #region check fewer physcian on month schedual
        //        double? daysDiff = (endDate - startDate).TotalDays;
        //        if (daysDiff != null && daysDiff > 29)
        //        {
        //            List<ScheduleRecordViewModel> list = new List<ScheduleRecordViewModel>();
        //            ScheduleRecordViewModel obj;

        //            foreach (var item in _res)
        //            {
        //                obj = new ScheduleRecordViewModel();
        //                obj.Id = item.Id;
        //                obj.UserId = item.UserId;
        //                obj.Title = item.Title;
        //                obj.TitleBig = item.TitleBig;
        //                obj.Description = item.Description;
        //                obj._scheduleData = item.ScheduleDate;
        //                obj._start = item.Start;
        //                obj._end = item.End;
        //                obj.IsActive = (bool)item.IsActive;
        //                obj.FullName = item.FullName;
        //                obj.Rate = item.Rate;
        //                obj.ShiftId = item.ShiftId;
        //                obj.PhyIndexRate = (decimal)item.PhyIndexRate;
        //                obj.Start = Convert.ToDateTime(item.Start);
        //                obj.End = Convert.ToDateTime(item.End);
        //                list.Add(obj);
        //            }

        //            var groupedSecdualedList = list.GroupBy(u => u._scheduleData).ToList();
        //            foreach (var item in groupedSecdualedList)
        //            {
        //                bool flag = _schedulerService.getAllPhyscianListDay(item.Key.ToString());
        //                if (flag)
        //                {

        //                    var result = list.Where(x => x._scheduleData == item.Key).OrderBy(x => x.Start).FirstOrDefault();
        //                    result.TitleBig = "<span class='k-more-events k-button' style='position: relative;right:10px; top: -5px; background: border-box; border: none; width: 5px;'><i class='fa fa-flag' style='color:white'></i></span>" + result.TitleBig;
        //                }
        //            }

        //            var mynewResM = list.Select(firstObj => new {
        //                Id = firstObj.Id,
        //                UserId = firstObj.UserId,
        //                Title = firstObj.Title,
        //                TitleBig = firstObj.TitleBig,
        //                Description = firstObj.Description,
        //                ScheduleDate = firstObj._scheduleData,
        //                Start = firstObj._start,
        //                End = firstObj._end,
        //                IsActive = firstObj.IsActive,
        //                FullName = firstObj.FullName,
        //                IsAllDay = firstObj.IsAllDay,
        //                Rate = firstObj.Rate,
        //                ShiftId = firstObj.ShiftId,
        //                PhyIndexRate = firstObj.PhyIndexRate
        //            });

        //            return Json(mynewResM, JsonRequestBehavior.AllowGet);
        //        }
        //        #endregion

        //        #region  Physician Today index Rate 
        //        if (startDate == endDate)
        //        {
        //            List<ScheduleRecordViewModel> list = new List<ScheduleRecordViewModel>();
        //            ScheduleRecordViewModel obj;

        //            foreach (var item in _res)
        //            {
        //                obj = new ScheduleRecordViewModel();
        //                obj.Id = item.Id;
        //                obj.UserId = item.UserId;
        //                obj.Title = item.Title;
        //                obj.TitleBig = item.TitleBig;
        //                obj.Description = item.Description;
        //                obj._scheduleData = item.ScheduleDate;
        //                obj._start = item.Start;
        //                obj._end = item.End;
        //                obj.IsActive = (bool)item.IsActive;
        //                obj.FullName = item.FullName;
        //                obj.Rate = item.Rate;
        //                obj.ShiftId = item.ShiftId;
        //                obj.PhyIndexRate = (decimal)item.PhyIndexRate;
        //                obj.Start = Convert.ToDateTime(item.Start);
        //                obj.End = Convert.ToDateTime(item.End);
        //                list.Add(obj);
        //            }
        //            var arrangedList = CallForindexCount(list);
        //            var mynewRes = arrangedList.Select(firstObj => new {
        //                Id = firstObj.Id,
        //                UserId = firstObj.UserId,
        //                Title = firstObj.Title,
        //                //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
        //                TitleBig = firstObj.TitleBig,
        //                Description = firstObj.Description,
        //                ScheduleDate = firstObj._scheduleData,
        //                Start = firstObj._start,
        //                End = firstObj._end,
        //                IsActive = firstObj.IsActive,
        //                FullName = firstObj.FullName,
        //                IsAllDay = firstObj.IsAllDay,
        //                Rate = firstObj.Rate,
        //                ShiftId = firstObj.ShiftId,
        //                PhyIndexRate = firstObj.PhyIndexRate
        //            });
        //            return Json(mynewRes, JsonRequestBehavior.AllowGet);

        //        }
        //        #endregion

        //        return Json(_res, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //        return Json(null);
        //    }

        //}
        public ActionResult GetScheduledPhysicians(bool onlySchedulePhys)
        {
            // check user role
            bool isgetAllRequest = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
               || User.IsInRole(UserRoles.Administrator.ToDescription())
               || User.IsInRole(UserRoles.Navigator.ToDescription())
               || User.IsInRole(UserRoles.RRCManager.ToDescription())
               || User.IsInRole(UserRoles.RRCDirector.ToDescription())
               || User.IsInRole(UserRoles.Finance.ToDescription())
                   || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                   || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
                   || User.IsInRole(UserRoles.QPS.ToDescription())
                   || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                   || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                   || User.IsInRole(UserRoles.VPQuality.ToDescription())
                   || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
               );
            // return data
            return Json(_schedulerService.GetScheduledPhysicians(isgetAllRequest, loggedInUser.Id, onlySchedulePhys, physicianType), JsonRequestBehavior.AllowGet);
        }
        public string GetImage(string id)
        {
            return _schedulerService.GetImage(id);
        }
        public ActionResult RenderImage(string id)
        {
            string base64String = _schedulerService.GetImage(id)?.Replace("data:image/jpeg;base64,", "");
            if (base64String != null)
                return File(Convert.FromBase64String(base64String), "image/jpeg");
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadSample()
        {
            _schedulerService.PrepareDownloadSample(Server.MapPath("/Content"));
            return File("/Content/Schedule-Template.csv", "text/csv", "Schedule-Template.csv");
        }

        // [DeleteFileAttribute]
        public ActionResult ExportSchedule(string startDate, string endDate, List<string> Physicians)
        {
            if (Physicians.Count() == 1)
            {
                if (Physicians[0] == "null")
                {
                    Physicians = null;
                }
                else
                {
                    Physicians = Physicians[0].Split(',').ToList();
                }
            }
            bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
                    || User.IsInRole(UserRoles.Administrator.ToDescription())
                    || User.IsInRole(UserRoles.Navigator.ToDescription())
                    || User.IsInRole(UserRoles.RRCManager.ToDescription())
                    || User.IsInRole(UserRoles.RRCDirector.ToDescription())
                    || User.IsInRole(UserRoles.Finance.ToDescription())

                    || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                    || User.IsInRole(UserRoles.MedicalStaff.ToDescription())

                    || User.IsInRole(UserRoles.QPS.ToDescription())
                    || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                    || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                    || User.IsInRole(UserRoles.VPQuality.ToDescription())
                    || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
                    );
            List<ScheduleExportVM> res = _schedulerService.GetAll(isGetAllRequeset, loggedInUser.Id, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), Physicians, physicianType)
                                       .Select(m => new ScheduleExportVM
                                       {
                                           Id = m.uss_key,
                                           UserId = m.uss_user_id,
                                           Title = m.AspNetUser.UserInitial,
                                           TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#"),
                                           Description = string.IsNullOrEmpty(m.uss_description) ? "" : m.uss_description,
                                           ScheduleDate = m.uss_date.FormatDateTime(),
                                           Start = m.uss_time_from_calc.Value.FormatDateTime(),
                                           End = m.uss_time_to_calc.Value.FormatDateTime(),
                                           IsActive = m.uss_is_active,
                                           FullName = string.Format("{0} {1}", m.AspNetUser.FirstName, m.AspNetUser.LastName),
                                           IsAllDay = (m.uss_time_to - TimeSpan.TicksPerDay) > 0,
                                           Rate = m.uss_custome_rate,
                                           ShiftId = m.uss_shift_key,
                                           PhyIndexRate = m.AspNetUser.CredentialIndex
                                       }).ToList();

            #region Get Only Required Physician
            var ids = _physicianDictionary.GetRecordAsList(physicianType);
            var foundIds = _adminService.GetAllUsersIds(ids);
            HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
            var matchIds = resIds.Intersect(foundIds).ToList();
            List<ScheduleExportVM> _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
            #endregion

            string filePath = Server.MapPath("~/Content/ScheduleExport");
            ViewBag.TempFilePath = filePath + ".csv";
            _schedulerService.PrepareScheduleExport(filePath, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), _res);
            return File("~/Content/ScheduleExport" + ".csv", "text/csv", "Schedule" + DateTime.Now.ToEST() + ".csv");
        }

        [HttpPost]
        public ActionResult GetFewerPhyfacilities(string startDate, string endDate)
        {
            var result = _schedulerService.getAllPhyscianList(startDate, endDate);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PublishSchedule(int month, int year)
        {
            var result = _schedulerService.PublishSchedule(month, year);
            return Json(new { Status = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CheckSchedulePublishFlag()
        {
            bool result = _schedulerService.getCheckSchedulePublishFlag();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Sleep Work
        public ActionResult GetAllSleep(DataSourceRequest request, DateTime startDate, DateTime endDate, List<string> Physicians)
        {
            try
            {
                bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
                    || User.IsInRole(UserRoles.Administrator.ToDescription())
                    || User.IsInRole(UserRoles.Navigator.ToDescription())
                    || User.IsInRole(UserRoles.RRCManager.ToDescription())
                    || User.IsInRole(UserRoles.RRCDirector.ToDescription())
                    || User.IsInRole(UserRoles.Finance.ToDescription())
                    || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                    || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
                    || User.IsInRole(UserRoles.QPS.ToDescription())
                    || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                    || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                    || User.IsInRole(UserRoles.VPQuality.ToDescription())
                    || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
                    );
                var res = _sleepService.GetAll(isGetAllRequeset, loggedInUser.Id, startDate, endDate, Physicians, physicianType)
                                           .Select(m => new
                                           {
                                               Id = m.uss_key,
                                               UserId = m.uss_user_id,
                                               Title = m.AspNetUser.UserInitial,
                                               //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                                               TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), ("#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                                               Description = string.IsNullOrEmpty(m.uss_description) ? "" : m.uss_description,
                                               ScheduleDate = m.uss_date.FormatDateTime(),
                                               Start = m.uss_time_from_calc.Value.FormatDateTime(),
                                               End = m.uss_time_to_calc.Value.FormatDateTime(),
                                               IsActive = m.uss_is_active,
                                               FullName = string.Format("{0} {1}", m.AspNetUser.FirstName, m.AspNetUser.LastName),
                                               IsAllDay = (m.uss_time_to - TimeSpan.TicksPerDay) > 0,
                                               Rate = m.uss_custome_rate,
                                               ShiftId = m.uss_shift_key,
                                               PhyIndexRate = m.AspNetUser.CredentialIndex
                                           });

                #region Get Only Required Physician
                var ids = _physicianDictionary.GetRecordAsList(physicianType);
                // request.Take = 0;
                var foundIds = _adminService.GetAllUsersIds(ids);
                HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
                var matchIds = resIds.Intersect(foundIds).ToList();
                var _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
                #endregion

                #region  Physician Today index Rate 
                if (startDate == endDate)
                {
                    List<ScheduleRecordViewModel> list = new List<ScheduleRecordViewModel>();
                    ScheduleRecordViewModel obj;

                    foreach (var item in _res)
                    {
                        obj = new ScheduleRecordViewModel();
                        obj.Id = item.Id;
                        obj.UserId = item.UserId;
                        obj.Title = item.Title;
                        obj.TitleBig = item.TitleBig;
                        obj.Description = item.Description;
                        obj._scheduleData = item.ScheduleDate;
                        obj._start = item.Start;
                        obj._end = item.End;
                        obj.IsActive = (bool)item.IsActive;
                        obj.FullName = item.FullName;
                        obj.Rate = item.Rate;
                        obj.ShiftId = item.ShiftId;
                        obj.PhyIndexRate = (decimal)item.PhyIndexRate;
                        obj.Start = Convert.ToDateTime(item.Start);
                        obj.End = Convert.ToDateTime(item.End);
                        list.Add(obj);
                    }
                    var arrangedList = CallForindexCount(list);
                    var mynewRes = arrangedList.Select(firstObj => new {
                        Id = firstObj.Id,
                        UserId = firstObj.UserId,
                        Title = firstObj.Title,
                        //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                        TitleBig = firstObj.TitleBig,
                        Description = firstObj.Description,
                        ScheduleDate = firstObj._scheduleData,
                        Start = firstObj._start,
                        End = firstObj._end,
                        IsActive = firstObj.IsActive,
                        FullName = firstObj.FullName,
                        IsAllDay = firstObj.IsAllDay,
                        Rate = firstObj.Rate,
                        ShiftId = firstObj.ShiftId,
                        PhyIndexRate = firstObj.PhyIndexRate
                    });
                    return Json(mynewRes, JsonRequestBehavior.AllowGet);

                }
                #endregion

                return Json(_res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(null);
            }

        }
        [HttpPost]
        public ActionResult CreateSleep(ScheduleRecordViewModel model)
        {
            try
            {
                var scheduleRecord = new user_schedule_sleep
                {
                    uss_key = model.Id,
                    uss_user_id = model.UserId,
                    uss_time_from_calc = model.Start,
                    uss_time_to_calc = model.End,
                    uss_date = model.Start.Date, // Start date -- current selected date of calender/scheduler
                    uss_description = model.Description,
                    uss_created_by = loggedInUser.Id,
                    uss_created_by_name = loggedInUser.FullName,
                    uss_created_date = DateTime.Now.ToEST(),
                    uss_is_active = true,
                    uss_custome_rate = model.Rate,
                    uss_shift_key = model.ShiftId
                };

                var response = _sleepService.AddSchedule(scheduleRecord, model.IsAllDay);
                if (!response.Value)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                //// Added by axim
                /*
                if(model.Rate != 0)
                {
                    _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                }
                */
                //// ended by axim
                var newScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                if (newScheduleEntry != null)
                    return ParseScheduleResult(newScheduleEntry);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult UpdateSleep(ScheduleRecordViewModel model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var dbModel = _sleepService.GetScheduleById(model.Id).FirstOrDefault();
                    if (dbModel != null)
                    {
                        dbModel.uss_key = model.Id;
                        dbModel.uss_user_id = model.UserId;
                        dbModel.uss_time_from_calc = model.Start;
                        dbModel.uss_time_to_calc = model.End;
                        dbModel.uss_date = model.ScheduleDate.Date;
                        dbModel.uss_description = model.Description;
                        dbModel.uss_modified_by = loggedInUser.Id;
                        dbModel.uss_is_active = true;
                        dbModel.uss_modified_by_name = loggedInUser.FullName;
                        dbModel.uss_modified_date = DateTime.Now.ToEST();
                        dbModel.uss_custome_rate = model.Rate;
                        dbModel.uss_shift_key = model.ShiftId;


                        var response = _sleepService.UpdateSchedule(dbModel, model.IsAllDay);
                        if (!response.Value)
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                        //// Added by axim
                        /*
                        if (model.Rate != 0)
                        {
                            _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                        }
                        */
                        //// ended by axim
                        var updatedScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                        if (updatedScheduleEntry != null)
                            return ParseScheduleResult(updatedScheduleEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult DestroySleep(ScheduleRecordViewModel model)
        {
            try
            {
                if (model != null)
                {
                    //_physicianHolidayRateService.Remove(model.Id.ToInt());
                    _sleepService.RemoveSchedule(model.Id.ToInt());
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return Json(null);
        }
        #endregion


        #region NH Work
        public ActionResult GetAllNH(DataSourceRequest request, DateTime startDate, DateTime endDate, List<string> Physicians)
        {
            try
            {
                bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
                    || User.IsInRole(UserRoles.Administrator.ToDescription())
                    || User.IsInRole(UserRoles.Navigator.ToDescription())
                    || User.IsInRole(UserRoles.RRCManager.ToDescription())
                    || User.IsInRole(UserRoles.RRCDirector.ToDescription())
                    || User.IsInRole(UserRoles.Finance.ToDescription())
                    || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                    || User.IsInRole(UserRoles.MedicalStaff.ToDescription())
                    || User.IsInRole(UserRoles.QPS.ToDescription())
                    || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                    || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                    || User.IsInRole(UserRoles.VPQuality.ToDescription())
                    || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
                    );
                var res = _nHService.GetAll(isGetAllRequeset, loggedInUser.Id, startDate, endDate, Physicians, physicianType)
                                           .Select(m => new
                                           {
                                               Id = m.uss_key,
                                               UserId = m.uss_user_id,
                                               Title = m.AspNetUser.UserInitial,
                                               //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                                               TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), ("#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                                               Description = string.IsNullOrEmpty(m.uss_description) ? "" : m.uss_description,
                                               ScheduleDate = m.uss_date.FormatDateTime(),
                                               Start = m.uss_time_from_calc.Value.FormatDateTime(),
                                               End = m.uss_time_to_calc.Value.FormatDateTime(),
                                               IsActive = m.uss_is_active,
                                               FullName = string.Format("{0} {1}", m.AspNetUser.FirstName, m.AspNetUser.LastName),
                                               IsAllDay = (m.uss_time_to - TimeSpan.TicksPerDay) > 0,
                                               Rate = m.uss_custome_rate,
                                               ShiftId = m.uss_shift_key,
                                               PhyIndexRate = m.AspNetUser.CredentialIndex
                                           });

                #region Get Only Required Physician
                var ids = _physicianDictionary.GetRecordAsList(physicianType);
                // request.Take = 0;
                var foundIds = _adminService.GetAllUsersIds(ids);
                HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
                var matchIds = resIds.Intersect(foundIds).ToList();
                var _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
                #endregion

                #region  Physician Today index Rate 
                if (startDate == endDate)
                {
                    List<ScheduleRecordViewModel> list = new List<ScheduleRecordViewModel>();
                    ScheduleRecordViewModel obj;

                    foreach (var item in _res)
                    {
                        obj = new ScheduleRecordViewModel();
                        obj.Id = item.Id;
                        obj.UserId = item.UserId;
                        obj.Title = item.Title;
                        obj.TitleBig = item.TitleBig;
                        obj.Description = item.Description;
                        obj._scheduleData = item.ScheduleDate;
                        obj._start = item.Start;
                        obj._end = item.End;
                        obj.IsActive = (bool)item.IsActive;
                        obj.FullName = item.FullName;
                        obj.Rate = item.Rate;
                        obj.ShiftId = item.ShiftId;
                        obj.PhyIndexRate = (decimal)item.PhyIndexRate;
                        obj.Start = Convert.ToDateTime(item.Start);
                        obj.End = Convert.ToDateTime(item.End);
                        list.Add(obj);
                    }
                    var arrangedList = CallForindexCount(list);
                    var mynewRes = arrangedList.Select(firstObj => new {
                        Id = firstObj.Id,
                        UserId = firstObj.UserId,
                        Title = firstObj.Title,
                        //TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), (m.AspNetUser.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#")),
                        TitleBig = firstObj.TitleBig,
                        Description = firstObj.Description,
                        ScheduleDate = firstObj._scheduleData,
                        Start = firstObj._start,
                        End = firstObj._end,
                        IsActive = firstObj.IsActive,
                        FullName = firstObj.FullName,
                        IsAllDay = firstObj.IsAllDay,
                        Rate = firstObj.Rate,
                        ShiftId = firstObj.ShiftId,
                        PhyIndexRate = firstObj.PhyIndexRate
                    });
                    return Json(mynewRes, JsonRequestBehavior.AllowGet);

                }
                #endregion

                return Json(_res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(null);
            }

        }
        [HttpPost]
        public ActionResult CreateNH(ScheduleRecordViewModel model)
        {
            try
            {
                var scheduleRecord = new user_schedule
                {
                    uss_key = model.Id,
                    uss_user_id = model.UserId,
                    uss_time_from_calc = model.Start,
                    uss_time_to_calc = model.End,
                    uss_date = model.Start.Date, // Start date -- current selected date of calender/scheduler
                    uss_description = model.Description,
                    uss_created_by = loggedInUser.Id,
                    uss_created_by_name = loggedInUser.FullName,
                    uss_created_date = DateTime.Now.ToEST(),
                    uss_is_active = true,
                    uss_custome_rate = model.Rate,
                    uss_shift_key = model.ShiftId,
                    uss_is_publish = true,
                    uss_date_num = Convert.ToInt64(model.Start.Date.Year.ToString() + model.Start.Date.DayOfYear.ToString("000")),
                    uss_time_from_calc_num = Convert.ToInt64(model.Start.Year.ToString() + model.Start.DayOfYear.ToString("000") + model.Start.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim()),
                    uss_time_to_calc_num = Convert.ToInt64(model.End.Year.ToString() + model.End.DayOfYear.ToString("000") + model.End.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim()),
                };

                var response = _nHService.AddSchedule(scheduleRecord, model.IsAllDay);
                if (!response.Value)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                //// Added by axim
                /*
                if(model.Rate != 0)
                {
                    _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                }
                */
                //// ended by axim
                var newScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                if (newScheduleEntry != null)
                    return ParseScheduleResult(newScheduleEntry);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult UpdateNH(ScheduleRecordViewModel model)
        {
            try
            {
                if (model.Id > 0)
                {
                    var dbModel = _nHService.GetScheduleById(model.Id).FirstOrDefault();
                    if (dbModel != null)
                    {
                        dbModel.uss_key = model.Id;
                        dbModel.uss_user_id = model.UserId;
                        dbModel.uss_time_from_calc = model.Start;
                        dbModel.uss_time_to_calc = model.End;
                        dbModel.uss_date = model.ScheduleDate.Date;
                        dbModel.uss_description = model.Description;
                        dbModel.uss_modified_by = loggedInUser.Id;
                        dbModel.uss_is_active = true;
                        dbModel.uss_modified_by_name = loggedInUser.FullName;
                        dbModel.uss_modified_date = DateTime.Now.ToEST();
                        dbModel.uss_custome_rate = model.Rate;
                        dbModel.uss_shift_key = model.ShiftId;
                        dbModel.uss_is_publish = true;

                        dbModel.uss_date_num = Convert.ToInt64(model.Start.Date.Year.ToString() + model.Start.Date.DayOfYear.ToString("000"));
                        dbModel.uss_time_from_calc_num = Convert.ToInt64(model.Start.Year.ToString() + model.Start.DayOfYear.ToString("000") + model.Start.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());
                        dbModel.uss_time_to_calc_num = Convert.ToInt64(model.End.Year.ToString() + model.End.DayOfYear.ToString("000") + model.End.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());


                        var response = _nHService.UpdateSchedule(dbModel, model.IsAllDay);
                        if (!response.Value)
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                        //// Added by axim
                        /*
                        if (model.Rate != 0)
                        {
                            _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                        }
                        */
                        //// ended by axim
                        var updatedScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                        if (updatedScheduleEntry != null)
                            return ParseScheduleResult(updatedScheduleEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult DestroyNH(ScheduleRecordViewModel model)
        {
            try
            {
                if (model != null)
                {
                    //_physicianHolidayRateService.Remove(model.Id.ToInt());
                    _nHService.RemoveSchedule(model.Id.ToInt());
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return Json(null);
        }

        public ActionResult ExportNHSchedule(string startDate, string endDate, List<string> Physicians)
        {
            if (Physicians.Count() == 1)
            {
                if (Physicians[0] == "null")
                {
                    Physicians = null;
                }
                else
                {
                    Physicians = Physicians[0].Split(',').ToList();
                }
            }
            bool isGetAllRequeset = (User.IsInRole(UserRoles.SuperAdmin.ToDescription())
                    || User.IsInRole(UserRoles.Administrator.ToDescription())
                    || User.IsInRole(UserRoles.Navigator.ToDescription())
                    || User.IsInRole(UserRoles.RRCManager.ToDescription())
                    || User.IsInRole(UserRoles.RRCDirector.ToDescription())
                    || User.IsInRole(UserRoles.Finance.ToDescription())

                    || User.IsInRole(UserRoles.CapacityResearcher.ToDescription())
                    || User.IsInRole(UserRoles.MedicalStaff.ToDescription())

                    || User.IsInRole(UserRoles.QPS.ToDescription())
                    || User.IsInRole(UserRoles.QualityDirector.ToDescription())
                    || User.IsInRole(UserRoles.QualityTeam.ToDescription())
                    || User.IsInRole(UserRoles.VPQuality.ToDescription())
                    || User.IsInRole(UserRoles.CredentialingTeam.ToDescription())
                    );
            List<ScheduleExportVM> res = _nHService.GetAll(isGetAllRequeset, loggedInUser.Id, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), Physicians, physicianType)
                                       .Select(m => new ScheduleExportVM
                                       {
                                           Id = m.uss_key,
                                           UserId = m.uss_user_id,
                                           Title = m.AspNetUser.UserInitial,
                                           TitleBig = string.Format("{0} - {1} to {2}{3}", m.AspNetUser.UserInitial, m.uss_date.AddTicks(m.uss_time_from).ToString("HH:mm"), m.uss_date.AddTicks(m.uss_time_to).ToString("HH:mm"), "#div#(" + m.AspNetUser.CredentialIndex.ToString("0.00") + ")#/div#"),
                                           Description = string.IsNullOrEmpty(m.uss_description) ? "" : m.uss_description,
                                           ScheduleDate = m.uss_date.FormatDateTime(),
                                           Start = m.uss_time_from_calc.Value.FormatDateTime(),
                                           End = m.uss_time_to_calc.Value.FormatDateTime(),
                                           IsActive = m.uss_is_active,
                                           FullName = string.Format("{0} {1}", m.AspNetUser.FirstName, m.AspNetUser.LastName),
                                           IsAllDay = (m.uss_time_to - TimeSpan.TicksPerDay) > 0,
                                           Rate = m.uss_custome_rate,
                                           ShiftId = m.uss_shift_key,
                                           PhyIndexRate = m.AspNetUser.CredentialIndex
                                       }).ToList();

            #region Get Only Required Physician

            var ids = _physicianDictionary.GetRecordAsList(physicianType);
            var foundIds = _adminService.GetAllUsersIds(ids);
            HashSet<string> resIds = new HashSet<string>(res.Select(s => s.UserId.ToString()));
            var matchIds = resIds.Intersect(foundIds).ToList();
            List<ScheduleExportVM> _res = res.Where(x => matchIds.Contains(x.UserId)).ToList();
            #endregion

            string filePath = Server.MapPath("~/Content/NHScheduleExport");
            ViewBag.TempFilePath = filePath + ".csv";
            _schedulerService.PrepareScheduleExport(filePath, Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), res);
            return File("~/Content/NHScheduleExport" + ".csv", "text/csv", "NHSchedule " + DateTime.Now.ToEST() + ".csv");
        }
        #endregion

        #region post-methods
        [HttpPost]
        [ActionName("Import")]
        public ActionResult parseFile(HttpPostedFileBase fileUpload)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string fileName = "Sch" + DateTime.Now.ToString("yyyyMMddhhmmsss") + ".csv";
                string filePath = Server.MapPath("/temp/");

                if (fileUpload.ContentLength > 0)
                {
                    fileUpload.SaveAs(System.IO.Path.Combine(filePath, fileName));
                    response = _schedulerService.SaveSchedule(filePath, fileName, loggedInUser.Id, loggedInUser.FullName, false, "Physician");
                    response.FileId = fileName;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(response);
        }
        [HttpPost]
        public ActionResult Create(ScheduleRecordViewModel model,string SchType)
        {
            try
            {
                var scheduleRecord = new user_schedule
                {
                    uss_key = model.Id,
                    uss_user_id = model.UserId,
                    uss_time_from_calc = model.Start,
                    uss_time_to_calc = model.End,
                    uss_date = model.Start.Date, // Start date -- current selected date of calender/scheduler
                    uss_description = model.Description,
                    uss_created_by = loggedInUser.Id,
                    uss_created_by_name = loggedInUser.FullName,
                    uss_created_date = DateTime.Now.ToEST(),
                    uss_is_active = true,
                    uss_is_publish = (User.IsInRole(UserRoles.SuperAdmin.ToDescription()) && SchType != "aoc") ? false : true,
                    uss_custome_rate = model.Rate,
                    uss_shift_key = model.ShiftId,

                   uss_date_num = Convert.ToInt64(model.Start.Date.Year.ToString() + model.Start.Date.DayOfYear.ToString("000")),
                uss_time_from_calc_num = Convert.ToInt64(model.Start.Year.ToString() + model.Start.DayOfYear.ToString("000") + model.Start.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim()),
                uss_time_to_calc_num = Convert.ToInt64(model.End.Year.ToString() + model.End.DayOfYear.ToString("000") + model.End.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim()),


            };

                var response = _schedulerService.AddSchedule(scheduleRecord, model.IsAllDay);
                if (!response.Value)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                //// Added by axim
                /*
                if(model.Rate != 0)
                {
                    _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                }
                */
                //// ended by axim
                var newScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                if (newScheduleEntry != null)
                    return ParseScheduleResult(newScheduleEntry);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult Update(ScheduleRecordViewModel model,string SchType)
        {
            try
            {
                if (model.Id > 0)
                {
                    var dbModel = _schedulerService.GetScheduleById(model.Id).FirstOrDefault();
                    if (dbModel != null)
                    {
                        dbModel.uss_key = model.Id;
                        dbModel.uss_user_id = model.UserId;
                        dbModel.uss_time_from_calc = model.Start;
                        dbModel.uss_time_to_calc = model.End;
                        dbModel.uss_date = model.ScheduleDate.Date;
                        dbModel.uss_description = model.Description;
                        dbModel.uss_modified_by = loggedInUser.Id;
                        dbModel.uss_is_active = true;
                        dbModel.uss_modified_by_name = loggedInUser.FullName;
                        dbModel.uss_modified_date = DateTime.Now.ToEST();
                        dbModel.uss_custome_rate = model.Rate;
                        dbModel.uss_shift_key = model.ShiftId;
                        if (User.IsInRole(UserRoles.SuperAdmin.ToDescription()) && SchType != "aoc")
                        {
                            dbModel.uss_is_publish = false;
                        }
                        else
                        {
                            dbModel.uss_is_publish = true;
                        }

                        dbModel.uss_date_num = Convert.ToInt64(model.Start.Date.Year.ToString() + model.Start.Date.DayOfYear.ToString("000"));
                        dbModel.uss_time_from_calc_num = Convert.ToInt64(model.Start.Year.ToString() + model.Start.DayOfYear.ToString("000") + model.Start.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());
                        dbModel.uss_time_to_calc_num = Convert.ToInt64(model.End.Year.ToString() + model.End.DayOfYear.ToString("000") + model.End.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());


                        var response = _schedulerService.UpdateSchedule(dbModel, model.IsAllDay);
                        if (!response.Value)
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "");
                        //// Added by axim
                        /*
                        if (model.Rate != 0)
                        {
                            _RateController.PhysicianHoliday(model, loggedInUser.FullName, loggedInUser.Id, response.Key);
                        }
                        */
                        //// ended by axim
                        var updatedScheduleEntry = _schedulerService.GetScheduleById(response.Key).FirstOrDefault();
                        if (updatedScheduleEntry != null)
                            return ParseScheduleResult(updatedScheduleEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult Destroy(ScheduleRecordViewModel model)
        {
            try
            {
                if (model != null)
                {
                    _physicianHolidayRateService.Remove(model.Id.ToInt());
                    _schedulerService.RemoveSchedule(model.Id.ToInt());
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return Json(null);
        }
        [HttpPost]
        public ActionResult ContinueImport(string FileId)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string filePath = Server.MapPath("/temp/");
                if (!string.IsNullOrEmpty(FileId))
                {
                    response = _schedulerService.SaveSchedule(filePath, FileId, loggedInUser.Id, loggedInUser.FullName, true, "Physician");
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could not find the uploaded file. please import a new file.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("Import", response);
        }
        #endregion

        #region Count Index Rates 
        private List<ScheduleRecordViewModel> CallForindexCount(List<ScheduleRecordViewModel> list)
        {
            ScheduleRecordViewModel createdObj;
            bool isSuperAdmin = User.IsInRole(UserRoles.SuperAdmin.ToDescription());
            var PhySchList = list.OrderBy(x => x.Start).ThenBy(x => x.End);
            foreach (var item in PhySchList)
            {
                var hours = (item.End - item.Start).TotalHours;
                hours = Math.Round(hours);
                DateTime dateTime = item.Start;
                for(int i = 1; i <= hours; i ++)
                {
                    createdObj = new ScheduleRecordViewModel();
                    createdObj = Createobject(item, dateTime,  i, list, isSuperAdmin);
                    list.Add(createdObj);
                    dateTime = dateTime.AddHours(1);
                }
            }
            return list;
        }
        //private ScheduleRecordViewModel Createobject(ScheduleRecordViewModel record, DateTime startTime, int i,1 List<ScheduleRecordViewModel> indexList)
        //{
        //    bool isChangeCredIndex = false;
        //    DateTime endTime = startTime.AddHours(1);
        //    var checkExist = indexList.Where(x => x.Start == startTime && x.End == endTime).FirstOrDefault();
        //    decimal? totalIndexCount = record.PhyIndexRate;
        //    decimal? changetotlaIndexCount = record.PhyIndexRate;
        //    if (checkExist != null)
        //    {
        //        totalIndexCount += checkExist.PhyIndexRate;
        //        indexList.Remove(checkExist);
        //    }
        //    decimal? previousTotalIndex = indexList[indexList.Count - 1].TotalIndex;
        //    if (i == 1 || (previousTotalIndex != totalIndexCount))
        //    {
        //        isChangeCredIndex = true;
        //    }

        //    ScheduleRecordViewModel obj = new ScheduleRecordViewModel();
        //    obj.UserId = i.ToString();//"132213";
        //    obj.Title = "CI";
        //    if (isChangeCredIndex)
        //    {
        //        obj.TitleBig = "CI (" + totalIndexCount + ") ";
        //        var result = _schedulerService.getAllPhyscianList(startTime.ToString(), endTime.ToString());
        //        if (result.Count > 0)
        //        {
        //            obj.TitleBig += "<span class='fewer-facility-list fa fa-flag ml-1' style='color:white;' onclick='getfacilities(\"" + startTime + "\",\"" + endTime + "\")' onmouseover='showTooltip()'></span>";        //string.Format("{0} - {1} to {2}{3}", "CI(" + totalIndexCount + ")", startTime.ToString("HH:mm"), endTime.ToString("HH:mm"),  "#div#(" + totalIndexCount + ")#/div#");
        //        }
        //        else
        //        {
        //            obj.TitleBig = "CI (" + totalIndexCount + ") ";
        //        }
        //    }
        //    else
        //    {
        //        //obj.TitleBig = "CI (" + totalIndexCount + ") ";
        //        obj.TitleBig = "";
        //    }
        //    obj.TotalIndex = totalIndexCount;
        //    obj.Description = "test";
        //    obj._scheduleData = record._scheduleData;
        //    obj._start = startTime.ToString(); //record._start;
        //    obj._end = endTime.ToString();//record._end;
        //    obj.IsActive = record.IsActive;
        //    obj.FullName = "Total Index (" + totalIndexCount + ")";
        //    obj.IsAllDay = record.IsAllDay;
        //    obj.Start = startTime;
        //    obj.End = endTime;
        //    //obj.Rate = 50;
        //    //obj.ShiftId = 1;
        //    obj.PhyIndexRate = totalIndexCount;
        //    return obj;
        //}
        private ScheduleRecordViewModel Createobject(ScheduleRecordViewModel record, DateTime startTime, int i, List<ScheduleRecordViewModel> indexList,bool isSuperAdmin)
        {
            bool isChangeCredIndex = false;
            DateTime endTime = startTime.AddHours(1);
            var checkExist = indexList.Where(x => x.Start == startTime && x.End == endTime).FirstOrDefault();
            decimal? totalIndexCount = record.PhyIndexRate;
            decimal? changetotlaIndexCount = record.PhyIndexRate;
            if (checkExist != null)
            {
                totalIndexCount += checkExist.PhyIndexRate;
                indexList.Remove(checkExist);
            }
            decimal? previousTotalIndex = indexList[indexList.Count - 1].TotalIndex;
            if (i == 1 || (previousTotalIndex != totalIndexCount))
            {
                isChangeCredIndex = true;
            }

            ScheduleRecordViewModel obj = new ScheduleRecordViewModel();
            obj.UserId = i.ToString();//"132213";
            obj.Title = "CI";
            if (isChangeCredIndex)
            {
                obj.TitleBig = "CI (" + totalIndexCount + ") ";
                if(isSuperAdmin)
                {
                    var result = _schedulerService.getAllPhyscianList(startTime.ToString(), endTime.ToString());
                    if (result.Count > 0)
                    {
                        obj.TitleBig += "<span class='fewer-facility-list fa fa-flag ml-1' style='color:red;' onclick='getfacilities(\"" + startTime + "\",\"" + endTime + "\")' onmouseover='showTooltip()'></span>";        //string.Format("{0} - {1} to {2}{3}", "CI(" + totalIndexCount + ")", startTime.ToString("HH:mm"), endTime.ToString("HH:mm"),  "#div#(" + totalIndexCount + ")#/div#");
                    }
                }
            }
            else
            {
                //obj.TitleBig = "CI (" + totalIndexCount + ") ";
                obj.TitleBig = "";
            }
            obj.TotalIndex = totalIndexCount;
            obj.Description = "test";
            obj.ScheduleDate = record.ScheduleDate;
            obj.Start = startTime; //record._start;
            obj.End = endTime;//record._end;
            obj.IsActive = record.IsActive;
            obj.FullName = "Total Index (" + totalIndexCount + ")";
            obj.IsAllDay = record.IsAllDay;
            obj.Start = startTime;
            obj.End = endTime;
            //obj.Rate = 50;
            //obj.ShiftId = 1;
            obj.PhyIndexRate = totalIndexCount;
            return obj;
        }
        //private ScheduleRecordViewModel  Createobject(ScheduleRecordViewModel record, DateTime startTime, int i, List<ScheduleRecordViewModel> indexList)
        //{
        //    DateTime endTime = startTime.AddHours(1);
        //    var checkExist = indexList.Where(x => x.Start == startTime && x.End == endTime).FirstOrDefault();
        //    decimal? totalIndexCount = record.PhyIndexRate;
        //    if (checkExist != null)
        //    {
        //        totalIndexCount += checkExist.PhyIndexRate;
        //        indexList.Remove(checkExist);
        //    }
        //    ScheduleRecordViewModel obj = new ScheduleRecordViewModel();
        //    obj.UserId = i.ToString();//"132213";
        //    obj.Title = "CI";
        //    obj.TitleBig = "CI (" + totalIndexCount + ")";//string.Format("{0} - {1} to {2}{3}", "CI(" + totalIndexCount + ")", startTime.ToString("HH:mm"), endTime.ToString("HH:mm"),  "#div#(" + totalIndexCount + ")#/div#");
        //    obj.Description = "test";
        //    obj._scheduleData = record._scheduleData;
        //    obj._start = startTime.ToString(); //record._start;
        //    obj._end = endTime.ToString();//record._end;
        //    obj.IsActive = record.IsActive;
        //    obj.FullName = "Total Index (" + totalIndexCount + ")";
        //    obj.IsAllDay = record.IsAllDay;
        //    obj.Start = startTime;
        //    obj.End = endTime;
        //    //obj.Rate = 50;
        //    //obj.ShiftId = 1;
        //    obj.PhyIndexRate = totalIndexCount;
        //    return obj;
        //}
        #endregion



        #region private-methods
        private JsonResult ParseScheduleResult(user_schedule model)
        {
            var user = model.AspNetUser == null ? _adminService.GetUser(model.uss_user_id) : model.AspNetUser;
            var result = new
            {
                Id = model.uss_key,
                UserId = model.uss_user_id,
                Title = user.UserInitial,
                TitleBig = string.Format("{0} - {1} to {2}{3}",
                                                            user.UserInitial,
                                                            model.uss_date.AddTicks(model.uss_time_from).ToString("HH:mm"),
                                                            model.uss_date.AddTicks(model.uss_time_to).ToString("HH:mm"),
                                                            (user.CredentialIndex.ToString("0.00") == "0.00" ? "" : "#div#(" + user.CredentialIndex.ToString("0.00") + ")#/div#")),
                Description = string.IsNullOrEmpty(model.uss_description) ? "" : model.uss_description,
                ScheduleDate = model.uss_date.FormatDateTime(),
                Start = model.uss_time_from_calc.Value.FormatDateTime(),
                End = model.uss_time_to_calc.Value.FormatDateTime(),
                FullName = string.Format("{0} {1}", user.FirstName, user.LastName),
                IsActive = model.uss_is_active,
                IsAllDay = (model.uss_time_to - TimeSpan.TicksPerDay) > 0,
                scheduleGetAll = false
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Import For PAOC 
        public ActionResult Imp()
        {
            return GetViewResult(new SchedulerResponseViewModel());
        }
        [HttpPost]
        [ActionName("Imp")]
        public ActionResult parseFileImp(HttpPostedFileBase fileUpload)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string fileName = "Sch" + DateTime.Now.ToString("yyyyMMddhhmmsss") + ".csv";
                string filePath = Server.MapPath("/temp/");

                if (fileUpload.ContentLength > 0)
                {
                    fileUpload.SaveAs(System.IO.Path.Combine(filePath, fileName));
                    response = _schedulerService.SaveSchedule(filePath, fileName, loggedInUser.Id, loggedInUser.FullName, false, "aoc");
                    response.FileId = fileName;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(response);
        }

        public ActionResult DownloadSampleImp()
        {
            _schedulerService.PrepareDownloadSampleImp(Server.MapPath("/Content"));
            return File("/Content/Schedule-Template.csv", "text/csv", "Schedule-Template.csv");
        }
        [HttpPost]
        public ActionResult ContinueImportImp(string FileId)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string filePath = Server.MapPath("/temp/");
                if (!string.IsNullOrEmpty(FileId))
                {
                    response = _schedulerService.SaveSchedule(filePath, FileId, loggedInUser.Id, loggedInUser.FullName, true, "aoc");
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could not find the uploaded file. please import a new file.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("Imp", response);
        }
        #endregion

        #region Import For Sleep 
        public ActionResult Slp()
        {
            return GetViewResult(new SchedulerResponseViewModel());
        }
        [HttpPost]
        [ActionName("Slp")]
        public ActionResult parseFileSlp(HttpPostedFileBase fileUpload)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string fileName = "Sch" + DateTime.Now.ToString("yyyyMMddhhmmsss") + ".csv";
                string filePath = Server.MapPath("/temp/");

                if (fileUpload.ContentLength > 0)
                {
                    fileUpload.SaveAs(System.IO.Path.Combine(filePath, fileName));
                    response = _sleepService.SaveSchedule(filePath, fileName, loggedInUser.Id, loggedInUser.FullName, false, "Physician");
                    response.FileId = fileName;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(response);
        }

        public ActionResult DownloadSampleSlp()
        {
            _schedulerService.PrepareDownloadSampleSlp(Server.MapPath("/Content"));
            return File("/Content/Schedule-Template.csv", "text/csv", "Schedule-Template.csv");
        }
        [HttpPost]
        public ActionResult ContinueImportSlp(string FileId)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string filePath = Server.MapPath("/temp/");
                if (!string.IsNullOrEmpty(FileId))
                {
                    response = _sleepService.SaveSchedule(filePath, FileId, loggedInUser.Id, loggedInUser.FullName, true, "Physician");
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could not find the uploaded file. please import a new file.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("Slp", response);
        }
        #endregion
       
        #region Import For NH 
        public ActionResult NH()
        {
            return GetViewResult(new SchedulerResponseViewModel());
        }
        [HttpPost]
        [ActionName("NH")]
        public ActionResult parseFileNh(HttpPostedFileBase fileUpload)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string fileName = "Sch" + DateTime.Now.ToString("yyyyMMddhhmmsss") + ".csv";
                string filePath = Server.MapPath("/temp/");

                if (fileUpload.ContentLength > 0)
                {
                    fileUpload.SaveAs(System.IO.Path.Combine(filePath, fileName));
                    response = _nHService.SaveSchedule(filePath, fileName, loggedInUser.Id, loggedInUser.FullName, false, "Physician");
                    response.FileId = fileName;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return View(response);
        }

        public ActionResult DownloadSampleNH()
        {
            _schedulerService.PrepareDownloadSampleNH(Server.MapPath("/Content"));
            return File("/Content/Schedule-Template.csv", "text/csv", "Schedule-Template.csv");
        }
        [HttpPost]
        public ActionResult ContinueImportNH(string FileId)
        {
            var response = new SchedulerResponseViewModel();
            try
            {
                string filePath = Server.MapPath("/temp/");
                if (!string.IsNullOrEmpty(FileId))
                {
                    response = _nHService.SaveSchedule(filePath, FileId, loggedInUser.Id, loggedInUser.FullName, true, "Physician");
                }
                else
                {
                    response.Success = false;
                    response.Message = "Could not find the uploaded file. please import a new file.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PartialView("NH", response);
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
                    // dispose-only, i.e. non-finalizable logic
                    this._schedulerService?.Dispose();
                    this._adminService?.Dispose();
                    this._physicianHolidayRateService?.Dispose();
                    this._RateController?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}