using Kendo.DynamicLinq;
using System.Linq;
using TeleSpecialists.BLL.Model;
using System.Data.Entity;
using System;
using System.Data;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using System.Reflection;
using TeleSpecialists.BLL.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Web;
using System.Data.SqlClient;

namespace TeleSpecialists.BLL.Service
{
    class SignalColClass { public string Value { get; set; } }
    public class CaseGridViewModel
    {
        public @case CaseModel { get; set; }
        public string CaseStatus { get; set; }
        public string CaseType { get; set; }
        public string BillingCode { get; set; }
        public string IsNavigatorBlast { get; set; }
        public string Navigator { get; set; }
    }


    public class CaseSignOutListingViewModel
    {
        public int cas_key { get; set; }
        public string fac_name { get; set; }
        public string cas_patient { get; set; }

        public string date_of_birth { get; set; }
        public string date_of_consult { get; set; }

        public string case_type { get; set; }
        public string billing_code { get; set; }

        public string case_status { get; set; }
        public string sign_off_follow_up { get; set; }

        public string comments { get; set; }
    }

    public class GridViewModel
    {
        public bool cas_is_flagged { get; set; }
        public int cas_key { get; set; }
        public long? cas_case_number { get; set; }
        public int cas_cst_key { get; set; }
        public string cas_metric_stamp_time_est { get; set; }
        public string cas_response_ts_notification { get; set; }
        public string cas_status_assign_date { get; set; }
        public string ctp_name { get; set; }
        public string fac_name { get; set; }
        public string cas_patient { get; set; }
        public string cas_callback { get; set; }
        public string cas_cart { get; set; }
        public string phy_name { get; set; }
        public string cst_name { get; set; }
        public string cas_billing_visit_type { get; set; }
        public string IsNavigatorBlast { get; set; }
        public bool cas_is_ealert { get; set; }
        public string Navigator { get; set; }
        public string BillingCode { get; set; }
        public string ResponseTime { get; set; }
        public string StartToStamp { get; set; }
        public string StartToAccept { get; set; }
        public string callType { get; set; }
        public string callerSource { get; set; }
        public string TPACandidate { get; set; }
        public string date_of_consult { get; set; }
        public string billing_code { get; set; }
        public string sign_off_follow_up { get; set; }
        public string comments { get; set; }
        public string date_of_birth { get; set; }
    }
    public class CaseService : BaseService
    {
        private readonly AdminService _adminService;
        private readonly UCLService _uclService;
        private readonly PhysicianStatusService _physicianStatusService;
        static object _caseLocker = new object();

        public CaseService() : base()
        {
            _adminService = new AdminService();
            _uclService = new UCLService();
            _physicianStatusService = new PhysicianStatusService();
        }





        public void UpdateTimeStamps(string cas_keys)
        {
            _unitOfWork.SqlQuery<int>("Exec usp_case_timestamp_calc_update @cas_key='" + cas_keys + "'");
        }


        public string GetCaseInitials(int cas_key)
        {

            var list = _unitOfWork.SqlQuery<SignalColClass>(string.Format("select dbo.GetPhysiciansInitial({0}) as Value", cas_key));
            return FormatPhysicianInitials(list.First().Value);

        }

        public void UpdateCaseInitials(int cas_key, string value)
        {
            _unitOfWork.ExecuteSqlCommand(string.Format("update [case] set cas_history_physician_initial = '{1}' where cas_key = {0}", cas_key, value));
        }



        public DataSourceResult GetAllForSignOutListing(DataSourceRequest request, string physician = "", List<Guid> facilities = null)
        {
            return GetAllQueerableForSignOutListing(request, physician, facilities).ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }

        public IQueryable<CaseSignOutListingViewModel> GetAllQueerableForSignOutListing(DataSourceRequest request, string physician = "", List<Guid> facilities = null, bool isExporting = false)
        {
            var query = GetForGrid(request, physician, facilities, EnforceFacilities: true).ToList().Select(m => new CaseSignOutListingViewModel
            {
                cas_key = m.cas_key,
                fac_name = m.fac_name,
                cas_patient = m.cas_patient,
                billing_code = m.billing_code,
                date_of_consult = m.date_of_consult,
                sign_off_follow_up = m.sign_off_follow_up,
                comments = isExporting ? m.comments : HttpUtility.HtmlEncode(m.comments?.Replace("`", "\\`")),
                date_of_birth = m.date_of_birth,
                case_status = m.cst_name,
                case_type = m.ctp_name
            });
            return query.AsQueryable();
        }


