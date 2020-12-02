using Kendo.DynamicLinq;
using System.Linq;
using System;
using TeleSpecialists.BLL.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.CasePage;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL.Service.CaseServices
{
    public class CaseGridService : BaseService
    {

        /// <summary>
        ///updated version of GetAllForDashboard, The code is moved to store procedure 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="physician"></param>
        /// <param name="facilities"></param>
        /// <returns></returns>
        public DataSourceResult GetCaseDashboardPageData(DataSourceRequest request, string physician = "", List<Guid> facilities = null)
        {
            var list = _unitOfWork.SqlQuery<ViewModels.CasePage.DashboardPage>("Exec usp_case_dashboard " + GetAllSqlParams(request,false, physician, facilities).ToString()).AsQueryable();
            int Total = list.FirstOrDefault()?.TotalRecords ?? 0;
            if (request.Take == 0)
                Total = list.Count();
            var kendoObj = list.ToDataSourceResult(Total, 0, request.Sort, null);
            kendoObj.Total = Total;
            return kendoObj;
        }




        /// <summary>
        /// updated version of GetAll, The code is moved to store procedure
        /// </summary>
        /// <param name="request"></param>
        /// <param name="physician"></param>
        /// <param name="facilities"></param>
        /// <returns></returns>
        public DataSourceResult GetCaseLisingPageData(DataSourceRequest request, string physician = "", List<Guid> facilities = null)
        {
            var list = _unitOfWork.SqlQuery<ViewModels.CasePage.CaseListing>("Exec usp_case_listing " + GetAllSqlParams(request, false, physician, facilities).ToString()).AsQueryable();
            int Total = list.FirstOrDefault()?.TotalRecords ?? 0;
            if (request.Take == 0)
                Total = list.Count();
            var kendoObj = list.ToDataSourceResult(Total, 0, request.Sort, null);
            kendoObj.Total = Total;
            return kendoObj;
        }

        public List<CaseListing_Export> GetCaseLisingPageData_Export(DataSourceRequest request, string physician = "", List<Guid> facilities = null)
        {
            return _unitOfWork.SqlQuery<CaseListing_Export>("Exec usp_case_listing_export " + GetAllSqlParams(request, true,physician, facilities).ToString());
        }


        public List<ViewModels.CasePage.DashboardPage> GetCaseDashboardPage_Export(DataSourceRequest request, string physician = "", List<Guid> facilities = null)
        {
            var list = _unitOfWork.SqlQuery<ViewModels.CasePage.DashboardPage>("Exec usp_case_dashboard " + GetAllSqlParams(request, false, physician, facilities).ToString());
            return list;
        }



        public StringBuilder GetAllSqlParams(DataSourceRequest request, bool isCaseListingExport, string physician = "", List<Guid> facilities = null)
        {


            StringBuilder Query = new StringBuilder();

            if (!isCaseListingExport)
            {
                if (request.Take > 0)
                    Query.Append("@Take=" + request.Take + ",@Skip=" + request.Skip);
                else
                    Query.Append("@Skip=0");
            }
            else
            {
                Query.Append("@DummyParam=1");
            }


            if (request.Sort?.FirstOrDefault() != null)
            {
                string direction = "";
                if (!string.IsNullOrEmpty(request.Sort.FirstOrDefault().Dir))
                {
                    direction = request.Sort.FirstOrDefault().Dir.ToUpper();
                    //request.Sort.FirstOrDefault().Field
                    Query.AppendSqlParam(",@SortType", request.Sort.FirstOrDefault().Field);
                    Query.AppendSqlParam(",@SortDir", direction);
                }

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

                    //FillAdvanceParamsSql(ref Query, request);
                    FillAdvanceParamsSql(ref Query, request, facilities);
                    //#endregion

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
                }
                #endregion
                #region Case Flagged Filter
                var flag_fiter = request.Filter.Filters.Where(m => m.Field == "cas_is_flagged").FirstOrDefault();
                if (flag_fiter != null)
                {
                    var flag_array = flag_fiter.Value.ToString().Split('-');
                    var source = flag_array.Last().ToInt();
                    int flag_value = flag_array.First().ToBool().ToInt();
                    //if (source == PageSource.Dashboard.ToInt())
                    //    predicate = predicate.Or(m => m.CaseModel.cas_is_flagged_dashboard.Equals(flag_value));
                    //else
                    //    predicate = predicate.Or(m => m.CaseModel.cas_is_flagged.Equals(flag_value));
                    Query.Append(",@Flagged=" + flag_value);

                    // applyFilter = true;
                }
                #endregion

                #region Case Physician Flagged Filter Added by Ahmad 09-09-2020 12:01 PM
                var physician_flag_fiter = request.Filter.Filters.Where(m => m.Field == "cas_is_flagged_physician").FirstOrDefault();
                if (physician_flag_fiter != null)
                {
                    var physician_flag_array = physician_flag_fiter.Value.ToString().Split('-');
                    var source = physician_flag_array.Last().ToInt();
                    int flag_value = physician_flag_array.First().ToBool().ToInt();
                    Query.Append(",@PhysicianFlagged=" + flag_value);
                }
                #endregion

                #region Case Status Filter
                var case_status_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_status");
                if (case_status_field != null)
                {
                    var case_status = case_status_field.Value?.ToString();
                    if (!string.IsNullOrEmpty(case_status))
                    {
                        //var filterIds = case_status.Split(',').Select(m => m.ToInt());
                        //predicate = predicate.And(m => filterIds.Contains(m.CaseModel.cas_cst_key));
                        //applyFilter = true;
                        Query.AppendSqlParam(",@CaseStatus", case_status);
                    }
                }
                #endregion

                #region User Initial Filter
                //var cas_history_physician_initial_cal_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_history_physician_initial_cal");
                //if (cas_history_physician_initial_cal_field != null)
                //{
                //    var cas__physician_ids = cas_history_physician_initial_cal_field.Value?.ToString();
                //    if (!string.IsNullOrEmpty(cas__physician_ids))
                //    {
                //        var splitedArry = cas__physician_ids.Split(',');
                //        var filteredPhysicianIds = splitedArry.Select(m => m.ToString());
                //        if (splitedArry.Length > 0)
                //        {
                //            predicate = predicate.And(m => filteredPhysicianIds.Contains(m.CaseModel.cas_phy_key));
                //        }
                //        applyFilter = true;
                //    }
                //}
                #endregion

                #region User Type Filter
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


                        //      var splitedArry = case_type_ids.Split(',');
                        //        if (splitedArry.Length > 0)
                        //        {
                        //            var filteredTypeIds = splitedArry.Select(m => m.ToInt());
                        //            var comparingoperator = cas_ctp_key_field.Operator;
                        //            if (comparingoperator == "eq")
                        //            {
                        //                predicate = predicate.And(m => filteredTypeIds.Contains(m.CaseModel.cas_ctp_key));
                        //            }
                        //            else if (comparingoperator == "neq")
                        //            {
                        //                predicate = predicate.And(m => !filteredTypeIds.Contains(m.CaseModel.cas_ctp_key));

                        //            }

                    }
                    //      applyFilter = true;
                }


                #endregion

                #region eAlert Filter
                var eAlert = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "cas_is_ealert");
                if (eAlert != null)
                {
                    var eAlertValues = eAlert.Value?.ToString();
                    if (!string.IsNullOrEmpty(eAlertValues))
                    {
                        //var splittedeAlerts = eAlertValues.Split(',');
                        //var filterEAlerts = splittedeAlerts.Select(m => m.ToString() == "true");
                        //if (splittedeAlerts.Length > 0)
                        //{
                        //    predicate = predicate.And(m => filterEAlerts.Contains(m.CaseModel.cas_is_ealert));
                        //}
                        //applyFilter = true;

                        if (!(eAlertValues.Contains("true") && eAlertValues.Contains("false")))
                        {
                            Query.Append((",@IsEAlert=" + (eAlertValues.ToLower() == "true" ? "1" : "0")));
                        }
                    }
                }
                #endregion

                #region Case Assign History Filter
                //userInitialFilter
                var userInitialFilter = request.Filter.Filters.FirstOrDefault(m => m.Field.Contains("cas_history_physician_initial_cal"));
                if (!string.IsNullOrEmpty(userInitialFilter?.Value?.ToString()))
                {
                    Query.AppendSqlParam(",@UserInitialFilter", userInitialFilter.Value.ToString());
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
                            Query.AppendSqlParam(",@CallerType", string.Join(",", splitedArry));
                            // predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_call_type));
                        }
                        // applyFilter = true;
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
                            Query.AppendSqlParam(",@CallerSource", string.Join(",", splitedArry));
                            // predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_caller_source_key));
                        }
                        // applyFilter = true;
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
                            Query.AppendSqlParam(",@TPAFilter", string.Join(",", splitedArry));
                            //  predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_metric_tpa_consult));
                        }
                        //applyFilter = true;
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
                            Query.AppendSqlParam(",@BillingCodeFilter", string.Join(",", splitedArry));
                            //  predicate = predicate.And(m => splitedArry.Contains(m.CaseModel.cas_billing_bic_key));
                        }
                        //  applyFilter = true;
                    }
                }
                #endregion


                //if (applyFilter)
                //{
                //    if (predicate != null)
                //        caseQuery = caseQuery.Where(predicate);
                //}
            }


            return Query;
        }


        private void FillAdvanceParamsSql(ref StringBuilder Query, DataSourceRequest request, List<Guid> facilities = null)
        {
            var currentDate = DateTime.Now.ToEST();
            if (request.Filter != null)
            {
                if (request.Filter.Filters != null)
                {
                    var result = request.Filter.Filters.ToList();
                    if (result.Contains(null))
                    {
                        result.Remove(null);

                        request.Filter.Filters = result.AsEnumerable();
                    }
                    //   var predicate = PredicateUtils.Null<CaseGridViewModel>();
                    //  bool applyFilter = false;
                    #region Advance Search Filters             
                    var filters = request.Filter.Filters.Where(m => m.Field.Contains("advance_") && m.Value != null);
                    var caseNumber = filters.Where(m => m.Field.Contains("cas_case_number")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(caseNumber?.Value?.ToString()))
                    {
                        Query.AppendSqlParam(",@CaseNumber", caseNumber.Value.ToString());
                    }

                    var patientName = filters.Where(m => m.Field.Contains("cas_patient")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(patientName?.Value?.ToString()))
                    {
                        Query.AppendSqlParam(",@PatientName", patientName.Value.ToString());
                    }



                    #region Sign off/Follow up
                    var cas_billing_visit_type = filters.Where(m => m.Field.Contains("cas_billing_visit_type")).FirstOrDefault();// request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "advance_cas_billing_visit_type");
                    if (cas_billing_visit_type != null)
                    {
                        var filter_ids = cas_billing_visit_type.Value?.ToString();
                        if (!string.IsNullOrEmpty(filter_ids))
                        {
                            Query.AppendSqlParam(",@VisitType", filter_ids);
                        }
                    }
                    #endregion



                    var facilityFilter = filters.Where(m => m.Field.Contains("cas_fac_key")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(facilityFilter?.Value?.ToString()))
                    {
                        //Query.AppendSqlParam(",@FacilityId", facilityFilter.Value.ToString());
                        if (facilities == null)
                            facilities = new List<Guid>();

                        facilities.Add(new Guid(facilityFilter.Value.ToString()));
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
                                //  applyFilter = true;
                            }
                        }

                        #endregion
                    }

                    //foreach (var filter in filters)
                    //{

                    //    if (filter.Value != null)
                    //    {
                    //        if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                    //        {
                    //            var fieledName = filter.Field.Replace("advance_", "");
                    //            // passing property info
                    //            var propertyInfo = (((PropertyInfo[])caseQuery.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty(fieledName);
                    //            caseQuery = caseQuery.WhereEquals("CaseModel." + fieledName, filter.Value, propertyInfo);
                    //        }
                    //    }
                    //}

                    #endregion

                    #region Complex Advance Search Filters

                    var complexFilters = request.Filter.Filters.Where(m => m.Field.Contains("complex_"));

                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                    foreach (var filter in complexFilters)
                    {

                        if (filter.Value != null)
                        {
                            if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                            {
                                var fieledName = filter.Field.ToLower().Replace("complex_", "").Replace("_cmp", "Sec");
                                var time = TimeSpan.Parse(filter.Value.ToString()).TotalSeconds.ToInt();
                                // passing property info                                
                                Query.AppendSqlParam(",@" + fieledName, time);
                                Query.AppendSqlParam(",@" + fieledName + "Opr", filter.Operator);
                            }
                        }
                    }

                    #endregion
                }
            }
        }


    }
}
