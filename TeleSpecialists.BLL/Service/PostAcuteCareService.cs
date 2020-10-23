using Kendo.DynamicLinq;
using System.Linq;
using TeleSpecialists.BLL.Model;
using System.Data.Entity;
using System;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using System.Reflection;
using TeleSpecialists.BLL.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Web;


namespace TeleSpecialists.BLL.Service
{
    public class PostAcuteCareService : BaseService
    {
        private readonly AdminService _adminService;
        private readonly UCLService _uclService;
        private readonly PhysicianStatusService _physicianStatusService;

        public PostAcuteCareService() : base()
        {
            _adminService = new AdminService();
            _uclService = new UCLService();
            _physicianStatusService = new PhysicianStatusService();
        }
        public List<AspNetUser> getPACPhysicains()
        {
            var query = string.Format("Select CONCAT(au.FirstName, ' ',au.LastName) AS FirstName, au.*  from [dbo].[AspNetUsers]  au Join AspNetUserRoles aur on aur.UserId = au.Id Where au.IsSleep = 'true' and au.IsActive = 'true'  and  (  aur.RoleId = '0029737b-f013-4e0b-8a31-1b09524194f9' or  aur.RoleId = '684c8b74-216a-48bb-a9c1-c9cd4c1014fc')"); //");
             
            return _unitOfWork.SqlQuery<AspNetUser>(query).ToList();
        }


        public void Create(post_acute_care model)
        { 
            _unitOfWork.PostAcuteCareRepository.Insert(model);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        // public DataSourceResult GetAll(DataSourceRequest request, string id, bool isPac)
        public DataSourceResult GetAll(DataSourceRequest request, string physician, List<Guid> facilities = null)
        {
            var list = _unitOfWork.SqlQuery<ViewModels.PACgridVM>("Exec [usp_pac_listing] " + GetAllSqlParams(request, false, physician,facilities).ToString()).AsQueryable();
            int Total = list.FirstOrDefault()?.totalRecords ?? 0;
            if (request.Take == 0)
                Total = list.Count();
            var kendoObj = list.ToDataSourceResult(Total, 0, request.Sort, null);
            kendoObj.Total = Total;
            return kendoObj;


            //var caseTypelist = from m in _unitOfWork.PostAcuteCareRepository.Query()
            //                   join n in _unitOfWork.ApplicationUsers on m.pac_phy_key equals n.Id into physicians 
            //                   join f in _unitOfWork.FacilityRepository.Query() on m.pac_fac_key equals f.fac_key into facility
            //                   orderby m.pac_created_date descending
            //                   select new
            //                   {
            //                       m.pac_key,
            //                       m.pac_ctp_key,
            //                       m.pac_fac_key,
            //                       CaseStatus = m.pac_cst_key != null ? ((PacStatus)m.pac_cst_key).ToString() : "",
            //                       casetype = m.pac_ctp_key != null ? "PAC Consult Request" : "",
            //                       facilityname = m.facility.fac_name,
            //                       PhysicianName = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
            //                       m.pac_cst_key,
            //                       m.pac_patient,
            //                       m.pac_patient_initials,
            //                       m.pac_callback, //Requestor Callback
            //                       m.pac_created_by_name, //Requestor Name 
            //                       pac_date_of_consult = m.pac_date_of_consult != null ? DBHelper.FormatDateTime(m.pac_date_of_consult, false) : "",
            //                       pac_date_of_completion = m.pac_date_of_completion != null ? DBHelper.FormatDateTime(m.pac_date_of_completion, false) : "",
            //                       m.pac_referring_physician,
            //                       m.pac_created_date,
            //                       m.pac_phy_key
            //                   };
            //if(isPac)
            //{
            //    caseTypelist = caseTypelist.Where(x => x.pac_phy_key == id);
            //}

            //return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }


        public StringBuilder GetAllSqlParams(DataSourceRequest request, bool isCaseListingExport, string physician = "", List<Guid> facilities = null)
        {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
            StringBuilder Query = new StringBuilder();

            Query.AppendSqlParam("@pacStatusEnumId", UclTypes.PacStatus.ToInt());
            Query.AppendSqlParam(",@pacCaseTypeEnumId", UclTypes.PacCaseType.ToInt());

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
                    FillAdvanceParamsSql(ref Query, request,facilities);

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




        public post_acute_care GetDetails(int id)
        {
            var model = _unitOfWork.PostAcuteCareRepository.Query().AsNoTracking()
                                   .Include(m => m.facility)
                                   .Include(m => m.AspNetUser)
                                   .FirstOrDefault(m => m.pac_key == id);
            return model;
        }
        public void Edit(post_acute_care model)
        {
            _unitOfWork.PostAcuteCareRepository.Update(model);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public post_acute_care GetDetailsWithoutTimeConversion(int id)
        {
            var model = _unitOfWork.PostAcuteCareRepository.Query().AsNoTracking()
                                   .Include(m => m.facility)
                                   .Include(m => m.AspNetUser)
                                   .FirstOrDefault(m => m.pac_key == id);
            return model;
        }
    }
}
