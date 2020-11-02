using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.Reports;


namespace TeleSpecialists.BLL.Service
{
    public class FacilityPIService : BaseService
    {
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private LookupService _lookUpService;
        #region constructor
        public FacilityPIService()
        {
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _lookUpService = new LookupService();
        }
        #endregion
        public DataSourceResult GetSpreadSheetReport(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId, quality_goals quality_Goals)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                List<FacilityDashboardcls> list = new List<FacilityDashboardcls>();
                int qag_key = 0;
                Guid fac_key = new Guid();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                var EDStay = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    case_number = x.ca.cas_case_number,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",

                    arrival_to_start = x.ca.cas_patient_type != inpatientType && x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
                    emspatienttype = x.ca.cas_patient_type == emspatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    pvpatienttype = x.ca.cas_patient_type == pvpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    //inpatienttype = x.ca.cas_metric_symptom_onset_during_ed_stay == true ? x.ca.cas_patient_type == inpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_symptom_onset_during_ed_stay_time) : "" : "",
                    start_to_response = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_first_atempt < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_response_first_atempt) : "" : "",
                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    tpatrue = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_tpa_consult : false,
                    arrival_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time) : "" : "" : "",
                    verbal_order_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time) : "",
                    start_to_needle_time = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_response_ts_notification) : "" : "" : "",
                    cpoe_order_to_needle = x.ca.cas_metric_tpa_consult == true ? DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time) : "00:00:00",
                });


                FacilityDashboardcls arrivaltostartave = new FacilityDashboardcls();
                FacilityDashboardcls arrivaltostartmed = new FacilityDashboardcls();
                FacilityDashboardcls MinorLessActivationEMS = new FacilityDashboardcls();
                FacilityDashboardcls MinorLessActivationPV = new FacilityDashboardcls();
                //FacilityDashboardcls MinorLessActivationINP = new FacilityDashboardcls();
                FacilityDashboardcls TSNotificationResponseAverageMinute = new FacilityDashboardcls();
                FacilityDashboardcls TSNotificationResponseMedianMinute = new FacilityDashboardcls();
                FacilityDashboardcls TSatBedside10Minutes = new FacilityDashboardcls();
                FacilityDashboardcls ALTEPLASEADMINISTERED = new FacilityDashboardcls();
                FacilityDashboardcls DoortoNeedleAverageminsec = new FacilityDashboardcls();
                FacilityDashboardcls DoortoNeedleMedianminsec = new FacilityDashboardcls();
                FacilityDashboardcls VerbalOrdertoAdministrationAverageMinutes = new FacilityDashboardcls();
                FacilityDashboardcls DTN30Minutes = new FacilityDashboardcls();
                FacilityDashboardcls DTN45Minutes = new FacilityDashboardcls();
                FacilityDashboardcls DTN60Minutes = new FacilityDashboardcls();
                FacilityDashboardcls TSNotificationtoNeedle30Minutes = new FacilityDashboardcls();
                FacilityDashboardcls TSNotificationtoNeedle45Minutes = new FacilityDashboardcls();
                FacilityDashboardcls TSNotificationtoNeedle60Minutes = new FacilityDashboardcls();
                FacilityDashboardcls CPOEtoNeedleTimeave = new FacilityDashboardcls();
                FacilityDashboardcls CPOEtoNeedleTimemed = new FacilityDashboardcls();

                var GoalsData = quality_Goals.goals_data.ToList();

                //string facilityname = "";

                #region Original Code
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                if (model.ReportType == "monthly")
                {
                    for (var i = StartDate; StartDate <= EndDate;)
                    {
                        QualityGoalscls quality = new QualityGoalscls();
                        var GetGoalsbyQuater = new goals_data();
                        string Month = StartDate.ToString("MMMM");
                        if (Month == "January" || Month == "February" || Month == "March")
                        {
                            if (GoalsData != null && GoalsData.Count > 0)
                            {
                                GetGoalsbyQuater = GoalsData.Where(x => x.gd_quater == "1").FirstOrDefault();
                            }

                        }
                        else if (Month == "April" || Month == "May" || Month == "June")
                        {
                            if (GoalsData != null && GoalsData.Count > 0)
                            {
                                GetGoalsbyQuater = GoalsData.Where(x => x.gd_quater == "2").FirstOrDefault();
                            }

                        }
                        else if (Month == "July" || Month == "August" || Month == "September")
                        {
                            if (GoalsData != null && GoalsData.Count > 0)
                            {
                                GetGoalsbyQuater = GoalsData.Where(x => x.gd_quater == "3").FirstOrDefault();
                            }

                        }
                        else if (Month == "October" || Month == "November" || Month == "December")
                        {
                            if (GoalsData != null && GoalsData.Count > 0)
                            {
                                GetGoalsbyQuater = GoalsData.Where(x => x.gd_quater == "4").FirstOrDefault();
                            }

                        }

                        DateTime enddate = StartDate.AddMonths(1).AddDays(-1);
                        var result = query.Where(x => DbFunctions.TruncateTime(x.created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                                            DbFunctions.TruncateTime(x.created_date) <= DbFunctions.TruncateTime(enddate))
                                                                       .Select(x => new
                                                                       {
                                                                           x.arrival_to_start,
                                                                           x.emspatienttype,
                                                                           x.pvpatienttype,
                                                                                   //x.inpatienttype,
                                                                                   x.start_to_response,
                                                                           x.bedside_response_time,
                                                                           x.tpatrue,
                                                                           x.arrival_to_needle_time,
                                                                           x.verbal_order_to_needle_time,
                                                                           x.start_to_needle_time,
                                                                           x.cpoe_order_to_needle
                                                                       }).ToList();
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        List<double> Start_to_Response_meanlist = new List<double>();
                        List<double> Start_to_Response_medianlist = new List<double>();
                        List<double> Bedside_meanlist = new List<double>();
                        List<double> Bedside_medianlist = new List<double>();
                        List<double> Door_to_Needle_meanlist = new List<double>();
                        List<double> Door_to_Needle_medianlist = new List<double>();
                        List<double> Verbal_to_Administration_meanlist = new List<double>();
                        List<double> CPOEtoneedle_meanlist = new List<double>();
                        List<double> CPOEtoneedle_medianlist = new List<double>();
                        int casecount = 0;
                        int emscount = 0;
                        int totalemscount = 0;
                        int pvcount = 0;
                        int totalpvcount = 0;
                        //int inpcount = 0;
                        //int totalinpcount = 0;
                        int BedsideCount = 0;
                        int BedsideTotal = 0;
                        int tpatrue = 0;
                        int DTN30 = 0;
                        int DTN45 = 0;
                        int DTN60 = 0;
                        int DTNTotal = 0;
                        int STN30 = 0;
                        int STN45 = 0;
                        int STN60 = 0;
                        int STNTotal = 0;
                        if (result.Count > 0)
                        {
                            foreach (var item in result)
                            {
                                if (item.arrival_to_start != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.arrival_to_start.Split(':')[0]), int.Parse(item.arrival_to_start.Split(':')[1]), int.Parse(item.arrival_to_start.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    _medianlist.Add(time);
                                    casecount++;
                                }
                                if (item.emspatienttype != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.emspatienttype.Split(':')[0]), int.Parse(item.emspatienttype.Split(':')[1]), int.Parse(item.emspatienttype.Split(':')[2])).TotalMinutes;
                                    if (time <= 10)
                                    {
                                        emscount++;
                                    }
                                    totalemscount++;
                                }
                                if (item.pvpatienttype != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.pvpatienttype.Split(':')[0]), int.Parse(item.pvpatienttype.Split(':')[1]), int.Parse(item.pvpatienttype.Split(':')[2])).TotalMinutes;
                                    if (time <= 10)
                                    {
                                        pvcount++;
                                    }
                                    totalpvcount++;
                                }
                                //if (item.inpatienttype != "")
                                //{
                                //    var time = new TimeSpan(int.Parse(item.inpatienttype.Split(':')[0]), int.Parse(item.inpatienttype.Split(':')[1]), int.Parse(item.inpatienttype.Split(':')[2])).TotalMinutes;
                                //    if (time <= 10)
                                //    {
                                //        inpcount++;
                                //    }
                                //    totalinpcount++;
                                //}
                                if (item.start_to_response != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.start_to_response.Split(':')[0]), int.Parse(item.start_to_response.Split(':')[1]), int.Parse(item.start_to_response.Split(':')[2])).TotalSeconds;
                                    Start_to_Response_meanlist.Add(time);
                                    Start_to_Response_medianlist.Add(time);
                                }
                                if (item.bedside_response_time != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalSeconds;
                                    var MiNtime = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalMinutes;
                                    Bedside_meanlist.Add(time);
                                    Bedside_medianlist.Add(time);
                                    if (MiNtime < 10)
                                    {
                                        BedsideCount++;
                                    }
                                    BedsideTotal++;
                                }
                                if (item.tpatrue == true)
                                {
                                    tpatrue++;
                                }
                                if (item.arrival_to_needle_time != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalSeconds;
                                    var MINtime = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalMinutes;
                                    Door_to_Needle_meanlist.Add(time);
                                    Door_to_Needle_medianlist.Add(time);
                                    if (MINtime <= 30)
                                    {
                                        DTN30++;
                                    }
                                    if (MINtime <= 45)
                                    {
                                        DTN45++;
                                    }
                                    if (MINtime <= 60)
                                    {
                                        DTN60++;
                                    }
                                    DTNTotal++;
                                }
                                if (item.verbal_order_to_needle_time != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.verbal_order_to_needle_time.Split(':')[0]), int.Parse(item.verbal_order_to_needle_time.Split(':')[1]), int.Parse(item.verbal_order_to_needle_time.Split(':')[2])).TotalSeconds;
                                    Verbal_to_Administration_meanlist.Add(time);
                                }
                                if (item.start_to_needle_time != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.start_to_needle_time.Split(':')[0]), int.Parse(item.start_to_needle_time.Split(':')[1]), int.Parse(item.start_to_needle_time.Split(':')[2])).TotalMinutes;
                                    if (time <= 30)
                                    {
                                        STN30++;
                                    }
                                    if (time <= 45)
                                    {
                                        STN45++;
                                    }
                                    if (time <= 60)
                                    {
                                        STN60++;
                                    }
                                    STNTotal++;
                                }
                                if (item.cpoe_order_to_needle != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.cpoe_order_to_needle.Split(':')[0]), int.Parse(item.cpoe_order_to_needle.Split(':')[1]), int.Parse(item.cpoe_order_to_needle.Split(':')[2])).TotalSeconds;
                                    CPOEtoneedle_meanlist.Add(time);
                                    CPOEtoneedle_medianlist.Add(time);
                                    casecount++;
                                }
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        TimeSpan _mediantime = new TimeSpan();
                        TimeSpan Start_to_Response_meantime = new TimeSpan();
                        TimeSpan Start_to_Response_mediantime = new TimeSpan();
                        //TimeSpan Bedside_meantime = new TimeSpan();
                        //TimeSpan Bedside_mediantime = new TimeSpan();
                        TimeSpan Door_to_Needle_meantime = new TimeSpan();
                        TimeSpan Door_to_Needle_mediantime = new TimeSpan();
                        TimeSpan Verbal_to_Administration_meantime = new TimeSpan();
                        TimeSpan CPOEtoneedle_meantime = new TimeSpan();
                        TimeSpan CPOEtoneedle_mediantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }
                        if (_medianlist.Count > 0)
                        {
                            int numbercount = _medianlist.Count();
                            int halfindex = _medianlist.Count() / 2;
                            var sortednumbers = _medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        if (Start_to_Response_meanlist.Count > 0)
                        {
                            double mean = Start_to_Response_meanlist.Average();
                            Start_to_Response_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }
                        if (Start_to_Response_medianlist.Count > 0)
                        {
                            int numbercount = Start_to_Response_medianlist.Count();
                            int halfindex = Start_to_Response_medianlist.Count() / 2;
                            var sortednumbers = Start_to_Response_medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            Start_to_Response_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        //if (Bedside_meanlist.Count > 0)
                        //{
                        //    double mean = Bedside_meanlist.Average();
                        //    Bedside_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        //}
                        //if (Bedside_medianlist.Count > 0)
                        //{
                        //    int numbercount = Bedside_medianlist.Count();
                        //    int halfindex = Bedside_medianlist.Count() / 2;
                        //    var sortednumbers = Bedside_medianlist.OrderBy(x => x);
                        //    double median;
                        //    if ((numbercount % 2) == 0)
                        //    {
                        //        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                        //    }
                        //    else
                        //    {
                        //        median = sortednumbers.ElementAt(halfindex);
                        //    }
                        //    Bedside_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        //}
                        if (Door_to_Needle_meanlist.Count > 0)
                        {
                            double mean = Door_to_Needle_meanlist.Average();
                            Door_to_Needle_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }
                        if (Door_to_Needle_medianlist.Count > 0)
                        {
                            int numbercount = Door_to_Needle_medianlist.Count();
                            int halfindex = Door_to_Needle_medianlist.Count() / 2;
                            var sortednumbers = Door_to_Needle_medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            Door_to_Needle_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        if (Verbal_to_Administration_meanlist.Count > 0)
                        {
                            double mean = Verbal_to_Administration_meanlist.Average();
                            Verbal_to_Administration_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }
                        if (CPOEtoneedle_meanlist.Count > 0)
                        {
                            double mean = CPOEtoneedle_meanlist.Average();
                            CPOEtoneedle_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }
                        if (CPOEtoneedle_medianlist.Count > 0)
                        {
                            int numbercount = CPOEtoneedle_medianlist.Count();
                            int halfindex = CPOEtoneedle_medianlist.Count() / 2;
                            var sortednumbers = CPOEtoneedle_medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            CPOEtoneedle_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        int emsresult = emscount != 0 && totalemscount != 0 ? (int)Math.Round((double)emscount / totalemscount * 100) : 0;
                        int pvresult = pvcount != 0 && totalpvcount != 0 ? (int)Math.Round((double)pvcount / totalpvcount * 100) : 0;
                        //int inpresult = inpcount != 0 && totalinpcount != 0 ? (int)Math.Round((double)inpcount / totalinpcount * 100) : 0;
                        int DTN30_Result = DTN30 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN30 / DTNTotal * 100) : 0;
                        int DTN45_Result = DTN45 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN45 / DTNTotal * 100) : 0;
                        int DTN60_Result = DTN60 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN60 / DTNTotal * 100) : 0;
                        int STN30_Result = STN30 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN30 / STNTotal * 100) : 0;
                        int STN45_Result = STN45 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN45 / STNTotal * 100) : 0;
                        int STN60_Result = STN60 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN60 / STNTotal * 100) : 0;
                        int BedSideResult = BedsideCount != 0 && BedsideTotal != 0 ? (int)Math.Round((double)BedsideCount / BedsideTotal * 100) : 0;

                        qag_key = quality_Goals.qag_key;
                        if (quality_Goals.qag_fac_key.HasValue)
                        {
                            fac_key = quality_Goals.qag_fac_key.Value;
                        }

                        arrivaltostartave.QualityMetrics = "Door to TS Notification Ave. Minutes";
                        arrivaltostartmed.QualityMetrics = "Door to TS Notification Median Minutes";
                        MinorLessActivationEMS.QualityMetrics = "% 10 Min or Less Activation (EMS)";
                        MinorLessActivationPV.QualityMetrics = "% 10 Min or Less Activation (PV)";
                        //MinorLessActivationINP.QualityMetrics = "% 10 Min or Less Activation (Inpt)";
                        TSNotificationResponseAverageMinute.QualityMetrics = "TS Notification to Response Average Minute";
                        TSNotificationResponseMedianMinute.QualityMetrics = "TS Notification to Response Median Minute";
                        TSatBedside10Minutes.QualityMetrics = "% TS at Bedside <10 Minutes";
                        ALTEPLASEADMINISTERED.QualityMetrics = "ALTEPLASE ADMINISTERED #";
                        DoortoNeedleAverageminsec.QualityMetrics = "Door to Needle Average (min:sec)";
                        DoortoNeedleMedianminsec.QualityMetrics = "Door to Needle Median (min:sec)";
                        VerbalOrdertoAdministrationAverageMinutes.QualityMetrics = "Alteplase early mix decision to Administration Average Minutes";
                        DTN30Minutes.QualityMetrics = "DTN Less or Equal 30 Minutes %";
                        DTN45Minutes.QualityMetrics = "DTN Less or Equal 45 Minutes %";
                        DTN60Minutes.QualityMetrics = "DTN Less or Equal 60 Minutes %";
                        TSNotificationtoNeedle30Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 30 Minutes %";
                        TSNotificationtoNeedle45Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 45 Minutes %";
                        TSNotificationtoNeedle60Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 60 Minutes %";
                        CPOEtoNeedleTimeave.QualityMetrics = "CPOE order to Needle time Ave. Minutes";
                        CPOEtoNeedleTimemed.QualityMetrics = "CPOE order to Needle time Median Minutes";

                        if (Month == "January")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }
                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }
                            }
                            arrivaltostartave.JanData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.JanData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.JanData = emsresult + "%";
                            MinorLessActivationPV.JanData = pvresult + "%";
                            //MinorLessActivationINP.JanData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }
                            }
                            TSNotificationResponseAverageMinute.JanData = Start_to_Response_minute + ":" + Start_to_Response_seconds; //(Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes + ":" + Start_to_Response_meantime.Seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.JanData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;//(Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes + ":" + Start_to_Response_mediantime.Seconds;
                            TSatBedside10Minutes.JanData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.JanData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.JanData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds; //(Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes + ":" + Door_to_Needle_meantime.Seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.JanData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;//(Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes + ":" + Door_to_Needle_mediantime.Seconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.JanData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;//(Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes + ":" + Verbal_to_Administration_meantime.Seconds;
                            DTN30Minutes.JanData = DTN30_Result + "%";
                            DTN45Minutes.JanData = DTN45_Result + "%";
                            DTN60Minutes.JanData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.JanData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.JanData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.JanData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.JanData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.JanData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.JanGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.JanGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    //MinorLessActivationINP.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    //MinorLessActivationINP.JanGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.JanGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.JanGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.JanGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.JanGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.JanGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JanGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JanGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.JanGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.JanGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.JanGoals = "";
                                CPOEtoNeedleTimemed.JanGoals = "";
                            }

                        }
                        else if (Month == "February")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.FebData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.FebData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.FebData = emsresult + "%";
                            MinorLessActivationPV.FebData = pvresult + "%";
                            //MinorLessActivationINP.FebData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.FebData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.FebData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.FebData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.FebData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.FebData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.FebData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.FebData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.FebData = DTN30_Result + "%";
                            DTN45Minutes.FebData = DTN45_Result + "%";
                            DTN60Minutes.FebData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.FebData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.FebData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.FebData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.FebData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.FebData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.FebGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.FebGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.FebGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.FebGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.FebGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.FebGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.FebGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.FebGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.FebGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.FebGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.FebGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.FebGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.FebGoals = "";
                                CPOEtoNeedleTimemed.FebGoals = "";
                            }

                        }
                        else if (Month == "March")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.MarData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.MarData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.MarData = emsresult + "%";
                            MinorLessActivationPV.MarData = pvresult + "%";
                            //MinorLessActivationINP.MarData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.MarData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.MarData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.MarData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.MarData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.MarData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.MarData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.MarData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.MarData = DTN30_Result + "%";
                            DTN45Minutes.MarData = DTN45_Result + "%";
                            DTN60Minutes.MarData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.MarData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.MarData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.MarData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.MarData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.MarData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.MarGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.MarGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.MarGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.MarGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.MarGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.MarGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.MarGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.MarGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.MarGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.MarGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.MarGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.MarGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.MarGoals = "";
                                CPOEtoNeedleTimemed.MarGoals = "";
                            }

                        }
                        else if (Month == "April")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.AprData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.AprData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.AprData = emsresult + "%";
                            MinorLessActivationPV.AprData = pvresult + "%";
                            //MinorLessActivationINP.AprData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.AprData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.AprData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.AprData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.AprData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.AprData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.AprData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.AprData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.AprData = DTN30_Result + "%";
                            DTN45Minutes.AprData = DTN45_Result + "%";
                            DTN60Minutes.AprData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.AprData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.AprData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.AprData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.AprData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.AprData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.AprGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.AprGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.AprGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.AprGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.AprGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.AprGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.AprGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.AprGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.AprGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.AprGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.AprGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.AprGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.AprGoals = "";
                                CPOEtoNeedleTimemed.AprGoals = "";
                            }

                        }
                        else if (Month == "May")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.MayData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.MayData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.MayData = emsresult + "%";
                            MinorLessActivationPV.MayData = pvresult + "%";
                            //MinorLessActivationINP.MayData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.MayData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.MayData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;

                            TSatBedside10Minutes.MayData = BedSideResult + "%";

                            ALTEPLASEADMINISTERED.MayData = tpatrue.ToString();

                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.MayData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;

                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.MayData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.MayData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.MayData = DTN30_Result + "%";
                            DTN45Minutes.MayData = DTN45_Result + "%";
                            DTN60Minutes.MayData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.MayData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.MayData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.MayData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.MayData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.MayData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.MayGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.MayGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.MayGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.MayGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.MayGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.MayGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.MayGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.MayGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.MayGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.MayGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.MayGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.MayGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.MayGoals = "";
                                CPOEtoNeedleTimemed.MayGoals = "";
                            }

                        }
                        else if (Month == "June")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.JunData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.JunData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.JunData = emsresult + "%";
                            MinorLessActivationPV.JunData = pvresult + "%";
                            //MinorLessActivationINP.JunData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.JunData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.JunData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.JunData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.JunData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.JunData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.JunData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.JunData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.JunData = DTN30_Result + "%";
                            DTN45Minutes.JunData = DTN45_Result + "%";
                            DTN60Minutes.JunData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.JunData = STN30_Result + "%";

                            TSNotificationtoNeedle45Minutes.JunData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.JunData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.JunData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.JunData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.JunGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.JunGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.JunGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.JunGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.JunGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.JunGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.JunGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.JunGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JunGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JunGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.JunGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.JunGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.JunGoals = "";
                                CPOEtoNeedleTimemed.JunGoals = "";
                            }

                        }
                        else if (Month == "July")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.JulData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.JulData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.JulData = emsresult + "%";
                            MinorLessActivationPV.JulData = pvresult + "%";
                            //MinorLessActivationINP.JulData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.JulData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.JulData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.JulData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.JulData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.JulData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.JulData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.JulData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.JulData = DTN30_Result + "%";
                            DTN45Minutes.JulData = DTN45_Result + "%";
                            DTN60Minutes.JulData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.JulData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.JulData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.JulData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.JulData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.JulData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.JulGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.JulGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.JulGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.JulGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.JulGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.JulGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.JulGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.JulGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JulGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.JulGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.JulGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.JulGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.JulGoals = "";
                                CPOEtoNeedleTimemed.JulGoals = "";
                            }

                        }
                        else if (Month == "August")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.AugData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.AugData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.AugData = emsresult + "%";
                            MinorLessActivationPV.AugData = pvresult + "%";
                            //MinorLessActivationINP.AugData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.AugData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            //TSNotificationResponseAverageMinute.AugData = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes + ":" + Start_to_Response_meantime.Seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.AugData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            //TSNotificationResponseMedianMinute.AugData = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes + ":" + Start_to_Response_mediantime.Seconds;
                            TSatBedside10Minutes.AugData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.AugData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.AugData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            //DoortoNeedleAverageminsec.AugData = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes + ":" + Door_to_Needle_meantime.Seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.AugData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            //DoortoNeedleMedianminsec.AugData = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes + ":" + Door_to_Needle_mediantime.Seconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.AugData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            //VerbalOrdertoAdministrationAverageMinutes.AugData = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes + ":" + Verbal_to_Administration_meantime.Seconds;
                            DTN30Minutes.AugData = DTN30_Result + "%";
                            DTN45Minutes.AugData = DTN45_Result + "%";
                            DTN60Minutes.AugData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.AugData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.AugData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.AugData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.AugData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.AugData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.AugGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.AugGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.AugGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.AugGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.AugGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.AugGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.AugGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.AugGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.AugGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.AugGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.AugGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.AugGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.AugGoals = "";
                                CPOEtoNeedleTimemed.AugGoals = "";
                            }

                        }
                        else if (Month == "September")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.SepData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.SepData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.SepData = emsresult + "%";
                            MinorLessActivationPV.SepData = pvresult + "%";
                            //MinorLessActivationINP.SepData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.SepData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.SepData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.SepData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.SepData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.SepData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.SepData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.SepData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.SepData = DTN30_Result + "%";
                            DTN45Minutes.SepData = DTN45_Result + "%";
                            DTN60Minutes.SepData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.SepData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.SepData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.SepData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.SepData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.SepData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.SepGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.SepGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.SepGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.SepGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.SepGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.SepGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.SepGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.SepGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.SepGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.SepGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.SepGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.SepGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.SepGoals = "";
                                CPOEtoNeedleTimemed.SepGoals = "";
                            }

                        }
                        else if (Month == "October")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.OctData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.OctData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.OctData = emsresult + "%";
                            MinorLessActivationPV.OctData = pvresult + "%";
                            //MinorLessActivationINP.OctData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.OctData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.OctData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.OctData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.OctData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.OctData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.OctData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.OctData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.OctData = DTN30_Result + "%";
                            DTN45Minutes.OctData = DTN45_Result + "%";
                            DTN60Minutes.OctData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.OctData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.OctData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.OctData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.OctData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.OctData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.OctGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.OctGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.OctGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.OctGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.OctGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.OctGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.OctGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.OctGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.OctGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.OctGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.OctGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.OctGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.OctGoals = "";
                                CPOEtoNeedleTimemed.OctGoals = "";
                            }

                        }
                        else if (Month == "November")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.NovData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.NovData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.NovData = emsresult + "%";
                            MinorLessActivationPV.NovData = pvresult + "%";
                            //MinorLessActivationINP.NovData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.NovData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.NovData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.NovData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.NovData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.NovData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.NovData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.NovData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.NovData = DTN30_Result + "%";
                            DTN45Minutes.NovData = DTN45_Result + "%";
                            DTN60Minutes.NovData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.NovData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.NovData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.NovData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.NovData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.NovData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.NovGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.NovGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.NovGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.NovGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.NovGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.NovGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.NovGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.NovGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.NovGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.NovGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.NovGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.NovGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.NovGoals = "";
                                CPOEtoNeedleTimemed.NovGoals = "";
                            }

                        }
                        else if (Month == "December")
                        {
                            int minutes = (_meantime.Hours * 60) + _meantime.Minutes;
                            string seconds = "00";
                            string minute = "00";
                            if (_meantime.Seconds != 0)
                            {
                                int sec = _meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    seconds = "0" + _meantime.Seconds.ToString();
                                }
                                else
                                {
                                    seconds = _meantime.Seconds.ToString();
                                }

                            }
                            if (minutes != 0)
                            {
                                int min = minutes.ToString().Length;
                                if (min == 1)
                                {
                                    minute = "0" + minutes.ToString();
                                }
                                else
                                {
                                    minute = minutes.ToString();
                                }

                            }
                            arrivaltostartave.DecData = minute + ":" + seconds;
                            int midminutes = (_mediantime.Hours * 60) + _mediantime.Minutes;
                            string midseconds = "00";
                            string midminute = "00";
                            if (_mediantime.Seconds != 0)
                            {
                                int sec = _mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    midseconds = "0" + _mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    midseconds = _mediantime.Seconds.ToString();
                                }
                            }
                            if (midminutes != 0)
                            {
                                int min = midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    midminute = "0" + midminutes.ToString();

                                }
                                else
                                {
                                    midminute = midminutes.ToString();
                                }
                            }
                            arrivaltostartmed.DecData = midminute + ":" + midseconds;
                            MinorLessActivationEMS.DecData = emsresult + "%";
                            MinorLessActivationPV.DecData = pvresult + "%";
                            //MinorLessActivationINP.DecData = inpresult + "%";
                            int Start_to_Response_minutes = (Start_to_Response_meantime.Hours * 60) + Start_to_Response_meantime.Minutes;
                            string Start_to_Response_seconds = "00";
                            string Start_to_Response_minute = "00";
                            if (Start_to_Response_meantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_seconds = "0" + Start_to_Response_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_seconds = Start_to_Response_meantime.Seconds.ToString();
                                }

                            }
                            if (Start_to_Response_minutes != 0)
                            {
                                int min = Start_to_Response_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_minute = Start_to_Response_minutes.ToString();
                                }

                            }
                            TSNotificationResponseAverageMinute.DecData = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                            int Start_to_Response_midminutes = (Start_to_Response_mediantime.Hours * 60) + Start_to_Response_mediantime.Minutes;
                            string Start_to_Response_midseconds = "00";
                            string Start_to_Response_midminute = "00";
                            if (Start_to_Response_mediantime.Seconds != 0)
                            {
                                int sec = Start_to_Response_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Start_to_Response_midseconds = "0" + Start_to_Response_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Start_to_Response_midseconds = Start_to_Response_mediantime.Seconds.ToString();
                                }
                            }
                            if (Start_to_Response_midminutes != 0)
                            {
                                int min = Start_to_Response_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                                }
                                else
                                {
                                    Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                                }
                            }
                            TSNotificationResponseMedianMinute.DecData = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                            TSatBedside10Minutes.DecData = BedSideResult + "%";
                            ALTEPLASEADMINISTERED.DecData = tpatrue.ToString();
                            int Door_to_Needle_minutes = (Door_to_Needle_meantime.Hours * 60) + Door_to_Needle_meantime.Minutes;
                            string Door_to_Needle_seconds = "00";
                            string Door_to_Needle_minute = "00";
                            if (Door_to_Needle_meantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_seconds = "0" + Door_to_Needle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_seconds = Door_to_Needle_meantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_minutes != 0)
                            {
                                int min = Door_to_Needle_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                                }

                            }
                            DoortoNeedleAverageminsec.DecData = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                            int Door_to_Needle_midminutes = (Door_to_Needle_mediantime.Hours * 60) + Door_to_Needle_mediantime.Minutes;
                            string Door_to_Needle_midseconds = "00";
                            string Door_to_Needle_midminute = "00";
                            if (Door_to_Needle_mediantime.Seconds != 0)
                            {
                                int sec = Door_to_Needle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Door_to_Needle_midseconds = "0" + Door_to_Needle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    Door_to_Needle_midseconds = Door_to_Needle_mediantime.Seconds.ToString();
                                }

                            }
                            if (Door_to_Needle_midminutes != 0)
                            {
                                int min = Door_to_Needle_midminutes.ToString().Length;
                                if (min == 1)
                                {
                                    Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                                }
                                else
                                {
                                    Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                                }

                            }
                            DoortoNeedleMedianminsec.DecData = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                            int Verbal_to_Administration_minutes = (Verbal_to_Administration_meantime.Hours * 60) + Verbal_to_Administration_meantime.Minutes;
                            string Verbal_to_Administration_seconds = "00";
                            string Verbal_to_Administration_minute = "00";
                            if (Verbal_to_Administration_meantime.Seconds != 0)
                            {
                                int sec = Verbal_to_Administration_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    Verbal_to_Administration_seconds = "0" + Verbal_to_Administration_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    Verbal_to_Administration_seconds = Verbal_to_Administration_meantime.Seconds.ToString();
                                }

                            }
                            if (Verbal_to_Administration_minutes != 0)
                            {
                                int min = Verbal_to_Administration_minutes.ToString().Length;
                                if (min == 1)
                                {
                                    Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                                }
                                else
                                {
                                    Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                                }

                            }
                            VerbalOrdertoAdministrationAverageMinutes.DecData = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                            DTN30Minutes.DecData = DTN30_Result + "%";
                            DTN45Minutes.DecData = DTN45_Result + "%";
                            DTN60Minutes.DecData = DTN60_Result + "%";
                            TSNotificationtoNeedle30Minutes.DecData = STN30_Result + "%";
                            TSNotificationtoNeedle45Minutes.DecData = STN45_Result + "%";
                            TSNotificationtoNeedle60Minutes.DecData = STN60_Result + "%";

                            int CPOEminutes = (CPOEtoneedle_meantime.Hours * 60) + CPOEtoneedle_meantime.Minutes;
                            string CPOEseconds = "00";
                            string CPOEminute = "00";
                            if (CPOEtoneedle_meantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_meantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEseconds = "0" + CPOEtoneedle_meantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEseconds = CPOEtoneedle_meantime.Seconds.ToString();
                                }
                            }
                            if (CPOEminutes != 0)
                            {
                                int min = CPOEminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEminute = "0" + CPOEminutes.ToString();
                                }
                                else
                                {
                                    CPOEminute = CPOEminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimeave.DecData = CPOEminute + ":" + CPOEseconds;
                            int CPOEmidminutes = (CPOEtoneedle_mediantime.Hours * 60) + CPOEtoneedle_mediantime.Minutes;
                            string CPOEmidseconds = "00";
                            string CPOEmidminute = "00";
                            if (CPOEtoneedle_mediantime.Seconds != 0)
                            {
                                int sec = CPOEtoneedle_mediantime.Seconds.ToString().Length;
                                if (sec == 1)
                                {
                                    CPOEmidseconds = "0" + CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                                else
                                {
                                    CPOEmidseconds = CPOEtoneedle_mediantime.Seconds.ToString();
                                }
                            }
                            if (CPOEmidminutes != 0)
                            {
                                int min = CPOEmidminutes.ToString().Length;
                                if (min == 1)
                                {
                                    CPOEmidminute = "0" + CPOEmidminutes.ToString();

                                }
                                else
                                {
                                    CPOEmidminute = CPOEmidminutes.ToString();
                                }
                            }
                            CPOEtoNeedleTimemed.DecData = CPOEmidminute + ":" + CPOEmidseconds;

                            if (GetGoalsbyQuater != null)
                            {
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes))
                                {
                                    arrivaltostartave.DecGoals = GetGoalsbyQuater.qag_door_to_TS_notification_ave_minutes;
                                }
                                else
                                {
                                    arrivaltostartave.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes))
                                {
                                    arrivaltostartmed.DecGoals = GetGoalsbyQuater.qag_door_to_TS_notification_median_minutes;
                                }
                                else
                                {
                                    arrivaltostartmed.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS))
                                {
                                    MinorLessActivationEMS.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS + "%";
                                }
                                else
                                {
                                    MinorLessActivationEMS.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_EMS;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV))
                                {
                                    MinorLessActivationPV.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV + "%";
                                }
                                else
                                {
                                    MinorLessActivationPV.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_PV;
                                }
                                //if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt))
                                //{
                                //    MinorLessActivationINP.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt + "%";
                                //}
                                //else
                                //{
                                //    MinorLessActivationINP.DecGoals = GetGoalsbyQuater.qag_percent10_min_or_less_activation_Inpt;
                                //}
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_average_minute))
                                {
                                    TSNotificationResponseAverageMinute.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_response_average_minute;
                                }
                                else
                                {
                                    TSNotificationResponseAverageMinute.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_response_median_minute))
                                {
                                    TSNotificationResponseMedianMinute.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_response_median_minute;
                                }
                                else
                                {
                                    TSNotificationResponseMedianMinute.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes))
                                {
                                    TSatBedside10Minutes.DecGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes + "%";
                                }
                                else
                                {
                                    TSatBedside10Minutes.DecGoals = GetGoalsbyQuater.qag_percent_TS_at_bedside_grterthan10_minutes;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_alteplase_administered))
                                {
                                    ALTEPLASEADMINISTERED.DecGoals = GetGoalsbyQuater.qag_alteplase_administered;
                                }
                                else
                                {
                                    ALTEPLASEADMINISTERED.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_average))
                                {
                                    DoortoNeedleAverageminsec.DecGoals = GetGoalsbyQuater.qag_door_to_needle_average;
                                }
                                else
                                {
                                    DoortoNeedleAverageminsec.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_door_to_needle_median))
                                {
                                    DoortoNeedleMedianminsec.DecGoals = GetGoalsbyQuater.qag_door_to_needle_median;
                                }
                                else
                                {
                                    DoortoNeedleMedianminsec.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes))
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.DecGoals = GetGoalsbyQuater.qag_verbal_order_to_administration_average_minutes;
                                }
                                else
                                {
                                    VerbalOrdertoAdministrationAverageMinutes.DecGoals = "";
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent))
                                {
                                    DTN30Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    DTN30Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent))
                                {
                                    DTN45Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    DTN45Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent))
                                {
                                    DTN60Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    DTN60Minutes.DecGoals = GetGoalsbyQuater.qag_DTN_grter_or_equal_60minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent))
                                {
                                    TSNotificationtoNeedle30Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle30Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent))
                                {
                                    TSNotificationtoNeedle45Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle45Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                                }
                                if (!string.IsNullOrEmpty(GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent))
                                {
                                    TSNotificationtoNeedle60Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent + "%";
                                }
                                else
                                {
                                    TSNotificationtoNeedle60Minutes.DecGoals = GetGoalsbyQuater.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                                }
                                CPOEtoNeedleTimeave.DecGoals = "";
                                CPOEtoNeedleTimemed.DecGoals = "";
                            }

                        }
                        StartDate = enddate.AddDays(1);
                    }
                }
                else if (model.ReportType == "cumulative")
                {
                    var result = query.ToList();
                    List<double> cum_meanlist = new List<double>();
                    List<double> cum_medianlist = new List<double>();
                    List<double> cum_Start_to_Response_meanlist = new List<double>();
                    List<double> cum_Start_to_Response_medianlist = new List<double>();
                    List<double> cum_Door_to_Needle_meanlist = new List<double>();
                    List<double> cum_Door_to_Needle_medianlist = new List<double>();
                    List<double> cum_Verbal_to_Administration_meanlist = new List<double>();
                    List<double> CPOE_cum_meanlist = new List<double>();
                    List<double> CPOE_cum_medianlist = new List<double>();
                    int cum_casecount = 0;
                    int cum_emscount = 0;
                    int cum_totalemscount = 0;
                    int cum_pvcount = 0;
                    int cum_totalpvcount = 0;
                    int cum_BedsideCount = 0;
                    int cum_BedsideTotal = 0;
                    int cum_tpatrue = 0;
                    int cum_DTN30 = 0;
                    int cum_DTN45 = 0;
                    int cum_DTN60 = 0;
                    int cum_DTNTotal = 0;
                    int cum_STN30 = 0;
                    int cum_STN45 = 0;
                    int cum_STN60 = 0;
                    int cum_STNTotal = 0;
                    if (result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            if (item.arrival_to_start != "")
                            {
                                var time = new TimeSpan(int.Parse(item.arrival_to_start.Split(':')[0]), int.Parse(item.arrival_to_start.Split(':')[1]), int.Parse(item.arrival_to_start.Split(':')[2])).TotalSeconds;
                                cum_meanlist.Add(time);
                                cum_medianlist.Add(time);
                                cum_casecount++;
                            }
                            if (item.emspatienttype != "")
                            {
                                var time = new TimeSpan(int.Parse(item.emspatienttype.Split(':')[0]), int.Parse(item.emspatienttype.Split(':')[1]), int.Parse(item.emspatienttype.Split(':')[2])).TotalMinutes;
                                if (time <= 10)
                                {
                                    cum_emscount++;
                                }
                                cum_totalemscount++;
                            }
                            if (item.pvpatienttype != "")
                            {
                                var time = new TimeSpan(int.Parse(item.pvpatienttype.Split(':')[0]), int.Parse(item.pvpatienttype.Split(':')[1]), int.Parse(item.pvpatienttype.Split(':')[2])).TotalMinutes;
                                if (time <= 10)
                                {
                                    cum_pvcount++;
                                }
                                cum_totalpvcount++;
                            }
                            if (item.start_to_response != "")
                            {
                                var time = new TimeSpan(int.Parse(item.start_to_response.Split(':')[0]), int.Parse(item.start_to_response.Split(':')[1]), int.Parse(item.start_to_response.Split(':')[2])).TotalSeconds;
                                cum_Start_to_Response_meanlist.Add(time);
                                cum_Start_to_Response_medianlist.Add(time);
                            }
                            if (item.bedside_response_time != "")
                            {
                                var time = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalSeconds;
                                var MiNtime = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalMinutes;
                                if (MiNtime < 10)
                                {
                                    cum_BedsideCount++;
                                }
                                cum_BedsideTotal++;
                            }
                            if (item.tpatrue == true)
                            {
                                cum_tpatrue++;
                            }
                            if (item.arrival_to_needle_time != "")
                            {
                                var time = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalSeconds;
                                var MINtime = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalMinutes;
                                cum_Door_to_Needle_meanlist.Add(time);
                                cum_Door_to_Needle_medianlist.Add(time);
                                if (MINtime <= 30)
                                {
                                    cum_DTN30++;
                                }
                                if (MINtime <= 45)
                                {
                                    cum_DTN45++;
                                }
                                if (MINtime <= 60)
                                {
                                    cum_DTN60++;
                                }
                                cum_DTNTotal++;
                            }
                            if (item.verbal_order_to_needle_time != "")
                            {
                                var time = new TimeSpan(int.Parse(item.verbal_order_to_needle_time.Split(':')[0]), int.Parse(item.verbal_order_to_needle_time.Split(':')[1]), int.Parse(item.verbal_order_to_needle_time.Split(':')[2])).TotalSeconds;
                                cum_Verbal_to_Administration_meanlist.Add(time);
                            }
                            if (item.start_to_needle_time != "")
                            {
                                var time = new TimeSpan(int.Parse(item.start_to_needle_time.Split(':')[0]), int.Parse(item.start_to_needle_time.Split(':')[1]), int.Parse(item.start_to_needle_time.Split(':')[2])).TotalMinutes;
                                if (time <= 30)
                                {
                                    cum_STN30++;
                                }
                                if (time <= 45)
                                {
                                    cum_STN45++;
                                }
                                if (time <= 60)
                                {
                                    cum_STN60++;
                                }
                                cum_STNTotal++;
                            }
                            if (item.cpoe_order_to_needle != "")
                            {
                                var time = new TimeSpan(int.Parse(item.cpoe_order_to_needle.Split(':')[0]), int.Parse(item.cpoe_order_to_needle.Split(':')[1]), int.Parse(item.cpoe_order_to_needle.Split(':')[2])).TotalSeconds;
                                CPOE_cum_meanlist.Add(time);
                                CPOE_cum_medianlist.Add(time);
                                cum_casecount++;
                            }
                        }
                    }

                    TimeSpan cum_meantime = new TimeSpan();
                    TimeSpan cum_mediantime = new TimeSpan();
                    TimeSpan cum_Start_to_Response_meantime = new TimeSpan();
                    TimeSpan cum_Start_to_Response_mediantime = new TimeSpan();
                    TimeSpan cum_Door_to_Needle_meantime = new TimeSpan();
                    TimeSpan cum_Door_to_Needle_mediantime = new TimeSpan();
                    TimeSpan cum_Verbal_to_Administration_meantime = new TimeSpan();
                    TimeSpan CPOE_cum_meantime = new TimeSpan();
                    TimeSpan CPOE_cum_mediantime = new TimeSpan();
                    if (cum_meanlist.Count > 0)
                    {
                        double mean = cum_meanlist.Average();
                        cum_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                    }
                    if (cum_medianlist.Count > 0)
                    {
                        int numbercount = cum_medianlist.Count();
                        int halfindex = cum_medianlist.Count() / 2;
                        var sortednumbers = cum_medianlist.OrderBy(x => x);
                        double median;
                        if ((numbercount % 2) == 0)
                        {
                            median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                        }
                        else
                        {
                            median = sortednumbers.ElementAt(halfindex);
                        }
                        cum_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                    }
                    if (cum_Start_to_Response_meanlist.Count > 0)
                    {
                        double mean = cum_Start_to_Response_meanlist.Average();
                        cum_Start_to_Response_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                    }
                    if (cum_Start_to_Response_medianlist.Count > 0)
                    {
                        int numbercount = cum_Start_to_Response_medianlist.Count();
                        int halfindex = cum_Start_to_Response_medianlist.Count() / 2;
                        var sortednumbers = cum_Start_to_Response_medianlist.OrderBy(x => x);
                        double median;
                        if ((numbercount % 2) == 0)
                        {
                            median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                        }
                        else
                        {
                            median = sortednumbers.ElementAt(halfindex);
                        }
                        cum_Start_to_Response_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                    }

                    if (cum_Door_to_Needle_meanlist.Count > 0)
                    {
                        double mean = cum_Door_to_Needle_meanlist.Average();
                        cum_Door_to_Needle_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                    }
                    if (cum_Door_to_Needle_medianlist.Count > 0)
                    {
                        int numbercount = cum_Door_to_Needle_medianlist.Count();
                        int halfindex = cum_Door_to_Needle_medianlist.Count() / 2;
                        var sortednumbers = cum_Door_to_Needle_medianlist.OrderBy(x => x);
                        double median;
                        if ((numbercount % 2) == 0)
                        {
                            median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                        }
                        else
                        {
                            median = sortednumbers.ElementAt(halfindex);
                        }
                        cum_Door_to_Needle_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                    }
                    if (cum_Verbal_to_Administration_meanlist.Count > 0)
                    {
                        double mean = cum_Verbal_to_Administration_meanlist.Average();
                        cum_Verbal_to_Administration_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                    }

                    if (CPOE_cum_meanlist.Count > 0)
                    {
                        double mean = CPOE_cum_meanlist.Average();
                        CPOE_cum_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                    }
                    if (CPOE_cum_medianlist.Count > 0)
                    {
                        int numbercount = CPOE_cum_medianlist.Count();
                        int halfindex = CPOE_cum_medianlist.Count() / 2;
                        var sortednumbers = CPOE_cum_medianlist.OrderBy(x => x);
                        double median;
                        if ((numbercount % 2) == 0)
                        {
                            median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                        }
                        else
                        {
                            median = sortednumbers.ElementAt(halfindex);
                        }
                        CPOE_cum_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                    }

                    int cum_emsresult = cum_emscount != 0 && cum_totalemscount != 0 ? (int)Math.Round((double)cum_emscount / cum_totalemscount * 100) : 0;
                    int cum_pvresult = cum_pvcount != 0 && cum_totalpvcount != 0 ? (int)Math.Round((double)cum_pvcount / cum_totalpvcount * 100) : 0;
                    int cum_DTN30_Result = cum_DTN30 != 0 && cum_DTNTotal != 0 ? (int)Math.Round((double)cum_DTN30 / cum_DTNTotal * 100) : 0;
                    int cum_DTN45_Result = cum_DTN45 != 0 && cum_DTNTotal != 0 ? (int)Math.Round((double)cum_DTN45 / cum_DTNTotal * 100) : 0;
                    int cum_DTN60_Result = cum_DTN60 != 0 && cum_DTNTotal != 0 ? (int)Math.Round((double)cum_DTN60 / cum_DTNTotal * 100) : 0;
                    int cum_STN30_Result = cum_STN30 != 0 && cum_STNTotal != 0 ? (int)Math.Round((double)cum_STN30 / cum_STNTotal * 100) : 0;
                    int cum_STN45_Result = cum_STN45 != 0 && cum_STNTotal != 0 ? (int)Math.Round((double)cum_STN45 / cum_STNTotal * 100) : 0;
                    int cum_STN60_Result = cum_STN60 != 0 && cum_STNTotal != 0 ? (int)Math.Round((double)cum_STN60 / cum_STNTotal * 100) : 0;
                    int cum_BedSideResult = cum_BedsideCount != 0 && cum_BedsideTotal != 0 ? (int)Math.Round((double)cum_BedsideCount / cum_BedsideTotal * 100) : 0;

                    qag_key = quality_Goals.qag_key;
                    if (quality_Goals.qag_fac_key.HasValue)
                    {
                        fac_key = quality_Goals.qag_fac_key.Value;
                    }

                    arrivaltostartave.QualityMetrics = "Door to TS Notification Ave. Minutes";
                    arrivaltostartmed.QualityMetrics = "Door to TS Notification Median Minutes";
                    MinorLessActivationEMS.QualityMetrics = "% 10 Min or Less Activation (EMS)";
                    MinorLessActivationPV.QualityMetrics = "% 10 Min or Less Activation (PV)";
                    TSNotificationResponseAverageMinute.QualityMetrics = "TS Notification to Response Average Minute";
                    TSNotificationResponseMedianMinute.QualityMetrics = "TS Notification to Response Median Minute";
                    TSatBedside10Minutes.QualityMetrics = "% TS at Bedside <10 Minutes";
                    ALTEPLASEADMINISTERED.QualityMetrics = "ALTEPLASE ADMINISTERED #";
                    DoortoNeedleAverageminsec.QualityMetrics = "Door to Needle Average (min:sec)";
                    DoortoNeedleMedianminsec.QualityMetrics = "Door to Needle Median (min:sec)";
                    VerbalOrdertoAdministrationAverageMinutes.QualityMetrics = "Alteplase early mix decision to Administration Average Minutes";
                    DTN30Minutes.QualityMetrics = "DTN Less or Equal 30 Minutes %";
                    DTN45Minutes.QualityMetrics = "DTN Less or Equal 45 Minutes %";
                    DTN60Minutes.QualityMetrics = "DTN Less or Equal 60 Minutes %";
                    TSNotificationtoNeedle30Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 30 Minutes %";
                    TSNotificationtoNeedle45Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 45 Minutes %";
                    TSNotificationtoNeedle60Minutes.QualityMetrics = "TS Notification to Needle Less or Equal 60 Minutes %";
                    CPOEtoNeedleTimeave.QualityMetrics = "CPOE order to Needle time Ave. Minutes";
                    CPOEtoNeedleTimemed.QualityMetrics = "CPOE order to Needle time Median Minutes";

                    int minutes = (cum_meantime.Hours * 60) + cum_meantime.Minutes;
                    string seconds = "00";
                    string minute = "00";
                    if (cum_meantime.Seconds != 0)
                    {
                        int sec = cum_meantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            seconds = "0" + cum_meantime.Seconds.ToString();
                        }
                        else
                        {
                            seconds = cum_meantime.Seconds.ToString();
                        }
                    }
                    if (minutes != 0)
                    {
                        int min = minutes.ToString().Length;
                        if (min == 1)
                        {
                            minute = "0" + minutes.ToString();
                        }
                        else
                        {
                            minute = minutes.ToString();
                        }
                    }
                    arrivaltostartave.Cumulative = minute + ":" + seconds;
                    int midminutes = (cum_mediantime.Hours * 60) + cum_mediantime.Minutes;
                    string midseconds = "00";
                    string midminute = "00";
                    if (cum_mediantime.Seconds != 0)
                    {
                        int sec = cum_mediantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            midseconds = "0" + cum_mediantime.Seconds.ToString();
                        }
                        else
                        {
                            midseconds = cum_mediantime.Seconds.ToString();
                        }
                    }
                    if (midminutes != 0)
                    {
                        int min = midminutes.ToString().Length;
                        if (min == 1)
                        {
                            midminute = "0" + midminutes.ToString();

                        }
                        else
                        {
                            midminute = midminutes.ToString();
                        }
                    }
                    arrivaltostartmed.Cumulative = midminute + ":" + midseconds;
                    MinorLessActivationEMS.Cumulative = cum_emsresult + "%";
                    MinorLessActivationPV.Cumulative = cum_pvresult + "%";
                    int Start_to_Response_minutes = (cum_Start_to_Response_meantime.Hours * 60) + cum_Start_to_Response_meantime.Minutes;
                    string Start_to_Response_seconds = "00";
                    string Start_to_Response_minute = "00";
                    if (cum_Start_to_Response_meantime.Seconds != 0)
                    {
                        int sec = cum_Start_to_Response_meantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            Start_to_Response_seconds = "0" + cum_Start_to_Response_meantime.Seconds.ToString();
                        }
                        else
                        {
                            Start_to_Response_seconds = cum_Start_to_Response_meantime.Seconds.ToString();
                        }
                    }
                    if (Start_to_Response_minutes != 0)
                    {
                        int min = Start_to_Response_minutes.ToString().Length;
                        if (min == 1)
                        {
                            Start_to_Response_minute = "0" + Start_to_Response_minutes.ToString();

                        }
                        else
                        {
                            Start_to_Response_minute = Start_to_Response_minutes.ToString();
                        }
                    }
                    TSNotificationResponseAverageMinute.Cumulative = Start_to_Response_minute + ":" + Start_to_Response_seconds;
                    int Start_to_Response_midminutes = (cum_Start_to_Response_mediantime.Hours * 60) + cum_Start_to_Response_mediantime.Minutes;
                    string Start_to_Response_midseconds = "00";
                    string Start_to_Response_midminute = "00";
                    if (cum_Start_to_Response_mediantime.Seconds != 0)
                    {
                        int sec = cum_Start_to_Response_mediantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            Start_to_Response_midseconds = "0" + cum_Start_to_Response_mediantime.Seconds.ToString();
                        }
                        else
                        {
                            Start_to_Response_midseconds = cum_Start_to_Response_mediantime.Seconds.ToString();
                        }
                    }
                    if (Start_to_Response_midminutes != 0)
                    {
                        int min = Start_to_Response_midminutes.ToString().Length;
                        if (min == 1)
                        {
                            Start_to_Response_midminute = "0" + Start_to_Response_midminutes.ToString();

                        }
                        else
                        {
                            Start_to_Response_midminute = Start_to_Response_midminutes.ToString();
                        }
                    }
                    TSNotificationResponseMedianMinute.Cumulative = Start_to_Response_midminute + ":" + Start_to_Response_midseconds;
                    TSatBedside10Minutes.Cumulative = cum_BedSideResult + "%";
                    ALTEPLASEADMINISTERED.Cumulative = cum_tpatrue.ToString();
                    int Door_to_Needle_minutes = (cum_Door_to_Needle_meantime.Hours * 60) + cum_Door_to_Needle_meantime.Minutes;
                    string Door_to_Needle_seconds = "00";
                    string Door_to_Needle_minute = "00";
                    if (cum_Door_to_Needle_meantime.Seconds != 0)
                    {
                        int sec = cum_Door_to_Needle_meantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            Door_to_Needle_seconds = "0" + cum_Door_to_Needle_meantime.Seconds.ToString();
                        }
                        else
                        {
                            Door_to_Needle_seconds = cum_Door_to_Needle_meantime.Seconds.ToString();
                        }

                    }
                    if (Door_to_Needle_minutes != 0)
                    {
                        int min = Door_to_Needle_minutes.ToString().Length;
                        if (min == 1)
                        {
                            Door_to_Needle_minute = "0" + Door_to_Needle_minutes.ToString();

                        }
                        else
                        {
                            Door_to_Needle_minute = Door_to_Needle_minutes.ToString();
                        }

                    }
                    DoortoNeedleAverageminsec.Cumulative = Door_to_Needle_minute + ":" + Door_to_Needle_seconds;
                    int Door_to_Needle_midminutes = (cum_Door_to_Needle_mediantime.Hours * 60) + cum_Door_to_Needle_mediantime.Minutes;
                    string Door_to_Needle_midseconds = "00";
                    string Door_to_Needle_midminute = "00";
                    if (cum_Door_to_Needle_mediantime.Seconds != 0)
                    {
                        int sec = cum_Door_to_Needle_mediantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            Door_to_Needle_midseconds = "0" + cum_Door_to_Needle_mediantime.Seconds.ToString();
                        }
                        else
                        {
                            Door_to_Needle_midseconds = cum_Door_to_Needle_mediantime.Seconds.ToString();
                        }

                    }
                    if (Door_to_Needle_midminutes != 0)
                    {
                        int min = Door_to_Needle_midminutes.ToString().Length;
                        if (min == 1)
                        {
                            Door_to_Needle_midminute = "0" + Door_to_Needle_midminutes.ToString();

                        }
                        else
                        {
                            Door_to_Needle_midminute = Door_to_Needle_midminutes.ToString();
                        }

                    }
                    DoortoNeedleMedianminsec.Cumulative = Door_to_Needle_midminute + ":" + Door_to_Needle_midseconds;
                    int Verbal_to_Administration_minutes = (cum_Verbal_to_Administration_meantime.Hours * 60) + cum_Verbal_to_Administration_meantime.Minutes;
                    string Verbal_to_Administration_seconds = "00";
                    string Verbal_to_Administration_minute = "00";
                    if (cum_Verbal_to_Administration_meantime.Seconds != 0)
                    {
                        int sec = cum_Verbal_to_Administration_meantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            Verbal_to_Administration_seconds = "0" + cum_Verbal_to_Administration_meantime.Seconds.ToString();
                        }
                        else
                        {
                            Verbal_to_Administration_seconds = cum_Verbal_to_Administration_meantime.Seconds.ToString();
                        }

                    }
                    if (Verbal_to_Administration_minutes != 0)
                    {
                        int min = Verbal_to_Administration_minutes.ToString().Length;
                        if (min == 1)
                        {
                            Verbal_to_Administration_minute = "0" + Verbal_to_Administration_minutes.ToString();

                        }
                        else
                        {
                            Verbal_to_Administration_minute = Verbal_to_Administration_minutes.ToString();
                        }

                    }
                    VerbalOrdertoAdministrationAverageMinutes.Cumulative = Verbal_to_Administration_minute + ":" + Verbal_to_Administration_seconds;
                    DTN30Minutes.Cumulative = cum_DTN30_Result + "%";
                    DTN45Minutes.Cumulative = cum_DTN45_Result + "%";
                    DTN60Minutes.Cumulative = cum_DTN60_Result + "%";
                    TSNotificationtoNeedle30Minutes.Cumulative = cum_STN30_Result + "%";
                    TSNotificationtoNeedle45Minutes.Cumulative = cum_STN45_Result + "%";
                    TSNotificationtoNeedle60Minutes.Cumulative = cum_STN60_Result + "%";

                    int CPOEminutes = (CPOE_cum_meantime.Hours * 60) + CPOE_cum_meantime.Minutes;
                    string CPOEseconds = "00";
                    string CPOEminute = "00";
                    if (CPOE_cum_meantime.Seconds != 0)
                    {
                        int sec = CPOE_cum_meantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            CPOEseconds = "0" + CPOE_cum_meantime.Seconds.ToString();
                        }
                        else
                        {
                            CPOEseconds = CPOE_cum_meantime.Seconds.ToString();
                        }
                    }
                    if (CPOEminutes != 0)
                    {
                        int min = CPOEminutes.ToString().Length;
                        if (min == 1)
                        {
                            CPOEminute = "0" + CPOEminutes.ToString();
                        }
                        else
                        {
                            CPOEminute = CPOEminutes.ToString();
                        }
                    }
                    CPOEtoNeedleTimeave.Cumulative = CPOEminute + ":" + CPOEseconds;
                    int CPOEmidminutes = (CPOE_cum_mediantime.Hours * 60) + CPOE_cum_mediantime.Minutes;
                    string CPOEmidseconds = "00";
                    string CPOEmidminute = "00";
                    if (CPOE_cum_mediantime.Seconds != 0)
                    {
                        int sec = CPOE_cum_mediantime.Seconds.ToString().Length;
                        if (sec == 1)
                        {
                            CPOEmidseconds = "0" + CPOE_cum_mediantime.Seconds.ToString();
                        }
                        else
                        {
                            CPOEmidseconds = CPOE_cum_mediantime.Seconds.ToString();
                        }
                    }
                    if (CPOEmidminutes != 0)
                    {
                        int min = CPOEmidminutes.ToString().Length;
                        if (min == 1)
                        {
                            CPOEmidminute = "0" + CPOEmidminutes.ToString();

                        }
                        else
                        {
                            CPOEmidminute = CPOEmidminutes.ToString();
                        }
                    }
                    CPOEtoNeedleTimemed.Cumulative = CPOEmidminute + ":" + CPOEmidseconds;
                }


                list.Add(arrivaltostartave);
                list.Add(arrivaltostartmed);
                list.Add(MinorLessActivationEMS);
                list.Add(MinorLessActivationPV);
                //list.Add(MinorLessActivationINP);
                list.Add(TSNotificationResponseAverageMinute);
                list.Add(TSNotificationResponseMedianMinute);
                list.Add(TSatBedside10Minutes);
                list.Add(ALTEPLASEADMINISTERED);
                list.Add(DoortoNeedleAverageminsec);
                list.Add(DoortoNeedleMedianminsec);
                list.Add(VerbalOrdertoAdministrationAverageMinutes);
                list.Add(DTN30Minutes);
                list.Add(DTN45Minutes);
                list.Add(DTN60Minutes);
                list.Add(TSNotificationtoNeedle30Minutes);
                list.Add(TSNotificationtoNeedle45Minutes);
                list.Add(TSNotificationtoNeedle60Minutes);
                list.Add(CPOEtoNeedleTimeave);
                list.Add(CPOEtoNeedleTimemed);
                // }
                #endregion
                // }

                var _result = list.Select(x => new
                {
                    id = qag_key,
                    fac_key,
                    x.QualityMetrics,
                    x.JanData,
                    x.JanGoals,
                    x.FebData,
                    x.FebGoals,
                    x.MarData,
                    x.MarGoals,
                    x.AprData,
                    x.AprGoals,
                    x.MayData,
                    x.MayGoals,
                    x.JunData,
                    x.JunGoals,
                    x.JulData,
                    x.JulGoals,
                    x.AugData,
                    x.AugGoals,
                    x.SepData,
                    x.SepGoals,
                    x.OctData,
                    x.OctGoals,
                    x.NovData,
                    x.NovGoals,
                    x.DecData,
                    x.DecGoals,
                    x.Cumulative,
                }).AsQueryable();

                return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetFacilityDashboardGraph(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId, string status)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }

                DateTime ModelStartDate = Convert.ToDateTime(model.fromMonth);
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                var EDStay = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    case_number = x.ca.cas_case_number,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",

                    arrival_to_start = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    emspatienttype = x.ca.cas_patient_type == emspatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    pvpatienttype = x.ca.cas_patient_type == pvpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    //inpatienttype = x.ca.cas_metric_symptom_onset_during_ed_stay == true ? x.ca.cas_patient_type == inpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_symptom_onset_during_ed_stay_time) : "" : "",
                    start_to_response = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_first_atempt < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification) : "" : "",
                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    tpatrue = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_tpa_consult : false,
                    arrival_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time) : "" : "" : "",
                    verbal_order_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time) : "",
                    start_to_needle_time = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_response_ts_notification) : "" : "" : "",
                    cpoe_order_to_needle = x.ca.cas_metric_tpa_consult == true ? DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time) : "00:00:00",
                });

                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                List<string> catgory = new List<string>();
                List<string> list = new List<string>();
                int count = 0;
                for (var i = StartDate; StartDate <= EndDate;)
                {
                    string monthName = StartDate.ToString("MMMM");

                    DateTime enddate = StartDate.AddMonths(1).AddDays(-1);
                    #region ----- Filters -----
                    var result = query.Where(x => DbFunctions.TruncateTime(x.created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                                        DbFunctions.TruncateTime(x.created_date) <= DbFunctions.TruncateTime(enddate))
                                                                   .Select(x => new
                                                                   {
                                                                       x.arrival_to_start,
                                                                       x.emspatienttype,
                                                                       x.pvpatienttype,
                                                                           //x.inpatienttype,
                                                                           x.start_to_response,
                                                                       x.bedside_response_time,
                                                                       x.tpatrue,
                                                                       x.arrival_to_needle_time,
                                                                       x.verbal_order_to_needle_time,
                                                                       x.start_to_needle_time,
                                                                       x.cpoe_order_to_needle
                                                                   }).ToList();

                    #endregion


                    List<string> times = new List<string>();
                    List<bool> tpatruelist = new List<bool>();
                    if (status == "Door to TS Notification Ave. Minutes")
                    {
                        graph.Mean = "Mean";
                        times = result.Select(x => x.arrival_to_start).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _meanlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _meantime = new TimeSpan();
                            if (_meanlist.Count > 0)
                            {
                                double mean = _meanlist.Average();
                                _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                            }
                            catgory.Add(monthName);
                            list.Add(_meantime.ToString());
                        }
                    }
                    else if (status == "Door to TS Notification Median Minutes")
                    {
                        graph.Mean = "Median";
                        times = result.Select(x => x.arrival_to_start).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _medianlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _medianlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _mediantime = new TimeSpan();
                            if (_medianlist.Count > 0)
                            {
                                int numbercount = _medianlist.Count();
                                int halfindex = _medianlist.Count() / 2;
                                var sortednumbers = _medianlist.OrderBy(x => x);
                                double median;
                                if ((numbercount % 2) == 0)
                                {
                                    median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                                }
                                else
                                {
                                    median = sortednumbers.ElementAt(halfindex);
                                }
                                _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                            }
                            list.Add(_mediantime.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "% 10 Min or Less Activation (EMS)")
                    {
                        graph.Mean = "EMS %";
                        times = result.Select(x => x.emspatienttype).ToList();

                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 10)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }

                    }
                    else if (status == "% 10 Min or Less Activation (PV)")
                    {
                        graph.Mean = "Triage %";
                        times = result.Select(x => x.pvpatienttype).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 10)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    //else if (status == "% 10 Min or Less Activation (Inpt)")
                    //{
                    //    graph.Mean = "Inpatient %";
                    //    times = result.Select(x => x.inpatienttype).ToList();
                    //    if (times.Count > 0)
                    //    {
                    //        int counts = 0;
                    //        int totalcount = 0;
                    //        foreach (var item in times)
                    //        {
                    //            if (item != "")
                    //            {
                    //                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                    //                if (time <= 10)
                    //                {
                    //                    counts++;
                    //                }
                    //                totalcount++;
                    //            }
                    //        }
                    //        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                    //        list.Add(_result.ToString());
                    //        catgory.Add(monthName);
                    //    }
                    //}
                    else if (status == "TS Notification to Response Average Minute")
                    {
                        graph.Mean = "Mean";
                        times = result.Select(x => x.start_to_response).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _meanlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _meantime = new TimeSpan();
                            if (_meanlist.Count > 0)
                            {
                                double mean = _meanlist.Average();
                                _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                            }
                            catgory.Add(monthName);
                            list.Add(_meantime.ToString());
                        }
                    }
                    else if (status == "TS Notification to Response Median Minute")
                    {
                        graph.Mean = "Median";
                        times = result.Select(x => x.start_to_response).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _medianlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _medianlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _mediantime = new TimeSpan();
                            if (_medianlist.Count > 0)
                            {
                                int numbercount = _medianlist.Count();
                                int halfindex = _medianlist.Count() / 2;
                                var sortednumbers = _medianlist.OrderBy(x => x);
                                double median;
                                if ((numbercount % 2) == 0)
                                {
                                    median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                                }
                                else
                                {
                                    median = sortednumbers.ElementAt(halfindex);
                                }
                                _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                            }
                            list.Add(_mediantime.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "% TS at Bedside <10 Minutes")
                    {
                        graph.Mean = "Bedside %";
                        times = result.Select(x => x.bedside_response_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time < 10)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "ALTEPLASE ADMINISTERED #")
                    {
                        graph.Mean = "TPA";
                        tpatruelist = result.Select(x => x.tpatrue).ToList();
                        if (tpatruelist.Count > 0)
                        {
                            int tpas = 0;
                            foreach (var item in tpatruelist)
                            {
                                if (item == true)
                                {
                                    tpas++;
                                }
                            }
                            list.Add(tpas.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "Door to Needle Average (min:sec)")
                    {
                        graph.Mean = "Mean";
                        times = result.Where(x => x.arrival_to_needle_time != "").Select(x => x.arrival_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _meanlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _meantime = new TimeSpan();
                            if (_meanlist.Count > 0)
                            {
                                double mean = _meanlist.Average();
                                _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                            }

                            catgory.Add(monthName);
                            list.Add(_meantime.ToString());
                        }
                    }
                    else if (status == "Door to Needle Median (min:sec)")
                    {
                        graph.Mean = "Median";
                        times = result.Where(x => x.arrival_to_needle_time != "").Select(x => x.arrival_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _medianlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _medianlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _mediantime = new TimeSpan();
                            if (_medianlist.Count > 0)
                            {
                                int numbercount = _medianlist.Count();
                                int halfindex = _medianlist.Count() / 2;
                                var sortednumbers = _medianlist.OrderBy(x => x);
                                double median;
                                if ((numbercount % 2) == 0)
                                {
                                    median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                                }
                                else
                                {
                                    median = sortednumbers.ElementAt(halfindex);
                                }
                                _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                            }

                            list.Add(_mediantime.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "Alteplase early mix decision to Administration Average Minutes")
                    {
                        graph.Mean = "Mean";
                        times = result.Where(x => x.verbal_order_to_needle_time != "").Select(x => x.verbal_order_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _meanlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _meantime = new TimeSpan();
                            if (_meanlist.Count > 0)
                            {
                                double mean = _meanlist.Average();
                                _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                            }

                            catgory.Add(monthName);
                            list.Add(_meantime.ToString());
                        }
                    }
                    else if (status == "DTN Less or Equal 30 Minutes %")
                    {
                        graph.Mean = "DTN %";
                        times = result.Select(x => x.arrival_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 30)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "DTN Less or Equal 45 Minutes %")
                    {
                        graph.Mean = "DTN %";
                        times = result.Select(x => x.arrival_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 45)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "DTN Less or Equal 60 Minutes %")
                    {
                        graph.Mean = "DTN %";
                        times = result.Select(x => x.arrival_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 60)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "TS Notification to Needle Less or Equal 30 Minutes %")
                    {
                        graph.Mean = "Start-to-Needle %";
                        times = result.Select(x => x.start_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 30)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "TS Notification to Needle Less or Equal 45 Minutes %")
                    {
                        graph.Mean = "Start-to-Needle %";
                        times = result.Select(x => x.start_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 45)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    else if (status == "TS Notification to Needle Less or Equal 60 Minutes %")
                    {
                        graph.Mean = "Start-to-Needle %";
                        times = result.Select(x => x.start_to_needle_time).ToList();
                        if (times.Count > 0)
                        {
                            int counts = 0;
                            int totalcount = 0;
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                    if (time <= 60)
                                    {
                                        counts++;
                                    }
                                    totalcount++;
                                }
                            }
                            int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                            list.Add(_result.ToString());
                            catgory.Add(monthName);
                        }
                    }
                    // Added by Axim 30-09-2020
                    else if (status == "CPOE order to Needle time Ave. Minutes")
                    {
                        graph.Mean = "Mean";
                        times = result.Select(x => x.cpoe_order_to_needle).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _meanlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _meantime = new TimeSpan();
                            if (_meanlist.Count > 0)
                            {
                                double mean = _meanlist.Average();
                                _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                            }
                            catgory.Add(monthName);
                            list.Add(_meantime.ToString());
                        }
                    }
                    else if (status == "CPOE order to Needle time Median Minutes")
                    {
                        graph.Mean = "Median";
                        times = result.Select(x => x.cpoe_order_to_needle).ToList();
                        if (times.Count > 0)
                        {
                            List<double> _medianlist = new List<double>();
                            foreach (var item in times)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _medianlist.Add(time);
                                    count++;
                                }
                            }
                            TimeSpan _mediantime = new TimeSpan();
                            if (_medianlist.Count > 0)
                            {
                                int numbercount = _medianlist.Count();
                                int halfindex = _medianlist.Count() / 2;
                                var sortednumbers = _medianlist.OrderBy(x => x);
                                double median;
                                if ((numbercount % 2) == 0)
                                {
                                    median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                                }
                                else
                                {
                                    median = sortednumbers.ElementAt(halfindex);
                                }
                                _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                            }
                            list.Add(_mediantime.ToString());
                            catgory.Add(monthName);
                        }
                    }

                    StartDate = enddate.AddDays(1);
                }
                if (catgory.Count > 0)
                {
                    graph.Title = status;
                    DateTime date = ModelStartDate;
                    DateTime mindate = date;
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = list;
                }
                //}

                List<QualityMetricsGraphReport> lists = new List<QualityMetricsGraphReport>();
                lists.Add(graph);
                var finalresult = lists.Select(x => new
                {
                    Title = x.Title,
                    Mean = x.Mean,
                    MinDate = x.MinDate,
                    Category = x.Category,
                    MeanCalculation = x.MeanCalculation
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetFacilityDashboardAllGraph(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases
                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1
                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                var EDStay = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    case_number = x.ca.cas_case_number,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",

                    arrival_to_start = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    emspatienttype = x.ca.cas_patient_type == emspatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    pvpatienttype = x.ca.cas_patient_type == pvpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "" : "",
                    //inpatienttype = x.ca.cas_metric_symptom_onset_during_ed_stay == true ? x.ca.cas_patient_type == inpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_symptom_onset_during_ed_stay_time) : "" : "",
                    start_to_response = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_response_first_atempt < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification) : "" : "",
                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    tpatrue = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_tpa_consult : false,
                    arrival_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time) : "" : "" : "",
                    verbal_order_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time) : "",
                    start_to_needle_time = x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_symptom_onset_during_ed_stay == false || x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_response_ts_notification) : "" : "" : "",
                    cpoe_order_to_needle = x.ca.cas_metric_tpa_consult == true ? DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time) : "00:00:00",
                });
                List<QualityMetricsGraphReport> graphlist = new List<QualityMetricsGraphReport>();
                QualityMetricsGraphReport doortostartgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport activationemsgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport activationpvgraph = new QualityMetricsGraphReport();
                //QualityMetricsGraphReport activationinpgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport starttoresponsegraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport bedsidegraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport alteplaseadmingraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport doortoneedlegraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport verbalordertoadmingraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport DTN30percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport DTN45percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport DTN60percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport STN30percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport STN45percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport STN60percentgraph = new QualityMetricsGraphReport();
                QualityMetricsGraphReport CPOEtoNeedlegraph = new QualityMetricsGraphReport();

                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                List<string> doortostartave = new List<string>();
                List<string> doortostartmed = new List<string>();
                List<string> activationems = new List<string>();
                List<string> activationpv = new List<string>();
                //List<string> activationinp = new List<string>();
                List<string> starttoresponseave = new List<string>();
                List<string> starttoresponsemed = new List<string>();
                List<string> bedside = new List<string>();
                List<string> alteplaseadmin = new List<string>();
                List<string> doortoneedleave = new List<string>();
                List<string> doortoneedlemed = new List<string>();
                List<string> verbalordertoadmin = new List<string>();
                List<string> DTN30percent = new List<string>();
                List<string> DTN45percent = new List<string>();
                List<string> DTN60percent = new List<string>();
                List<string> STN30percent = new List<string>();
                List<string> STN45percent = new List<string>();
                List<string> STN60percent = new List<string>();
                List<string> CPOEtoNeedleave = new List<string>();
                List<string> CPOEtoNeedlemed = new List<string>();


                List<string> doortostartCatgory = new List<string>();
                List<string> activationemsCatgory = new List<string>();
                List<string> activationpvCatgory = new List<string>();
                //List<string> activationinpCatgory = new List<string>();
                List<string> starttoresponseCatgory = new List<string>();
                List<string> bedsideCatgory = new List<string>();
                List<string> alteplaseadminCatgory = new List<string>();
                List<string> doortoneedleCatgory = new List<string>();
                List<string> verbalordertoadminCatgory = new List<string>();
                List<string> DTN30percentCatgory = new List<string>();
                List<string> DTN45percentCatgory = new List<string>();
                List<string> DTN60percentCatgory = new List<string>();
                List<string> STN30percentCatgory = new List<string>();
                List<string> STN45percentCatgory = new List<string>();
                List<string> STN60percentCatgory = new List<string>();
                List<string> CPOEtoNeedlaCatgory = new List<string>();
                int count = 0;
                for (var i = StartDate; StartDate <= EndDate;)
                {
                    string monthName = StartDate.ToString("MMMM");

                    DateTime enddate = StartDate.AddMonths(1).AddDays(-1);
                    #region ----- Filters -----
                    var result = query.Where(x => DbFunctions.TruncateTime(x.created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                                        DbFunctions.TruncateTime(x.created_date) <= DbFunctions.TruncateTime(enddate))
                                                                   .Select(x => new
                                                                   {
                                                                       x.arrival_to_start,
                                                                       x.emspatienttype,
                                                                       x.pvpatienttype,
                                                                           //x.inpatienttype,
                                                                           x.start_to_response,
                                                                       x.bedside_response_time,
                                                                       x.tpatrue,
                                                                       x.arrival_to_needle_time,
                                                                       x.verbal_order_to_needle_time,
                                                                       x.start_to_needle_time,
                                                                       x.cpoe_order_to_needle
                                                                   }).ToList();
                    #endregion
                    List<string> doortostarttime = new List<string>();
                    List<string> activationemstime = new List<string>();
                    List<string> activationpvtime = new List<string>();
                    List<string> activationinptime = new List<string>();
                    List<string> starttoresponsetime = new List<string>();
                    List<string> bedsidetime = new List<string>();
                    List<bool> alteplaseadmintime = new List<bool>();
                    List<string> doortoneedletime = new List<string>();
                    List<string> verbalordertoadmintime = new List<string>();
                    List<string> DTN30percenttime = new List<string>();
                    List<string> DTN45percenttime = new List<string>();
                    List<string> DTN60percenttime = new List<string>();
                    List<string> STN30percenttime = new List<string>();
                    List<string> STN45percenttime = new List<string>();
                    List<string> STN60percenttime = new List<string>();
                    List<string> CPOEtoNeedletime = new List<string>();
                    doortostartgraph.Mean = "Mean";
                    doortostartgraph.Median = "Median";
                    doortostarttime = result.Select(x => x.arrival_to_start).ToList();
                    if (doortostarttime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in doortostarttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                _meanlist.Add(time);
                                _medianlist.Add(time);
                                count++;
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }

                        TimeSpan _mediantime = new TimeSpan();
                        if (_medianlist.Count > 0)
                        {
                            int numbercount = _medianlist.Count();
                            int halfindex = _medianlist.Count() / 2;
                            var sortednumbers = _medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        doortostartave.Add(_meantime.ToString());
                        doortostartmed.Add(_mediantime.ToString());
                        doortostartCatgory.Add(monthName);

                    }
                    activationemsgraph.Mean = "EMS %";
                    activationemstime = result.Select(x => x.emspatienttype).ToList();
                    if (activationemstime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in activationemstime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 10)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        activationems.Add(_result.ToString());

                    }
                    activationpvgraph.Mean = "Triage %";
                    activationpvtime = result.Select(x => x.pvpatienttype).ToList();
                    if (activationpvtime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in activationpvtime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 10)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        activationpv.Add(_result.ToString());

                    }
                    //activationinpgraph.Mean = "Inpatient %";
                    //activationinptime = result.Select(x => x.inpatienttype).ToList();
                    //if (activationinptime.Count > 0)
                    //{
                    //    int counts = 0;
                    //    int totalcount = 0;
                    //    foreach (var item in activationinptime)
                    //    {
                    //        if (item != "")
                    //        {
                    //            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                    //            if (time <= 10)
                    //            {
                    //                counts++;
                    //            }
                    //            totalcount++;
                    //        }
                    //    }
                    //    int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                    //    //activationinp.Add(_result.ToString());

                    //}
                    if (activationemstime.Count > 0 || activationpvtime.Count > 0 || activationinptime.Count > 0)
                    {
                        activationemsCatgory.Add(monthName);
                        activationpvCatgory.Add(monthName);
                        //activationinpCatgory.Add(monthName);
                    }
                    starttoresponsegraph.Mean = "Mean";
                    starttoresponsegraph.Median = "Median";
                    starttoresponsetime = result.Select(x => x.start_to_response).ToList();
                    if (starttoresponsetime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in starttoresponsetime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                _meanlist.Add(time);
                                _medianlist.Add(time);
                                count++;
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }

                        TimeSpan _mediantime = new TimeSpan();
                        if (_medianlist.Count > 0)
                        {
                            int numbercount = _medianlist.Count();
                            int halfindex = _medianlist.Count() / 2;
                            var sortednumbers = _medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        starttoresponsemed.Add(_mediantime.ToString());
                        starttoresponseave.Add(_meantime.ToString());
                        starttoresponseCatgory.Add(monthName);
                    }
                    bedsidegraph.Mean = "Bedside %";
                    bedsidetime = result.Select(x => x.bedside_response_time).ToList();
                    if (bedsidetime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in bedsidetime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time < 10)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        bedside.Add(_result.ToString());
                        bedsideCatgory.Add(monthName);
                    }
                    alteplaseadmingraph.Mean = "TPA";
                    alteplaseadmintime = result.Select(x => x.tpatrue).ToList();
                    if (alteplaseadmintime.Count > 0)
                    {
                        int tpas = 0;
                        foreach (var item in alteplaseadmintime)
                        {
                            if (item == true)
                            {
                                tpas++;
                            }
                        }
                        alteplaseadmin.Add(tpas.ToString());
                        alteplaseadminCatgory.Add(monthName);
                    }
                    doortoneedlegraph.Mean = "Mean";
                    doortoneedlegraph.Median = "Median";
                    doortoneedletime = result.Where(x => x.arrival_to_needle_time != "").Select(x => x.arrival_to_needle_time).ToList();
                    if (doortoneedletime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in doortoneedletime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                _meanlist.Add(time);
                                _medianlist.Add(time);
                                count++;
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }

                        TimeSpan _mediantime = new TimeSpan();
                        if (_medianlist.Count > 0)
                        {
                            int numbercount = _medianlist.Count();
                            int halfindex = _medianlist.Count() / 2;
                            var sortednumbers = _medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        doortoneedlemed.Add(_mediantime.ToString());
                        doortoneedleave.Add(_meantime.ToString());
                        doortoneedleCatgory.Add(monthName);
                    }

                    verbalordertoadmingraph.Mean = "Mean";
                    verbalordertoadmintime = result.Where(x => x.verbal_order_to_needle_time != "").Select(x => x.verbal_order_to_needle_time).ToList();
                    if (verbalordertoadmintime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        foreach (var item in verbalordertoadmintime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                _meanlist.Add(time);
                                count++;
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }

                        verbalordertoadminCatgory.Add(monthName);
                        verbalordertoadmin.Add(_meantime.ToString());
                    }
                    DTN30percentgraph.Mean = "DTN %";
                    DTN30percenttime = result.Select(x => x.arrival_to_needle_time).ToList();
                    if (DTN30percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in DTN30percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 30)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        DTN30percent.Add(_result.ToString());

                    }

                    DTN45percentgraph.Mean = "DTN %";
                    DTN45percenttime = result.Select(x => x.arrival_to_needle_time).ToList();
                    if (DTN45percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in DTN45percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 45)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        DTN45percent.Add(_result.ToString());

                    }

                    DTN60percentgraph.Mean = "DTN %";
                    DTN60percenttime = result.Select(x => x.arrival_to_needle_time).ToList();
                    if (DTN60percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in DTN60percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 60)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        DTN60percent.Add(_result.ToString());

                    }
                    if (DTN30percenttime.Count > 0 || DTN45percenttime.Count > 0 || DTN60percenttime.Count > 0)
                    {
                        DTN30percentCatgory.Add(monthName);
                        DTN45percentCatgory.Add(monthName);
                        DTN60percentCatgory.Add(monthName);
                    }
                    STN30percentgraph.Mean = "Start-to-Needle %";
                    STN30percenttime = result.Select(x => x.start_to_needle_time).ToList();
                    if (STN30percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in STN30percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 30)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        STN30percent.Add(_result.ToString());
                    }

                    STN45percentgraph.Mean = "Start-to-Needle %";
                    STN45percenttime = result.Select(x => x.start_to_needle_time).ToList();
                    if (STN45percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in STN45percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 45)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        STN45percent.Add(_result.ToString());

                    }

                    STN60percentgraph.Mean = "Start-to-Needle %";
                    STN60percenttime = result.Select(x => x.start_to_needle_time).ToList();
                    if (STN60percenttime.Count > 0)
                    {
                        int counts = 0;
                        int totalcount = 0;
                        foreach (var item in STN60percenttime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalMinutes;
                                if (time <= 60)
                                {
                                    counts++;
                                }
                                totalcount++;
                            }
                        }
                        int _result = counts != 0 && totalcount != 0 ? (int)Math.Round((double)counts / totalcount * 100) : 0;
                        STN60percent.Add(_result.ToString());

                    }
                    if (STN30percenttime.Count > 0 || STN45percenttime.Count > 0 || STN60percenttime.Count > 0)
                    {
                        STN30percentCatgory.Add(monthName);
                        STN45percentCatgory.Add(monthName);
                        STN60percentCatgory.Add(monthName);
                    }

                    CPOEtoNeedlegraph.Mean = "Mean";
                    CPOEtoNeedlegraph.Median = "Median";
                    CPOEtoNeedletime = result.Select(x => x.cpoe_order_to_needle).ToList();
                    if (CPOEtoNeedletime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in CPOEtoNeedletime)
                        {
                            if (item != "")
                            {
                                var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                _meanlist.Add(time);
                                _medianlist.Add(time);
                                count++;
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        if (_meanlist.Count > 0)
                        {
                            double mean = _meanlist.Average();
                            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                        }

                        TimeSpan _mediantime = new TimeSpan();
                        if (_medianlist.Count > 0)
                        {
                            int numbercount = _medianlist.Count();
                            int halfindex = _medianlist.Count() / 2;
                            var sortednumbers = _medianlist.OrderBy(x => x);
                            double median;
                            if ((numbercount % 2) == 0)
                            {
                                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                            }
                            else
                            {
                                median = sortednumbers.ElementAt(halfindex);
                            }
                            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                        }
                        CPOEtoNeedleave.Add(_meantime.ToString());
                        CPOEtoNeedlemed.Add(_mediantime.ToString());
                        CPOEtoNeedlaCatgory.Add(monthName);
                    }


                    StartDate = enddate.AddDays(1);
                }





                if (doortostartCatgory.Count > 0)
                {
                    doortostartgraph.Title = "Door to TS Notification Ave. & Median Minutes";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    doortostartgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    doortostartgraph.Category = doortostartCatgory;
                    doortostartgraph.MeanCalculation = doortostartave;
                    doortostartgraph.MedianCalculation = doortostartmed;
                }
                if (activationemsCatgory.Count > 0)
                {
                    activationemsgraph.Title = "% 10 Min or Less Activation (EMS)";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    activationemsgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    activationemsgraph.Category = activationemsCatgory;
                    activationemsgraph.MeanCalculation = activationems;
                }
                if (activationpvCatgory.Count > 0)
                {
                    activationpvgraph.Title = "% 10 Min or Less Activation (PV)";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    activationpvgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    activationpvgraph.Category = activationpvCatgory;
                    activationpvgraph.MeanCalculation = activationpv;
                }
                //if (activationinpCatgory.Count > 0)
                //{
                //    activationinpgraph.Title = "% 10 Min or Less Activation (Inpt)";
                //    DateTime date = StartDate.AddYears(-1);
                //    DateTime mindate = date;
                //    activationinpgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                //    activationinpgraph.Category = activationinpCatgory;
                //    activationinpgraph.MeanCalculation = activationinp;
                //}
                if (starttoresponseCatgory.Count > 0)
                {
                    starttoresponsegraph.Title = "TS Notification to Response Average & Median Minute";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    starttoresponsegraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    starttoresponsegraph.Category = starttoresponseCatgory;
                    starttoresponsegraph.MeanCalculation = starttoresponseave;
                    starttoresponsegraph.MedianCalculation = starttoresponsemed;
                }
                if (bedsideCatgory.Count > 0)
                {
                    bedsidegraph.Title = "% TS at Bedside <10 Minutes";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    bedsidegraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    bedsidegraph.Category = bedsideCatgory;
                    bedsidegraph.MeanCalculation = bedside;
                }
                if (alteplaseadminCatgory.Count > 0)
                {
                    alteplaseadmingraph.Title = "ALTEPLASE ADMINISTERED #";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    alteplaseadmingraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    alteplaseadmingraph.Category = alteplaseadminCatgory;
                    alteplaseadmingraph.MeanCalculation = alteplaseadmin;
                }
                if (doortoneedleCatgory.Count > 0)
                {
                    doortoneedlegraph.Title = "Door to Needle Average & Median (min:sec)";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    doortoneedlegraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    doortoneedlegraph.Category = doortoneedleCatgory;
                    doortoneedlegraph.MeanCalculation = doortoneedleave;
                    doortoneedlegraph.MedianCalculation = doortoneedlemed;
                }
                if (verbalordertoadminCatgory.Count > 0)
                {
                    verbalordertoadmingraph.Title = "Alteplase early mix decision to Administration Average Minutes";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    verbalordertoadmingraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    verbalordertoadmingraph.Category = verbalordertoadminCatgory;
                    verbalordertoadmingraph.MeanCalculation = verbalordertoadmin;
                }
                if (DTN30percentCatgory.Count > 0)
                {
                    DTN30percentgraph.Title = "DTN Less or Equal 30 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    DTN30percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    DTN30percentgraph.Category = DTN30percentCatgory;
                    DTN30percentgraph.MeanCalculation = DTN30percent;
                }
                if (DTN45percentCatgory.Count > 0)
                {
                    DTN45percentgraph.Title = "DTN Less or Equal 45 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    DTN45percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    DTN45percentgraph.Category = DTN45percentCatgory;
                    DTN45percentgraph.MeanCalculation = DTN45percent;
                }
                if (DTN60percentCatgory.Count > 0)
                {
                    DTN60percentgraph.Title = "DTN Less or Equal 60 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    DTN60percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    DTN60percentgraph.Category = DTN60percentCatgory;
                    DTN60percentgraph.MeanCalculation = DTN60percent;
                }
                if (STN30percentCatgory.Count > 0)
                {
                    STN30percentgraph.Title = "TS Notification to Needle Less or Equal 30 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    STN30percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    STN30percentgraph.Category = STN30percentCatgory;
                    STN30percentgraph.MeanCalculation = STN30percent;
                }
                if (STN45percentCatgory.Count > 0)
                {
                    STN45percentgraph.Title = "TS Notification to Needle Less or Equal 45 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    STN45percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    STN45percentgraph.Category = STN45percentCatgory;
                    STN45percentgraph.MeanCalculation = STN45percent;
                }
                if (STN60percentCatgory.Count > 0)
                {
                    STN60percentgraph.Title = "TS Notification to Needle Less or Equal 60 Minutes %";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    STN60percentgraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    STN60percentgraph.Category = STN60percentCatgory;
                    STN60percentgraph.MeanCalculation = STN60percent;
                }

                if (CPOEtoNeedlaCatgory.Count > 0)
                {
                    CPOEtoNeedlegraph.Title = "CPOE order to Needle Ave. & Median Minutes";
                    DateTime date = StartDate.AddYears(-1);
                    DateTime mindate = date;
                    CPOEtoNeedlegraph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    CPOEtoNeedlegraph.Category = CPOEtoNeedlaCatgory;
                    CPOEtoNeedlegraph.MeanCalculation = CPOEtoNeedleave;
                    CPOEtoNeedlegraph.MedianCalculation = CPOEtoNeedlemed;
                }

                //}

                List<QualityMetricsGraphReport> lists = new List<QualityMetricsGraphReport>();
                lists.Add(doortostartgraph);
                lists.Add(activationemsgraph);
                lists.Add(activationpvgraph);
                //lists.Add(activationinpgraph);
                lists.Add(starttoresponsegraph);
                lists.Add(bedsidegraph);
                lists.Add(alteplaseadmingraph);
                lists.Add(doortoneedlegraph);
                lists.Add(verbalordertoadmingraph);
                lists.Add(DTN30percentgraph);
                lists.Add(DTN45percentgraph);
                lists.Add(DTN60percentgraph);
                lists.Add(STN30percentgraph);
                lists.Add(STN45percentgraph);
                lists.Add(STN60percentgraph);
                lists.Add(CPOEtoNeedlegraph);
                var finalresult = lists.Select(x => new
                {
                    Title = x.Title,
                    Mean = x.Mean,
                    Median = x.Median,
                    MinDate = x.MinDate,
                    MeanCalculation = x.MeanCalculation,
                    MedianCalculation = x.MedianCalculation,
                    Category = x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        #region Commented Code
        //public DataSourceResult GetFacilityDashboardPieChart(QualityGoalsViewModel model, string facilityAdminId, DataSourceRequest request)
        //{
        //    using (var context = new Model.TeleSpecialistsContext())
        //    {
        //        context.Configuration.AutoDetectChangesEnabled = false;
        //        context.Configuration.ProxyCreationEnabled = false;
        //        context.Configuration.LazyLoadingEnabled = false;
        //        string facilityTimeZone = BLL.Settings.DefaultTimeZone;
        //        var cases = from ca in context.cases

        //                    where ca.cas_is_active == true && ca.cas_cst_key == 20

        //                    select (new
        //                    {
        //                        ca
        //                    });
        //        if (!string.IsNullOrEmpty(facilityAdminId))
        //        {
        //            var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
        //                                                     .Select(m => m.Facility).ToList();

        //            cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
        //        }
        //        if (model.Facilities != null && model.Facilities.Count > 0)
        //        {
        //            if (model.Facilities[0] != Guid.Empty)
        //                cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
        //        }
        //        DateTime StartDate = Convert.ToDateTime(model.fromMonth);
        //        DateTime EndDate = Convert.ToDateTime(model.toMonth);
        //        EndDate = EndDate.AddMonths(1).AddDays(-1);
        //        cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
        //                                     DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
        //        var inpatientType = PatientType.Inpatient.ToInt();
        //        var emspatientType = PatientType.EMS.ToInt();
        //        var pvpatientType = PatientType.Triage.ToInt();
        //        var query = cases.Select(x => new
        //        {
        //            id = x.ca.cas_key,
        //            facility_key = x.ca.cas_fac_key,
        //            created_date = x.ca.cas_created_date,
        //            case_number = x.ca.cas_case_number,
        //            facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",

        //            arrival_to_start = x.ca.cas_patient_type != inpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
        //            emspatienttype = x.ca.cas_patient_type == emspatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
        //            pvpatienttype = x.ca.cas_patient_type == pvpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
        //            inpatienttype = x.ca.cas_metric_symptom_onset_during_ed_stay == true ? x.ca.cas_patient_type == inpatientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_symptom_onset_during_ed_stay_time) : "" : "",
        //            start_to_response = x.ca.cas_response_ts_notification < x.ca.cas_response_first_atempt ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_response_first_atempt),
        //            bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
        //            tpatrue = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_tpa_consult : false,
        //            arrival_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != inpatientType ? x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time) : "" : "",
        //            verbal_order_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time) : "",
        //            start_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_response_ts_notification) : "",
        //        });



        //        #region ----- Filters -----
        //        var result = query.Select(x => new
        //        {
        //            x.arrival_to_start,
        //            x.emspatienttype,
        //            x.pvpatienttype,
        //            x.inpatienttype,
        //            x.start_to_response,
        //            x.bedside_response_time,
        //            x.tpatrue,
        //            x.arrival_to_needle_time,
        //            x.verbal_order_to_needle_time,
        //            x.start_to_needle_time
        //        }).ToList();

        //        #endregion
        //        List<QualityGoalscls> list = new List<QualityGoalscls>();
        //        QualityGoalscls quality = new QualityGoalscls();
        //        List<double> _meanlist = new List<double>();
        //        List<double> _medianlist = new List<double>();
        //        List<double> Start_to_Response_meanlist = new List<double>();
        //        List<double> Start_to_Response_medianlist = new List<double>();
        //        List<double> Bedside_meanlist = new List<double>();
        //        List<double> Bedside_medianlist = new List<double>();
        //        List<double> Door_to_Needle_meanlist = new List<double>();
        //        List<double> Door_to_Needle_medianlist = new List<double>();
        //        List<double> Verbal_to_Administration_meanlist = new List<double>();
        //        int casecount = 0;
        //        int emscount = 0;
        //        int totalemscount = 0;
        //        int pvcount = 0;
        //        int totalpvcount = 0;
        //        int inpcount = 0;
        //        int totalinpcount = 0;
        //        int BedsideCount = 0;
        //        int BedsideTotal = 0;
        //        int tpatrue = 0;
        //        int DTN30 = 0;
        //        int DTN45 = 0;
        //        int DTN60 = 0;
        //        int DTNTotal = 0;
        //        int STN30 = 0;
        //        int STN45 = 0;
        //        int STN60 = 0;
        //        int STNTotal = 0;
        //        if (result.Count > 0)
        //        {
        //            foreach (var item in result)
        //            {
        //                if (item.arrival_to_start != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.arrival_to_start.Split(':')[0]), int.Parse(item.arrival_to_start.Split(':')[1]), int.Parse(item.arrival_to_start.Split(':')[2])).TotalSeconds;
        //                    _meanlist.Add(time);
        //                    _medianlist.Add(time);
        //                    casecount++;
        //                }
        //                if (item.emspatienttype != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.emspatienttype.Split(':')[0]), int.Parse(item.emspatienttype.Split(':')[1]), int.Parse(item.emspatienttype.Split(':')[2])).TotalMinutes;
        //                    if (time <= 10)
        //                    {
        //                        emscount++;
        //                    }
        //                    totalemscount++;
        //                }
        //                if (item.pvpatienttype != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.pvpatienttype.Split(':')[0]), int.Parse(item.pvpatienttype.Split(':')[1]), int.Parse(item.pvpatienttype.Split(':')[2])).TotalMinutes;
        //                    if (time <= 10)
        //                    {
        //                        pvcount++;
        //                    }
        //                    totalpvcount++;
        //                }
        //                if (item.inpatienttype != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.inpatienttype.Split(':')[0]), int.Parse(item.inpatienttype.Split(':')[1]), int.Parse(item.inpatienttype.Split(':')[2])).TotalMinutes;
        //                    if (time <= 10)
        //                    {
        //                        inpcount++;
        //                    }
        //                    totalinpcount++;
        //                }
        //                if (item.start_to_response != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.start_to_response.Split(':')[0]), int.Parse(item.start_to_response.Split(':')[1]), int.Parse(item.start_to_response.Split(':')[2])).TotalSeconds;
        //                    Start_to_Response_meanlist.Add(time);
        //                    Start_to_Response_medianlist.Add(time);
        //                }
        //                if (item.bedside_response_time != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalSeconds;
        //                    var MiNtime = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalMinutes;
        //                    Bedside_meanlist.Add(time);
        //                    Bedside_medianlist.Add(time);
        //                    if (MiNtime < 10)
        //                    {
        //                        BedsideCount++;
        //                    }
        //                    BedsideTotal++;
        //                }
        //                if (item.tpatrue == true)
        //                {
        //                    tpatrue++;
        //                }
        //                if (item.arrival_to_needle_time != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalSeconds;
        //                    var MINtime = new TimeSpan(int.Parse(item.arrival_to_needle_time.Split(':')[0]), int.Parse(item.arrival_to_needle_time.Split(':')[1]), int.Parse(item.arrival_to_needle_time.Split(':')[2])).TotalMinutes;
        //                    Door_to_Needle_meanlist.Add(time);
        //                    Door_to_Needle_medianlist.Add(time);
        //                    if (MINtime <= 30)
        //                    {
        //                        DTN30++;
        //                    }
        //                    if (MINtime <= 45)
        //                    {
        //                        DTN45++;
        //                    }
        //                    if (MINtime <= 60)
        //                    {
        //                        DTN60++;
        //                    }
        //                    DTNTotal++;
        //                }
        //                if (item.verbal_order_to_needle_time != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.verbal_order_to_needle_time.Split(':')[0]), int.Parse(item.verbal_order_to_needle_time.Split(':')[1]), int.Parse(item.verbal_order_to_needle_time.Split(':')[2])).TotalSeconds;
        //                    Verbal_to_Administration_meanlist.Add(time);
        //                }
        //                if (item.start_to_needle_time != "")
        //                {
        //                    var time = new TimeSpan(int.Parse(item.start_to_needle_time.Split(':')[0]), int.Parse(item.start_to_needle_time.Split(':')[1]), int.Parse(item.start_to_needle_time.Split(':')[2])).TotalMinutes;
        //                    if (time <= 30)
        //                    {
        //                        STN30++;
        //                    }
        //                    if (time <= 45)
        //                    {
        //                        STN45++;
        //                    }
        //                    if (time <= 60)
        //                    {
        //                        STN60++;
        //                    }
        //                    STNTotal++;
        //                }

        //            }
        //        }
        //        TimeSpan _meantime = new TimeSpan();
        //        TimeSpan _mediantime = new TimeSpan();
        //        TimeSpan Start_to_Response_meantime = new TimeSpan();
        //        TimeSpan Start_to_Response_mediantime = new TimeSpan();
        //        TimeSpan Bedside_meantime = new TimeSpan();
        //        TimeSpan Bedside_mediantime = new TimeSpan();
        //        TimeSpan Door_to_Needle_meantime = new TimeSpan();
        //        TimeSpan Door_to_Needle_mediantime = new TimeSpan();
        //        TimeSpan Verbal_to_Administration_meantime = new TimeSpan();
        //        if (_meanlist.Count > 0)
        //        {
        //            double mean = _meanlist.Average();
        //            _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
        //        }
        //        if (_medianlist.Count > 0)
        //        {
        //            int numbercount = _medianlist.Count();
        //            int halfindex = _medianlist.Count() / 2;
        //            var sortednumbers = _medianlist.OrderBy(x => x);
        //            double median;
        //            if ((numbercount % 2) == 0)
        //            {
        //                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
        //            }
        //            else
        //            {
        //                median = sortednumbers.ElementAt(halfindex);
        //            }
        //            _mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
        //        }
        //        if (Start_to_Response_meanlist.Count > 0)
        //        {
        //            double mean = Start_to_Response_meanlist.Average();
        //            Start_to_Response_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
        //        }
        //        if (Start_to_Response_medianlist.Count > 0)
        //        {
        //            int numbercount = Start_to_Response_medianlist.Count();
        //            int halfindex = Start_to_Response_medianlist.Count() / 2;
        //            var sortednumbers = Start_to_Response_medianlist.OrderBy(x => x);
        //            double median;
        //            if ((numbercount % 2) == 0)
        //            {
        //                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
        //            }
        //            else
        //            {
        //                median = sortednumbers.ElementAt(halfindex);
        //            }
        //            Start_to_Response_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
        //        }
        //        if (Bedside_meanlist.Count > 0)
        //        {
        //            double mean = Bedside_meanlist.Average();
        //            Bedside_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
        //        }
        //        if (Bedside_medianlist.Count > 0)
        //        {
        //            int numbercount = Bedside_medianlist.Count();
        //            int halfindex = Bedside_medianlist.Count() / 2;
        //            var sortednumbers = Bedside_medianlist.OrderBy(x => x);
        //            double median;
        //            if ((numbercount % 2) == 0)
        //            {
        //                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
        //            }
        //            else
        //            {
        //                median = sortednumbers.ElementAt(halfindex);
        //            }
        //            Bedside_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
        //        }
        //        if (Door_to_Needle_meanlist.Count > 0)
        //        {
        //            double mean = Door_to_Needle_meanlist.Average();
        //            Door_to_Needle_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
        //        }
        //        if (Door_to_Needle_medianlist.Count > 0)
        //        {
        //            int numbercount = Door_to_Needle_medianlist.Count();
        //            int halfindex = Door_to_Needle_medianlist.Count() / 2;
        //            var sortednumbers = Door_to_Needle_medianlist.OrderBy(x => x);
        //            double median;
        //            if ((numbercount % 2) == 0)
        //            {
        //                median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
        //            }
        //            else
        //            {
        //                median = sortednumbers.ElementAt(halfindex);
        //            }
        //            Door_to_Needle_mediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
        //        }
        //        if (Verbal_to_Administration_meanlist.Count > 0)
        //        {
        //            double mean = Verbal_to_Administration_meanlist.Average();
        //            Verbal_to_Administration_meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
        //        }
        //        int emsresult = emscount != 0 && totalemscount != 0 ? (int)Math.Round((double)emscount / totalemscount * 100) : 0;
        //        int pvresult = pvcount != 0 && totalpvcount != 0 ? (int)Math.Round((double)pvcount / totalpvcount * 100) : 0;
        //        int inpresult = inpcount != 0 && totalinpcount != 0 ? (int)Math.Round((double)inpcount / totalinpcount * 100) : 0;
        //        int DTN30_Result = DTN30 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN30 / DTNTotal * 100) : 0;
        //        int DTN45_Result = DTN45 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN45 / DTNTotal * 100) : 0;
        //        int DTN60_Result = DTN60 != 0 && DTNTotal != 0 ? (int)Math.Round((double)DTN60 / DTNTotal * 100) : 0;
        //        int STN30_Result = STN30 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN30 / STNTotal * 100) : 0;
        //        int STN45_Result = STN45 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN45 / STNTotal * 100) : 0;
        //        int STN60_Result = STN60 != 0 && STNTotal != 0 ? (int)Math.Round((double)STN60 / STNTotal * 100) : 0;
        //        int BedSideResult = BedsideCount != 0 && BedsideTotal != 0 ? (int)Math.Round((double)BedsideCount / BedsideTotal * 100) : 0;
        //        string ctime = "00:10:00";
        //        TimeSpan checktime = TimeSpan.Parse(ctime);

        //        quality.qag_time_frame = StartDate.ToString("MMMM yyyy") + " - " + EndDate.ToString("MMMM yyyy");
        //        quality.qag_door_to_TS_notification_ave_minutes = _meantime.Minutes + ":" + _meantime.Seconds;
        //        quality.qag_door_to_TS_notification_median_minutes = _mediantime.Minutes + ":" + _mediantime.Seconds;
        //        quality.qag_percent10_min_or_less_activation_EMS = emsresult + "%".ToString();
        //        quality.qag_percent10_min_or_less_activation_PV = pvresult + "%".ToString();
        //        quality.qag_percent10_min_or_less_activation_Inpt = inpresult + "%".ToString();
        //        quality.qag_TS_notification_to_response_average_minute = Start_to_Response_meantime.Minutes + ":" + Start_to_Response_meantime.Seconds;
        //        quality.qag_TS_notification_to_response_median_minute = Start_to_Response_mediantime.Minutes + ":" + Start_to_Response_mediantime.Seconds;
        //        quality.qag_percent_TS_at_bedside_grterthan10_minutes = BedSideResult.ToString();
        //        quality.qag_alteplase_administered = tpatrue.ToString();
        //        quality.qag_door_to_needle_average = Door_to_Needle_meantime.Minutes + ":" + Door_to_Needle_meantime.Seconds;
        //        quality.qag_door_to_needle_median = Door_to_Needle_mediantime.Minutes + ":" + Door_to_Needle_mediantime.Seconds;
        //        quality.qag_verbal_order_to_administration_average_minutes = Verbal_to_Administration_meantime.Minutes + ":" + Verbal_to_Administration_meantime.Seconds;
        //        quality.qag_DTN_grter_or_equal_30minutes_percent = DTN30_Result.ToString();
        //        quality.qag_DTN_grter_or_equal_45minutes_percent = DTN45_Result.ToString();
        //        quality.qag_DTN_grter_or_equal_60minutes_percent = DTN60_Result.ToString();
        //        quality.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = STN30_Result.ToString();
        //        quality.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = STN45_Result.ToString();
        //        quality.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = STN60_Result.ToString();
        //        list.Add(quality);

        //        var finalresult = list.Select(x => new
        //        {
        //            date = x.qag_time_frame,
        //            doortostartave = x.qag_door_to_TS_notification_ave_minutes,
        //            doortostartmed = x.qag_door_to_TS_notification_median_minutes,
        //            emsperc = x.qag_percent10_min_or_less_activation_EMS,
        //            pvperc = x.qag_percent10_min_or_less_activation_PV,
        //            inpperc = x.qag_percent10_min_or_less_activation_Inpt,
        //            starttoresponseave = x.qag_TS_notification_to_response_average_minute,
        //            starttoresponsemed = x.qag_TS_notification_to_response_median_minute,
        //            bedside = x.qag_percent_TS_at_bedside_grterthan10_minutes,
        //            tpaadmin = x.qag_alteplase_administered,
        //            doortoneedleave = x.qag_door_to_needle_average,
        //            doortoneedlemed = x.qag_door_to_needle_median,
        //            verbalorderave = x.qag_verbal_order_to_administration_average_minutes,
        //            dtn30perc = x.qag_DTN_grter_or_equal_30minutes_percent,
        //            dtn45perc = x.qag_DTN_grter_or_equal_45minutes_percent,
        //            dtn60perc = x.qag_DTN_grter_or_equal_60minutes_percent,
        //            stn30perc = x.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent,
        //            stn45perc = x.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent,
        //            stn60perc = x.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent
        //        }).AsQueryable();


        //        return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        //    }
        //}
        #endregion
        public DataSourceResult GetTPAAnalysis(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                List<int> workflowlist = new List<int>();
                workflowlist.Add(1);
                workflowlist.Add(3);
                cases = cases.Where(m => workflowlist.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                List<bool> tPA = new List<bool>();
                tPA.Add(true);
                cases = cases.Where(c => tPA.Contains(c.ca.cas_metric_tpa_consult));
                //cases = cases.Where(c => c.ca.cas_metric_symptom_onset_during_ed_stay == false);
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();

                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    case_key = x.ca.cas_key,
                    case_number = x.ca.cas_case_number,
                    created_date = x.ca.cas_created_date,
                    process = x.ca.cas_patient_type,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    doortoneedle = x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                    tPADelayNotes = x.ca.cas_billing_tpa_delay_notes,
                    qpsanalysis = x.ca.cas_response_case_qps_assessment,
                    medicaldirectoranalysis = x.ca.cas_response_case_research
                });
                var querys = query.OrderBy(x => x.case_number).ToList();
                List<tPACaseAnalysis> list = new List<tPACaseAnalysis>();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                if (querys.Count > 0)
                {
                    foreach (var item in querys)
                    {
                        tPACaseAnalysis tpa = new tPACaseAnalysis();
                        if (item.doortoneedle != "")
                        {
                            var time = new TimeSpan(int.Parse(item.doortoneedle.Split(':')[0]), int.Parse(item.doortoneedle.Split(':')[1]), int.Parse(item.doortoneedle.Split(':')[2])).TotalMinutes;
                            time = Math.Round(time);
                            tpa.DTN = time.ToString();
                            tpa.Case_Key = item.case_key;
                            tpa.Facility = item.facility;
                            tpa.CaseNumber = Convert.ToInt32(item.case_number);
                            tpa.Date = item.created_date.ToString("MM-dd-yyyy");
                            if (item.process == inpatientType)
                            {
                                tpa.Process = "Inpatient";
                            }
                            else if (item.process == emspatientType)
                            {
                                tpa.Process = "EMS";
                            }
                            else if (item.process == pvpatientType)
                            {
                                tpa.Process = "Triage/Walk-In";
                            }
                            tpa.tPAdelaynotes = item.tPADelayNotes;
                            tpa.QPSanalysis = item.qpsanalysis;
                            tpa.MedicalDirectorAnalysis = item.medicaldirectoranalysis;
                            list.Add(tpa);
                        }

                    }
                }
                //}

                var _result = list.Select(x => new
                {
                    name = x.Facility,
                    case_number = x.CaseNumber,
                    cas_key = x.Case_Key,
                    created_date = x.Date,
                    process = x.Process,
                    doortoneedle = x.DTN,
                    tpanotes = x.tPAdelaynotes,
                    qpsanalysis = x.QPSanalysis,
                    medicaldirectoranalysis = x.MedicalDirectorAnalysis
                }).AsQueryable();

                return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetRootCauseTrends(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));

                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    case_number = x.ca.cas_case_number,
                    created_date = x.ca.cas_created_date,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    rootcause = x.ca.cas_work_flow_ids
                }).ToList();
                var rootcause = query.Where(x => x.rootcause != null).Select(x => new { x.rootcause, x.case_number}).ToList();
                var PrimaryRootCause = Enum.GetValues(typeof(PrimaryRootCause)).Cast<PrimaryRootCause>()
                          .Select(m => new
                          {
                              Key = Convert.ToInt32(m).ToString(),
                              Value = m.ToDescription()
                          }).ToDictionary(m => m.Key, m => m.Value);
                var SecondaryRootCause = Enum.GetValues(typeof(SecondaryRootCause)).Cast<SecondaryRootCause>()
                          .Select(m => new
                          {
                              Key = Convert.ToInt32(m).ToString(),
                              Value = m.ToDescription()
                          }).ToDictionary(m => m.Key, m => m.Value);
                var TertiaryRootCause = Enum.GetValues(typeof(TertiaryRootCause)).Cast<TertiaryRootCause>()
                          .Select(m => new
                          {
                              Key = Convert.ToInt32(m).ToString(),
                              Value = m.ToDescription()
                          }).ToDictionary(m => m.Key, m => m.Value);
                List<tPACaseAnalysis> list = new List<tPACaseAnalysis>();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                List<tPACaseAnalysis> primarylist = new List<tPACaseAnalysis>();
                List<tPACaseAnalysis> secondarylist = new List<tPACaseAnalysis>();
                List<tPACaseAnalysis> tertiarylist = new List<tPACaseAnalysis>();
                if (rootcause.Count > 0)
                {
                    foreach (var item in rootcause)
                    {
                        if (item.rootcause != null)
                        {
                            var workflowids = item.rootcause;
                            var ids = workflowids.Split(',').Distinct().ToList();
                            foreach (var id in ids)
                            {
                                tPACaseAnalysis tpa = new tPACaseAnalysis();
                                var isPrimary = PrimaryRootCause.Where(x => x.Key == id).FirstOrDefault();
                                var isSecondary = SecondaryRootCause.Where(x => x.Key == id).FirstOrDefault();
                                var isTertiary = TertiaryRootCause.Where(x => x.Key == id).FirstOrDefault();
                                if (!string.IsNullOrEmpty(isPrimary.Key))
                                {
                                    if (primarylist.Count > 0)
                                    {
                                        var isExist = primarylist.Where(x => x.RootId == Convert.ToInt32(isPrimary.Key)).FirstOrDefault();
                                        if (isExist != null)
                                        {
                                            isExist.RootCauseCount++;
                                        }
                                        else
                                        {
                                            tpa.RootId = Convert.ToInt32(isPrimary.Key);
                                            tpa.RootCause = isPrimary.Value;
                                            tpa.RootCauseCount = 1;
                                            primarylist.Add(tpa);
                                        }
                                    }
                                    else
                                    {
                                        tpa.RootCauseGrp = "Primary RootCause";
                                        tpa.RootId = Convert.ToInt32(isPrimary.Key);
                                        tpa.RootCause = isPrimary.Value;
                                        tpa.RootCauseCount = 1;
                                        primarylist.Add(tpa);
                                    }

                                }
                                else if (!string.IsNullOrEmpty(isSecondary.Key))
                                {
                                    if (secondarylist.Count > 0)
                                    {
                                        var isExist = secondarylist.Where(x => x.RootId == Convert.ToInt32(isSecondary.Key)).FirstOrDefault();
                                        if (isExist != null)
                                        {
                                            isExist.RootCauseCount++;
                                        }
                                        else
                                        {
                                            tpa.RootId = Convert.ToInt32(isSecondary.Key);
                                            tpa.RootCause = isSecondary.Value;
                                            tpa.RootCauseCount = 1;
                                            secondarylist.Add(tpa);
                                        }
                                    }
                                    else
                                    {
                                        tpa.RootCauseGrp = "Secondary RootCause";
                                        tpa.RootId = Convert.ToInt32(isSecondary.Key);
                                        tpa.RootCause = isSecondary.Value;
                                        tpa.RootCauseCount = 1;
                                        secondarylist.Add(tpa);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(isTertiary.Key))
                                {
                                    if (tertiarylist.Count > 0)
                                    {
                                        var isExist = tertiarylist.Where(x => x.RootId == Convert.ToInt32(isTertiary.Key)).FirstOrDefault();
                                        if (isExist != null)
                                        {
                                            isExist.RootCauseCount++;
                                        }
                                        else
                                        {
                                            tpa.RootId = Convert.ToInt32(isTertiary.Key);
                                            tpa.RootCause = isTertiary.Value;
                                            tpa.RootCauseCount = 1;
                                            tertiarylist.Add(tpa);
                                        }
                                    }
                                    else
                                    {
                                        tpa.RootCauseGrp = "Tertiary RootCause";
                                        tpa.RootId = Convert.ToInt32(isTertiary.Key);
                                        tpa.RootCause = isTertiary.Value;
                                        tpa.RootCauseCount = 1;
                                        tertiarylist.Add(tpa);
                                    }
                                }

                            }
                        }
                    }
                    if (primarylist.Count > 0)
                    {
                        //tPACaseAnalysis tpa = new tPACaseAnalysis();

                        //list.Add(tpa);
                        list.AddRange(primarylist);
                    }
                    if (secondarylist.Count > 0)
                    {
                        //tPACaseAnalysis tpa = new tPACaseAnalysis();

                        //list.Add(tpa);
                        list.AddRange(secondarylist);
                    }
                    if (tertiarylist.Count > 0)
                    {
                        //tPACaseAnalysis tpa = new tPACaseAnalysis();

                        //list.Add(tpa);
                        list.AddRange(tertiarylist);
                    }
                }
                //}

                var _result = list.Select(x => new
                {
                    rootgrp = x.RootCauseGrp == null ? "" : x.RootCauseGrp,
                    rootcause = x.RootCause,
                    rootcausecount = x.RootCauseCount
                }).AsQueryable();

                return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetCaseReviewTrends(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));

                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    case_number = x.ca.cas_case_number,
                    created_date = x.ca.cas_created_date,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",

                    tpaverbaltime = x.ca.cas_metric_tpa_verbal_order_time ?? null,
                    needletime = x.ca.cas_metric_needle_time ?? null,
                    arrival = x.ca.cas_metric_door_time ?? null,
                    starttime_ems = x.ca.cas_patient_type != emspatientType ? null : x.ca.cas_response_ts_notification ?? null,
                    starttime_triage = x.ca.cas_patient_type != pvpatientType ? null : x.ca.cas_response_ts_notification ?? null,
                    logintime = x.ca.cas_response_first_atempt ?? null,
                    nihsstime = x.ca.cas_metric_assesment_time ?? null,
                });
                List<CaseReviewTrends> list = new List<CaseReviewTrends>();
                var data = query.ToList();
                if (data.Count > 0)
                {
                    CaseReviewTrends tpatrends = new CaseReviewTrends();
                    CaseReviewTrends arrivaltostartemstrends = new CaseReviewTrends();
                    CaseReviewTrends arrialtostarttriagetrends = new CaseReviewTrends();
                    CaseReviewTrends logintonihsstrends = new CaseReviewTrends();
                    int tpatrendscount = 0;
                    int arrivaltostartemstrendscount = 0;
                    int arrialtostarttriagetrendscount = 0;
                    int logintonihsstrendscount = 0;
                    foreach (var item in data)
                    {

                        bool tpatoneedle = item.needletime > item.tpaverbaltime ? System.Math.Abs((item.tpaverbaltime.Value.Subtract(item.needletime.Value)).TotalMinutes) > 15 : false;
                        bool arrivaltostartems = item.starttime_ems > item.arrival ? System.Math.Abs((item.arrival.Value.Subtract(item.starttime_ems.Value)).TotalMinutes) > 10 : false;
                        bool arrivaltostarttriage = item.starttime_triage > item.arrival ? System.Math.Abs((item.arrival.Value.Subtract(item.starttime_triage.Value)).TotalMinutes) > 10 : false;
                        bool logintonihss = item.nihsstime > item.logintime ? System.Math.Abs((item.logintime.Value.Subtract(item.nihsstime.Value)).TotalMinutes) > 10 : false;
                        if (tpatoneedle)
                        {
                            tpatrendscount += 1;
                        }
                        if (arrivaltostartems)
                        {
                            arrivaltostartemstrendscount += 1;
                        }
                        if (arrivaltostarttriage)
                        {
                            arrialtostarttriagetrendscount += 1;
                        }
                        if (logintonihss)
                        {
                            logintonihsstrendscount += 1;
                        }

                    }
                    tpatrends.Title = "Alteplase Early Mix Decision to Needle Time";
                    tpatrends.Total = tpatrendscount;
                    arrivaltostartemstrends.Title = "EMS TS Activation Time";
                    arrivaltostartemstrends.Total = arrivaltostartemstrendscount;
                    arrialtostarttriagetrends.Title = "Triage/Walk in TS Activation Time";
                    arrialtostarttriagetrends.Total = arrialtostarttriagetrendscount;
                    logintonihsstrends.Title = "Time First Login Attempt to NIHSS Start Assessment Time";
                    logintonihsstrends.Total = logintonihsstrendscount;

                    list.Add(tpatrends);
                    list.Add(arrivaltostartemstrends);
                    list.Add(arrialtostarttriagetrends);
                    list.Add(logintonihsstrends);
                }

                var _result = list.Select(x => new
                {
                    title = x.Title,
                    total = x.Total
                }).AsQueryable();

                return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetCounterMeasure(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 9 && ca.cas_billing_bic_key == 1

                            select (new
                            {
                                ca
                            });
                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(EndDate));

                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    case_number = x.ca.cas_case_number,
                    created_date = x.ca.cas_created_date,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                });
                var casenumber = query.Select(x => new { x.case_number, x.id }).ToList();
                List<tPACaseAnalysis> list = new List<tPACaseAnalysis>();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                if (casenumber.Count > 0)
                {
                    foreach (var item in casenumber)
                    {
                        tPACaseAnalysis casenum = new tPACaseAnalysis();
                        if (item.case_number != null)
                        {
                            casenum.CaseNumber = Convert.ToInt32(item.case_number);
                            casenum.RootId = item.id;
                            list.Add(casenum);
                        }
                    }
                }
                //}

                var _result = list.Select(x => new
                {
                    cas_key = x.RootId,
                    casenumber = x.CaseNumber
                }).AsQueryable();

                return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetVolumeMetrics(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20

                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                ca.cas_patient_type,
                                ca.cas_created_date,
                                datetime = ca.cas_response_ts_notification
                            });
                #region ----- Filters -----

                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                DateTime querysdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                DateTime enddate = EndDate.ToUniversalTimeZone(facilityTimeZone);
                for (var i = StartDate; StartDate <= enddate;)
                {
                    VolumeMetricsReport report = new VolumeMetricsReport();
                    DateTime enddateofmonth = StartDate.AddMonths(1);
                    DateTime sdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                    DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                    var result = cases.Where(x => x.datetime >= sdate && x.datetime < edate);

                    report.TimeCycle = StartDate.ToString("MMMM yyyy");
                    report.StrokeAlert = result.Where(x => x.cas_billing_bic_key == 1).Count();
                    report.STAT = result.Where(x => x.cas_billing_bic_key == 2).Count();
                    report.New = result.Where(x => x.cas_billing_bic_key == 3).Count();
                    report.FollowUp = result.Where(x => x.cas_billing_bic_key == 4).Count();
                    report.EEG = result.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                    volumelist.Add(report);
                    StartDate = StartDate.AddMonths(1);
                }
                // }

                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    strokealert = x.StrokeAlert,
                    stat = x.STAT,
                    New = x.New,
                    followup = x.FollowUp,
                    eeg = x.EEG,
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetVolumeGraph(QualityGoalsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases
                            where ca.cas_is_active == true && ca.cas_cst_key == 20
                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                ca.cas_patient_type,
                                ca.cas_created_date,
                                datetime = ca.cas_response_ts_notification
                            });
                #region ----- Filters -----

                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                DateTime querysdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                var result = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();

                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                int casescount = 0;
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                VolumeMetricsReport report = new VolumeMetricsReport();

                report.TimeCycle = StartDate.ToString("MM-dd-yyyy") + " - " + EndDate.ToString("MM-dd-yyyy");


                report.StrokeAlert = result.Where(x => x.cas_billing_bic_key == 1).Count();
                report.STAT = result.Where(x => x.cas_billing_bic_key == 2).Count();
                report.New = result.Where(x => x.cas_billing_bic_key == 3).Count();
                report.FollowUp = result.Where(x => x.cas_billing_bic_key == 4).Count();
                report.EEG = result.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                casescount = report.StrokeAlert + report.STAT + report.New + report.FollowUp + report.EEG;
                volumelist.Add(report);

                //}

                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    strokealert = x.StrokeAlert,
                    stat = x.STAT,
                    New = x.New,
                    followup = x.FollowUp,
                    eeg = x.EEG,
                    casecount = casescount,
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetStrokeAlertCases(DataSourceRequest request, QualityGoalsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20

                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                ca.cas_patient_type,
                                ca.cas_created_date,
                                datetime = ca.cas_response_ts_notification
                            });
                #region ----- Filters -----

                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                DateTime querysdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);
                cases = cases.Where(x => x.cas_billing_bic_key == 1);

                #endregion
                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();
                DateTime enddate = EndDate.ToUniversalTimeZone(facilityTimeZone);
                for (var i = StartDate; StartDate <= enddate;)
                {
                    VolumeMetricsReport report = new VolumeMetricsReport();
                    DateTime enddateofmonth = StartDate.AddMonths(1);
                    DateTime sdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                    DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                    var patienttypes = cases.Where(x => x.datetime >= sdate && x.datetime < edate).Select(x => new { x.cas_patient_type, x.cas_created_date }).ToList();

                    int patientscount = patienttypes.Count();

                    int emscount = patienttypes.Where(x => x.cas_patient_type == emspatientType).Count();
                    int emsper = emscount != 0 ? patientscount != 0 ? (int)Math.Round((double)emscount / patientscount * 100) : 0 : 0;

                    int triagecount = patienttypes.Where(x => x.cas_patient_type == pvpatientType).Count();
                    int triageper = triagecount != 0 ? patientscount != 0 ? (int)Math.Round((double)triagecount / patientscount * 100) : 0 : 0;

                    int inpcount = patienttypes.Where(x => x.cas_patient_type == inpatientType).Count();
                    int inpper = inpcount != 0 ? patientscount != 0 ? (int)Math.Round((double)inpcount / patientscount * 100) : 0 : 0;

                    report.EMS = emscount;
                    report.EMSPercent = emsper + "%";
                    report.Triage = triagecount;
                    report.TriagePercent = triageper + "%";
                    report.Inpatient = inpcount;
                    report.InpatientPercent = inpper + "%";
                    report.TimeCycle = StartDate.ToString("MMMM yyyy");
                    volumelist.Add(report);
                    StartDate = StartDate.AddMonths(1);
                }
                // }


                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    x.EMS,
                    x.EMSPercent,
                    x.Triage,
                    x.TriagePercent,
                    x.Inpatient,
                    x.InpatientPercent
                }).AsQueryable();
                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetStrokeAlertPieChart(QualityGoalsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases
                            where ca.cas_is_active == true && ca.cas_cst_key == 20
                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                ca.cas_patient_type,
                                ca.cas_created_date,
                                datetime = ca.cas_response_ts_notification
                            });
                #region ----- Filters -----

                DateTime StartDate = Convert.ToDateTime(model.fromMonth);
                DateTime EndDate = Convert.ToDateTime(model.toMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                DateTime querysdate = StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.System != null && model.System.Count > 0)
                {
                    IQueryable<int?> System = model.System.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityBySystem(null, System).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.Regional != null && model.Regional.Count > 0)
                {
                    IQueryable<int?> Region = model.Regional.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByRegion(null, Region).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.States != null && model.States.Count > 0)
                {
                    IQueryable<int?> States = model.States.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (model.QPS != null && model.QPS.Count > 0)
                {
                    IQueryable<string> QPS = model.QPS.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByQPS(null, QPS).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);
                cases = cases.Where(x => x.cas_billing_bic_key == 1);

                var patienttypes = cases.OrderBy(x => x.cas_patient_type).ToList();
                #endregion
                var inpatientType = PatientType.Inpatient.ToInt();
                var emspatientType = PatientType.EMS.ToInt();
                var pvpatientType = PatientType.Triage.ToInt();

                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                int patienttypecounts = 0;
                //if (model.Facilities != null && model.Facilities.Count > 0)
                //{
                VolumeMetricsReport report = new VolumeMetricsReport();

                report.TimeCycle = StartDate.ToString("MM-dd-yyyy") + " - " + EndDate.ToString("MM-dd-yyyy");

                int patientscount = patienttypes.Count();

                int emscount = patienttypes.Where(x => x.cas_patient_type == emspatientType).Count();
                int emsper = emscount != 0 ? patientscount != 0 ? (int)Math.Round((double)emscount / patientscount * 100) : 0 : 0;

                int triagecount = patienttypes.Where(x => x.cas_patient_type == pvpatientType).Count();
                int triageper = triagecount != 0 ? patientscount != 0 ? (int)Math.Round((double)triagecount / patientscount * 100) : 0 : 0;

                int inpcount = patienttypes.Where(x => x.cas_patient_type == inpatientType).Count();
                int inpper = inpcount != 0 ? patientscount != 0 ? (int)Math.Round((double)inpcount / patientscount * 100) : 0 : 0;

                report.EMS = emscount;
                report.EMSPercent = emsper + "%";
                report.Triage = triagecount;
                report.TriagePercent = triageper + "%";
                report.Inpatient = inpcount;
                report.InpatientPercent = inpper + "%";
                patienttypecounts = emscount + triagecount + inpcount;
                volumelist.Add(report);
                //}

                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    EMS = x.EMS,
                    EMSPercent = x.EMSPercent,
                    Triage = x.Triage,
                    TriagePercent = x.TriagePercent,
                    Inpatient = x.Inpatient,
                    InpatientPercent = x.InpatientPercent,
                    patienttypecounts = patienttypecounts
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
    }
}
