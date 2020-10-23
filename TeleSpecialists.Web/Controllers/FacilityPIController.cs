using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Helpers;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using System.Linq;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels.Reports;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class FacilityPIController : BaseController
    {
        private readonly QualityGoalService _qualityGoalsService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly LookupService _lookUpService;
        private readonly FacilityPIService _facilityPIService;
        private readonly UCLService _uCLService;
        private readonly FacilityService _facilityService;

        public FacilityPIController()
        {
            _qualityGoalsService = new QualityGoalService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _lookUpService = new LookupService();
            _facilityPIService = new FacilityPIService();
            _uCLService = new UCLService();
            _facilityService = new FacilityService();
        }
        public ActionResult FacilityPI()
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            if (isFacilityAdmin)
            {
                var facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                        .Select(m => new { fac_key = m.Facility, fac_name = m.FacilityName });
                ViewBag.Facilities = facList;
            }
            else
            {
                var facList = _lookUpService.GetAllFacility(null)
                                            .Select(m => new SelectListItem
                                            {
                                                Value = m.fac_key.ToString(),
                                                Text = m.fac_name
                                            }).Prepend(new SelectListItem() { Text = "-- Select --", Value = "0" });
                ViewBag.Facilities = facList;
            }
            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString()
            //}).Prepend(new SelectListItem() { Text = "-- Select --", Value = "0" });
            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;

            string selected = "";
            List<string> roles = new List<string>();

            var QPS = UserRoles.QPS.ToDescription();
            var QualityDirector = UserRoles.QualityDirector.ToDescription();
            var VPQuality = UserRoles.VPQuality.ToDescription();

            var QPSId = RoleManager.Roles.Where(x => x.Description == QPS).Select(x => x.Id).FirstOrDefault();
            var QualityDirectorId = RoleManager.Roles.Where(x => x.Description == QualityDirector).Select(x => x.Id).FirstOrDefault();
            var VPQualityId = RoleManager.Roles.Where(x => x.Description == VPQuality).Select(x => x.Id).FirstOrDefault();

            roles.Add(QPSId);
            roles.Add(QualityDirectorId);
            roles.Add(VPQualityId);

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            ViewBag.QPS_Numbers_List = QPSList;

            return GetViewResult();
        }
        public ActionResult Index(Guid fac_key)
        {
            ViewBag.fac_key = fac_key;
            return PartialView();
        }
        public ActionResult Create(Guid fac_key)
        {
            var facList = new List<SelectListItem>();
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();
            }
            else
            {
                facList = _lookUpService.GetAllFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.fac_key = fac_key;
            return PartialView();
        }
        public ActionResult Edit(int id)
        {
            var GetDetail = _qualityGoalsService.GetGoalDataById(id);
            ViewBag.fac_key = GetDetail.quality_goals.qag_fac_key;

            return PartialView(GetDetail);
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request, Guid fac_key)
        {
            var res = _qualityGoalsService.GetAll(request, fac_key);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(QualityGoalsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isExist = _qualityGoalsService.GetGoalDataById(model.obj.gd_key);
                if (isExist == null)
                {

                    for (var i = 0; i < model.Facilities.Count; i++)
                    {
                        var CheckData = _qualityGoalsService.GetDetailByYear(model.Facilities[i], model.qag_time_frame);
                        if (CheckData == null)
                        {
                            quality_goals Goals = new quality_goals();
                            Goals.qag_fac_key = model.Facilities[i];
                            Goals.qag_time_frame = model.qag_time_frame;
                            _qualityGoalsService.Create(Goals);
                            var GetDetail = _qualityGoalsService.GetQualityGoalsByFacility(model.Facilities[i], model.qag_time_frame);
                            for (var j = 0; j < model.obj.qag_door_to_TS_notification_ave_minutes.Count; j++)
                            {
                                goals_data data = new goals_data();
                                data.gd_qag_key = GetDetail.qag_key;
                                data.gd_quater = model.obj.Quater;
                                data.qag_door_to_TS_notification_ave_minutes = model.obj.qag_door_to_TS_notification_ave_minutes[j];
                                data.qag_door_to_TS_notification_median_minutes = model.obj.qag_door_to_TS_notification_median_minutes[j];
                                data.qag_percent10_min_or_less_activation_EMS = model.obj.qag_percent10_min_or_less_activation_EMS[j];
                                data.qag_percent10_min_or_less_activation_PV = model.obj.qag_percent10_min_or_less_activation_PV[j];
                                //data.qag_percent10_min_or_less_activation_Inpt = model.obj.qag_percent10_min_or_less_activation_Inpt[j];
                                data.qag_TS_notification_to_response_average_minute = model.obj.qag_TS_notification_to_response_average_minute[j];
                                data.qag_TS_notification_to_response_median_minute = model.obj.qag_TS_notification_to_response_median_minute[j];
                                data.qag_percent_TS_at_bedside_grterthan10_minutes = model.obj.qag_percent_TS_at_bedside_grterthan10_minutes[j];
                                //data.qag_alteplase_administered = model.obj.qag_alteplase_administered[j];
                                data.qag_door_to_needle_average = model.obj.qag_door_to_needle_average[j];
                                data.qag_door_to_needle_median = model.obj.qag_door_to_needle_median[j];
                                data.qag_verbal_order_to_administration_average_minutes = model.obj.qag_verbal_order_to_administration_average_minutes[j];
                                data.qag_DTN_grter_or_equal_30minutes_percent = model.obj.qag_DTN_grter_or_equal_30minutes_percent[j];
                                data.qag_DTN_grter_or_equal_45minutes_percent = model.obj.qag_DTN_grter_or_equal_45minutes_percent[j];
                                data.qag_DTN_grter_or_equal_60minutes_percent = model.obj.qag_DTN_grter_or_equal_60minutes_percent[j];
                                data.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent[j];
                                data.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent[j];
                                data.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent[j];
                                _qualityGoalsService.CreateGoalData(data);
                            }
                        }
                        else
                        {
                            var issExist = CheckData.goals_data.Where(x => x.gd_qag_key == CheckData.qag_key && x.gd_quater == model.obj.Quater).ToList();
                            if (issExist.Count > 0)
                            {
                                for (var j = 0; j < issExist.Count; j++)
                                {
                                    issExist[j].qag_door_to_TS_notification_ave_minutes = model.obj.qag_door_to_TS_notification_ave_minutes[j];
                                    issExist[j].qag_door_to_TS_notification_median_minutes = model.obj.qag_door_to_TS_notification_median_minutes[j];
                                    issExist[j].qag_percent10_min_or_less_activation_EMS = model.obj.qag_percent10_min_or_less_activation_EMS[j];
                                    issExist[j].qag_percent10_min_or_less_activation_PV = model.obj.qag_percent10_min_or_less_activation_PV[j];
                                    //issExist[j].qag_percent10_min_or_less_activation_Inpt = model.obj.qag_percent10_min_or_less_activation_Inpt[j];
                                    issExist[j].qag_TS_notification_to_response_average_minute = model.obj.qag_TS_notification_to_response_average_minute[j];
                                    issExist[j].qag_TS_notification_to_response_median_minute = model.obj.qag_TS_notification_to_response_median_minute[j];
                                    issExist[j].qag_percent_TS_at_bedside_grterthan10_minutes = model.obj.qag_percent_TS_at_bedside_grterthan10_minutes[j];
                                    //issExist[j].qag_alteplase_administered = model.obj.qag_alteplase_administered[j];
                                    issExist[j].qag_door_to_needle_average = model.obj.qag_door_to_needle_average[j];
                                    issExist[j].qag_door_to_needle_median = model.obj.qag_door_to_needle_median[j];
                                    issExist[j].qag_verbal_order_to_administration_average_minutes = model.obj.qag_verbal_order_to_administration_average_minutes[j];
                                    issExist[j].qag_DTN_grter_or_equal_30minutes_percent = model.obj.qag_DTN_grter_or_equal_30minutes_percent[j];
                                    issExist[j].qag_DTN_grter_or_equal_45minutes_percent = model.obj.qag_DTN_grter_or_equal_45minutes_percent[j];
                                    issExist[j].qag_DTN_grter_or_equal_60minutes_percent = model.obj.qag_DTN_grter_or_equal_60minutes_percent[j];
                                    issExist[j].qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent[j];
                                    issExist[j].qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent[j];
                                    issExist[j].qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent[j];
                                    _qualityGoalsService.EditGoalData(issExist[j]);
                                    _qualityGoalsService.SaveChanges();
                                }
                            }
                            else
                            {
                                goals_data data = new goals_data();
                                data.gd_qag_key = CheckData.qag_key;
                                data.gd_quater = model.obj.Quater;
                                data.qag_door_to_TS_notification_ave_minutes = model.obj.qag_door_to_TS_notification_ave_minutes[0];
                                data.qag_door_to_TS_notification_median_minutes = model.obj.qag_door_to_TS_notification_median_minutes[0];
                                data.qag_percent10_min_or_less_activation_EMS = model.obj.qag_percent10_min_or_less_activation_EMS[0];
                                data.qag_percent10_min_or_less_activation_PV = model.obj.qag_percent10_min_or_less_activation_PV[0];
                                //data.qag_percent10_min_or_less_activation_Inpt = model.obj.qag_percent10_min_or_less_activation_Inpt[0];
                                data.qag_TS_notification_to_response_average_minute = model.obj.qag_TS_notification_to_response_average_minute[0];
                                data.qag_TS_notification_to_response_median_minute = model.obj.qag_TS_notification_to_response_median_minute[0];
                                data.qag_percent_TS_at_bedside_grterthan10_minutes = model.obj.qag_percent_TS_at_bedside_grterthan10_minutes[0];
                                data.qag_door_to_needle_average = model.obj.qag_door_to_needle_average[0];
                                data.qag_door_to_needle_median = model.obj.qag_door_to_needle_median[0];
                                data.qag_verbal_order_to_administration_average_minutes = model.obj.qag_verbal_order_to_administration_average_minutes[0];
                                data.qag_DTN_grter_or_equal_30minutes_percent = model.obj.qag_DTN_grter_or_equal_30minutes_percent[0];
                                data.qag_DTN_grter_or_equal_45minutes_percent = model.obj.qag_DTN_grter_or_equal_45minutes_percent[0];
                                data.qag_DTN_grter_or_equal_60minutes_percent = model.obj.qag_DTN_grter_or_equal_60minutes_percent[0];
                                data.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent[0];
                                data.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent[0];
                                data.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent[0];
                                _qualityGoalsService.CreateGoalData(data);
                                _qualityGoalsService.SaveChanges();
                            }
                        }
                    }
                }
                else
                {
                    isExist.qag_door_to_TS_notification_ave_minutes = model.obj.qag_door_to_TS_notification_ave_minutes[0];
                    isExist.qag_door_to_TS_notification_median_minutes = model.obj.qag_door_to_TS_notification_median_minutes[0];
                    isExist.qag_percent10_min_or_less_activation_EMS = model.obj.qag_percent10_min_or_less_activation_EMS[0];
                    isExist.qag_percent10_min_or_less_activation_PV = model.obj.qag_percent10_min_or_less_activation_PV[0];
                    //isExist.qag_percent10_min_or_less_activation_Inpt = model.obj.qag_percent10_min_or_less_activation_Inpt[0];
                    isExist.qag_TS_notification_to_response_average_minute = model.obj.qag_TS_notification_to_response_average_minute[0];
                    isExist.qag_TS_notification_to_response_median_minute = model.obj.qag_TS_notification_to_response_median_minute[0];
                    isExist.qag_percent_TS_at_bedside_grterthan10_minutes = model.obj.qag_percent_TS_at_bedside_grterthan10_minutes[0];
                    //isExist.qag_alteplase_administered = model.obj.qag_alteplase_administered[0];
                    isExist.qag_door_to_needle_average = model.obj.qag_door_to_needle_average[0];
                    isExist.qag_door_to_needle_median = model.obj.qag_door_to_needle_median[0];
                    isExist.qag_verbal_order_to_administration_average_minutes = model.obj.qag_verbal_order_to_administration_average_minutes[0];
                    isExist.qag_DTN_grter_or_equal_30minutes_percent = model.obj.qag_DTN_grter_or_equal_30minutes_percent[0];
                    isExist.qag_DTN_grter_or_equal_45minutes_percent = model.obj.qag_DTN_grter_or_equal_45minutes_percent[0];
                    isExist.qag_DTN_grter_or_equal_60minutes_percent = model.obj.qag_DTN_grter_or_equal_60minutes_percent[0];
                    isExist.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent[0];
                    isExist.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent[0];
                    isExist.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = model.obj.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent[0];
                    _qualityGoalsService.EditGoalData(isExist);
                    _qualityGoalsService.SaveChanges();
                }

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }

        public ActionResult Remove(int id)
        {
            try
            {
                _qualityGoalsService.DeleteRange(id);
                quality_goals qualityGoal = _qualityGoalsService.GetDetails(id);
                _qualityGoalsService.Delete(qualityGoal);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CheckQualityGoalExist(List<Guid> facilities, string timeframe, string quater)
        {
            var isExist = _qualityGoalsService.GetQualityGoals(facilities, timeframe);
            List<QualityGoalscls> qualities = new List<QualityGoalscls>();
            string facilityExist = "";
            foreach (var item in isExist)
            {
                if (quater == "search")
                {
                    facilityExist += item.facility.fac_name + ", ";
                }
                else
                {
                    var RecordExist = item.goals_data.Where(x => x.gd_qag_key == item.qag_key && x.gd_quater == quater).FirstOrDefault();
                    if (RecordExist != null)
                    {
                        facilityExist += item.facility.fac_name + ", ";
                    }
                }
            }
            facilityExist = facilityExist.TrimEnd(' ', ',');
            if (facilityExist != "")
            {
                facilityExist += " (Quarter-" + quater + " Quality Goals are available)";
            }
            else
            {
                facilityExist += "Quality Goals are Not Available";
            }

            var _result = qualities.AsQueryable();
            return Json(facilityExist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FacilityDashboard()
        {
            return PartialView();
        }
        public ActionResult tPAcaseanalysis()
        {
            return PartialView();
        }
        public ActionResult PITrends()
        {
            return PartialView();
        }
        public ActionResult CounterMeasure()
        {
            return PartialView();
        }
        public ActionResult VolumeMetric()
        {
            return PartialView();
        }
        public ActionResult StrokeAlertCases()
        {
            return PartialView();
        }
        [HttpGet]
        public JsonResult GetFacility()
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                if (isFacilityAdmin)
                {
                    var facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                            .Select(m => new { fac_key = m.Facility, fac_name = m.FacilityName });
                    return Json(facList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var facList = _lookUpService.GetAllFacility(null)
                                                .Select(m => new { fac_key = m.fac_key, fac_name = m.fac_name });
                    return Json(facList, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSpreadSheetReport(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.ReportType != null)
                {
                    if (model.qag_time_frame == null)
                    {
                        DataSourceResult result = new DataSourceResult();
                        result.Data = new List<string>();
                        return JsonMax(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        quality_goals QualityGoals = new quality_goals();
                        if (model.Facilities != null && model.Facilities.Count > 0)
                        {
                            var date = Convert.ToDateTime(model.qag_time_frame);
                            QualityGoals = _qualityGoalsService.GetQualityGoalsByFacility(model.Facilities[0], date.Year.ToString());

                            if (QualityGoals == null)
                            {
                                DataSourceResult rsult = new DataSourceResult();
                                rsult.Data = new List<string>();
                                return JsonMax(rsult, JsonRequestBehavior.AllowGet);
                            }
                        }
                        var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                        var result = _facilityPIService.GetSpreadSheetReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, QualityGoals);

                        return JsonMax(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetTPAAnalysis(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetTPAAnalysis(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetRootCauseTrends(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetRootCauseTrends(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCaseReviewTrends(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetCaseReviewTrends(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCounterMeasure(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetCounterMeasure(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetVolumeMetrics(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetVolumeMetrics(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetStrokeAlertCases(DataSourceRequest request, QualityGoalsViewModel model)
        {
            try
            {
                if (model.qag_time_frame == null)
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _facilityPIService.GetStrokeAlertCases(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetVolumePieChart(DataSourceRequest request, QualityGoalsViewModel model)
        {
            if (model.Facilities != null && model.Facilities.Count > 0)
            {
                Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
                if (model.Facilities[0] == guid)
                {
                    model.Facilities.RemoveAt(0);
                }
            }
            if (model.QPS != null && model.QPS.Count > 0)
            {
                if (model.QPS[0] == null)
                {
                    model.QPS.RemoveAt(0);
                }
            }
            if (model.System != null && model.System.Count > 0)
            {
                if (model.System[0] == null)
                {
                    model.System.RemoveAt(0);
                }
            }
            if (model.Regional != null && model.Regional.Count > 0)
            {
                if (model.Regional[0] == null)
                {
                    model.Regional.RemoveAt(0);
                }
            }
            if (model.States != null && model.States.Count > 0)
            {
                if (model.States[0] == null)
                {
                    model.States.RemoveAt(0);
                }
            }
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _facilityPIService.GetVolumeGraph(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStrokeAlertPieChart(DataSourceRequest request, QualityGoalsViewModel model)
        {
            if (model.Facilities != null && model.Facilities.Count > 0)
            {
                Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
                if (model.Facilities[0] == guid)
                {
                    model.Facilities.RemoveAt(0);
                }
            }
            if (model.QPS != null && model.QPS.Count > 0)
            {
                if (model.QPS[0] == null)
                {
                    model.QPS.RemoveAt(0);
                }
            }
            if (model.System != null && model.System.Count > 0)
            {
                if (model.System[0] == null)
                {
                    model.System.RemoveAt(0);
                }
            }
            if (model.Regional != null && model.Regional.Count > 0)
            {
                if (model.Regional[0] == null)
                {
                    model.Regional.RemoveAt(0);
                }
            }
            if (model.States != null && model.States.Count > 0)
            {
                if (model.States[0] == null)
                {
                    model.States.RemoveAt(0);
                }
            }
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _facilityPIService.GetStrokeAlertPieChart(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFacilityDashboardAllCharts(DataSourceRequest request, QualityGoalsViewModel model)
        {
            if (model.Facilities != null && model.Facilities.Count > 0)
            {
                Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
                if (model.Facilities[0] == guid)
                {
                    model.Facilities.RemoveAt(0);
                }
            }
            if (model.QPS != null && model.QPS.Count > 0)
            {
                if (model.QPS[0] == null)
                {
                    model.QPS.RemoveAt(0);
                }
            }
            if (model.System != null && model.System.Count > 0)
            {
                if (model.System[0] == null)
                {
                    model.System.RemoveAt(0);
                }
            }
            if (model.Regional != null && model.Regional.Count > 0)
            {
                if (model.Regional[0] == null)
                {
                    model.Regional.RemoveAt(0);
                }
            }
            if (model.States != null && model.States.Count > 0)
            {
                if (model.States[0] == null)
                {
                    model.States.RemoveAt(0);
                }
            }
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _facilityPIService.GetFacilityDashboardAllGraph(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        #region Commented Code
        //public ActionResult GetFacilityDashboardPieChart(DataSourceRequest request, QualityGoalsViewModel model)
        //{
        //    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
        //    var result = _facilityPIService.GetFacilityDashboardPieChart(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
        //    var _result = result.Data;
        //    return Json(_result, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult GetFacilityDashboardGraph(DataSourceRequest request, QualityGoalsViewModel model, string status)
        {
            try
            {
                if(model.Facilities != null && model.Facilities.Count > 0)
                {
                    Guid guid = new Guid("00000000-0000-0000-0000-000000000000");
                    if (model.Facilities[0] == guid)
                    {
                        model.Facilities.RemoveAt(0);
                    }
                }
                if (model.QPS != null && model.QPS.Count > 0)
                {
                    if (model.QPS[0] == null)
                    {
                        model.QPS.RemoveAt(0);
                    }
                }
                if (model.System != null && model.System.Count > 0)
                {
                    if (model.System[0] == null)
                    {
                        model.System.RemoveAt(0);
                    }
                }
                if (model.Regional != null && model.Regional.Count > 0)
                {
                    if (model.Regional[0] == null)
                    {
                        model.Regional.RemoveAt(0);
                    }
                }
                if (model.States != null && model.States.Count > 0)
                {
                    if (model.States[0] == null)
                    {
                        model.States.RemoveAt(0);
                    }
                }
                
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _facilityPIService.GetFacilityDashboardGraph(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, status);
                var _result = result.Data;
                return JsonMax(_result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCounterMeasureModal(int cas_key)
        {
            try
            {
                var GetRecord = _qualityGoalsService.GetRca_Counter_Measure(cas_key);

                return Json(GetRecord.Select(x => new
                {
                    x.rca_rootcause_id,
                    x.rca_root_cause,
                    x.rca_proposed_countermeasure,
                    x.rca_responsible_party,
                    rca_proposed_due_date = x.rca_proposed_due_date.HasValue ? x.rca_proposed_due_date.Value.FormatDate() : "",
                    rca_completed_date = x.rca_completed_date.HasValue ? x.rca_completed_date.Value.FormatDate() : "",
                    x.rca_Id
                }), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
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
                    _qualityGoalsService?.Dispose();
                    _ealertFacilitiesService?.Dispose();
                    _lookUpService?.Dispose();
                    _facilityPIService?.Dispose();
                    _uCLService?.Dispose();
                    _facilityService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}