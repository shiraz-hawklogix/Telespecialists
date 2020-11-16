using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class MockCaseService : BaseService
    {
        static object _caseLocker = new object();

        public MockCaseService() : base()
        {

        }

        public void Create(mock_case entity, bool commit = true)
        {
            lock (_caseLocker)
            {
                entity.mcas_case_number = GetCaseNumber();

                _unitOfWork.MockCaseRepository .Insert(entity);
                if (commit)
                {
                    _unitOfWork.Save(); 
                    _unitOfWork.Commit();
                }
            }
        }
 
        private long GetCaseNumber()
        {
            var list = _unitOfWork.SqlQuery<long>("exec usp_mock_case_number_get");
            return list.First();
        }

        public DataSourceResult GetAll(DataSourceRequest request, string physician, List<Guid> facilities = null)
        {
            var list = _unitOfWork.SqlQuery<mCasegridVM>("Exec [usp_mock_case_listing] " + GetAllSqlParams(request, false, physician, facilities).ToString()).AsQueryable();
            int Total = list.FirstOrDefault()?.totalRecords ?? 0;
            if (request.Take == 0)
                Total = list.Count();
            var kendoObj = list.OrderByDescending(x => x.mcas_created_date).ToDataSourceResult(Total, 0, request.Sort, null);
            kendoObj.Total = Total;
            return kendoObj;

        }


        public class mCasegridVM
        {
            public int? mcas_key { get; set; }
            public int? mcas_ctp_key { get; set; }
            public Guid mcas_fac_key { get; set; }
            public string CaseStatus { get; set; }
            public string casetype { get; set; }
            public string facilityname { get; set; }
            public string PhysicianName { get; set; }
            public int? mcas_cst_key { get; set; }
            public string mcas_patient { get; set; }
            public string mcas_patient_initials { get; set; }
            public string mcas_callback { get; set; }
            public string mcas_created_by_name { get; set; }
            public string mcas_date_of_consult { get; set; }
            public string mcas_date_of_completion { get; set; }
            public string mcas_referring_physician { get; set; }
            public string mcas_created_date { get; set; }
            public string mcas_phy_key { get; set; }
            public int? totalRecords { get; set; } // Property to set Total Records
        }

        public StringBuilder GetAllSqlParams(DataSourceRequest request, bool isCaseListingExport, string physician = "", List<Guid> facilities = null)
        {

            StringBuilder Query = new StringBuilder();

            Query.AppendSqlParam("@mCaseStatusEnumId", UclTypes.CaseStatus.ToInt());
            Query.AppendSqlParam(",@mCaseTypeEnumId", UclTypes.MockCaseType.ToInt());

            if (!isCaseListingExport)
            {
                if (request.Take > 0)
                    Query.Append(",@Take=" + request.Take + ",@Skip=" + request.Skip);
                else
                    Query.Append("@Skip=0");
            }
            else
            {
                Query.Append("@DummyParam=1");
            }

            if (!string.IsNullOrEmpty(physician))
            {
                Query.AppendSqlParam(",@Physician", physician);
            }


            var currentDate = DateTime.Now.ToEST();
            if (request.Filter != null)
            {
                if (request.Filter.Filters != null)
                {
                    FillAdvanceParamsSql(ref Query, request, facilities);

                    #region Search Text Filter
                    var search_fiter = request.Filter.Filters.Where(m => m.Field == "search_text").FirstOrDefault();
                    if (search_fiter != null)
                    {
                        string search_value = search_fiter.Value?.ToString();
                        if (!string.IsNullOrEmpty(search_value))
                        {
                            Query.AppendSqlParam(",@SearchText", search_value);
                        }
                    }
                    #endregion

                }

                #region Case Status Filter
                var case_status_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_status");
                if (case_status_field != null)
                {
                    var case_status = case_status_field.Value?.ToString();
                    if (!string.IsNullOrEmpty(case_status))
                    {
                        Query.AppendSqlParam(",@CaseStatus", case_status);
                    }
                }
                #endregion

                #region Case Type Filter
                // cas_ctp_key_selected
                Filter cas_ctp_key_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_ctp_key_selected");
                if (cas_ctp_key_field == null)
                    cas_ctp_key_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "multi_cas_ctp_key");

                if (cas_ctp_key_field != null)
                {
                    var case_type_ids = cas_ctp_key_field.Value?.ToString();
                    if (!string.IsNullOrEmpty(case_type_ids))
                    {
                        Query.AppendSqlParam(",@CaseType", case_type_ids);
                        var comparingoperator = cas_ctp_key_field.Operator;
                        if (!string.IsNullOrEmpty(comparingoperator))
                        {
                            Query.AppendSqlParam(",@CaseTypeOpr", comparingoperator);
                        }
                        else
                        {
                            Query.AppendSqlParam(",@CaseTypeOpr", "eq");
                        }
                    }
                }


                #endregion

                #region @UserInitialFilter
                //userInitialFilter
                var userInitialFilter = request.Filter.Filters.FirstOrDefault(m => m.Field.Contains("cas_history_physician_initial_cal"));
                if (!string.IsNullOrEmpty(userInitialFilter?.Value?.ToString()))
                {
                    Query.AppendSqlParam(",@UserInitialFilter", userInitialFilter.Value.ToString());
                }
                #endregion

            }
            return Query;
        }

        private void FillAdvanceParamsSql(ref StringBuilder Query, DataSourceRequest request, List<Guid> facilities = null)
        {
            var currentDate = DateTime.Now;
            if (request.Filter != null)
            {
                if (request.Filter.Filters != null)
                {
                    #region Advance Search Filters             
                    var filters = request.Filter.Filters.Where(m => m.Field.Contains("advance_") && m.Value != null);

                    var patientName = filters.Where(m => m.Field.Contains("cas_patient")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(patientName?.Value?.ToString()))
                    {
                        Query.AppendSqlParam(",@PatientName", patientName.Value.ToString());
                    }
                    if (facilities != null)
                    {
                        Query.AppendSqlParam(",@FacilityIds", string.Join(",", facilities));
                        // Query.Append("@FacilityId='" + facilities.First() + "'");
                        // caseQuery = caseQuery.Where(m => facilities.Contains(m.CaseModel.cas_fac_key));
                    }

                    var cas_created_dated = filters.Where(m => m.Field.Contains("cas_created_date")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(cas_created_dated?.Value?.ToString()))
                    {
                        Query.AppendSqlParam(",@StartDate", cas_created_dated.Value.ToString());
                        Query.AppendSqlParam(",@DateFilter", "Today");
                    }
                    else
                    {
                        #region Date Filter

                        var date_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "date_filter");
                        if (date_filter_field != null)
                        {
                            DateTime? StartDate = null;
                            DateTime? EndDate = null;
                            var date_filter = date_filter_field.Value?.ToString();
                            if (!string.IsNullOrEmpty(date_filter))
                            {
                                switch (date_filter)
                                {
                                    case "Today":
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(currentDate));
                                        StartDate = currentDate.Date;
                                        break;

                                    case "Yesterday":
                                        StartDate = currentDate.AddDays(-1).Date;
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(StartDate));
                                        break;
                                    case "Last24Hours":
                                        StartDate = currentDate.AddDays(-1);
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate));
                                        break;
                                    case "Last48Hours":
                                        StartDate = currentDate.AddDays(-2);
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate));
                                        break;

                                    case "LastSevenDays":
                                        StartDate = currentDate.AddDays(-7);
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(currentDate));
                                        break;

                                    case "Last30Days":
                                        StartDate = currentDate.AddDays(-30);
                                        //predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(currentDate));
                                        break;

                                    case "PreviousWeek":
                                        StartDate = currentDate.AddDays(-14);
                                        EndDate = currentDate.AddDays(-7);
                                        // predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                        break;

                                    case "PreviousMonth":
                                        StartDate = currentDate.AddMonths(-2);
                                        EndDate = currentDate.AddMonths(-1);
                                        // predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                        break;

                                    case "MonthToDate":
                                        StartDate = new DateTime(currentDate.Year, currentDate.Month, 01);
                                        EndDate = currentDate;
                                        // predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                        break;
                                    case "DateRange":
                                        #region Date Range
                                        var start_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "start_date");
                                        if (start_date_field != null)
                                        {
                                            var start_date = start_date_field.Value?.ToString();
                                            StartDate = start_date.ToDateTime();
                                            // applyFilter = true;
                                        }

                                        var end_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "end_date");
                                        if (end_date_field != null)
                                        {
                                            var end_date = end_date_field.Value?.ToString();
                                            EndDate = end_date.ToDateTime();
                                            // applyFilter = true;
                                        }
                                        // predicate = predicate.And(m => (StartDate == null || DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate)) && (EndDate == null || DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate)));
                                        #endregion
                                        break;

                                    case "SpecificDate":
                                        #region Specific Date
                                        var specific_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "specific_date");
                                        if (specific_date_field != null)
                                        {
                                            var specific_date = specific_date_field.Value?.ToString();
                                            if (!string.IsNullOrEmpty(specific_date))
                                            {
                                                StartDate = specific_date.ToDateTime();
                                                //       predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(StartDate));
                                            }
                                        }
                                        #endregion
                                        break;
                                }

                                if (StartDate.HasValue)
                                    Query.AppendSqlParam(",@StartDate", StartDate);
                                if (EndDate.HasValue)
                                    Query.AppendSqlParam(",@EndDate", EndDate);
                                if ((StartDate.HasValue || EndDate.HasValue))
                                {
                                    Query.AppendSqlParam(",@DateFilter", date_filter);
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
            }
        }

        public mock_case GetDetails(int id)
        {
            var model = _unitOfWork.MockCaseRepository.Query().AsNoTracking()
                                   .Include(m => m.facility)
                                   .Include(m => m.AspNetUser2)
                                   .FirstOrDefault(m => m.mcas_key == id);
            if (model != null)
            {
                // Convert timestamps as per facility timezone
                string facilityTimeZone = Settings.DefaultTimeZone;
                if (model.facility != null)
                {
                    #region Shiraz Code
                    var EMR_Value = model.facility.fac_cst_key;
                    var get_name = _unitOfWork.UCL_UCDRepository.Query().Where(f => f.ucd_key == EMR_Value).Select(x => x.ucd_title).FirstOrDefault();
                    model.facility.fac_cst_key_with_Name = get_name;
                    #endregion

                    if (!string.IsNullOrEmpty(model.facility.fac_timezone))
                        facilityTimeZone = model.facility.fac_timezone;

                    //if (model.mcas_metric_tpa_verbal_order_time.HasValue) model.mcas_metric_tpa_verbal_order_time_est = model.mcas_metric_tpa_verbal_order_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_lastwell_date.HasValue) model.mcas_metric_lastwell_date_est = model.mcas_metric_lastwell_date.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_response_ts_notification.HasValue)
                    //{
                    //    //model.mcas_response_ts_notification_utc = model.mcas_response_ts_notification;
                    //    model.mcas_response_ts_notification = model.mcas_response_ts_notification.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //}
                    //if (model.mcas_metric_door_time.HasValue) model.mcas_metric_door_time_est = model.mcas_metric_door_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_stamp_time.HasValue) model.mcas_metric_stamp_time_est = model.mcas_metric_stamp_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_assesment_time.HasValue) model.mcas_metric_assesment_time_est = model.mcas_metric_assesment_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_pa_ordertime.HasValue) model.mcas_metric_pa_ordertime_est = model.mcas_metric_pa_ordertime.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_needle_time.HasValue) model.mcas_metric_needle_time_est = model.mcas_metric_needle_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_video_start_time.HasValue) model.mcas_metric_video_start_time_est = model.mcas_metric_video_start_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_video_end_time.HasValue) model.mcas_metric_video_end_time_est = model.mcas_metric_video_end_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_documentation_end_time.HasValue) model.mcas_metric_documentation_end_time_est = model.mcas_metric_documentation_end_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    //if (model.mcas_metric_symptom_onset_during_ed_stay_time.HasValue) model.mcas_metric_symptom_onset_during_ed_stay_time_est = model.mcas_metric_symptom_onset_during_ed_stay_time.Value.ToTimezoneFromUtc(facilityTimeZone);

                    if (model.mcas_response_time_physician.HasValue) model.mcas_response_time_physician = model.mcas_response_time_physician.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.mcas_response_first_atempt.HasValue) model.mcas_response_first_atempt = model.mcas_response_first_atempt.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.mcas_phy_technical_issue_date.HasValue) model.mcas_phy_technical_issue_date_est = model.mcas_phy_technical_issue_date.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.mcas_callback_response_time_est.HasValue) model.mcas_callback_response_time_est = model.mcas_callback_response_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                }
            }
            return model;
        }


        public void Edit(mock_case entity, bool commit = true)
        {
           
            _unitOfWork.MockCaseRepository.Update(entity);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }

        }

        public IQueryable<string> GetCart(Guid key)
        {
            return _unitOfWork.FacilityRepository.Query().Where(x => x.fac_key == key).Select(x => x.fac_cart_numbers);
        }
    }
}