        private IQueryable<GridViewModel> GetForGrid(DataSourceRequest request, string physician, List<Guid> facilities, List<int> dashboardCaseTypes = null, bool EnforceFacilities = false)
        {

            var queryForAll = GetAllQuerable(request, physician, facilities, EnforceFacilities);

            if (dashboardCaseTypes != null)
            {
                queryForAll = queryForAll.Where(x => dashboardCaseTypes.Contains(x.CaseModel.cas_ctp_key));
            }

            var initialQuery = (from ca in queryForAll
                                from accepted in ca.CaseModel.case_assign_history.Where(x => x.cah_action == "Accepted").OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                                join source in GetUclData(UclTypes.CallerSource) on ca.CaseModel.cas_caller_source_key equals source.ucd_key into CaseCallerSource
                                from callersource in CaseCallerSource.DefaultIfEmpty()
                                select new
                                {
                                    ca.CaseModel,
                                    ca.Navigator,
                                    ca.CaseStatus,
                                    ca.CaseType,
                                    ca.BillingCode,
                                    callersource,
                                    ResponseTime = DBHelper.FormatSeconds(ca.CaseModel.cas_response_ts_notification.Value, ca.CaseModel.cas_response_first_atempt.Value),
                                    StartToStamp = DBHelper.FormatSeconds(ca.CaseModel.cas_metric_stamp_time.Value, ca.CaseModel.cas_response_ts_notification.Value),
                                    StartToAccept = ca.CaseModel.cas_response_ts_notification.HasValue && accepted.cah_created_date_utc != null ? DBHelper.FormatSeconds(ca.CaseModel.cas_response_ts_notification.Value, accepted.cah_created_date_utc) : "00:00:00",

                                    StartToAccept_Cmp = ca.CaseModel.cas_response_ts_notification.HasValue && accepted != null
                                    ? ca.CaseModel.cas_response_ts_notification.Value > accepted.cah_created_date_utc
                                        ? DbFunctions.DiffSeconds(accepted.cah_created_date_utc, ca.CaseModel.cas_response_ts_notification.Value)
                                        : DbFunctions.DiffSeconds(ca.CaseModel.cas_response_ts_notification.Value, accepted.cah_created_date_utc)
                                    : 0,

                                    ResponseTime_Cmp = ca.CaseModel.cas_response_first_atempt < ca.CaseModel.cas_response_ts_notification ? 0 : DBHelper.DiffSeconds(ca.CaseModel.cas_response_first_atempt, ca.CaseModel.cas_response_ts_notification),
                                    StartToStamp_Cmp = DBHelper.DiffSeconds(ca.CaseModel.cas_metric_stamp_time.Value, ca.CaseModel.cas_response_ts_notification.Value),
                                });

            #region Complex Advance Search Filters

            var complexFilters = request.Filter.Filters.Where(m => m.Field.Contains("complex_"));

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            foreach (var filter in complexFilters)
            {

                if (filter.Value != null)
                {
                    if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                    {
                        var fieledName = filter.Field.Replace("complex_", "");
                        var time = TimeSpan.Parse(filter.Value.ToString()).TotalSeconds.ToInt();
                        // passing property info                                
                        initialQuery = initialQuery.WhereCriteria(fieledName, time, filter.Operator);
                    }
                }
            }

            #endregion


            var query = (from ca in initialQuery
                         select new GridViewModel
                         {
                             cas_is_flagged = ca.CaseModel.cas_is_flagged_dashboard,
                             cas_key = ca.CaseModel.cas_key,
                             cas_case_number = ca.CaseModel.cas_case_number,
                             callType = ca.CaseModel.cas_call_type != null && ca.CaseModel.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)ca.CaseModel.cas_call_type).ToString() : "",
                             callerSource = ca.CaseModel.cas_call_type != ((int)CallType.Direct) ? ca.callersource != null ?
                                                                    (ca.callersource.ucd_title.ToLower() == "other" && !string.IsNullOrEmpty(ca.CaseModel.cas_caller_source_text.Trim()) ? ca.CaseModel.cas_caller_source_text : ca.callersource.ucd_title)
                                                                    : "" : "",
                             cas_cst_key = ca.CaseModel.cas_cst_key,
                             cas_metric_stamp_time_est = DBHelper.FormatDateTime(ca.CaseModel.cas_metric_stamp_time_est, true),
                             cas_response_ts_notification = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone("Eastern Standard Time", ca.CaseModel.cas_response_ts_notification), true),
                             cas_status_assign_date = ca.CaseModel.cas_status_assign_date.HasValue ? DBHelper.FormatDateTime(ca.CaseModel.cas_status_assign_date.Value, true) : "",
                             ctp_name = ca.CaseType,
                             fac_name = ca.CaseModel.facility.fac_name,
                             cas_patient = ca.CaseModel.cas_patient,
                             cas_callback = ca.CaseModel.cas_callback,
                             cas_cart = ca.CaseModel.cas_cart,
                             phy_name = ca.CaseModel.cas_history_physician_initial_cal,
                             cst_name = ca.CaseStatus,
                             cas_billing_visit_type = ca.CaseModel.cas_billing_visit_type,
                             IsNavigatorBlast = ca.CaseModel.cas_is_nav_blast ? "Yes" : "No",
                             cas_is_ealert = ca.CaseModel.cas_is_ealert,
                             Navigator = ca.Navigator,
                             BillingCode = ca.BillingCode,
                             TPACandidate = ca.CaseModel.cas_metric_tpa_consult ? "Yes" : "No",
                             //ResponseTime = (DbFunctions.DiffSeconds(ca.CaseModel.cas_response_first_atempt, ca.CaseModel.cas_metric_stamp_time_est))
                             ResponseTime = string.IsNullOrEmpty(ca.ResponseTime) == false ? ca.ResponseTime : "00:00:00",

                             StartToStamp = string.IsNullOrEmpty(ca.StartToStamp) == false ? ca.StartToStamp : "00:00:00",

                             StartToAccept = string.IsNullOrEmpty(ca.StartToAccept) == false ? ca.StartToAccept : "00:00:00",
                             ////
                             billing_code = ca.BillingCode,
                             date_of_consult = DBHelper.FormatDateTime(ca.CaseModel.cas_billing_date_of_consult.Value, false),
                             sign_off_follow_up = ca.CaseModel.cas_billing_visit_type,
                             comments = ca.CaseModel.cas_billing_comments,
                             date_of_birth = DBHelper.FormatDateTime(ca.CaseModel.cas_billing_dob.Value, false)


                         })
                     .OrderByDescending(x => x.cas_key);

            return query;
        }

        public IQueryable<CaseGridViewModel> GetAllQuerable(DataSourceRequest request, string physician = "", List<Guid> facilities = null, bool EnforceFacilities = false)
        {
            DateTime? StartDate = null;
            DateTime? EndDate = null;

            var caseQuery = from ca in _unitOfWork.CaseRepository.Query()
                            join billingCode in GetUclData(UclTypes.BillingCode) on ca.cas_billing_bic_key equals billingCode.ucd_key into CaseBillingCodeEntity
                            join status in GetUclData(UclTypes.CaseStatus) on ca.cas_cst_key equals status.ucd_key into CaseStatusEntity
                            join type in GetUclData(UclTypes.CaseType) on ca.cas_ctp_key equals type.ucd_key into CaseTypeEntity
                            from case_status in CaseStatusEntity.DefaultIfEmpty()
                            from case_type in CaseTypeEntity.DefaultIfEmpty()
                            from case_billingcode in CaseBillingCodeEntity.DefaultIfEmpty()
                            where ca.cas_is_active &&
                            (physician == "" || ca.cas_phy_key == physician)
                            select new CaseGridViewModel
                            {
                                CaseModel = ca,
                                Navigator = ca.cas_created_by_name,
                                CaseStatus = case_status != null ? case_status.ucd_title : "",
                                CaseType = case_type != null ? case_type.ucd_title : "",
                                BillingCode = case_billingcode != null ? case_billingcode.ucd_title : ""
                            };
            if (EnforceFacilities)
            {
                if (facilities != null)
                {
                    caseQuery = caseQuery.Where(m => facilities.Contains(m.CaseModel.cas_fac_key));
                }
            }
            else if (facilities != null && facilities.Count > 0)
            {
                caseQuery = caseQuery.Where(m => facilities.Contains(m.CaseModel.cas_fac_key));
            }

            var currentDate = DateTime.Now.ToEST();
            if (request.Filter != null)
            {
                if (request.Filter.Filters != null)
                {
                    var predicate = PredicateUtils.Null<CaseGridViewModel>();
                    bool applyFilter = false;
                    #region Advance Search Filters             
                    var filters = request.Filter.Filters.Where(m => m.Field.Contains("advance_"));

                    foreach (var filter in filters)
                    {

                        if (filter.Value != null)
                        {
                            if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                            {
                                var fieledName = filter.Field.Replace("advance_", "");
                                // passing property info
                                var propertyInfo = (((PropertyInfo[])caseQuery.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty(fieledName);
                                caseQuery = caseQuery.WhereEquals("CaseModel." + fieledName, filter.Value, propertyInfo);
                            }
                        }
                    }

                    #endregion

                    #region Search Text Filter
                    var search_fiter = request.Filter.Filters.Where(m => m.Field == "search_text").FirstOrDefault();
                    if (search_fiter != null)
                    {
                        string search_value = search_fiter.Value?.ToString();

                        if (!string.IsNullOrEmpty(search_value))
                        {
                            long case_number = search_value.ToLong();

                            predicate = predicate.Or(m => m.CaseModel.cas_case_number == case_number);
                            predicate = predicate.Or(m => m.CaseModel.AspNetUser1.FirstName.ToLower().Contains(search_value));
                            predicate = predicate.Or(m => m.CaseModel.AspNetUser1.LastName.ToLower().Contains(search_value));

                            predicate = predicate.Or(m => m.CaseStatus.ToLower().Contains(search_value));
                            predicate = predicate.Or(m => m.CaseType.ToLower().Contains(search_value));

                            predicate = predicate.Or(m => m.CaseModel.AspNetUser2.LastName.ToLower().Contains(search_value));

                            predicate = predicate.Or(m => m.CaseModel.facility.fac_name.ToLower().Contains(search_value));
                            predicate = predicate.Or(m => m.CaseModel.cas_patient.Contains(search_value));
                            predicate = predicate.Or(m => m.CaseModel.cas_callback.ToLower().Contains(search_value));
                            predicate = predicate.Or(m => m.CaseModel.cas_cart == search_value);


                            applyFilter = true;
                        }
                    }
                    #endregion
                    #region Case Flagged Filter
                    var flag_fiter = request.Filter.Filters.Where(m => m.Field == "cas_is_flagged").FirstOrDefault();
                    if (flag_fiter != null)
                    {
                        var flag_array = flag_fiter.Value.ToString().Split('-');
                        var source = flag_array.Last().ToInt();
                        bool flag_value = flag_array.First().ToBool();
                        if (source == PageSource.Dashboard.ToInt())
                            predicate = predicate.Or(m => m.CaseModel.cas_is_flagged_dashboard.Equals(flag_value));
                        else
                            predicate = predicate.Or(m => m.CaseModel.cas_is_flagged.Equals(flag_value));

                        applyFilter = true;
                    }
                    #endregion

                    #region Case Status Filter
                    var case_status_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_status");
                    if (case_status_field != null)
                    {
                        var case_status = case_status_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(case_status))
                        {
                            var filterIds = case_status.Split(',').Select(m => m.ToInt());
                            predicate = predicate.And(m => filterIds.Contains(m.CaseModel.cas_cst_key));
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region User Initial Filter
                    var cas_history_physician_initial_cal_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_history_physician_initial_cal");
                    if (cas_history_physician_initial_cal_field != null)
                    {
                        var cas__physician_ids = cas_history_physician_initial_cal_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(cas__physician_ids))
                        {
                            var splitedArry = cas__physician_ids.Split(',');
                            var filteredPhysicianIds = splitedArry.Select(m => m.ToString());
                            if (splitedArry.Length > 0)
                            {
                                predicate = predicate.And(m => filteredPhysicianIds.Contains(m.CaseModel.cas_phy_key));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region User Type Filter

                    var cas_ctp_key_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "multi_cas_ctp_key");
                    if (cas_ctp_key_field != null)
                    {
                        var case_type_ids = cas_ctp_key_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(case_type_ids))
                        {
                            var splitedArry = case_type_ids.Split(',');
                            if (splitedArry.Length > 0)
                            {
                                var filteredTypeIds = splitedArry.Select(m => m.ToInt());
                                var comparingoperator = cas_ctp_key_field.Operator;
                                if (comparingoperator == "eq")
                                {
                                    predicate = predicate.And(m => filteredTypeIds.Contains(m.CaseModel.cas_ctp_key));
                                }
                                else if (comparingoperator == "neq")
                                {
                                    predicate = predicate.And(m => !filteredTypeIds.Contains(m.CaseModel.cas_ctp_key));

                                }

                            }
                            applyFilter = true;
                        }
                    }

                    #endregion

                    #region Date Filter

                    var date_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "date_filter");
                    if (date_filter_field != null)
                    {
                        var date_filter = date_filter_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(date_filter))
                        {
                            switch (date_filter)
                            {
                                case "Today":
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(currentDate));
                                    break;

                                case "Yesterday":
                                    StartDate = currentDate.AddDays(-1);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(StartDate));
                                    break;
                                case "Last24Hours":
                                    StartDate = currentDate.AddDays(-1);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate));
                                    break;
                                case "Last48Hours":
                                    StartDate = currentDate.AddDays(-2);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate));
                                    break;

                                case "LastSevenDays":
                                    StartDate = currentDate.AddDays(-7);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(currentDate));
                                    break;

                                case "Last30Days":
                                    StartDate = currentDate.AddDays(-30);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(currentDate));
                                    break;

                                case "PreviousWeek":
                                    StartDate = currentDate.AddDays(-14);
                                    EndDate = currentDate.AddDays(-7);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                    break;

                                case "PreviousMonth":
                                    StartDate = currentDate.AddMonths(-2);
                                    EndDate = currentDate.AddMonths(-1);
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                    break;

                                case "MonthToDate":
                                    StartDate = new DateTime(currentDate.Year, currentDate.Month, 01);
                                    EndDate = currentDate;
                                    predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate));
                                    break;
                                case "DateRange":
                                    #region Date Range
                                    var start_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "start_date");
                                    if (start_date_field != null)
                                    {
                                        var start_date = start_date_field.Value?.ToString();
                                        StartDate = start_date.ToDateTime();
                                        applyFilter = true;
                                    }

                                    var end_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "end_date");
                                    if (end_date_field != null)
                                    {
                                        var end_date = end_date_field.Value?.ToString();
                                        EndDate = end_date.ToDateTime();
                                        applyFilter = true;
                                    }
                                    predicate = predicate.And(m => (StartDate == null || DbFunctions.TruncateTime(m.CaseModel.cas_created_date) >= DbFunctions.TruncateTime(StartDate)) && (EndDate == null || DbFunctions.TruncateTime(m.CaseModel.cas_created_date) <= DbFunctions.TruncateTime(EndDate)));
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
                                            predicate = predicate.And(m => DbFunctions.TruncateTime(m.CaseModel.cas_created_date) == DbFunctions.TruncateTime(StartDate));
                                        }
                                    }
                                    #endregion
                                    break;
                            }
                            applyFilter = true;
                        }
                    }

                    #endregion

                    #region eAlert Filter
                    var eAlert = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_is_ealert");
                    if (eAlert != null)
                    {
                        var eAlertValues = eAlert.Value?.ToString();
                        if (!string.IsNullOrEmpty(eAlertValues))
                        {
                            var splittedeAlerts = eAlertValues.Split(',');
                            var filterEAlerts = splittedeAlerts.Select(m => m.ToString() == "true");
                            if (splittedeAlerts.Length > 0)
                            {
                                predicate = predicate.And(m => filterEAlerts.Contains(m.CaseModel.cas_is_ealert));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region Call Type Filter
                    var cas_callertype_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_callertype_filter");
                    if (cas_callertype_filter_field != null)
                    {
                        var call_type_ids = cas_callertype_filter_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(call_type_ids))
                        {
                            var splitedArry = call_type_ids.Split(',').Select(m => m?.ToInt());
                            if (splitedArry.Count() > 0)
                            {
                                predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_call_type));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region Caller Source Filter
                    var cas_callersource_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_callersource_filter");
                    if (cas_callersource_filter_field != null)
                    {
                        var call_source_ids = cas_callersource_filter_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(call_source_ids))
                        {
                            var splitedArry = call_source_ids.Split(',').Select(m => m?.ToInt());
                            if (splitedArry.Count() > 0)
                            {
                                predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_caller_source_key));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region TPA Filter                   
                    var cas_metric_tpa_consult_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_metric_tpa_consult_filter");
                    if (cas_metric_tpa_consult_filter_field != null)
                    {
                        var filter_ids = cas_metric_tpa_consult_filter_field.Value?.ToString();
                        if (!string.IsNullOrEmpty(filter_ids))
                        {
                            var splitedArry = filter_ids.Split(',').Select(m => m.ToBool());
                            if (splitedArry.Count() > 0)
                            {
                                predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_metric_tpa_consult));
                            }
                            applyFilter = true;
                        }
                    }

                    #endregion

                    #region Sign off/Follow up
                    var cas_billing_visit_type = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_billing_visit_type");
                    if (cas_billing_visit_type != null)
                    {
                        var filter_ids = cas_billing_visit_type.Value?.ToString();
                        if (!string.IsNullOrEmpty(filter_ids))
                        {
                            var splitedArry = filter_ids.Split(',').Select(m => m.ToString());
                            if (splitedArry.Count() > 0)
                            {
                                predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_billing_visit_type));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion

                    #region Billing Code 
                    var cas_billing_bic_key = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_billing_bic_key");
                    if (cas_billing_bic_key != null)
                    {
                        var filter_ids = cas_billing_bic_key.Value?.ToString();
                        if (!string.IsNullOrEmpty(filter_ids))
                        {
                            var splitedArry = filter_ids.Split(',').Select(m => m?.ToInt());
                            if (splitedArry.Count() > 0)
                            {
                                predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_billing_bic_key));
                            }
                            applyFilter = true;
                        }
                    }
                    #endregion 

                    if (applyFilter)
                    {
                        if (predicate != null)
                            caseQuery = caseQuery.Where(predicate);
                    }
                }
            }

            return caseQuery;
        }

        public string FormatPhysicianInitials(string input)
        {
            string result = "";
            if (!string.IsNullOrEmpty(input))
            {
                var list = input.Split('/');
                StringBuilder builder = new StringBuilder();
                string previous = "";
                foreach (var item in list)
                {
                    if (previous != item)
                    {
                        builder.Append(item).Append("/");
                    }
                    previous = item;
                }
                result = builder.ToString();

                result = result.Remove(result.Length - 1, 1);
            }
            return result;

        }

        public IQueryable<@case> GetPhysiciansLastCases(List<string> physician)
        {
            return _unitOfWork.CaseRepository.Query()
                 .Where(x => physician.Contains(x.cas_phy_key))
                 .GroupBy(x => x.cas_phy_key)
                 .Select(x => x.OrderByDescending(y => y.cas_physician_assign_date).FirstOrDefault());
        }

        public IQueryable<@case> GetPhysicianLastCase(string physician)
        {
            return _unitOfWork.CaseRepository.Query()
                              .Where(x => x.cas_phy_key == physician)
                             .OrderByDescending(y => y.cas_physician_assign_date);

        }

        public void SaveMultipleCases(List<@case> caseList)
        {
            try
            {
                var batchId = Guid.NewGuid().ToString();
                _unitOfWork.BeginTransaction();
                List<int> CaseKeyList = new List<int>();
                foreach (var caseModel in caseList)
                {
                    //caseModel.cas_case_number = GetCaseNumber();
                    caseModel.cas_batch_id = batchId;

                    Create(caseModel, false);

                    #region handling logging in case of physician updated
                    if (caseModel.cas_cst_key > 0)
                    {

                        _unitOfWork.CaseAssignHistoryRepository.Insert(new case_assign_history
                        {
                            cah_cas_key = caseModel.cas_key,
                            cah_phy_key = caseModel.cas_phy_key,
                            cah_action = _uclService.GetDetails(caseModel.cas_cst_key)?.ucd_title,
                            cah_created_date = DateTime.Now.ToEST(),
                            cah_created_date_utc = DateTime.UtcNow,
                            cah_created_by = caseModel.cas_created_by,
                            cah_is_active = true,
                            cah_request_sent_time = DateTime.Now.ToEST(),
                            cah_request_sent_time_utc = DateTime.UtcNow,
                            cah_action_time = DateTime.Now.ToEST(),
                            cah_action_time_utc = DateTime.UtcNow,
                            cah_is_manuall_assign = true
                        });
                    }
                    #endregion

                    _unitOfWork.Save();
                    var initials = GetCaseInitials(caseModel.cas_key);
                    UpdateCaseInitials(caseModel.cas_key, initials);
                    CaseKeyList.Add(caseModel.cas_key);
                }

                _unitOfWork.Commit();
                string casKeys = string.Join(",", CaseKeyList);
                UpdateTimeStamps(casKeys);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }
        #region Shiraz Code For Link   
        public PhysicianCaseListing GetCTACaseslist(string physician, application_setting appSetting)
        {
            //var dateLimit = (appSetting.aps_clear_pending_cases_date != null) ? appSetting.aps_clear_pending_cases_date : Convert.ToDateTime("10/10/2019");
            var pendingPhyClearanceDate = (appSetting.aps_clear_physician_pending_cases_date != null) ? appSetting.aps_clear_physician_pending_cases_date : Convert.ToDateTime("10/10/2019");
            var pendingPhyCTAClearanceDate = (appSetting.aps_clear_physician_cta_cases_date != null) ? appSetting.aps_clear_physician_cta_cases_date : Convert.ToDateTime("10/10/2019");

            var result = new PhysicianCaseListing();
            var query = _unitOfWork.CaseRepository.Query();

            var ctaList = query.Where(x => (x.cas_metric_in_cta_queue == true)
                                    && x.cas_phy_key == physician)
                                 .Where(x => DbFunctions.TruncateTime(x.cas_physician_assign_date) >= DbFunctions.TruncateTime(pendingPhyCTAClearanceDate))
                            .Select(x => new PhysicianCTACase
                            {
                                CaseKey = x.cas_key,
                                FacilityName = x.facility.fac_name,
                                CaseStartTime = x.cas_created_date,
                            })
                            .OrderByDescending(x => x.CaseKey);

            var queuList = query.Where(x => x.cas_phy_key == physician
                                         && (x.cas_cst_key == (int)CaseStatus.Accepted
                                            || x.cas_cst_key == (int)CaseStatus.WaitingToAccept
                                            || x.cas_cst_key == (int)CaseStatus.Open))
                               .Where(x => DbFunctions.TruncateTime(x.cas_physician_assign_date) >= DbFunctions.TruncateTime(pendingPhyClearanceDate))
                           .Select(x => new PhysicianQueueCase
                           {
                               CaseKey = x.cas_key,
                               FacilityName = x.facility.fac_name,
                               CaseStartTime = x.cas_created_date,
                               CaseStatus = x.cas_cst_key,
                               PatientName = x.cas_patient
                           })
                           .OrderByDescending(x => x.CaseKey);

            var phyFacList = GetPhsicianNonBoardedFacilitiesOnBoard(physician)
                          .Select(m => new PhysicianFacility
                          {
                              FacilityId = m.fac_key,
                              FacilityName = m.fac_name,
                              fap_key = m.facility_physician.Where(x => x.fap_fac_key == m.fac_key && x.fap_user_key == physician).Select(o => o.fap_key).FirstOrDefault(),
                              fap_Credentials_confirmed_date = m.facility_physician.Where(x => x.fap_fac_key == m.fac_key && x.fap_user_key == physician).Select(z => z.fap_Credentials_confirmed_date).FirstOrDefault()

                          }).OrderBy(x => x.FacilityName);

            result.CTACases = ctaList.ToList();
            result.QueueCases = queuList.ToList();
            result.FacilityList = phyFacList.ToList();
            return result;
        }

        private IQueryable<facility> GetPhsicianNonBoardedFacilitiesOnBoard(string phy_key)
        {
            var currentDate = DateTime.Now.ToEST();
            var maxDate = DateTime.MaxValue;
            var defaultStatus = _physicianStatusService.GetDefault();
            var physicianLicenseStates = (_unitOfWork.PhysicianLicenseRepository.Query()
                                          .Where(m => m.phl_is_active)
                                          .Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                          .Where(m => m.phl_user_key == phy_key)
                                          //.Where(m => m.phl_license_state == null || facility.fac_stt_key == null || m.phl_license_state == facility.fac_stt_key)
                                          .Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
                                          ).Select(m => m.phl_license_state).ToList();

            var facilities = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                             join contact in _unitOfWork.ContactRepository.Query() on m.fap_fac_key equals contact.cnt_fac_key into facilityContactJoin
                             from facilityContact in facilityContactJoin.DefaultIfEmpty()
                             where
                             m.fap_user_key == phy_key
                             && m.fap_is_on_boarded == false
                              && m.fap_start_date != null
                              && (m.facility.fac_go_live || m.facility.fac_is_active)
                              && m.fap_end_date != null
                              && m.fap_is_active == true
                             && m.fap_hide == false
                             select m.facility;

            return facilities.Distinct();
        }

        #endregion




        public @case GetDetailsWithoutTimeConversion(int id)
        {
            var model = _unitOfWork.CaseRepository.Query().AsNoTracking()
                                   .Include(m => m.facility)
                                   .Include(m => m.AspNetUser2)
                                   .FirstOrDefault(m => m.cas_key == id);
            return model;
        }



        public @case GetDetails(int id)
        {
            var model = _unitOfWork.CaseRepository.Query().AsNoTracking()
                                   .Include(m => m.facility)
                                   .Include(m => m.AspNetUser2)
                                   .FirstOrDefault(m => m.cas_key == id);
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

                    if (model.cas_metric_tpa_verbal_order_time.HasValue) model.cas_metric_tpa_verbal_order_time_est = model.cas_metric_tpa_verbal_order_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_lastwell_date.HasValue) model.cas_metric_lastwell_date_est = model.cas_metric_lastwell_date.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_response_ts_notification.HasValue)
                    {
                        model.cas_response_ts_notification_utc = model.cas_response_ts_notification;
                        model.cas_response_ts_notification = model.cas_response_ts_notification.Value.ToTimezoneFromUtc(facilityTimeZone);
                    }
                    if (model.cas_metric_door_time.HasValue) model.cas_metric_door_time_est = model.cas_metric_door_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_stamp_time.HasValue) model.cas_metric_stamp_time_est = model.cas_metric_stamp_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_assesment_time.HasValue) model.cas_metric_assesment_time_est = model.cas_metric_assesment_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_pa_ordertime.HasValue) model.cas_metric_pa_ordertime_est = model.cas_metric_pa_ordertime.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_needle_time.HasValue) model.cas_metric_needle_time_est = model.cas_metric_needle_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_video_start_time.HasValue) model.cas_metric_video_start_time_est = model.cas_metric_video_start_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_video_end_time.HasValue) model.cas_metric_video_end_time_est = model.cas_metric_video_end_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_documentation_end_time.HasValue) model.cas_metric_documentation_end_time_est = model.cas_metric_documentation_end_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_metric_symptom_onset_during_ed_stay_time.HasValue) model.cas_metric_symptom_onset_during_ed_stay_time_est = model.cas_metric_symptom_onset_during_ed_stay_time.Value.ToTimezoneFromUtc(facilityTimeZone);

                    if (model.cas_response_time_physician.HasValue) model.cas_response_time_physician = model.cas_response_time_physician.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_response_first_atempt.HasValue) model.cas_response_first_atempt = model.cas_response_first_atempt.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_phy_technical_issue_date.HasValue) model.cas_phy_technical_issue_date_est = model.cas_phy_technical_issue_date.Value.ToTimezoneFromUtc(facilityTimeZone);
                    if (model.cas_callback_response_time_est.HasValue) model.cas_callback_response_time_est = model.cas_callback_response_time.Value.ToTimezoneFromUtc(facilityTimeZone);
                }
            }
            return model;
        }
        public IQueryable<@case> GetQueryable()
        {
            return _unitOfWork.CaseRepository.Query();
        }
        public IQueryable<@case> GetCaseListByFacility(Guid facilityId, application_setting appSetting)
        {

            int offset = 12;
            if (appSetting != null)
            {
                if (appSetting.aps_duplicate_popup_timer.HasValue && appSetting.aps_duplicate_popup_timer.Value > 0)
                {
                    offset = appSetting.aps_duplicate_popup_timer.Value;
                }
            }
            var criteriaDate = DateTime.Now.ToEST().AddHours(-offset);
            return _unitOfWork.CaseRepository.Query()
                    .Where(x => x.cas_fac_key != null
                           && x.cas_fac_key == facilityId
                           && x.cas_created_date >= criteriaDate)
                           .OrderByDescending(m => m.cas_key)
                    .Take(10);
        }
        public void Create(@case entity, bool commit = true)
        {
            lock (_caseLocker)
            {
                entity.cas_case_number = GetCaseNumber();

                _unitOfWork.CaseRepository.Insert(entity);
                if (commit)
                {
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }
                Helpers.DBHelper.ExecuteNonQuery("usp_refresh_case");
            }
        }
        public void Edit(@case entity, bool commit = true)
        {
            HandleTemplateData(entity);

            _unitOfWork.CaseRepository.Update(entity);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            Helpers.DBHelper.ExecuteNonQuery("usp_refresh_case");
        }

        public void EditCaseOnly(@case entity, bool commit = true)
        {

            _unitOfWork.CaseRepository.Update(entity);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            Helpers.DBHelper.ExecuteNonQuery("usp_refresh_case");
        }
        public void UpdateBlast (int cas_key, string response_key, string associated_key, string phy_key, int ctp_key, string modifiedby, DateTime modifiedon, int cst_key, DateTime status_assign, DateTime responseTime, DateTime phy_assignDate, string initials)
        {
            //_unitOfWork.SqlQuery<int>("Exec usp_Update_caseForBlast @cas_key='" + cas_keys + "'");
            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@cas_key", cas_key));
            sqlParameters.Add(new SqlParameter("@response_phy_key", response_key));
            sqlParameters.Add(new SqlParameter("@associated_key", associated_key));
            sqlParameters.Add(new SqlParameter("@cas_phy_key", phy_key));
            sqlParameters.Add(new SqlParameter("@ctp_key", ctp_key));
            sqlParameters.Add(new SqlParameter("@modified_by", modifiedby));
            sqlParameters.Add(new SqlParameter("@modified_on", modifiedon));
            sqlParameters.Add(new SqlParameter("@cst_key", cst_key));
            sqlParameters.Add(new SqlParameter("@status_assign_date", status_assign));
            sqlParameters.Add(new SqlParameter("@response_time_phy", responseTime));
            sqlParameters.Add(new SqlParameter("@phy_assign_date", phy_assignDate));
            sqlParameters.Add(new SqlParameter("@phy_initial", initials));

            Helpers.DBHelper.ExecuteNonQuery("usp_Update_caseForBlast", sqlParameters.ToArray());
        }

        public bool CheckRead(int id)
        {
            var record = _unitOfWork.SqlQuery<int>("Exec usp_case_read_status @cas_key='" + id + "'");
            if (record.First() == CaseStatus.WaitingToAccept.ToInt())
                return false;
            else
                return true;
        }

        public void detached(@case entity)
        {
            _unitOfWork.DetachedTbl(entity);
        }
        public void EditCaseOnlyForBlast(@case entity, bool commit = true)
        {
            _unitOfWork.SaveBlast(entity);
            _unitOfWork.Commit();
            Helpers.DBHelper.ExecuteNonQuery("usp_refresh_case");
        }

        public EntityTypes? CheckTemplateType(int cas_key, Guid fac_key, bool tpa, bool isStrokeAlert)
        {
            var model = _unitOfWork.CaseRepository.Find(cas_key);
            var facility = _unitOfWork.FacilityRepository.Query().FirstOrDefault(f => f.fac_key == fac_key);
            EntityTypes? entityType = null;
            // For tpa template
            if (tpa && isStrokeAlert && model.facility != null && facility.fac_not_templated_used && facility.facility_contract != null &&
                        (
                        facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                        || facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower())))
            {
                if (facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                        && facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower()))
                {
                    entityType = EntityTypes.NeuroStrokeAlertTemplateTpa;
                }
                else if (facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower()))
                {
                    entityType = EntityTypes.StrokeAlertTemplateTpa;

                }
            }
            // For non tpa template
            else if (!tpa && isStrokeAlert && facility != null && facility.fac_not_templated_used && facility.facility_contract != null)
            {
                if (facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleStroke.ToDescription().ToLower())
                    && facility.facility_contract.fct_service_calc.ToLower().Contains(ContractTypes.TeleNeuro.ToDescription().ToLower()))
                {
                    entityType = EntityTypes.StrokeAlertTemplateNoTpa;
                }
                else if (facility.facility_contract.fct_service_calc.ToLower().Equals(ContractTypes.TeleStroke.ToDescription().ToLower()))
                {
                    entityType = EntityTypes.StrokeAlertTemplateNoTpaTeleStroke;
                }
            }

            return entityType;
        }
        //public void HandleFollowUpCase(@case entity, @case followupEntity)
        //{
        //    try
        //    {
        //        HandleTemplateData(entity);
        //        followupEntity.cas_case_number = GetCaseNumber();

        //        _unitOfWork.BeginTransaction();
        //        _unitOfWork.CaseRepository.Insert(followupEntity);
        //        _unitOfWork.CaseRepository.Update(entity);
        //        _unitOfWork.Save();
        //        _unitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        _unitOfWork.Rollback();
        //    }
        //}

        private long GetCaseNumber()
        {
            var list = _unitOfWork.SqlQuery<long>("exec usp_case_number_get");
            return list.First();
        }
        private void HandleTemplateData(@case entity)
        {
            int cas_key = entity.cas_key;
            #region handling templates data saving
            // For stroke alert tpa tempalte
            if (entity.case_template_stroke_tpa != null)
            {
                var stroke_template = _unitOfWork.CaseTemplateStrokeTpaRepository.Query().AsNoTracking().FirstOrDefault(m => m.cts_cas_key == cas_key);
                if (stroke_template == null)
                {
                    entity.case_template_stroke_tpa.cts_created_by = entity.cas_modified_by;
                    entity.case_template_stroke_tpa.cts_created_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_tpa.cts_created_date = DateTime.Now.ToEST();
                    _unitOfWork.CaseTemplateStrokeTpaRepository.Insert(entity.case_template_stroke_tpa);
                }
                else
                {
                    entity.case_template_stroke_tpa.cts_modified_by = entity.cas_modified_by;
                    entity.case_template_stroke_tpa.cts_modfied_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_tpa.cts_modified_date = DateTime.Now.ToEST();
                    // created by fields
                    entity.case_template_stroke_tpa.cts_created_by = stroke_template.cts_created_by;
                    entity.case_template_stroke_tpa.cts_created_date = stroke_template.cts_created_date;
                    entity.case_template_stroke_tpa.cts_created_by_name = stroke_template.cts_created_by_name;
                    _unitOfWork.CaseTemplateStrokeTpaRepository.Update(entity.case_template_stroke_tpa);
                }
            }
            else if (entity.case_template_stroke_neuro_tpa != null)
            {
                var stroke_template = _unitOfWork.CaseTemplateStrokeNeuroTPARepository.Query().AsNoTracking().FirstOrDefault(m => m.csn_cas_key == cas_key);
                if (stroke_template == null)
                {
                    entity.case_template_stroke_neuro_tpa.csn_created_by = entity.cas_modified_by;
                    entity.case_template_stroke_neuro_tpa.csn_created_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_neuro_tpa.csn_created_date = DateTime.Now.ToEST();
                    _unitOfWork.CaseTemplateStrokeNeuroTPARepository.Insert(entity.case_template_stroke_neuro_tpa);
                }
                else
                {
                    entity.case_template_stroke_neuro_tpa.csn_modified_by = entity.cas_modified_by;
                    entity.case_template_stroke_neuro_tpa.csn_modfied_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_neuro_tpa.csn_modified_date = DateTime.Now.ToEST();
                    // created by fields                   
                    entity.case_template_stroke_neuro_tpa.csn_created_by = stroke_template.csn_created_by;
                    entity.case_template_stroke_neuro_tpa.csn_created_date = stroke_template.csn_created_date;
                    entity.case_template_stroke_neuro_tpa.csn_created_by_name = stroke_template.csn_created_by_name;
                    _unitOfWork.CaseTemplateStrokeNeuroTPARepository.Update(entity.case_template_stroke_neuro_tpa);
                }
            }
            // For stroke alert non tpa tempalte
            else if (entity.case_template_stroke_notpa != null)
            {
                var stroke_template_notpa = _unitOfWork.CaseTemplateStrokeNoTpaRepository.Query().AsNoTracking().FirstOrDefault(m => m.ctn_cas_key == cas_key);
                if (stroke_template_notpa == null)
                {
                    entity.case_template_stroke_notpa.ctn_created_by = entity.cas_modified_by;
                    entity.case_template_stroke_notpa.ctn_created_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_notpa.ctn_created_date = DateTime.Now.ToEST();
                    _unitOfWork.CaseTemplateStrokeNoTpaRepository.Insert(entity.case_template_stroke_notpa);
                }
                else
                {
                    entity.case_template_stroke_notpa.ctn_modified_by = entity.cas_modified_by;
                    entity.case_template_stroke_notpa.ctn_modfied_by_name = entity.cas_modified_by_name;
                    entity.case_template_stroke_notpa.ctn_modified_date = DateTime.Now.ToEST();
                    // created by fields
                    entity.case_template_stroke_notpa.ctn_created_by = stroke_template_notpa.ctn_created_by;
                    entity.case_template_stroke_notpa.ctn_created_date = stroke_template_notpa.ctn_created_date;
                    entity.case_template_stroke_notpa.ctn_created_by_name = stroke_template_notpa.ctn_created_by_name;
                    _unitOfWork.CaseTemplateStrokeNoTpaRepository.Update(entity.case_template_stroke_notpa);
                }
            }
            else if (entity.case_template_telestroke_notpa != null)
            {
                var stroke_template = _unitOfWork.CaseTemplateTelestrokeNotpaRepository.Query().AsNoTracking().FirstOrDefault(m => m.ctt_cas_key == cas_key);
                if (stroke_template == null)
                {
                    entity.case_template_telestroke_notpa.ctt_created_by = entity.cas_modified_by;
                    entity.case_template_telestroke_notpa.ctt_created_by_name = entity.cas_modified_by_name;
                    entity.case_template_telestroke_notpa.ctt_created_date = DateTime.Now.ToEST();
                    _unitOfWork.CaseTemplateTelestrokeNotpaRepository.Insert(entity.case_template_telestroke_notpa);
                }
                else
                {
                    entity.case_template_telestroke_notpa.ctt_modified_by = entity.cas_modified_by;
                    entity.case_template_telestroke_notpa.ctt_modfied_by_name = entity.cas_modified_by_name;
                    entity.case_template_telestroke_notpa.ctt_modified_date = DateTime.Now.ToEST();
                    // created by fields                   
                    entity.case_template_telestroke_notpa.ctt_created_by = stroke_template.ctt_created_by;
                    entity.case_template_telestroke_notpa.ctt_created_date = stroke_template.ctt_created_date;
                    entity.case_template_telestroke_notpa.ctt_created_by_name = stroke_template.ctt_created_by_name;
                    _unitOfWork.CaseTemplateTelestrokeNotpaRepository.Update(entity.case_template_telestroke_notpa);
                }
            }
            else if (entity.case_template_statconsult != null)
            {
                var stroke_template = _unitOfWork.CaseTemplateStatConsultRepository.Query().AsNoTracking().FirstOrDefault(m => m.ctt_cas_key == cas_key);
                if (stroke_template == null)
                {
                    entity.case_template_statconsult.ctt_created_by = entity.cas_modified_by;
                    entity.case_template_statconsult.ctt_created_by_name = entity.cas_modified_by_name;
                    entity.case_template_statconsult.ctt_created_date = DateTime.Now.ToEST();
                    _unitOfWork.CaseTemplateStatConsultRepository.Insert(entity.case_template_statconsult);
                }
                else
                {
                    entity.case_template_statconsult.ctt_modified_by = entity.cas_modified_by;
                    entity.case_template_statconsult.ctt_modfied_by_name = entity.cas_modified_by_name;
                    entity.case_template_statconsult.ctt_modified_date = DateTime.Now.ToEST();
                    // created by fields                   
                    entity.case_template_statconsult.ctt_created_by = stroke_template.ctt_created_by;
                    entity.case_template_statconsult.ctt_created_date = stroke_template.ctt_created_date;
                    entity.case_template_statconsult.ctt_created_by_name = stroke_template.ctt_created_by_name;
                    _unitOfWork.CaseTemplateStatConsultRepository.Update(entity.case_template_statconsult);
                }
            }
            #endregion
        }
        private IQueryable<facility> GetPhsicianNonBoardedFacilities(string phy_key)
        {
            var currentDate = DateTime.Now.ToEST();
            var maxDate = DateTime.MaxValue;
            var defaultStatus = _physicianStatusService.GetDefault();
            var physicianLicenseStates = (_unitOfWork.PhysicianLicenseRepository.Query()
                                          .Where(m => m.phl_is_active)
                                          .Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                          .Where(m => m.phl_user_key == phy_key)
                                          //.Where(m => m.phl_license_state == null || facility.fac_stt_key == null || m.phl_license_state == facility.fac_stt_key)
                                          .Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
                                          ).Select(m => m.phl_license_state).ToList();

            var facilities = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                             join contact in _unitOfWork.ContactRepository.Query() on m.fap_fac_key equals contact.cnt_fac_key into facilityContactJoin
                             from facilityContact in facilityContactJoin.DefaultIfEmpty()
                             where
                             m.fap_user_key == phy_key
                             && m.fap_is_on_boarded == false
                             // for ticket - 370
                             && m.fap_is_hide_pending_onboarding == false
                             && physicianLicenseStates.Contains(m.facility.fac_stt_key)
                             // for ticket - 400
                             && m.fap_start_date != null
                             && m.facility.fac_go_live
                             && m.fap_hide == false

                             select m.facility;

            return facilities.Distinct();
        }
        public IQueryable<string> GetCart(Guid key)
        {
            return _unitOfWork.FacilityRepository.Query().Where(x => x.fac_key == key).Select(x => x.fac_cart_numbers);
        }
        public void EditMorbid(PremorbidCorrespondnce premorbid, int cas_key, bool commit = true)
        {
            //if (premorbid.pmc_cas_premorbid_completedby.Count > 0 && premorbid.pmc_cas_premorbid_completedby[0] != "")
            //{
                var isExist = _unitOfWork.premorbidRepository.Query().Where(x => x.pmc_cas_key == cas_key).ToList();
                if (isExist.Count > 0)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        isExist[i].pmc_cas_premorbid_patient_phone = premorbid.pmc_cas_premorbid_patient_phone;
                        var checkdate = premorbid.pmc_cas_premorbid_datetime_of_contact[i];
                        if(checkdate != "")
                        {
                            isExist[i].pmc_cas_premorbid_datetime_of_contact = Convert.ToDateTime(checkdate);
                        }
                        else
                        {
                            isExist[i].pmc_cas_premorbid_datetime_of_contact = null;
                        }
                        
                        isExist[i].pmc_cas_premorbid_spokewith = premorbid.pmc_cas_premorbid_spokewith[i];
                        isExist[i].pmc_cas_premorbid_comments = premorbid.pmc_cas_premorbid_comments[i];
                        if (i == 0)
                        {
                            isExist[i].pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_first;
                        }
                        else if (i == 1)
                        {
                            isExist[i].pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_second;
                        }
                        else
                        {
                            isExist[i].pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_third;
                        }
                        isExist[i].pmc_cas_premorbid_completedby = premorbid.pmc_cas_premorbid_completedby[i];
                        isExist[i].pmc_cas_patient_satisfaction_video_experience = premorbid.pmc_cas_patient_satisfaction_video_experience;
                        isExist[i].pmc_cas_patient_satisfaction_communication = premorbid.pmc_cas_patient_satisfaction_communication;
                        isExist[i].pmc_cas_willing_todo_interview = premorbid.pmc_cas_willing_todo_interview;
                        isExist[i].pmc_cas_consent_sent = premorbid.pmc_cas_consent_sent;
                        isExist[i].pmc_cas_consent_received = premorbid.pmc_cas_consent_received;
                    }
                    _unitOfWork.premorbidRepository.UpdateRange(isExist);
                    if (commit)
                    {
                        _unitOfWork.Save();
                        _unitOfWork.Commit();
                    }
                }
                else
                {

                    List<premorbid_correspondnce> entity = new List<premorbid_correspondnce>();
                    premorbid_correspondnce obj;
                    for (var i = 0; i < 3; i++)
                    {
                        obj = new premorbid_correspondnce();
                        obj.pmc_cas_key = cas_key;
                        obj.pmc_cas_premorbid_patient_phone = premorbid.pmc_cas_premorbid_patient_phone;
                        //obj.pmc_cas_premorbid_datetime_of_contact = Convert.ToDateTime(premorbid.pmc_cas_premorbid_datetime_of_contact[i]);
                        var checkdate = premorbid.pmc_cas_premorbid_datetime_of_contact[i];
                        if (checkdate != "")
                        {
                            obj.pmc_cas_premorbid_datetime_of_contact = Convert.ToDateTime(checkdate);
                        }
                        else
                        {
                            obj.pmc_cas_premorbid_datetime_of_contact = null;
                        }
                        obj.pmc_cas_premorbid_spokewith = premorbid.pmc_cas_premorbid_spokewith[i];
                        obj.pmc_cas_premorbid_comments = premorbid.pmc_cas_premorbid_comments[i];
                        if (i == 0)
                        {
                            obj.pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_first;
                        }
                        else if (i == 1)
                        {
                            obj.pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_second;
                        }
                        else
                        {
                            obj.pmc_cas_premorbid_successful_or_unsuccessful = premorbid.pmc_cas_premorbid_successful_or_unsuccessful_third;
                        }
                        obj.pmc_cas_premorbid_completedby = premorbid.pmc_cas_premorbid_completedby[i];
                        obj.pmc_cas_patient_satisfaction_video_experience = premorbid.pmc_cas_patient_satisfaction_video_experience;
                        obj.pmc_cas_patient_satisfaction_communication = premorbid.pmc_cas_patient_satisfaction_communication;
                        obj.pmc_cas_willing_todo_interview = premorbid.pmc_cas_willing_todo_interview;
                        obj.pmc_cas_consent_sent = premorbid.pmc_cas_consent_sent;
                        obj.pmc_cas_consent_received = premorbid.pmc_cas_consent_received;
                        entity.Add(obj);
                    }
                    if (entity.Count > 0)
                    {
                        _unitOfWork.premorbidRepository.InsertRange(entity);
                    }


                }
                if (commit)
                {
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }
            //}

        }
        public PremorbidCorrespondnce GetPremorbidCorrespondnces(int cas_key)
        {

            PremorbidCorrespondnce obj = new PremorbidCorrespondnce();

            List<string> _listpmc_cas_premorbid_datetime_of_contact = new List<string>();
            string _objpmc_cas_premorbid_datetime_of_contact;

            List<int?> _listpmc_cas_premorbid_spokewith = new List<int?>();
            int? _objpmc_cas_premorbid_spokewith;

            List<string> _listpmc_cas_premorbid_comments = new List<string>();
            string _objpmc_cas_premorbid_comments;

            List<string> _listpmc_cas_premorbid_completedby = new List<string>();
            string _objpmc_cas_premorbid_completedby;

            var model = _unitOfWork.premorbidRepository.Query().Where(x => x.pmc_cas_key == cas_key).ToList();



            if (model.Count() > 0)
            {
                for (int i = 0; i < model.Count(); i++)
                {

                    _objpmc_cas_premorbid_datetime_of_contact = "";
                    _objpmc_cas_premorbid_datetime_of_contact = model[i].pmc_cas_premorbid_datetime_of_contact.ToString();
                    _listpmc_cas_premorbid_datetime_of_contact.Add(_objpmc_cas_premorbid_datetime_of_contact);

                    _objpmc_cas_premorbid_spokewith = new int();
                    _objpmc_cas_premorbid_spokewith = model[i].pmc_cas_premorbid_spokewith;
                    _listpmc_cas_premorbid_spokewith.Add(_objpmc_cas_premorbid_spokewith);

                    _objpmc_cas_premorbid_comments = "";
                    _objpmc_cas_premorbid_comments = model[i].pmc_cas_premorbid_comments;
                    _listpmc_cas_premorbid_comments.Add(_objpmc_cas_premorbid_comments);

                    _objpmc_cas_premorbid_completedby = "";
                    _objpmc_cas_premorbid_completedby = model[i].pmc_cas_premorbid_completedby;
                    _listpmc_cas_premorbid_completedby.Add(_objpmc_cas_premorbid_completedby);

                }
                obj = new PremorbidCorrespondnce();
                obj.pmc_cas_premorbid_patient_phone = model[0].pmc_cas_premorbid_patient_phone;
                obj.pmc_cas_premorbid_successful_or_unsuccessful_first = model[0].pmc_cas_premorbid_successful_or_unsuccessful;
                obj.pmc_cas_premorbid_successful_or_unsuccessful_second = model[1].pmc_cas_premorbid_successful_or_unsuccessful;
                obj.pmc_cas_premorbid_successful_or_unsuccessful_third = model[2].pmc_cas_premorbid_successful_or_unsuccessful;
                obj.pmc_cas_patient_satisfaction_video_experience = model[0].pmc_cas_patient_satisfaction_video_experience;
                obj.pmc_cas_patient_satisfaction_communication = model[0].pmc_cas_patient_satisfaction_communication;
                obj.pmc_cas_willing_todo_interview = model[0].pmc_cas_willing_todo_interview;
                obj.pmc_cas_consent_sent = model[0].pmc_cas_consent_sent;
                obj.pmc_cas_consent_received = model[0].pmc_cas_consent_received;
                obj.pmc_cas_premorbid_datetime_of_contact = _listpmc_cas_premorbid_datetime_of_contact;
                obj.pmc_cas_premorbid_spokewith = _listpmc_cas_premorbid_spokewith;
                obj.pmc_cas_premorbid_comments = _listpmc_cas_premorbid_comments;
                obj.pmc_cas_premorbid_completedby = _listpmc_cas_premorbid_completedby;
            }
            return obj;
        }
    }
}
