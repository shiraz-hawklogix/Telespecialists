using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.ViewModels.Reports;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.BLL.ModelEx;
using System.IO;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class ReportService : BaseService
    {
        private readonly SchedulerService _schedulerService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly UCLService _uCLService;
        private LookupService _lookUpService;
        private FacilityService _FacilityService;
        private readonly AdminService _adminService;

        #region constructor
        public ReportService()
        {
            _schedulerService = new SchedulerService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _uCLService = new UCLService();
            _lookUpService = new LookupService();
            _FacilityService = new FacilityService();
            _adminService = new AdminService();
        }
        #endregion

        #region public-methods
        public DataSourceResult GetCredentials(DataSourceRequest request,
                                                List<string> facilities,
                                                List<string> physicians,
                                                bool? isStartDate,
                                                bool? isEndDate,
                                                bool? isOnBoarded,
                                                bool? fac_IsActive,
                                                bool? goLive,
                                                bool? phy_IsActive,
                                         DateTime? StartDate,
                                         DateTime? EndDate)
        {



            var query = from p in _unitOfWork.ApplicationUsers
                        join fp in _unitOfWork.FacilityPhysicianRepository.Query() on p.Id equals fp.fap_user_key
                        join f in _unitOfWork.FacilityRepository.Query() on fp.fap_fac_key equals f.fac_key
                        join stt in _unitOfWork.UCL_UCDRepository.Query() on f.fac_stt_key.Value equals stt.ucd_key
                        where !p.IsDeleted && !fp.fap_hide //552 to only show physicians that are not hide in report
                        orderby p.LastName, p.FirstName, f.fac_name
                        select new
                        {
                            state = stt.ucd_title,
                            phyId = p.Id,
                            PhysicianId = p.PhysicianId,
                            fac_id = f.fac_key.ToString(),
                            FullName = p.LastName + " " + p.FirstName,
                            f.fac_name,
                            fp.fap_credential_specialist,
                            fp.fap_date_assigned,
                            fp.fap_initial_app_received,
                            fp.fap_app_started,
                            fp.fap_app_submitted_to_hospital,
                            fp.fap_vcaa_date,
                            fp.fap_start_date,
                            fp.fap_Credentials_confirmed_date,
                            fp.fap_end_date,
                            isStartDate = fp.fap_start_date.HasValue,
                            isEndDate = fp.fap_end_date.HasValue,
                            fp.fap_is_on_boarded,
                            fp.fap_onboarded_date,
                            p.IsActive,
                            fp.fap_onboarded_by_name,
                            f.fac_go_live,
                            f.fac_is_active,
                            onBoarded = fp.fap_is_on_boarded && fp.fap_onboarded_date != null ? DBHelper.FormatDateTime(fp.fap_onboarded_date, false) : ""
                        };
            if (facilities != null)
            {


                query = query.Where(m => facilities.Contains(m.fac_id));


            }
            if (StartDate != null && EndDate != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_Credentials_confirmed_date) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(x.fap_Credentials_confirmed_date) <= DbFunctions.TruncateTime(EndDate));
            }
            if (physicians != null)
            {
                query = query.Where(m => physicians.Contains(m.phyId));
            }
            if (isStartDate != null)
            {
                query = query.Where(m => m.isStartDate == isStartDate);
            }
            if (isEndDate != null)
            {
                query = query.Where(m => m.isEndDate == isEndDate);
            }
            if (isOnBoarded != null)
            {
                query = query.Where(m => m.fap_is_on_boarded == isOnBoarded);
            }
            if (fac_IsActive != null)
            {
                query = query.Where(m => m.fac_is_active == fac_IsActive);
            }
            if (goLive != null)
            {
                query = query.Where(m => m.fac_go_live == goLive);
            }
            if (phy_IsActive != null)
            {
                query = query.Where(m => m.IsActive == phy_IsActive);
            }
            return query.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetLicense(DataSourceRequest request, List<string> physicians, List<string> states)
        {
            var query = from p in _unitOfWork.ApplicationUsers
                        join f in _unitOfWork.PhysicianLicenseRepository.Query() on p.Id equals f.phl_user_key
                        join s in _unitOfWork.UCL_UCDRepository.Query() on f.phl_license_state.Value equals s.ucd_key

                        where p.IsActive
                        && f.phl_is_active
                        && f.phl_license_state.HasValue
                        && s.ucd_ucl_key == (int)UclTypes.State

                        orderby p.LastName, p.FirstName, s.ucd_title, f.phl_issued_date
                        select new
                        {
                            phyId = p.Id,
                            PhysicianId = p.PhysicianId,
                            FullName = p.LastName + " " + p.FirstName,
                            ucd_key = s.ucd_key.ToString(),
                            s.ucd_title,
                            f.phl_issued_date,
                            f.phl_expired_date,
                            f.phl_is_in_use,
                            f.phl_assigned_to_name,
                            f.phl_date_assigned,
                            f.phl_app_started,
                            f.phl_app_submitted_to_board
                        };
            if (physicians != null)
            {
                query = query.Where(m => physicians.Contains(m.phyId));
            }
            if (states != null)
            {
                query = query.Where(m => states.Contains(m.ucd_key));
            }

            return query.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetCaseAssignHistory(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            var resultList = _unitOfWork.CaseAssignHistoryRepository.Query()
                .Where(m => DbFunctions.TruncateTime(m.cah_created_date) >= DbFunctions.TruncateTime(startDate)
                    && DbFunctions.TruncateTime(m.cah_created_date) <= DbFunctions.TruncateTime(endDate))

                .Select(m => new
                {
                    m.cah_cas_key,
                    m.@case.cas_case_number,
                    cas_billing_date_of_consult = m.@case.cas_billing_date_of_consult.HasValue ? DBHelper.FormatDateTime(m.@case.cas_billing_date_of_consult.Value, false) : ""
                })
                .GroupBy(x => x.cah_cas_key)
                .Select(x => x.FirstOrDefault())
                .OrderByDescending(x => x.cas_billing_date_of_consult);
            return resultList.ToDataSourceResult(request);
        }
        public DataSourceResult GetFacilityBillingReport(DataSourceRequest request,
                                                          List<Guid> facilities,
                                                          List<string> physicians,
                                                          List<int> billingCodes,
                                                          DateTime startDate,
                                                          DateTime endDate)
        {

            var Query = from m in _unitOfWork.CaseRepository.Query()
                        join identificationType in GetUclData(UclTypes.IdentificationType) on m.cas_identification_type equals identificationType.ucd_key into IdentTypeEntity
                        join billingCodeType in GetUclData(UclTypes.BillingCode) on m.cas_billing_bic_key equals billingCodeType.ucd_key into billingCodeEntity
                        join caseType in GetUclData(UclTypes.CaseType) on m.cas_ctp_key equals caseType.ucd_key into caseTypeEntity
                        join caseStatus in GetUclData(UclTypes.CaseStatus) on m.cas_cst_key equals caseStatus.ucd_key into caseStatusEntity

                        from billingCode in billingCodeEntity.DefaultIfEmpty()
                        from identitificationCode in IdentTypeEntity.DefaultIfEmpty()
                        from caseType in caseTypeEntity.DefaultIfEmpty()
                        from caseStatus in caseStatusEntity.DefaultIfEmpty()
                        where
                           DbFunctions.TruncateTime(m.cas_created_date) >= DbFunctions.TruncateTime(startDate)
                        && DbFunctions.TruncateTime(m.cas_created_date) <= DbFunctions.TruncateTime(endDate) && m.cas_cst_key == 20
                        select new
                        {
                            ca = m,
                            billingCode,
                            caseType,
                            caseStatus,
                            identitificationCode
                        };


            if (billingCodes != null)
            {
                Query = Query.Where(m => billingCodes.Contains(m.ca.cas_billing_bic_key.HasValue ? m.ca.cas_billing_bic_key.Value : 0));
            }

            if (facilities != null)
            {
                Query = Query.Where(m => facilities.Contains(m.ca.cas_fac_key));
            }


            if (physicians != null)
            {
                Query = Query.Where(m => physicians.Contains(m.ca.cas_phy_key));
            }

            string facilityTimeZone = Settings.DefaultTimeZone;

            var result = Query.Select(m => new
            {
                //
                cas_billing_date_of_consult = DBHelper.FormatDateTime(m.ca.cas_billing_date_of_consult, false),
                cas_identification_type = m.identitificationCode != null ? m.identitificationCode.ucd_title : "",
                m.ca.cas_identification_number,
                start_date = m.ca.cas_response_ts_notification != null ? DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone((m.ca.facility.fac_timezone != null ? m.ca.facility.fac_timezone : facilityTimeZone), m.ca.cas_response_ts_notification.Value), true) : "",
                patient_name = m.ca.cas_patient,
                cas_billing_dob = m.ca.cas_billing_dob.HasValue ? DBHelper.FormatDateTime(m.ca.cas_billing_dob.Value, false) : "",
                billing_code = m.billingCode != null ? m.billingCode.ucd_title : "",
                case_type = m.caseType != null ? m.caseType.ucd_title : "",
                Physician = m.ca.AspNetUser2 != null ? m.ca.AspNetUser2.LastName + " " + m.ca.AspNetUser2.FirstName : "",
                PhysicianId = m.ca.AspNetUser2.PhysicianId,
                cas_metric_firstlogin_date_est = m.ca.cas_response_first_atempt != null ? DBHelper.FormatDateTime(m.ca.cas_response_first_atempt.Value, true) : "",
                //stampt_to_login_time = DBHelper.FormatSeconds(DbFunctions.DiffSeconds(m.ca.cas_metric_stamp_time_est, m.ca.cas_response_first_atempt)),
                stampt_to_login_time = DBHelper.FormatSeconds(m.ca.cas_metric_stamp_time_est, m.ca.cas_response_first_atempt),
                m.ca.facility.fac_name,
                caseStatus = m.caseStatus != null ? m.caseStatus.ucd_title : "",
                m.ca.cas_case_number,

                m.ca.cas_key
            }).OrderBy(m => m.cas_key);

            return result.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);

        }
        public List<PhysicianBillingByShiftViewModel> GetPhysicainBillingByShift(DataSourceRequest request,
                                                            List<string> physicians,
                                                            DateTime startDate,
                                                            DateTime endDate,
                                                            List<int> caseStatus,
                                                            ShiftType shiftType)
        {
            List<PhysicianBillingByShiftViewModel> casesList = new List<PhysicianBillingByShiftViewModel>();
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            List<PhysicianBillingByShiftViewModel> offShiftCasesList = null;
            //endDate = endDate.AddDays(1);
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join s in _unitOfWork.ScheduleRepository.Query() on c.cas_phy_key equals s.uss_user_id
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                //let hus = SqlFunctions.DateAdd("hh", 1, s.uss_time_to_calc)
                                where c.cas_phy_key != null &&
                                (c.cas_ctp_key == 9 || c.cas_ctp_key == 10 || c.cas_ctp_key == 11)
                                //c.cas_ctp_key != 12 && c.cas_ctp_key != 13 && c.cas_ctp_key != 14 && c.cas_ctp_key != 15 && c.cas_ctp_key != 16 && c.cas_ctp_key != 227
                                //&& c.cas_ctp_key != 228 && c.cas_ctp_key != 163 && c.cas_ctp_key != 164 && c.cas_ctp_key != 220
                                && c.cas_cst_key != 140
                                && c.cas_billing_physician_blast == false
                                //(c.cas_ctp_key = 163 && c.cas_ctp_key != 164 && c.cas_ctp_key != 14 && c.cas_ctp_key != 15 && c.cas_cst_key != 140)
                                && c.cas_physician_assign_date != null
                                //&&  DbFunctions.TruncateTime(s.uss_time_from_calc) <= DbFunctions.TruncateTime(c.cas_physician_assign_date)
                                //&& DbFunctions.TruncateTime(s.uss_time_to_calc) >= DbFunctions.TruncateTime(c.cas_physician_assign_date)
                                && s.uss_time_from_calc <= c.cas_physician_assign_date
                                && s.uss_time_to_calc >= c.cas_physician_assign_date

                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_phy_key)
                                select new { c, s, u });

            if (physicians != null)
                onShiftQuery = onShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            if (caseStatus != null)
                onShiftQuery = onShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    new { onShiftModel.c, onShiftModel.s } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.s.uss_date).Value, false),
                                            Schedule = DbFunctions.Right("00" + SqlFunctions.DateName("hour", onShiftModel.s.uss_time_from_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", onShiftModel.s.uss_time_from_calc.Value), 2)
                                                + " - "
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("hour", onShiftModel.s.uss_time_to_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", onShiftModel.s.uss_time_to_calc.Value), 2),
                                            Physician = onShiftModel.u.LastName + " " + onShiftModel.u.FirstName,
                                            PhysicianKey = onShiftModel.c.cas_phy_key,
                                            uss_time_from_calc = onShiftModel.s.uss_time_from_calc.Value,
                                            uss_time_to_calc = onShiftModel.s.uss_time_to_calc.Value,
                                            //assign_date = onShiftModel.c.cas_physician_assign_date
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    //assign_date = (DateTime)g.Key.assign_date,
                                    Schedule = g.Key.Schedule,
                                    Physician = g.Key.Physician,
                                    PhysicianKey = g.Key.PhysicianKey,
                                    Open = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Open ? 1 : 0),
                                    WaitingToAccept = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0),
                                    Accepted = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0),
                                    Complete = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0),
                                    CC1_StrokeAlert = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0),
                                    CC1_STAT = g.Sum(x => x.c.cas_billing_bic_key == 2 ? 1 : 0),
                                    New = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0),
                                    FU = g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0),
                                    EEG = g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0),
                                    LTM_EEG = g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0),
                                    TC = g.Sum(x => x.c.cas_billing_bic_key == 7 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.c.cas_key > 0 ? 1 : 0),
                                }).ToList();

            #region Get record after shift time 

            ShiftReport shiftReport = new ShiftReport();
            var getRecordList = shiftReport.GetRecords(physicians, startDate, endDate, onShiftCasesList, caseStatus, shiftType);
            onShiftCasesList = getRecordList;
            #endregion



            var offShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                 join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                 where c.cas_phy_key != null
                                 && c.cas_physician_assign_date != null
                                 && c.cas_billing_bic_key == 1
                                 && c.cas_billing_physician_blast
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startDate)
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(endDate)
                                 select new { c, u }).Except(from d in onShiftQuery select new { d.c, d.u });

            if (physicians != null)
                offShiftQuery = offShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            if (caseStatus != null)
                offShiftQuery = offShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            offShiftCasesList = (from offShiftModel in offShiftQuery
                                 group
                                    new { offShiftModel.c } by
                                        new
                                        {
                                            Schedule = "",
                                            Physician = offShiftModel.u.LastName + " " + offShiftModel.u.FirstName,
                                            PhysicianKey = offShiftModel.c.cas_phy_key,
                                        } into g
                                 select new PhysicianBillingByShiftViewModel
                                 {
                                     AssignDate = "Blast",
                                     Schedule = "",
                                     Physician = g.Key.Physician,
                                     PhysicianKey = g.Key.PhysicianKey,
                                     Open = null,
                                     WaitingToAccept = null,
                                     Accepted = null,
                                     Complete = null,
                                     CC1_StrokeAlert = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0),
                                     CC1_STAT = null,
                                     New = null,
                                     FU = null,
                                     EEG = null,
                                     LTM_EEG = null,
                                     TC = null,
                                     Not_Seen = null,
                                     Blast = null,
                                     Total = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0)
                                 }).ToList();

            foreach (var item in onShiftCasesList)
            {
                var isExist = offShiftCasesList.Any(x => x.PhysicianKey == item.PhysicianKey);
                if (!isExist)
                {
                    offShiftCasesList.Add(new PhysicianBillingByShiftViewModel
                    {
                        AssignDate = "Blast",
                        Schedule = "",
                        Physician = item.Physician,
                        PhysicianKey = item.PhysicianKey,
                        Open = null,
                        WaitingToAccept = null,
                        Accepted = null,
                        Complete = null,
                        CC1_StrokeAlert = 0,
                        CC1_STAT = null,
                        New = null,
                        FU = null,
                        EEG = null,
                        LTM_EEG = null,
                        TC = null,
                        Not_Seen = null,
                        Blast = null,
                        Total = 0
                    });
                }
            }

            if (shiftType == ShiftType.All)
            {
                if (onShiftCasesList != null && offShiftCasesList != null)
                {
                    var result = offShiftCasesList.Concat(onShiftCasesList);
                    return result.OrderBy(x => x.PhysicianKey).ThenByDescending(x => x.AssignDate != "Blast").ToList();
                }
                if (onShiftCasesList != null)
                {
                    return onShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
                if (offShiftCasesList != null)
                {
                    return offShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            else if (shiftType == ShiftType.OnShift)
            {
                if (onShiftCasesList != null)
                {
                    return onShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            else if (shiftType == ShiftType.OffShift)
            {
                if (offShiftCasesList != null)
                {
                    return offShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            return null;
        }


        #region Add By husnain 
        public List<PhysicianBillingByShiftViewModel> GetPhysicainBillingByConsult(DataSourceRequest request,
                                                           List<string> physicians,
                                                           DateTime startDate,
                                                           DateTime endDate,
                                                           List<int> caseStatus,
                                                           ShiftType shiftType)
        {
            List<PhysicianBillingByShiftViewModel> casesList = new List<PhysicianBillingByShiftViewModel>();
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            List<PhysicianBillingByShiftViewModel> offShiftCasesList = null;

            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                    //join s in _unitOfWork.ScheduleRepository.Query() on c.cas_phy_key equals s.uss_user_id
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                where c.cas_phy_key != null && (c.cas_billing_bic_key == 3 || c.cas_billing_bic_key == 4 || c.cas_billing_bic_key == 5 || c.cas_billing_bic_key == 6 || c.cas_billing_bic_key == 8)
                                && c.cas_cst_key != 140
                                && c.cas_billing_date_of_consult != null
                                //&& DbFunctions.TruncateTime( s.uss_time_from_calc) <= DbFunctions.TruncateTime(c.cas_billing_date_of_consult)
                                //&& DbFunctions.TruncateTime( s.uss_time_to_calc) >= DbFunctions.TruncateTime( c.cas_billing_date_of_consult)

                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_phy_key)
                                // select new { c, s, u });
                                select new { c, u });

            if (physicians != null)
                onShiftQuery = onShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            if (caseStatus != null)
                onShiftQuery = onShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    //new { onShiftModel.c, onShiftModel.s } by
                                    new { onShiftModel.c } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.c.cas_billing_date_of_consult).Value, false),
                                            //Schedule = DbFunctions.Right("00" + SqlFunctions.DateName("hour", onShiftModel.s.uss_time_from_calc.Value), 2)
                                            //    + ":"
                                            //    + DbFunctions.Right("00" + SqlFunctions.DateName("minute", onShiftModel.s.uss_time_from_calc.Value), 2)
                                            //    + " - "
                                            //    + DbFunctions.Right("00" + SqlFunctions.DateName("hour", onShiftModel.s.uss_time_to_calc.Value), 2)
                                            //    + ":"
                                            //    + DbFunctions.Right("00" + SqlFunctions.DateName("minute", onShiftModel.s.uss_time_to_calc.Value), 2),
                                            Physician = onShiftModel.u.LastName + " " + onShiftModel.u.FirstName,
                                            PhysicianKey = onShiftModel.c.cas_phy_key,
                                            //uss_time_from_calc = onShiftModel.s.uss_time_from_calc.Value,
                                            //uss_time_to_calc = onShiftModel.s.uss_time_to_calc.Value
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Schedule = "",//g.Key.Schedule,
                                    Physician = g.Key.Physician,
                                    PhysicianKey = g.Key.PhysicianKey,
                                    Open = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Open ? 1 : 0),
                                    WaitingToAccept = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0),
                                    Accepted = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0),
                                    //Complete = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0),
                                    Complete = g.Sum(x => x.c.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0),
                                    CC1_StrokeAlert = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0),
                                    CC1_STAT = g.Sum(x => x.c.cas_billing_bic_key == 2 ? 1 : 0),
                                    New = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0),
                                    FU = g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0),
                                    EEG = g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0),
                                    LTM_EEG = g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0),
                                    TC = g.Sum(x => x.c.cas_billing_bic_key == 7 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),//g.Sum(x => x.c.cas_key > 0 ? 1 : 0),
                                }).ToList();



            //var getStrokelist = onShiftCasesList.Where(x => x.CC1_StrokeAlert != 0 || x.CC1_STAT != 0).ToList();
            //onShiftCasesList =  onShiftCasesList.Except(getStrokelist).ToList();
            #region Check Schedule date 
            foreach (var item in onShiftCasesList)
            {
                try
                {
                    DateTime _dt = Convert.ToDateTime(item.AssignDate);
                    var indexOf = onShiftCasesList.IndexOf(item);
                    var getRecord = _unitOfWork.ScheduleRepository.Query().Where(x => x.uss_user_id == item.PhysicianKey && DbFunctions.TruncateTime(_dt) == DbFunctions.TruncateTime(x.uss_date)).FirstOrDefault();
                    if (getRecord != null)
                    {
                        DateTime _fromtime = Convert.ToDateTime(getRecord.uss_time_from_calc);
                        DateTime _totime = Convert.ToDateTime(getRecord.uss_time_to_calc);
                        string timeFrom = _fromtime.ToString("HH:mm");
                        string toTime = _totime.ToString("HH:mm");
                        item.Schedule = timeFrom + " - " + toTime;
                        //item.Schedule = "Off - " +_dt.ToString("hh:mm tt") ;
                    }
                    else
                    {
                        item.Schedule = "--";
                        //item.Schedule = "Off - " + _dt.ToString("hh:mm tt");
                    }
                    //onShiftCasesList[indexOf] = item;
                }
                catch (Exception e)
                {

                }
            }

            #endregion
            var dayshiftList = onShiftCasesList.Where(x => x.Schedule == "07:00 - 19:00").ToList();
            //onShiftCasesList = dayshiftList;

            var offShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                 join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                 where c.cas_phy_key != null
                                 && c.cas_physician_assign_date != null
                                 && c.cas_billing_bic_key == 1
                                 && c.cas_billing_physician_blast
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startDate)
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(endDate)
                                 select new { c, u }).Except(from d in onShiftQuery select new { d.c, d.u });

            if (physicians != null)
                offShiftQuery = offShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            if (caseStatus != null)
                offShiftQuery = offShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            offShiftCasesList = (from offShiftModel in offShiftQuery
                                 group
                                    new { offShiftModel.c } by
                                        new
                                        {
                                            Schedule = "",
                                            Physician = offShiftModel.u.LastName + " " + offShiftModel.u.FirstName,
                                            PhysicianKey = offShiftModel.c.cas_phy_key,
                                        } into g
                                 select new PhysicianBillingByShiftViewModel
                                 {
                                     AssignDate = "Blast",
                                     Schedule = "",
                                     Physician = g.Key.Physician,
                                     PhysicianKey = g.Key.PhysicianKey,
                                     Open = null,
                                     WaitingToAccept = null,
                                     Accepted = null,
                                     Complete = null,
                                     CC1_StrokeAlert = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0),
                                     CC1_STAT = null,
                                     New = null,
                                     FU = null,
                                     EEG = null,
                                     LTM_EEG = null,
                                     TC = null,
                                     Not_Seen = null,
                                     Blast = null,
                                     Total = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0)
                                 }).ToList();

            foreach (var item in onShiftCasesList)
            {
                var isExist = offShiftCasesList.Any(x => x.PhysicianKey == item.PhysicianKey);
                if (!isExist)
                {
                    offShiftCasesList.Add(new PhysicianBillingByShiftViewModel
                    {
                        AssignDate = "Blast",
                        Schedule = "",
                        Physician = item.Physician,
                        PhysicianKey = item.PhysicianKey,
                        Open = null,
                        WaitingToAccept = null,
                        Accepted = null,
                        Complete = null,
                        CC1_StrokeAlert = 0,
                        CC1_STAT = null,
                        New = null,
                        FU = null,
                        EEG = null,
                        LTM_EEG = null,
                        TC = null,
                        Not_Seen = null,
                        Blast = null,
                        Total = 0
                    });
                }
            }

            if (shiftType == ShiftType.All)
            {
                if (onShiftCasesList != null && offShiftCasesList != null)
                {
                    var result = offShiftCasesList.Concat(onShiftCasesList);
                    return result.OrderBy(x => x.PhysicianKey).ThenByDescending(x => x.AssignDate != "Blast").ToList();
                }
                if (onShiftCasesList != null)
                {
                    return onShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
                if (offShiftCasesList != null)
                {
                    return offShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            else if (shiftType == ShiftType.OnShift)
            {
                if (onShiftCasesList != null)
                {
                    return onShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            else if (shiftType == ShiftType.OffShift)
            {
                if (offShiftCasesList != null)
                {
                    return offShiftCasesList.OrderBy(x => x.PhysicianKey).ToList();
                }
            }
            return null;
        }
        #endregion

        public DataSourceResult GetFacilityBillingWithMetrics(DataSourceRequest request, List<Guid> facilities, List<string> physicians, DateTime startDate, DateTime endDate)
        {
            var query = from ca in _unitOfWork.CaseRepository.Query()
                        join billingCode in GetUclData(UclTypes.BillingCode) on ca.cas_billing_bic_key equals billingCode.ucd_key into BillingCodeEntity
                        join identificationType in GetUclData(UclTypes.IdentificationType) on ca.cas_identification_type equals identificationType.ucd_key into IdentTypeEntity
                        join loginDelay in GetUclData(UclTypes.LoginDelay) on ca.cas_billing_lod_key equals loginDelay.ucd_key into LoginDelayEntity
                        // Added by husnain start //
                        join caseType in GetUclData(UclTypes.CaseType) on ca.cas_ctp_key equals caseType.ucd_key into caseTypeEntity
                        join caseStatus in GetUclData(UclTypes.CaseStatus) on ca.cas_cst_key equals caseStatus.ucd_key into caseStatusEntity
                        // husnain code end //

                        join tPADelay in GetUclData(UclTypes.TpaDelay) on ca.cas_metric_tpaDelay_key equals tPADelay.ucd_key into tPADelayEntity
                        from billingCode in BillingCodeEntity.DefaultIfEmpty()
                        from identificationType in IdentTypeEntity.DefaultIfEmpty()
                            // added by husnain
                        from caseType in caseTypeEntity.DefaultIfEmpty()
                        from caseStatus in caseStatusEntity.DefaultIfEmpty()
                            // husnan code end
                        from loginDelay in LoginDelayEntity.DefaultIfEmpty()
                        from tPADelay in tPADelayEntity.DefaultIfEmpty()
                        where (ca.cas_is_active == true &&
                               DbFunctions.TruncateTime(ca.cas_created_date) >= DbFunctions.TruncateTime(startDate) &&
                               DbFunctions.TruncateTime(ca.cas_created_date) <= DbFunctions.TruncateTime(endDate)
                               )
                        select (new { ca, billingCode, caseType, caseStatus, identificationType, loginDelay, tPADelay });
            query = query.OrderBy(x => x.ca.cas_billing_date_of_consult);
            if (facilities != null)
            {
                query = query.Where(m => facilities.Contains(m.ca.cas_fac_key));
            }
            // added by husnain
            if (physicians != null)
            {
                query = query.Where(m => physicians.Contains(m.ca.cas_phy_key));
            }
            // husnain code end
            var result = query.Select(x => new
            {
                x.ca.cas_key,
                x.ca.cas_case_number,
                fac_name = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                cas_created_date = DBHelper.FormatDateTime(x.ca.cas_created_date, false),
                cas_is_nav_blast = x.ca.cas_is_nav_blast ? "Yes" : "No",
                #region billing
                billing_code = x.billingCode != null ? x.billingCode.ucd_title : "",
                // added by husnain
                case_type = x.caseType != null ? x.caseType.ucd_title : "",
                caseStatus = x.caseStatus != null ? x.caseStatus.ucd_title : "",
                Physician = x.ca.AspNetUser2 != null ? x.ca.AspNetUser2.LastName + " " + x.ca.AspNetUser2.FirstName : "",
                PhysicianId = x.ca.AspNetUser2.PhysicianId,
                // husnain code end
                cas_billing_date_of_consult = x.ca.cas_billing_date_of_consult.HasValue
                                    ? DBHelper.FormatDateTime(x.ca.cas_billing_date_of_consult.Value, false)
                                    : "",
                x.ca.cas_billing_patient_name,
                cas_billing_dob = x.ca.cas_billing_dob.HasValue
                                    ? DBHelper.FormatDateTime(x.ca.cas_billing_dob.Value, false)
                                    : "",
                identification_type = x.identificationType != null ? x.identificationType.ucd_title : "",
                x.ca.cas_identification_number,
                x.ca.cas_billing_visit_type,
                billing_followUp_date = x.ca.cas_follow_up_date.HasValue
                                        ? DBHelper.FormatDateTime(x.ca.cas_follow_up_date.Value, false)
                                        : "",
                x.ca.cas_billing_diagnosis,
                x.ca.cas_billing_notes,
                cas_billing_physician_blast = x.ca.cas_billing_physician_blast ? "Yes" : "No",
                on_shift_physician_blast = x.ca.cas_billing_physician_blast_date_est.HasValue && !string.IsNullOrEmpty(x.ca.cas_phy_key) ?
                                                                    DBHelper.CheckPhysBlastOnShift(x.ca.cas_billing_physician_blast_date_est.Value, x.ca.cas_phy_key)
                                                                    : "No",
                #endregion

                #region physician-metric
                cas_metric_is_lastwell_unknown = x.ca.cas_metric_is_lastwell_unknown ? "Yes" : "No",
                cas_metric_lastwell_date = x.ca.cas_metric_lastwell_date.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_lastwell_date.Value)
                                                , true)
                                            : "",
                cas_response_ts_notification = x.ca.cas_response_ts_notification.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_ts_notification.Value)
                                                , true)
                                            : "",
                cas_patient_type = x.ca.cas_patient_type > 0 ? ((PatientType)x.ca.cas_patient_type).ToString() : "",
                cas_metric_door_time = x.ca.cas_metric_door_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_door_time.Value)
                                                , true)
                                            : "",
                cas_metric_stamp_time = x.ca.cas_metric_stamp_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_stamp_time.Value)
                                                , true)
                                            : "",
                cas_response_first_atempt = x.ca.cas_response_first_atempt.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_first_atempt.Value)
                                                , true)
                                            : "",
                cas_metric_video_start_time = x.ca.cas_metric_video_start_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_start_time.Value)
                                                , true)
                                            : "",
                x.ca.cas_metric_symptoms,
                cas_metric_assesment_time = x.ca.cas_metric_assesment_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_assesment_time.Value)
                                                , true)
                                            : "",
                login_delay_reason = x.loginDelay != null ? x.loginDelay.ucd_title : "",
                x.ca.cas_metric_notes,

                cas_metric_last_seen_normal = ((LB2S2CriteriaOptions)x.ca.cas_metric_last_seen_normal).ToString(),
                cas_metric_has_hemorrhgic_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_hemorrhgic_history).ToString(),
                cas_metric_has_recent_anticoagulants = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_recent_anticoagulants).ToString(),
                cas_metric_has_major_surgery_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_major_surgery_history).ToString(),
                cas_metric_has_stroke_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_stroke_history).ToString(),

                cas_metric_tpa_verbal_order_time = x.ca.cas_metric_tpa_verbal_order_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_tpa_verbal_order_time.Value)
                                                , true)
                                            : "",
                cas_metric_tpa_consult = x.ca.cas_metric_tpa_consult ? "Yes" : "No",
                cas_metric_pa_ordertime = x.ca.cas_metric_pa_ordertime.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_pa_ordertime.Value)
                                                , true)
                                            : "",
                cas_metric_needle_time = x.ca.cas_metric_needle_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_needle_time.Value)
                                                , true)
                                            : "",
                cas_metric_weight = x.ca.cas_metric_weight.HasValue ? x.ca.cas_metric_weight.Value.ToString() : "",
                x.ca.cas_metric_weight_unit,
                cas_metric_total_dose = x.ca.cas_metric_total_dose.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_total_dose.Value, 10, 1) : "",
                cas_metric_bolus = x.ca.cas_metric_bolus.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_bolus.Value, 10, 1) : "",
                cas_metric_infusion = x.ca.cas_metric_infusion.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_infusion.Value, 10, 1) : "",
                cas_metric_discard_quantity = x.ca.cas_metric_discard_quantity.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_discard_quantity.Value, 10, 1) : "",
                cas_metric_video_end_time = x.ca.cas_metric_video_end_time.HasValue
                                            ? DBHelper.FormatDateTime(
                                                DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_end_time.Value)
                                                , true)
                                            : "",
                cas_metric_tpaDelay_key = x.tPADelay != null ? x.tPADelay.ucd_description : "",
                x.ca.cas_billing_tpa_delay_notes,
                cas_metric_ct_head_has_no_acture_hemorrhage = x.ca.cas_metric_ct_head_has_no_acture_hemorrhage ? "Yes" : "No",
                cas_metric_ct_head_is_reviewed = x.ca.cas_metric_ct_head_is_reviewed ? "Yes" : "No",
                cas_metric_ct_head_is_not_reviewed = x.ca.cas_metric_ct_head_is_not_reviewed ? "Yes" : "No",
                cas_metric_advance_imaging_cta_head_and_neck = x.ca.cas_metric_advance_imaging_ct_perfusion ? "Yes" : "No",
                cas_metric_advance_imaging_ct_perfusion = x.ca.cas_metric_advance_imaging_ct_perfusion ? "Yes" : "No",
                cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir = x.ca.cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir ? "Yes" : "No",
                cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion = x.ca.cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion ? "Yes" : "No",
                cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus = x.ca.cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus ? "Yes" : "No",
                cas_metric_is_neuro_interventional = x.ca.cas_metric_is_neuro_interventional.HasValue ? x.ca.cas_metric_is_neuro_interventional.Value ? "Yes" : "No" : "",
                cas_metric_discussed_with_neurointerventionalist = x.ca.cas_metric_discussed_with_neurointerventionalist.HasValue ? x.ca.cas_metric_discussed_with_neurointerventionalist.Value ? "Yes" : "No" : "",
                cas_metric_physician_notified_of_thrombolytics = x.ca.cas_metric_physician_notified_of_thrombolytics.HasValue ? x.ca.cas_metric_physician_notified_of_thrombolytics.Value ? "Yes" : "No" : "",
                cas_metric_physician_recommented_consult_neurointerventionalist = x.ca.cas_metric_physician_recommented_consult_neurointerventionalist ? "Yes" : "No",
                #endregion

            });
            return result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetPhysicianBillingWithMetrics(DataSourceRequest request, List<string> physicians, DateTime startDate, DateTime endDate)
        {
            var cases = from ca in _unitOfWork.CaseRepository.Query()
                        join physician in _unitOfWork.ApplicationUsers on ca.cas_phy_key equals physician.Id into physicianEntity
                        join billingCode in GetUclData(UclTypes.BillingCode) on ca.cas_billing_bic_key equals billingCode.ucd_key into BillingCodeEntity
                        join identificationType in GetUclData(UclTypes.IdentificationType) on ca.cas_identification_type equals identificationType.ucd_key into IdentTypeEntity
                        join loginDelay in GetUclData(UclTypes.LoginDelay) on ca.cas_billing_lod_key equals loginDelay.ucd_key into LoginDelayEntity
                        join tpaDelay in GetUclData(UclTypes.TpaDelay) on ca.cas_metric_tpaDelay_key equals tpaDelay.ucd_key into tpaDelayEntity
                        join status in GetUclData(UclTypes.CaseStatus) on ca.cas_cst_key equals status.ucd_key into CaseStatusEntity
                        join type in GetUclData(UclTypes.CaseType) on ca.cas_ctp_key equals type.ucd_key into CaseTypeEntity
                        from physicianInfo in physicianEntity.DefaultIfEmpty()
                        from case_status in CaseStatusEntity.DefaultIfEmpty()
                        from case_type in CaseTypeEntity.DefaultIfEmpty()
                        from billingCode in BillingCodeEntity.DefaultIfEmpty()
                        from identificationType in IdentTypeEntity.DefaultIfEmpty()
                        from loginDelay in LoginDelayEntity.DefaultIfEmpty()
                        from tpaDelay in tpaDelayEntity.DefaultIfEmpty()
                        where (ca.cas_is_active == true &&
                         ca.cas_cst_key == 20 &&
                                DbFunctions.TruncateTime(ca.cas_created_date) >= DbFunctions.TruncateTime(startDate) &&
                                DbFunctions.TruncateTime(ca.cas_created_date) <= DbFunctions.TruncateTime(endDate)
                               )
                        select (new
                        {
                            ca,
                            billingCode,
                            identificationType,
                            loginDelay,
                            tpaDelay,
                            CaseStatus = case_status != null ? case_status.ucd_title : "",
                            CaseType = case_type != null ? case_type.ucd_title : "",
                            physician = physicianInfo.LastName + " " + (string.IsNullOrEmpty(physicianInfo.FirstName) ? "" : physicianInfo.FirstName.Substring(0, 1)),
                        });
            cases = cases.OrderBy(x => x.ca.cas_billing_date_of_consult);
            if (physicians != null)
            {
                cases = cases.Where(m => physicians.Contains(m.ca.cas_phy_key));
            }
            var result = cases.Select(x => new
            {
                x.ca.cas_key,
                x.ca.cas_case_number,
                ctp_name = x.CaseType,
                cst_name = x.CaseStatus,
                x.physician,
                fac_name = x.ca.facility != null ? x.ca.facility.fac_name : "",
                cas_created_date = DBHelper.FormatDateTime(x.ca.cas_created_date, false),
                cas_is_nav_blast = x.ca.cas_is_nav_blast ? "Yes" : "No",

                #region billing
                billing_code = x.billingCode != null ? x.billingCode.ucd_title : "",
                cas_billing_date_of_consult = x.ca.cas_billing_date_of_consult.HasValue ? DBHelper.FormatDateTime(x.ca.cas_billing_date_of_consult.Value, false) : "",
                x.ca.cas_billing_patient_name,
                cas_billing_dob = x.ca.cas_billing_dob.HasValue ? DBHelper.FormatDateTime(x.ca.cas_billing_dob.Value, false) : "",
                identification_type = x.identificationType != null ? x.identificationType.ucd_title : "",
                x.ca.cas_identification_number,
                x.ca.cas_billing_visit_type,
                billing_followUp_date = x.ca.cas_follow_up_date.HasValue ? DBHelper.FormatDateTime(x.ca.cas_follow_up_date.Value, false) : "",
                x.ca.cas_billing_diagnosis,
                x.ca.cas_billing_notes,
                cas_billing_physician_blast = x.ca.cas_billing_physician_blast ? "Yes" : "No",
                on_shift_physician_blast = x.ca.cas_billing_physician_blast_date_est.HasValue && x.ca.cas_phy_key != null ?
                                                                   DBHelper.CheckPhysBlastOnShift(x.ca.cas_billing_physician_blast_date_est.Value, x.ca.cas_phy_key)
                                                                   : "No",
                Timestamp = DBHelper.FormatDateTime(x.ca.cas_created_date, true),
                Navigator = x.ca.cas_created_by_name,
                #endregion

                #region physician-metric
                cas_metric_is_lastwell_unknown = x.ca.cas_metric_is_lastwell_unknown ? "Yes" : "No",
                cas_metric_lastwell_date = x.ca.cas_metric_lastwell_date.HasValue
                                          ? DBHelper.FormatDateTime(
                                              DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_lastwell_date.Value)
                                              , true)
                                          : "",
                cas_response_ts_notification = x.ca.cas_response_ts_notification.HasValue
                                           ? DBHelper.FormatDateTime(
                                               DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_ts_notification.Value)
                                               , true)
                                           : "",
                cas_patient_type = x.ca.cas_patient_type > 0 ? ((PatientType)x.ca.cas_patient_type).ToString() : "",
                cas_metric_door_time = x.ca.cas_metric_door_time.HasValue
                                       ? DBHelper.FormatDateTime(
                                           DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_door_time.Value)
                                           , true)
                                       : "",
                cas_metric_stamp_time = x.ca.cas_metric_stamp_time.HasValue
                                       ? DBHelper.FormatDateTime(
                                           DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_stamp_time.Value)
                                           , true)
                                       : "",
                cas_response_first_atempt = x.ca.cas_response_first_atempt.HasValue
                                           ? DBHelper.FormatDateTime(
                                               DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_first_atempt.Value)
                                               , true)
                                           : "",
                cas_metric_video_start_time = x.ca.cas_metric_video_start_time.HasValue
                                             ? DBHelper.FormatDateTime(
                                                 DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_start_time.Value)
                                                 , true)
                                             : "",
                x.ca.cas_metric_symptoms,
                cas_metric_assesment_time = x.ca.cas_metric_assesment_time.HasValue
                                       ? DBHelper.FormatDateTime(
                                           DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_assesment_time.Value)
                                           , true)
                                       : "",
                login_delay_reason = x.loginDelay != null ? x.loginDelay.ucd_title : "",
                x.ca.cas_metric_notes,

                cas_metric_last_seen_normal = ((LB2S2CriteriaOptions)x.ca.cas_metric_last_seen_normal).ToString(),
                cas_metric_has_hemorrhgic_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_hemorrhgic_history).ToString(),
                cas_metric_has_recent_anticoagulants = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_recent_anticoagulants),
                cas_metric_has_major_surgery_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_major_surgery_history).ToString(),
                cas_metric_has_stroke_history = ((LB2S2CriteriaOptions)x.ca.cas_metric_has_stroke_history).ToString(),

                cas_metric_tpa_verbal_order_time = x.ca.cas_metric_tpa_verbal_order_time.HasValue
                                                   ? DBHelper.FormatDateTime(
                                                       DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_tpa_verbal_order_time.Value)
                                                       , true)
                                                   : "",
                cas_metric_tpa_consult = x.ca.cas_metric_tpa_consult ? "Yes" : "No",
                cas_metric_pa_ordertime = x.ca.cas_metric_pa_ordertime.HasValue
                                       ? DBHelper.FormatDateTime(
                                           DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_pa_ordertime.Value)
                                           , true)
                                       : "",
                cas_metric_needle_time = x.ca.cas_metric_needle_time.HasValue
                                       ? DBHelper.FormatDateTime(
                                           DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_needle_time.Value)
                                           , true)
                                       : "",
                cas_metric_weight = x.ca.cas_metric_weight.HasValue ? x.ca.cas_metric_weight.Value.ToString() : "",
                x.ca.cas_metric_weight_unit,
                cas_metric_total_dose = x.ca.cas_metric_total_dose.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_total_dose, 10, 1) : "",
                cas_metric_bolus = x.ca.cas_metric_bolus.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_bolus, 10, 1) : "",
                cas_metric_infusion = x.ca.cas_metric_infusion.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_infusion, 10, 1) : "",
                cas_metric_discard_quantity = x.ca.cas_metric_discard_quantity.HasValue ? SqlFunctions.StringConvert(x.ca.cas_metric_discard_quantity, 10, 1) : "",
                cas_metric_video_end_time = x.ca.cas_metric_video_end_time.HasValue
                                           ? DBHelper.FormatDateTime(
                                               DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_end_time.Value)
                                               , true)
                                           : "",
                cas_metric_tpaDelay_key = x.tpaDelay != null ? x.tpaDelay.ucd_description : "",
                x.ca.cas_billing_tpa_delay_notes,
                cas_metric_ct_head_has_no_acture_hemorrhage = x.ca.cas_metric_ct_head_has_no_acture_hemorrhage ? "Yes" : "No",
                cas_metric_ct_head_is_reviewed = x.ca.cas_metric_ct_head_is_reviewed ? "Yes" : "No",
                cas_metric_ct_head_is_not_reviewed = x.ca.cas_metric_ct_head_is_not_reviewed ? "Yes" : "No",
                cas_metric_advance_imaging_cta_head_and_neck = x.ca.cas_metric_advance_imaging_ct_perfusion ? "Yes" : "No",
                cas_metric_advance_imaging_ct_perfusion = x.ca.cas_metric_advance_imaging_ct_perfusion ? "Yes" : "No",
                cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir = x.ca.cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir ? "Yes" : "No",
                cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion = x.ca.cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion ? "Yes" : "No",
                cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus = x.ca.cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus ? "Yes" : "No",
                cas_metric_is_neuro_interventional = x.ca.cas_metric_is_neuro_interventional.HasValue ? x.ca.cas_metric_is_neuro_interventional.Value ? "Yes" : "No" : "",
                cas_metric_discussed_with_neurointerventionalist = x.ca.cas_metric_discussed_with_neurointerventionalist.HasValue ? x.ca.cas_metric_discussed_with_neurointerventionalist.Value ? "Yes" : "No" : "",
                cas_metric_physician_notified_of_thrombolytics = x.ca.cas_metric_physician_notified_of_thrombolytics.HasValue ? x.ca.cas_metric_physician_notified_of_thrombolytics.Value ? "Yes" : "No" : "",
                cas_metric_physician_recommented_consult_neurointerventionalist = x.ca.cas_metric_physician_recommented_consult_neurointerventionalist ? "Yes" : "No",
                #endregion

            });
            return result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }


        public DataSourceResult GetQualityMetrics1(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            var cases = from ca in _unitOfWork.CaseRepository.Query()
                        join navigator in _unitOfWork.ApplicationUsers on ca.cas_created_by equals navigator.Id
                        into navigatorEntity


                        join code in GetUclData(UclTypes.BillingCode) on ca.cas_billing_bic_key equals code.ucd_key
                        into billingCodeEntity

                        join type in GetUclData(UclTypes.CaseType) on ca.cas_ctp_key equals type.ucd_key
                        into caseTypeEntity

                        join status in GetUclData(UclTypes.CaseStatus) on ca.cas_cst_key equals status.ucd_key
                        into caseStatusEntity

                        from navigatorInfo in navigatorEntity.DefaultIfEmpty()

                            /*
                            join waitingToAccept in _unitOfWork.CaseAssignHistoryRepository.Query() on ca.cas_key equals waitingToAccept.cah_cas_key
                            into waitingAcceptEntity

                            join accepted in _unitOfWork.CaseAssignHistoryRepository.Query() on ca.cas_key equals accepted.cah_cas_key
                            into acceptedEntity


                            from waitingAcceptInfo in waitingAcceptEntity.Where(x => x.cah_action == "Waiting to Accept").OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from acceptedInfo in acceptedEntity.Where(x => x.cah_action == "Accepted").OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            */


                            /*
                            from waitingToAccept in _unitOfWork.CaseAssignHistoryRepository.Query().Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from accepted in _unitOfWork.CaseAssignHistoryRepository.Query().Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            */

                        from waitingToAccept in _unitOfWork.CaseAssignHistoryRepository.Query()

                            //from waitingToAccept in _unitOfWork.CaseAssignHistoryRepository.Query().Where(x => x.cah_cas_key == ca.cas_key).Take(1)
                            /* .Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date)  .Take(1).DefaultIfEmpty() */

                            /*

                            join waitingToAccept in _unitOfWork.CaseAssignHistoryRepository.Query().Where(x => x.cah_action == "Waiting to Accept") on ca.cas_key equals waitingToAccept.cah_cas_key
                            into waitingAcceptEntity

                            from waitingAcceptInfo in waitingAcceptEntity.OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            */
                            ////////////////////////////////////////////////////////////////////////

                            /*
                            //join cashis in _unitOfWork.CaseAssignHistoryRepository.Query() on ca.cas_key equals cashis.cah_cas_key
                            //into cashisEntity

                            //from waitingAcceptInfo in cashisEntity.Where(x => x.cah_action == "Waiting to Accept").OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            //from acceptedInfo in cashisEntity.Where(x => x.cah_action == "Accepted").OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            */

                        from billing_code in billingCodeEntity.DefaultIfEmpty()
                        from case_type in caseTypeEntity.DefaultIfEmpty()
                        from case_status in caseStatusEntity.DefaultIfEmpty()
                        where ca.cas_is_active == true

                        select (new
                        {
                            ca,
                            navigator = navigatorInfo.LastName + " " + (string.IsNullOrEmpty(navigatorInfo.FirstName) ? "" : navigatorInfo.FirstName.Substring(0, 1)),
                            //waiting_to_accept_date = waitingAcceptInfo.cah_created_date,
                            //acceptedInfo_date = acceptedInfo.cah_created_date,

                            waiting_to_accept_date = DateTime.Now, // waitingToAccept.cah_created_date,
                            acceptedInfo_date = DateTime.Now, //  accepted.cah_created_date,

                            billingCode = billing_code != null ? billing_code.ucd_title : "",
                            caseType = case_type != null ? case_type.ucd_title : "",
                            caseStatus = case_status != null ? case_status.ucd_title : ""
                        });

            #region ----- Filters -----

            if (model.IncludeTime)
            {
                cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
            }
            else
            {
                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
            }

            if (!string.IsNullOrEmpty(facilityAdminId))
            {
                var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                         .Select(m => m.Facility).ToList();

                cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
            }

            if (model.WorkFlowType != null)
                cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
            if (model.CaseStatus != null)
                cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
            if (model.CaseType != null)
                cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
            if (model.BillingCode != null)
                cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
            if (model.Physicians != null)
                cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));
            if (model.Facilities != null && model.Facilities.Count > 0)
            {
                if (model.Facilities[0] != Guid.Empty)
                    cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
            }

            #endregion

            #region ----- Calculations -----

            var query = cases.Select(x => new
            {
                id = x.ca.cas_key,
                created_date = x.ca.cas_created_date,
                case_number = x.ca.cas_case_number,
                patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                x.navigator,
                start_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_ts_notification), true),
                stamp_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_stamp_time), true),
                time_waiting_to_accept = DBHelper.FormatDateTime(x.waiting_to_accept_date, true),
                time_accepted = DBHelper.FormatDateTime(x.acceptedInfo_date, true),
                physicians = DBHelper.GetPhysiciansInitialsCount(x.ca.cas_history_physician_initial_cal),
                physicianName = DBHelper.GetUserFullName(x.ca.cas_phy_key),//x.ca.AspNetUser2 != null ? x.ca.AspNetUser2.LastName + " " + x.ca.AspNetUser2.FirstName : "",
                x.billingCode,
                x.caseType,
                x.caseStatus,

                handle_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),
                handle_time_cmp = DBHelper.DiffSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),

                assignment_time = DBHelper.FormatSeconds(x.waiting_to_accept_date, x.acceptedInfo_date),
                assignment_time_cmp = DBHelper.DiffSeconds(x.waiting_to_accept_date, x.acceptedInfo_date),

                start_accepted = x.ca.cas_response_ts_notification.HasValue && x.acceptedInfo_date != null
                               ? DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value) > x.acceptedInfo_date
                                   ? DBHelper.FormatSeconds(DbFunctions.DiffSeconds(x.acceptedInfo_date, DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value)))
                                   : DBHelper.FormatSeconds(DbFunctions.DiffSeconds(DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value), x.acceptedInfo_date))
                               : "",
                start_accepted_cmp = x.ca.cas_response_ts_notification.HasValue && x.acceptedInfo_date != null
                               ? DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value) > x.acceptedInfo_date
                                   ? DbFunctions.DiffSeconds(x.acceptedInfo_date, DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value))
                                   : DbFunctions.DiffSeconds(DBHelper.ConvertToFacilityTimeZone(Settings.DefaultTimeZone, x.ca.cas_response_ts_notification.Value), x.acceptedInfo_date)
                               : 0,

                last_known_well = x.ca.cas_metric_is_lastwell_unknown ? "Unknown" : DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_lastwell_date), true),
                workflow_type = x.ca.cas_ctp_key == (int)CaseType.StrokeAlert ? ((PatientType)x.ca.cas_patient_type).ToString() : "",
                arrival_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_door_time), true),
                first_login_attempt = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_first_atempt), true),
                video_start_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_start_time), true),
                video_end_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_end_time), true),
                tpa_verbel_order_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_tpa_verbal_order_time), true),
                tpa_cpoe_order_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_pa_ordertime), true),
                needle_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_needle_time), true),

                bedside_response_time = DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                bedside_response_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),

                login_handletime = DBHelper.FormatSeconds(x.ca.cas_metric_video_start_time, x.ca.cas_response_first_atempt),
                login_handletime_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_video_start_time, x.ca.cas_response_first_atempt),

                on_screen_time = DBHelper.FormatSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                on_screen_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),

                activation_time = DBHelper.FormatSeconds(x.ca.cas_metric_door_time, x.ca.cas_response_ts_notification),
                activation_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_door_time, x.ca.cas_response_ts_notification),

                medical_descision_making_time = DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),
                medical_descision_making_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),

                arrival_needle_time = DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                arrival_needle_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),

                physician_MDM = DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),
                physician_MDM_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),

                tPA_administration_time = DbFunctions.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_needle_time),
                tPA_administration_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_needle_time),

                ts_response_time = DbFunctions.DiffSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification),
                ts_response_time_cmp = DBHelper.DiffSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification),

                order_to_needle = DbFunctions.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),
                order_to_needle_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),

                verbal_order_to_ocopr_order = DbFunctions.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),
                verbal_order_to_ocopr_order_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),


                cpoe_order_to_needle = DbFunctions.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time),
                cpoe_order_to_needle_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time),
            });

            #endregion

            #region ----- Advanced Search -----

            //Handle Time
            if (model.AdvanceSearchCriteria.HandleTime != null)
            {
                var time = model.AdvanceSearchCriteria.HandleTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.HandleTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("handle_time_cmp");

                query = query.WhereCriteria("handle_time_cmp", time, comparisonOperator);
            }
            //Bedside Response Time
            if (model.AdvanceSearchCriteria.BedsideResponseTime != null)
            {
                var time = model.AdvanceSearchCriteria.BedsideResponseTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.BedsideResponseTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("bedside_response_time_cmp");

                query = query.WhereCriteria("bedside_response_time_cmp", time, comparisonOperator);
            }
            //LogIn Handle Time
            if (model.AdvanceSearchCriteria.LogInHandleTime != null)
            {
                var time = model.AdvanceSearchCriteria.LogInHandleTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.LogInHandleTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("login_handletime_cmp");

                query = query.WhereCriteria("login_handletime_cmp", time, comparisonOperator);
            }
            //On Screen Time
            if (model.AdvanceSearchCriteria.OnScreenTime != null)
            {
                var time = model.AdvanceSearchCriteria.OnScreenTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.OnScreenTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("on_screen_time_cmp");

                query = query.WhereCriteria("on_screen_time_cmp", time, comparisonOperator);
            }
            //Assignment Time
            if (model.AdvanceSearchCriteria.AssignmentTime != null)
            {
                var time = model.AdvanceSearchCriteria.AssignmentTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.AssignmentTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("assignment_time_cmp");

                query = query.WhereCriteria("assignment_time_cmp", time, comparisonOperator);
            }
            //Arrival To Needle Time
            if (model.AdvanceSearchCriteria.ArrivalToNeedleTime != null)
            {
                var time = model.AdvanceSearchCriteria.ArrivalToNeedleTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.ArrivalToNeedleTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("arrival_needle_time_cmp");

                query = query.WhereCriteria("arrival_needle_time_cmp", time, comparisonOperator);
            }
            //Activation Time
            if (model.AdvanceSearchCriteria.ActivationTime != null)
            {
                var time = model.AdvanceSearchCriteria.ActivationTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.ActivationTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("activation_time_cmp");

                query = query.WhereCriteria("activation_time_cmp", time, comparisonOperator);
            }
            //Physician MDM
            if (model.AdvanceSearchCriteria.PhysicianMDM != null)
            {
                var time = model.AdvanceSearchCriteria.PhysicianMDM.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.PhysicianMDM.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("physician_MDM_cmp");

                query = query.WhereCriteria("physician_MDM_cmp", time, comparisonOperator);
            }
            //tap Administrator Time
            if (model.AdvanceSearchCriteria.TPAAdministratorTime != null)
            {
                var time = model.AdvanceSearchCriteria.TPAAdministratorTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.TPAAdministratorTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("tPA_administration_time_cmp");

                query = query.WhereCriteria("tPA_administration_time_cmp", time, comparisonOperator);
            }
            //TS Response Time
            if (model.AdvanceSearchCriteria.TSResponseTime != null)
            {
                var time = model.AdvanceSearchCriteria.TSResponseTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.TSResponseTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("ts_response_time_cmp");

                query = query.WhereCriteria("ts_response_time_cmp", time, comparisonOperator);
            }
            //Alteplase early mix decision To CPOE Order
            if (model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder != null)
            {
                var time = model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("verbal_order_to_ocopr_order_cmp");

                query = query.WhereCriteria("verbal_order_to_ocopr_order_cmp", time, comparisonOperator);
            }
            //CPOE Order To Needle
            if (model.AdvanceSearchCriteria.CPOEOrderToNeedle != null)
            {
                var time = model.AdvanceSearchCriteria.CPOEOrderToNeedle.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.CPOEOrderToNeedle.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("cpoe_order_to_needle_cmp");

                query = query.WhereCriteria("cpoe_order_to_needle_cmp", time, comparisonOperator);
            }
            //Start To Accept Time
            if (model.AdvanceSearchCriteria.StartAcceptTime != null)
            {
                var time = model.AdvanceSearchCriteria.StartAcceptTime.TimeToEvaluate.TotalSeconds.ToInt();
                var comparisonOperator = model.AdvanceSearchCriteria.StartAcceptTime.ComparisonOperator.ToString();
                //var propertyInfo = (((PropertyInfo[])query.GetType().GetGenericArguments()[0].GetRuntimeProperties())[0]).PropertyType.GetTypeInfo().GetProperty("start_accepted_cmp");

                query = query.WhereCriteria("start_accepted_cmp", time, comparisonOperator);
            }

            #endregion

            query = query.OrderBy(m => m.created_date);
            return query.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public DataSourceResult GetQualityMetrics(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()
                            from reasonFortPADelay in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.TpaDelay && ca.cas_metric_tpaDelay_key == x.ucd_key).DefaultIfEmpty()
                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text),
                                reasonFortPADelay = reasonFortPADelay != null ? reasonFortPADelay.ucd_description : ""
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }

                if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();

                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                if (model.WorkFlowType != null)
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }



                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion

                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var EDStay = PatientType.SymptomOnsetDuringEDStay.ToInt();
                var canceledCASE = CaseStatus.Cancelled.ToInt();
                var statentitytype = EntityTypes.StateAlertTemplate.ToInt();
                var case_template_stroke_neuro_tpa = EntityTypes.NeuroStrokeAlertTemplateTpa.ToInt();
                var case_template_telestroke_notpa = EntityTypes.StrokeAlertTemplateNoTpaTeleStroke.ToInt();
                var case_template_stroke_tpa = EntityTypes.StrokeAlertTemplateTpa.ToInt();
                var case_template_stroke_notpa = EntityTypes.StrokeAlertTemplateNoTpa.ToInt();
                Nullable<int> value = null;
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    created_date = x.ca.cas_created_date,
                    //ticket 528, add callback response time, present in STAT Consult
                    callback_response_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_callback_response_time.Value), true),
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    identification_number = x.ca.cas_identification_number,
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,
                    start_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_ts_notification), true),
                    stamp_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_stamp_time), true),

                    time_waiting_to_accept = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.waiting_to_accept_date), true),
                    time_accepted = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.acceptedInfo_date), true),


                    physicians = DBHelper.GetPhysiciansInitialsCount(x.ca.cas_history_physician_initial_cal),
                    physicianName = DBHelper.GetUserFullName(x.ca.cas_phy_key),//x.ca.AspNetUser2 != null ? x.ca.AspNetUser2.LastName + " " + x.ca.AspNetUser2.FirstName : "",
                    x.billingCode,
                    x.caseType,
                    x.caseStatus,
                    x.callerSource,

                    handle_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),
                    handle_time_cmp = DBHelper.DiffSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),

                    assignment_time = DBHelper.FormatSeconds(x.waiting_to_accept_date, x.acceptedInfo_date),
                    assignment_time_cmp = DBHelper.DiffSeconds(x.waiting_to_accept_date, x.acceptedInfo_date),

                    start_accepted = x.ca.cas_response_ts_notification.HasValue && x.acceptedInfo_date != null ? DBHelper.FormatSeconds(x.ca.cas_response_ts_notification.Value, x.acceptedInfo_date) : "00:00:00",
                    start_accepted_cmp = x.ca.cas_response_ts_notification.HasValue && x.acceptedInfo_date != null
                                   ? x.ca.cas_response_ts_notification.Value > x.acceptedInfo_date
                                       ? DbFunctions.DiffSeconds(x.acceptedInfo_date, x.ca.cas_response_ts_notification.Value)
                                       : DbFunctions.DiffSeconds(x.ca.cas_response_ts_notification.Value, x.acceptedInfo_date)
                                   : 0,

                    last_known_well = x.ca.cas_metric_is_lastwell_unknown ? "Unknown" : DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_lastwell_date), true),
                    workflow_type = x.ca.cas_ctp_key == (int)CaseType.StrokeAlert ? ((PatientType)x.ca.cas_patient_type).ToString() == "SymptomOnsetDuringEDStay" ? "Symptom Onset During ED Stay" : ((PatientType)x.ca.cas_patient_type).ToString() : "",
                    arrival_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_door_time), true),
                    first_login_attempt = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_response_first_atempt), true),
                    video_start_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_start_time), true),
                    video_end_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_video_end_time), true),
                    tpa_verbel_order_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_tpa_verbal_order_time), true),
                    tpa_cpoe_order_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_pa_ordertime), true),
                    needle_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_needle_time), true),

                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    bedside_response_time_cmp = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),


                    login_handletime = x.ca.cas_metric_video_start_time < x.ca.cas_response_first_atempt ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_video_start_time, x.ca.cas_response_first_atempt),
                    login_handletime_cmp = x.ca.cas_metric_video_start_time < x.ca.cas_response_first_atempt ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_video_start_time, x.ca.cas_response_first_atempt),

                    on_screen_time = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                    on_screen_time_cmp = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),

                    activation_time = (!x.ca.cas_metric_door_time.HasValue) || (!x.ca.cas_response_ts_notification.HasValue) ? "" : (x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time) ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time),
                    activation_time_cmp = (!x.ca.cas_metric_door_time.HasValue) || (!x.ca.cas_response_ts_notification.HasValue) || (x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time) ? 0 : DBHelper.DiffSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time),

                    medical_descision_making_time = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),
                    medical_descision_making_time_cmp = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_video_start_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),

                    arrival_needle_time = x.ca.cas_patient_type == EDStay && x.ca.cas_metric_symptom_onset_during_ed_stay_time.HasValue && x.ca.cas_metric_needle_time.HasValue ?
                    x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time) :
                    x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),

                    arrival_needle_time_cmp = x.ca.cas_patient_type == EDStay && x.ca.cas_metric_symptom_onset_during_ed_stay_time.HasValue && x.ca.cas_metric_needle_time.HasValue ? x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time) :
                      x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),

                    symptom_needle_time = x.ca.cas_patient_type == EDStay && x.ca.cas_metric_symptom_onset_during_ed_stay_time.HasValue && x.ca.cas_metric_needle_time.HasValue ?
                    x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time) :
                    "",

                    symptom_needle_time_cmp = x.ca.cas_metric_symptom_onset_during_ed_stay_time.HasValue && x.ca.cas_metric_needle_time.HasValue ? x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time) :
                    0,

                    physician_MDM = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),
                    physician_MDM_cmp = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_video_start_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),

                    tPA_administration_time = DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_needle_time),
                    tPA_administration_time_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_needle_time),

                    ts_response_time = x.ca.cas_response_first_atempt < x.ca.cas_response_ts_notification ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification),
                    ts_response_time_cmp = x.ca.cas_response_first_atempt < x.ca.cas_response_ts_notification ? 0 : DBHelper.DiffSeconds(x.ca.cas_response_first_atempt, x.ca.cas_response_ts_notification),

                    last_known_well_to_needle_time = x.ca.cas_metric_needle_time.HasValue && x.ca.cas_metric_lastwell_date.HasValue ? x.ca.cas_metric_needle_time < x.ca.cas_metric_lastwell_date ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_lastwell_date) : "",
                    last_known_well_to_needle_time_cmp = x.ca.cas_metric_needle_time < x.ca.cas_metric_lastwell_date ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_lastwell_date),

                    order_to_needle = DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),
                    order_to_needle_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),

                    verbal_order_to_ocopr_order = DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),
                    verbal_order_to_ocopr_order_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),

                    video_start_to_tpa_verbal_order = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_video_start_time),
                    video_start_to_needle = x.ca.cas_metric_needle_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_video_start_time),
                    nihss_to_verbal_tpa_order = x.ca.cas_metric_tpa_verbal_order_time < x.ca.cas_metric_assesment_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_tpa_verbal_order_time, x.ca.cas_metric_assesment_time),
                    nihss_to_needle = x.ca.cas_metric_needle_time < x.ca.cas_metric_assesment_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_assesment_time),

                    cpoe_order_to_needle = DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time),
                    cpoe_order_to_needle_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_needle_time),
                    tPADelayNotes = x.ca.cas_billing_tpa_delay_notes,
                    loginDelayNotes = x.ca.cas_metric_notes,
                    date_of_consult = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_billing_date_of_consult.Value), false),
                    tpa = x.ca.cas_metric_tpa_consult ? "Yes" : "No",
                    login_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_response_first_atempt ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_response_first_atempt),


                    cas_metric_assesment_time_est = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(x.ca.facility.fac_timezone, x.ca.cas_metric_assesment_time), true),
                    eAlert = x.ca.cas_is_ealert ? "Yes" : "No",

                    nihss_totalnum = x.ca.TemplateEntityType == null || x.ca.TemplateEntityType == 0 ? x.ca.case_template_statconsult.ctt_nihss_totalscore.HasValue ? x.ca.case_template_statconsult.ctt_nihss_totalscore.Value : x.ca.cas_metric_tpa_consult == true && x.ca.case_template_stroke_neuro_tpa.csn_nihss_totalscore.HasValue ? x.ca.case_template_stroke_neuro_tpa.csn_nihss_totalscore.Value : x.ca.cas_metric_tpa_consult == false && x.ca.case_template_telestroke_notpa.ctt_nihss_totalscore.HasValue ? x.ca.case_template_telestroke_notpa.ctt_nihss_totalscore.Value : x.ca.cas_metric_tpa_consult == true && x.ca.case_template_stroke_tpa.cts_nihss_totalscore.HasValue ? x.ca.case_template_stroke_tpa.cts_nihss_totalscore.Value : x.ca.cas_metric_tpa_consult == false && x.ca.case_template_stroke_notpa.ctn_nihss_totalscore.HasValue ? x.ca.case_template_stroke_notpa.ctn_nihss_totalscore.Value : x.ca.cas_ctp_key == 9 || x.ca.cas_ctp_key == 10 ? 0 : value : x.ca.TemplateEntityType == statentitytype ? x.ca.case_template_statconsult.ctt_nihss_totalscore : x.ca.TemplateEntityType == case_template_stroke_neuro_tpa ? x.ca.case_template_stroke_neuro_tpa.csn_nihss_totalscore : x.ca.TemplateEntityType == case_template_telestroke_notpa ? x.ca.case_template_telestroke_notpa.ctt_nihss_totalscore : x.ca.TemplateEntityType == case_template_stroke_tpa ? x.ca.case_template_stroke_tpa.cts_nihss_totalscore : x.ca.TemplateEntityType == case_template_stroke_notpa ? x.ca.case_template_stroke_notpa.ctn_nihss_totalscore : value,
                    neurointerventional_case = x.ca.cas_metric_is_neuro_interventional.HasValue ? x.ca.cas_metric_is_neuro_interventional.Value == true ? "Yes" : x.ca.cas_metric_is_neuro_interventional.Value == false ? "No" : "" : "",
                    patient_name = x.ca.cas_patient,
                    NIHSS_cannot_patient_status = x.ca.cas_nihss_cannot_completed,
                    Physician_Blast = x.ca.cas_billing_physician_blast ? "Yes" : "No",
                    x.reasonFortPADelay,
                    ts_account_id = x.ca.facility.fac_ts_account_ID,
                });

                #endregion

                #region ----- Advanced Search -----

                //Handle Time
                if (model.AdvanceSearchCriteria.HandleTime != null)
                {
                    var time = model.AdvanceSearchCriteria.HandleTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.HandleTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("handle_time_cmp", time, comparisonOperator);
                }
                //Bedside Response Time
                if (model.AdvanceSearchCriteria.BedsideResponseTime != null)
                {
                    var time = model.AdvanceSearchCriteria.BedsideResponseTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.BedsideResponseTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("bedside_response_time_cmp", time, comparisonOperator);
                }
                //LogIn Handle Time
                if (model.AdvanceSearchCriteria.LogInHandleTime != null)
                {
                    var time = model.AdvanceSearchCriteria.LogInHandleTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.LogInHandleTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("login_handletime_cmp", time, comparisonOperator);
                }
                //On Screen Time
                if (model.AdvanceSearchCriteria.OnScreenTime != null)
                {
                    var time = model.AdvanceSearchCriteria.OnScreenTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.OnScreenTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("on_screen_time_cmp", time, comparisonOperator);
                }
                //Assignment Time
                if (model.AdvanceSearchCriteria.AssignmentTime != null)
                {
                    var time = model.AdvanceSearchCriteria.AssignmentTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.AssignmentTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("assignment_time_cmp", time, comparisonOperator);
                }
                //Arrival To Needle Time
                if (model.AdvanceSearchCriteria.ArrivalToNeedleTime != null)
                {
                    var time = model.AdvanceSearchCriteria.ArrivalToNeedleTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.ArrivalToNeedleTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("arrival_needle_time_cmp", time, comparisonOperator);
                }
                //Activation Time
                if (model.AdvanceSearchCriteria.ActivationTime != null)
                {
                    var time = model.AdvanceSearchCriteria.ActivationTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.ActivationTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("activation_time_cmp", time, comparisonOperator);
                }
                //Physician MDM
                if (model.AdvanceSearchCriteria.PhysicianMDM != null)
                {
                    var time = model.AdvanceSearchCriteria.PhysicianMDM.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.PhysicianMDM.ComparisonOperator.ToString();

                    query = query.WhereCriteria("physician_MDM_cmp", time, comparisonOperator);
                }
                //tap Administrator Time
                if (model.AdvanceSearchCriteria.TPAAdministratorTime != null)
                {
                    var time = model.AdvanceSearchCriteria.TPAAdministratorTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.TPAAdministratorTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("tPA_administration_time_cmp", time, comparisonOperator);
                }
                //TS Response Time
                if (model.AdvanceSearchCriteria.TSResponseTime != null)
                {
                    var time = model.AdvanceSearchCriteria.TSResponseTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.TSResponseTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("ts_response_time_cmp", time, comparisonOperator);
                }
                //Alteplase early mix decision To CPOE Order
                if (model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder != null)
                {
                    var time = model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.VerbalOrderToCPOEOrder.ComparisonOperator.ToString();

                    query = query.WhereCriteria("verbal_order_to_ocopr_order_cmp", time, comparisonOperator);
                }
                //CPOE Order To Needle
                if (model.AdvanceSearchCriteria.CPOEOrderToNeedle != null)
                {
                    var time = model.AdvanceSearchCriteria.CPOEOrderToNeedle.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.CPOEOrderToNeedle.ComparisonOperator.ToString();

                    query = query.WhereCriteria("cpoe_order_to_needle_cmp", time, comparisonOperator);
                }
                //Start To Accept Time
                if (model.AdvanceSearchCriteria.StartAcceptTime != null)
                {
                    var time = model.AdvanceSearchCriteria.StartAcceptTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.StartAcceptTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("start_accepted_cmp", time, comparisonOperator);
                }
                //symptom to needle time
                if (model.AdvanceSearchCriteria.SymptomsToNeedleTime != null)
                {
                    var time = model.AdvanceSearchCriteria.SymptomsToNeedleTime.TimeToEvaluate.TotalSeconds.ToInt();
                    var comparisonOperator = model.AdvanceSearchCriteria.SymptomsToNeedleTime.ComparisonOperator.ToString();

                    query = query.WhereCriteria("symptom_needle_time_cmp", time, comparisonOperator);
                }

                #endregion

                query = query.OrderBy(m => m.created_date);
                return query.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }

        #endregion

        #region facility Settings Report by Axim
        public DataSourceResult GetFacilitysettings(DataSourceRequest request, List<string> facilities, int? system, int? region, List<string> states, int? coverageType, List<string> serviceType, bool? active, bool? goLive)
        {
            var facilitiesList = (from u in _unitOfWork.FacilityRepository.Query()
                                  join k in _unitOfWork.FacilityContractRepository.Query() on u.fac_key equals k.fct_key into facilitiess
                                  from k in facilitiess.DefaultIfEmpty()
                                  join n in GetUclData(UclTypes.State) on u.fac_stt_key equals n.ucd_key into FacilityStates
                                  from state in FacilityStates.DefaultIfEmpty()
                                  join l in GetUclData(UclTypes.Regional) on u.fac_ucd_region_key equals l.ucd_key into regions
                                  from regiond in regions.DefaultIfEmpty()
                                  join s in GetUclData(UclTypes.System) on u.fac_ucd_key_system equals s.ucd_key into systems
                                  from systemd in systems.DefaultIfEmpty()
                                  join t in GetUclData(UclTypes.CoverageType) on u.facility_contract.fct_cvr_key equals t.ucd_key into coverageTypes
                                  from cvrTyp in coverageTypes.DefaultIfEmpty()
                                  join service in _unitOfWork.FacilityContractServiceRepository.Query() on u.fac_key equals service.fcs_fct_key into servicesss
                                  from srv in servicesss.DefaultIfEmpty()
                                  join w in GetUclData(UclTypes.ServiceType) on srv.fcs_srv_key equals w.ucd_key into srvType
                                  from srsT in srvType.DefaultIfEmpty()
                                  orderby u.fac_name
                                  select new
                                  {
                                      id = u.fac_key,
                                      facility = u.fac_name,
                                      systems = systemd != null ? systemd.ucd_title : "",
                                      systemID = systemd != null ? systemd.ucd_key : 0,
                                      regions = regiond != null ? regiond.ucd_title : "",
                                      regionID = regiond != null ? regiond.ucd_key : 0,
                                      states = state != null ? state.ucd_title : "",
                                      stateID = state != null ? state.ucd_key : 0,
                                      coverage_Type = cvrTyp != null ? cvrTyp.ucd_title : "",
                                      coverageID = cvrTyp != null ? cvrTyp.ucd_key : 0,
                                      service_Type = u.facility_contract.fct_service_calc,//srsT != null ? srsT.ucd_title : "",
                                      serviceID = srsT != null ? srsT.ucd_key : 0,
                                      isActive = u.fac_is_active,
                                      isgoLive = u.fac_go_live,
                                  });

            if (facilities != null)
                facilitiesList = facilitiesList.Where(x => facilities.Contains(x.id.ToString()));
            if (system != null)
                facilitiesList = facilitiesList.Where(x => x.systemID == system);
            if (region != null)
                facilitiesList = facilitiesList.Where(x => x.regionID == region);
            if (states != null)
                facilitiesList = facilitiesList.Where(x => states.Contains(x.stateID.ToString()));
            if (serviceType != null)
                facilitiesList = facilitiesList.Where(x => serviceType.Contains(x.serviceID.ToString()));
            if (coverageType != null)
                facilitiesList = facilitiesList.Where(x => x.coverageID == coverageType);
            if (active != null)
                facilitiesList = facilitiesList.Where(x => x.isActive == active);
            if (goLive != null)
                facilitiesList = facilitiesList.Where(x => x.isgoLive == goLive);
            //facilitiesList = facilitiesList.OrderBy(x => x.facility);
            var fac_list = facilitiesList.Select(x => new { x.id, x.facility, x.systems, x.regions, x.states, x.coverage_Type, x.service_Type, x.isActive, x.isgoLive }).Distinct();
            fac_list = fac_list.OrderBy(x => x.facility);

            return fac_list.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        #endregion
        
        #region User Presence Report by Axim
        public DataSourceResult GetUserPresence(DataSourceRequest request, List<string> UserStatus, DateTime startTime, DateTime endTime, string ReportType)
        {
            List<UserPresenceListings> _list = new List<UserPresenceListings>();
            List<UserPresenceListings> UsersList = new List<UserPresenceListings>();
            UserPresenceListings obj;
            //var status = String.Join(",", UserStatus);
            if (UserStatus != null)
            {
                while (startTime <= endTime)
                {
                    var SpStartTime = startTime;
                    var SpEndTime = startTime.AddDays(1);
                    for (int i = 0; i < UserStatus.Count; i++)
                    {
                        var status = UserStatus[i].ToString();
                        UsersList = _unitOfWork.SqlQuery<UserPresenceListings>(string.Format("Exec usp_UserPresence_Report @starttime = '{0}',@endtime = '{1}',@status = '{2}',@reportType='{3}'", SpStartTime, SpEndTime, status, ReportType)).ToList();
                        foreach (var item in UsersList)
                        {
                            obj = new UserPresenceListings();
                            obj.Id = item.Id;
                            obj.date = item.date;
                            obj.CreatedDate = item.CreatedDate;
                            obj.Physician = item.Physician;
                            obj.Available = item.Available;
                            obj.AvailableS = string.Format("{0:D2}:{1:D2}:{2:D2}", (item.Available / 3600), ((item.Available % 3600) / 60), item.Available % 60);
                            obj.TPA = item.TPA;
                            obj.TPAS = string.Format("{0:D2}:{1:D2}:{2:D2}", item.TPA / 3600, (item.TPA % 3600) / 60, item.TPA % 60);
                            obj.StrokeAlert = item.StrokeAlert;
                            obj.StrokeAlertS = string.Format("{0:D2}:{1:D2}:{2:D2}", item.StrokeAlert / 3600, (item.StrokeAlert % 3600) / 60, item.StrokeAlert % 60);
                            obj.Rounding = item.Rounding;
                            obj.RoundingS = string.Format("{0:D2}:{1:D2}:{2:D2}", item.Rounding / 3600, (item.Rounding % 3600) / 60, item.Rounding % 60);
                            obj.STATConsult = item.STATConsult;
                            obj.STATConsultS = string.Format("{0:D2}:{1:D2}:{2:D2}", item.STATConsult / 3600, (item.STATConsult % 3600) / 60, item.STATConsult % 60);
                            obj.Break = item.Break;
                            obj.BreakS = string.Format("{0:D2}:{1:D2}:{2:D2}", item.Break / 3600, (item.Break % 3600) / 60, item.Break % 60);
                            _list.Add(obj);
                        }
                    }

                    startTime = startTime.AddDays(1);
                }
            }
            var Final_list = _list.Select(x => new
            {
                x.Id,
                x.date,
                x.CreatedDate,
                x.Physician,
                x.AvailableS,
                x.TPAS,
                x.StrokeAlertS,
                x.RoundingS,
                x.STATConsultS,
                x.BreakS
            }).AsQueryable();

            return Final_list.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public DataSourceResult GetUserPresenceGraph(DataSourceRequest request, string UserStatus, DateTime startTime)
        {
           
            var UsersList = _unitOfWork.SqlQuery<UserPresenceGraph>(string.Format("Exec usp_UserPresence_Graph_Report @starttime = '{0}',@endtime = '{1}',@status = '{2}'", startTime,startTime, UserStatus)).ToList();

            var final_list = UsersList.AsQueryable();

            return final_list.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        #endregion
        
        #region Quality Report by Ahmad junaid
        public DataSourceResult GetVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId, string Status)
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
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----
                if (model.TimeFrame == "monthly")
                {
                    model.EndDate = model.EndDate.AddMonths(1).AddDays(-1);
                }
                DateTime querysdate = model.StartDate;
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();

                int clockcount = 0;
                string ndtime = "";
                int loopcount = 0;
                List<string> Category = new List<string>();
                if (model.TimeFrame == "q15min")
                {
                    clockcount = 15;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 15, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q20min")
                {
                    clockcount = 20;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 20, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q30min")
                {
                    clockcount = 30;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 30, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q40min")
                {
                    clockcount = 40;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 40, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q60min")
                {
                    clockcount = 60;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour + 1, model.StartDate.Minute, model.StartDate.Second);
                }
                if (model.TimeFrame == "q15min" || model.TimeFrame == "q20min" || model.TimeFrame == "q30min" || model.TimeFrame == "q40min" || model.TimeFrame == "q60min")
                {
                    var result1 = cases.ToList();
                    string stime = "00:00:00";
                    TimeSpan starttime = TimeSpan.Parse(stime);
                    DateTime startdate = model.StartDate.Add(starttime);
                    int q15count = 0;
                    int q20count = 0;
                    int q40count = 0;
                    int q30count = 0;
                    TimeSpan ndtimes = TimeSpan.Parse(ndtime);
                    DateTime enddate = model.EndDate.Add(ndtimes);
                    DateTime loopstartdate = model.StartDate;
                    DateTime loopenddate = model.EndDate.AddDays(1);
                    for (var j = loopstartdate; j < loopenddate;)
                    {
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddate.ToUniversalTimeZone(facilityTimeZone);
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        var resultforstrokestat = result1.Where(x => x.datetime >= sdate && x.datetime <= edate);
                        var resultforothers = result1.Where(x => x.datetime >= startdate && x.datetime <= enddate);
                        report.TimeCycle = startdate.ToString("MM-dd-yyyy HH:mm") + " - " + enddate.ToString("HH:mm");
                        if (model.DefaultType == "casetype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt()).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StatConsult.ToInt()).Count();
                            report.New = resultforothers.Where(x => x.cas_ctp_key == CaseType.RoutineConsultNew.ToInt() || x.cas_ctp_key == CaseType.RoundingNew.ToInt()).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_ctp_key == CaseType.RoutineConsultFollowUp.ToInt() || x.cas_ctp_key == CaseType.RoundingFollowUp.ToInt()).Count();
                            report.EEG = resultforothers.Where(x => x.cas_ctp_key == CaseType.StatEEG.ToInt() || x.cas_ctp_key == CaseType.RoutineEEG.ToInt() || x.cas_ctp_key == CaseType.LongTermEEG.ToInt()).Count();
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt()).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_STAT.ToInt()).Count();
                            report.New = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.New.ToInt()).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.FU.ToInt()).Count();
                            report.EEG = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.EEG.ToInt() || x.cas_billing_bic_key == CaseBillingCode.LTMEEG.ToInt()).Count();
                        }
                        report.xAxisLabel = "Time";
                        volumelist.Add(report);
                        if (model.TimeFrame == "q60min")
                        {
                            Category.Add(startdate.ToString("HH"));
                        }
                        startdate = enddate.AddSeconds(1);
                        enddate = enddate.AddMinutes(clockcount);
                        string checkendtime = startdate.ToString("hh:mm tt");
                        if (model.TimeFrame == "q15min")
                        {
                            if (loopcount == q15count)
                            {
                                Category.Add(startdate.ToString("HH"));
                                q15count = q15count + 4;
                            }
                            else
                            {
                                Category.Add("");
                            }
                        }
                        else if (model.TimeFrame == "q20min")
                        {
                            if (loopcount == q20count)
                            {
                                Category.Add(startdate.ToString("HH"));
                                q20count = q20count + 3;
                            }
                            else
                            {
                                Category.Add("");
                            }
                        }
                        else if (model.TimeFrame == "q30min")
                        {
                            if (loopcount == q30count)
                            {
                                Category.Add(startdate.ToString("HH"));
                                q30count = q30count + 2;
                            }
                            else
                            {
                                Category.Add("");
                            }
                        }
                        else if (model.TimeFrame == "q40min")
                        {
                            if (loopcount == q40count)
                            {
                                string check = startdate.ToString("HH");
                                if (check == "01" || check == "03" || check == "05" || check == "07" || check == "09" || check == "11" || check == "13" || check == "15" || check == "17" || check == "19" || check == "21")
                                {
                                    Category.Add("");
                                    q40count += 1;
                                }
                                else if (check == "23")
                                {
                                    Category.Add("");
                                    q40count += 2;
                                }
                                else
                                {
                                    Category.Add(startdate.ToString("HH"));
                                    q40count += 2;
                                }
                            }
                            else
                            {
                                Category.Add("");
                            }
                        }

                        loopcount++;
                        if (enddate >= loopenddate)
                        {
                            enddate = enddate.AddDays(-1);
                            enddate = enddate.AddHours(23);
                            enddate = enddate.AddMinutes(59);
                            enddate = enddate.AddSeconds(59);
                        }
                        if (loopcount == 96)
                        {
                            if (model.TimeFrame == "q15min")
                            {
                                j = loopstartdate.AddDays(1);
                            }
                        }
                        else if (loopcount == 72)
                        {
                            if (model.TimeFrame == "q20min")
                            {
                                j = loopstartdate.AddDays(1);
                            }
                        }
                        else if (loopcount == 48)
                        {
                            if (model.TimeFrame == "q30min")
                            {
                                j = loopstartdate.AddDays(1);
                            }
                        }
                        else if (loopcount == 36)
                        {
                            if (model.TimeFrame == "q40min")
                            {
                                j = loopstartdate.AddDays(1);
                            }
                        }
                        else if (loopcount == 24)
                        {
                            if (model.TimeFrame == "q60min")
                            {
                                j = loopstartdate.AddDays(1);
                            }
                        }
                    }
                }
                if (model.TimeFrame == "daily")
                {
                    var result1 = cases.ToList();
                    DateTime startdate = model.StartDate;
                    DateTime enddate = model.EndDate;
                    for (var i = startdate; startdate <= enddate;)
                    {
                        DateTime enddatess = startdate.AddDays(1);
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddatess.ToUniversalTimeZone(facilityTimeZone);
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        var resultforstrokestat = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                        var resultforothers = result1.Where(x => x.datetime >= startdate && x.datetime < enddatess);
                        report.TimeCycle = startdate.ToString("MM-dd-yyyy");
                        if (model.DefaultType == "casetype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                            report.New = resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                            report.EEG = resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                            report.New = resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                            report.EEG = resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                        }
                        report.xAxisLabel = "Date";
                        volumelist.Add(report);
                        Category.Add(startdate.ToString("MM-dd"));
                        startdate = startdate.AddDays(1);
                    }
                }
                if (model.TimeFrame == "monthly")
                {
                    DateTime startdate = model.StartDate;
                    DateTime enddate = model.EndDate.ToUniversalTimeZone(facilityTimeZone);
                    for (var i = startdate; startdate <= enddate;)
                    {
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        DateTime enddateofmonth = startdate.AddMonths(1);
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                        var resultforstrokestat = cases.Where(x => x.datetime >= sdate && x.datetime < edate);
                        var resultforothers = cases.Where(x => x.datetime >= startdate && x.datetime < enddateofmonth);
                        report.TimeCycle = startdate.ToString("MMMM yyyy");
                        if (model.DefaultType == "casetype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                            }
                            report.STAT = resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                            report.New = resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                            report.EEG = resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                            report.New = resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                            report.EEG = resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                        }
                        report.xAxisLabel = "Month";
                        volumelist.Add(report);
                        Category.Add(startdate.ToString("MMMM yyyy"));
                        startdate = startdate.AddMonths(1);
                    }
                }
                if (model.TimeFrame == "quarterly")
                {
                    DateTime startdate = model.StartDate;
                    DateTime enddate = model.EndDate.ToUniversalTimeZone(facilityTimeZone);

                    for (var i = startdate; startdate <= enddate;)
                    {
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        DateTime enddateofmonth = startdate.AddMonths(3);
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                        Category.Add(startdate.ToString("MM yyyy") + "-" + enddateofmonth.AddDays(-1).ToString("MM yyyy"));
                        var resultforstrokestat = cases.Where(x => x.datetime >= sdate && x.datetime < edate).ToList();
                        var resultforothers = cases.Where(x => x.datetime >= startdate && x.datetime < enddateofmonth).ToList();
                        report.TimeCycle = startdate.ToString("MM-dd-yyyy") + " - " + enddateofmonth.AddDays(-1).ToString("MM-dd-yyyy");
                        if (model.DefaultType == "casetype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt()).Count();
                            }
                            report.STAT = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StatConsult.ToInt()).Count();
                            report.New = resultforothers.Where(x => x.cas_ctp_key == CaseType.RoutineConsultNew.ToInt() || x.cas_ctp_key == CaseType.RoundingNew.ToInt()).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_ctp_key == CaseType.RoutineConsultFollowUp.ToInt() || x.cas_ctp_key == CaseType.RoundingFollowUp.ToInt()).Count();
                            report.EEG = resultforothers.Where(x => x.cas_ctp_key == CaseType.StatEEG.ToInt() || x.cas_ctp_key == CaseType.RoutineEEG.ToInt() || x.cas_ctp_key == CaseType.LongTermEEG.ToInt()).Count();
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (model.Blast != null)
                            {
                                bool isblast = true;
                                if (model.Blast == "false")
                                {
                                    isblast = false;
                                }
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            }
                            else
                            {
                                report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt()).Count();
                            }

                            report.STAT = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_STAT.ToInt()).Count();
                            report.New = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.New.ToInt()).Count();
                            report.FollowUp = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.FU.ToInt()).Count();
                            report.EEG = resultforothers.Where(x => x.cas_billing_bic_key == CaseBillingCode.EEG.ToInt() || x.cas_billing_bic_key == CaseBillingCode.LTMEEG.ToInt()).Count();
                        }
                        report.xAxisLabel = "Quarter";
                        volumelist.Add(report);
                        startdate = startdate.AddMonths(3);
                    }
                }

                if (Status != "")
                {
                    string Name = "";
                    List<int> datalist = new List<int>();
                    if (Status == "strokealert")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.StrokeAlert);
                            if (model.Blast == "true")
                            {
                                Name = "StrokeAlert (Blast Only)";
                            }
                            else if (model.Blast == "false")
                            {
                                Name = "StrokeAlert (Blast Excluded)";
                            }
                            else
                            {
                                Name = "StrokeAlert";
                            }

                        }
                    }
                    else if (Status == "stat")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.STAT);
                            Name = "STAT";
                        }
                    }
                    else if (Status == "New")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.New);
                            Name = "New";
                        }
                    }
                    else if (Status == "followup")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.FollowUp);
                            Name = "FU";
                        }
                    }
                    else if (Status == "eeg")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.EEG);
                            Name = "EEG";
                        }
                    }
                    VolumeMetricsReport metrics = new VolumeMetricsReport();
                    metrics.Category = Category;
                    metrics.Name = Name;
                    metrics.xAxisLabel = volumelist.Select(x => x.xAxisLabel).FirstOrDefault();
                    metrics.DataList = datalist;
                    volumelist.Add(metrics);
                }

                List<string> emptylist = new List<string>();
                List<int> empty = new List<int>();
                var finalresult = volumelist.Select(x => new
                {
                    id = Status == "" ? "" : "",
                    date = Status == "" ? x.TimeCycle : "",
                    strokealert = Status == "" ? x.StrokeAlert : 0,
                    stat = Status == "" ? x.STAT : 0,
                    New = Status == "" ? x.New : 0,
                    followup = Status == "" ? x.FollowUp : 0,
                    eeg = Status == "" ? x.EEG : 0,
                    name = Status != "" ? x.Name : "",
                    xlabel = Status != "" ? x.xAxisLabel : "",
                    category = Status != "" ? x.Category : emptylist,
                    datalist = Status != "" ? x.DataList : empty,
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetHourlyVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId, string Status)
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
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----

                DateTime querysdate = model.StartDate;
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();

                List<string> Category = new List<string>();
                var result1 = cases.ToList();
                DateTime startdate = model.StartDate;
                DateTime enddate = model.EndDate;
                for (var i = startdate; startdate <= enddate;)
                {
                    TimeSpan hour = TimeSpan.Parse(model.TimeFrame);
                    DateTime sdate = startdate.Add(hour);
                    DateTime edate = new DateTime();
                    if (model.TimeCycle == "q60min")
                    {
                        edate = sdate.AddMinutes(60);
                    }
                    else if (model.TimeCycle == "q40min")
                    {
                        edate = sdate.AddMinutes(40);
                    }
                    else if (model.TimeCycle == "q30min")
                    {
                        edate = sdate.AddMinutes(30);
                    }
                    else if (model.TimeCycle == "q20min")
                    {
                        edate = sdate.AddMinutes(20);
                    }
                    DateTime stdate = sdate.ToUniversalTimeZone(facilityTimeZone);
                    DateTime etdate = edate.ToUniversalTimeZone(facilityTimeZone);
                    VolumeMetricsReport report = new VolumeMetricsReport();
                    var resultforstrokestat = result1.Where(x => x.datetime >= stdate && x.datetime < etdate);
                    report.TimeCycle = sdate.ToString("MM-dd-yyyy") + " (" + sdate.ToString("HH:mm") + "-" + edate.ToString("HH:mm") + ")";
                    if (model.DefaultType == "casetype")
                    {
                        if (model.Blast != null)
                        {
                            bool isblast = true;
                            if (model.Blast == "false")
                            {
                                isblast = false;
                            }
                            report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                        }
                        else
                        {
                            report.StrokeAlert = resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                        }

                        report.STAT = resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                    }
                    else if (model.DefaultType == "billingtype")
                    {
                        if (model.Blast != null)
                        {
                            bool isblast = true;
                            if (model.Blast == "false")
                            {
                                isblast = false;
                            }
                            report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                        }
                        else
                        {
                            report.StrokeAlert = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                        }

                        report.STAT = resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                    }
                    report.xAxisLabel = "Date";
                    volumelist.Add(report);
                    Category.Add(startdate.ToString("MM-dd"));
                    startdate = startdate.AddDays(1);
                }
                if (Status != "")
                {
                    string Name = "";
                    List<int> datalist = new List<int>();
                    if (Status == "strokealert")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.StrokeAlert);
                            if (model.Blast == "true")
                            {
                                Name = "StrokeAlert (Blast Only)";
                            }
                            else if (model.Blast == "false")
                            {
                                Name = "StrokeAlert (Blast Excluded)";
                            }
                            else
                            {
                                Name = "StrokeAlert";
                            }
                        }
                    }
                    else if (Status == "stat")
                    {
                        foreach (var item in volumelist)
                        {
                            datalist.Add(item.STAT);
                            Name = "STAT";
                        }
                    }
                    VolumeMetricsReport metrics = new VolumeMetricsReport();
                    metrics.Category = Category;
                    metrics.Name = Name;
                    metrics.xAxisLabel = volumelist.Select(x => x.xAxisLabel).FirstOrDefault();
                    metrics.DataList = datalist;
                    volumelist.Add(metrics);
                }
                List<string> emptylist = new List<string>();
                List<int> empty = new List<int>();
                var finalresult = volumelist.Select(x => new
                {
                    id = Status == "" ? "" : "",
                    date = Status == "" ? x.TimeCycle : "",
                    strokealert = Status == "" ? x.StrokeAlert : 0,
                    stat = Status == "" ? x.STAT : 0,
                    name = Status != "" ? x.Name : "",
                    xlabel = Status != "" ? x.xAxisLabel : "",
                    category = Status != "" ? x.Category : emptylist,
                    datalist = Status != "" ? x.DataList : empty,
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetHourlyMeanVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId, string Status)
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
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----

                DateTime querysdate = model.StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                var query = cases.Select(x => new
                {
                    cas_billing_physician_blast = x.cas_billing_physician_blast,
                    cas_fac_key = x.cas_fac_key,
                    cas_ctp_key = x.cas_ctp_key,
                    cas_billing_bic_key = x.cas_billing_bic_key,
                    time = DbFunctions.CreateTime(x.datetime.Value.Hour, x.datetime.Value.Minute, x.datetime.Value.Second),
                    numberofdays = DbFunctions.DiffDays(model.StartDate, model.EndDate),
                });

                var data = query.ToList();
                List<HourlyMeanReport> volumelist = new List<HourlyMeanReport>();
                int clockcount = 0;
                string ndtime = "";
                int loopcount = 0;
                List<string> Category = new List<string>();
                if (model.TimeFrame == "q20min")
                {
                    clockcount = 20;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 20, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q30min")
                {
                    clockcount = 30;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 30, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q40min")
                {
                    clockcount = 40;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour, model.StartDate.Minute + 40, model.StartDate.Second);
                }
                else if (model.TimeFrame == "q60min")
                {
                    clockcount = 60;
                    ndtime = string.Format("{0:00}:{1:00}:{2:00}", model.StartDate.Hour + 1, model.StartDate.Minute, model.StartDate.Second);
                }

                string stime = "00:00:00";
                string etime = "23:59:59";
                TimeSpan starttime = TimeSpan.Parse(stime);
                DateTime startdate = model.StartDate.Add(starttime);
                int q20count = 0;
                int q40count = 0;
                int q30count = 0;
                TimeSpan ndtimes = TimeSpan.Parse(ndtime);
                DateTime enddate = model.StartDate.Add(ndtimes);
                TimeSpan loopstartdate = starttime;
                TimeSpan loopenddate = TimeSpan.Parse(etime);
                var totaldays = query.Select(x => x.numberofdays).FirstOrDefault();
                double numberofdays = Convert.ToDouble(totaldays + 1);
                for (var j = loopstartdate; j <= loopenddate;)
                {
                    //DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                    //DateTime edate = enddate.ToUniversalTimeZone(facilityTimeZone);
                    DateTime sdate = startdate;
                    DateTime edate = enddate;
                    TimeSpan fromtime = TimeSpan.Parse(sdate.Hour + ":" + sdate.Minute + ":" + sdate.Second);
                    TimeSpan totime = TimeSpan.Parse(edate.Hour + ":" + edate.Minute + ":" + edate.Second);
                    HourlyMeanReport report = new HourlyMeanReport();
                    var resultforstrokestat = data.Where(x => x.time >= fromtime && x.time <= totime);

                    if (model.DefaultType == "casetype")
                    {
                        if (model.Blast != null)
                        {
                            bool isblast = true;
                            if (model.Blast == "false")
                            {
                                isblast = false;
                            }
                            double stroke = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            double strokemean = stroke != 0 && numberofdays != 0 ? Math.Round(stroke / numberofdays, 2) : 0;
                            report.StrokeAlert = strokemean;
                        }
                        else
                        {
                            double stroke = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StrokeAlert.ToInt()).Count();
                            double strokemean = stroke != 0 && numberofdays != 0 ? Math.Round(stroke / numberofdays, 2) : 0;
                            report.StrokeAlert = strokemean;
                        }
                        double STAT = resultforstrokestat.Where(x => x.cas_ctp_key == CaseType.StatConsult.ToInt()).Count();
                        double STATmean = STAT != 0 && numberofdays != 0 ? Math.Round(STAT / numberofdays, 2) : 0;
                        report.STAT = STATmean;

                    }
                    else if (model.DefaultType == "billingtype")
                    {
                        if (model.Blast != null)
                        {
                            bool isblast = true;
                            if (model.Blast == "false")
                            {
                                isblast = false;
                            }
                            double stroke = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt() && x.cas_billing_physician_blast == isblast).Count();
                            double strokemean = stroke != 0 && numberofdays != 0 ? Math.Round(stroke / numberofdays, 2) : 0;
                            report.StrokeAlert = strokemean;
                        }
                        else
                        {
                            double stroke = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_StrokeAlert.ToInt()).Count();
                            double strokemean = stroke != 0 && numberofdays != 0 ? Math.Round(stroke / numberofdays, 2) : 0;
                            report.StrokeAlert = strokemean;
                        }
                        double stat = resultforstrokestat.Where(x => x.cas_billing_bic_key == CaseBillingCode.CC1_STAT.ToInt()).Count();
                        double statmean = stat != 0 && numberofdays != 0 ? Math.Round(stat / numberofdays, 2) : 0;
                        report.STAT = statmean;

                    }
                    report.xAxisLabel = "Time";

                    DateTime titlesdate = startdate.AddHours(-4);
                    DateTime titledate = enddate.AddHours(-4);
                    string chetitle = titledate.ToString("HH:mm");

                    if (chetitle == "19:59")
                    {
                        titledate = titledate.AddSeconds(1);
                    }
                    report.TimeCycle = model.StartDate.ToString("MM-dd") + " to " + model.EndDate.ToString("MM-dd-yyyy") + " " + titlesdate.ToString("HH:mm") + "-" + titledate.ToString("HH:mm");
                    volumelist.Add(report);
                    if (model.TimeFrame == "q60min")
                    {
                        Category.Add(startdate.ToString("HH"));
                    }
                    startdate = enddate.AddSeconds(1);
                    enddate = enddate.AddMinutes(clockcount);

                    string checkendtime = startdate.ToString("hh:mm tt");
                    if (model.TimeFrame == "q20min")
                    {
                        if (loopcount == q20count)
                        {
                            Category.Add(startdate.ToString("HH"));
                            q20count = q20count + 3;
                        }
                        else
                        {
                            Category.Add("");
                        }
                    }
                    else if (model.TimeFrame == "q30min")
                    {
                        if (loopcount == q30count)
                        {
                            Category.Add(startdate.ToString("HH"));
                            q30count = q30count + 2;
                        }
                        else
                        {
                            Category.Add("");
                        }
                    }
                    else if (model.TimeFrame == "q40min")
                    {
                        if (loopcount == q40count)
                        {
                            string check = startdate.ToString("HH");
                            if (check == "01" || check == "03" || check == "05" || check == "07" || check == "09" || check == "11" || check == "13" || check == "15" || check == "17" || check == "19" || check == "21")
                            {
                                Category.Add("");
                                q40count += 1;
                            }
                            else if (check == "23")
                            {
                                Category.Add("");
                                q40count += 2;
                            }
                            else
                            {
                                Category.Add(startdate.ToString("HH"));
                                q40count += 2;
                            }
                        }
                        else
                        {
                            Category.Add("");
                        }
                    }

                    loopcount++;
                    TimeSpan forloopend = TimeSpan.Parse(enddate.Hour + ":" + enddate.Minute + ":" + enddate.Second);
                    if (forloopend == starttime)
                    {
                        enddate = enddate.AddDays(-1);
                        enddate = enddate.AddHours(23);
                        enddate = enddate.AddMinutes(59);
                        enddate = enddate.AddSeconds(59);
                    }
                    TimeSpan checkend60 = TimeSpan.Parse("00:59:59");
                    TimeSpan checkend40 = TimeSpan.Parse("00:39:59");
                    TimeSpan checkend30 = TimeSpan.Parse("00:29:59");
                    TimeSpan checkend20 = TimeSpan.Parse("00:19:59");
                    if (forloopend == checkend60)
                    {
                        break;
                    }
                    if (forloopend == checkend40)
                    {
                        break;
                    }
                    if (forloopend == checkend30)
                    {
                        break;
                    }
                    if (forloopend == checkend20)
                    {
                        break;
                    }
                    TimeSpan forloop = TimeSpan.Parse(enddate.Hour + ":" + enddate.Minute + ":" + enddate.Second);
                    j = forloop;
                }
                var finallist = volumelist.OrderBy(x => x.TimeCycle).ToList();
                if (Status != "")
                {
                    string Name = "";
                    List<double> datalist = new List<double>();
                    if (Status == "strokealert")
                    {
                        foreach (var item in finallist)
                        {
                            datalist.Add(item.StrokeAlert);
                            if (model.Blast == "true")
                            {
                                Name = "StrokeAlert (Blast Only)";
                            }
                            else if (model.Blast == "false")
                            {
                                Name = "StrokeAlert (Blast Excluded)";
                            }
                            else
                            {
                                Name = "StrokeAlert";
                            }
                        }
                    }
                    else if (Status == "stat")
                    {
                        foreach (var item in finallist)
                        {
                            datalist.Add(item.STAT);
                            Name = "STAT";
                        }
                    }

                    HourlyMeanReport metrics = new HourlyMeanReport();
                    metrics.Category = Category;
                    metrics.Name = Name;
                    metrics.xAxisLabel = finallist.Select(x => x.xAxisLabel).FirstOrDefault();
                    metrics.DataList = datalist;
                    finallist.Add(metrics);
                }

                List<string> emptylist = new List<string>();
                List<double> empty = new List<double>();
                var finalresult = finallist.Select(x => new
                {
                    id = Status == "" ? "" : "",
                    date = Status == "" ? x.TimeCycle : "",
                    strokealert = Status == "" ? x.StrokeAlert : 0,
                    stat = Status == "" ? x.STAT : 0,
                    name = Status != "" ? x.Name : "",
                    xlabel = Status != "" ? x.xAxisLabel : "",
                    category = Status != "" ? x.Category : emptylist,
                    datalist = Status != "" ? x.DataList : empty,
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetQualitySummaryReport(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }



                if (model.WorkFlowType != null)
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }



                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion

                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var EDStay = PatientType.SymptomOnsetDuringEDStay.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,

                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    bedside_response_time_cmp = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    arrival_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != patientType && x.ca.cas_patient_type != EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time) : "" : "",
                    arrival_needle_time_cmp = x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                    verbal_order_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time) : "",
                    on_screen_time = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                    on_screen_time_cmp = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                    activation_time = x.ca.cas_patient_type != patientType && x.ca.cas_patient_type != EDStay ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
                    symptoms_to_needle_time = x.ca.cas_metric_tpa_consult == true ? x.ca.cas_patient_type != patientType ? x.ca.cas_patient_type == EDStay ? x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time) : "" : "" : "",
                    symptoms_to_needle_time_cmp = x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time),
                });

                #endregion
                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                List<double> _bedsidemeanlist = new List<double>();
                List<double> _bedsidemedianlist = new List<double>();
                List<double> _arrivalneedlemeanlist = new List<double>();
                List<double> _arrivalneedlemedianlist = new List<double>();
                List<double> _activationmeanlist = new List<double>();
                List<double> _activationmedianlist = new List<double>();
                List<double> _onscreenmeanlist = new List<double>();
                List<double> _onscreenmedianlist = new List<double>();
                List<double> _verbalordermeanlist = new List<double>();
                List<double> _verbalordermedianlist = new List<double>();
                List<double> _symptommeanlist = new List<double>();
                List<double> _symptommedianlist = new List<double>();
                var qualitytimes = query.Select(x => new { x.bedside_response_time, x.arrival_needle_time, x.verbal_order_to_needle_time, x.on_screen_time, x.activation_time, x.symptoms_to_needle_time, x.created_date, x.id }).ToList();
                int bedsidecount = 0;
                int arrivaltoneedlecount = 0;
                int activationcount = 0;
                int onscreencount = 0;
                int verbalcount = 0;
                int symptomtoneedlecount = 0;
                if (qualitytimes.Count > 0)
                {

                    foreach (var item in qualitytimes)
                    {
                        if (item.bedside_response_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.bedside_response_time.Split(':')[0]), int.Parse(item.bedside_response_time.Split(':')[1]), int.Parse(item.bedside_response_time.Split(':')[2])).TotalSeconds;
                            _bedsidemeanlist.Add(time);
                            _bedsidemedianlist.Add(time);
                            bedsidecount++;
                        }
                        if (item.arrival_needle_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.arrival_needle_time.Split(':')[0]), int.Parse(item.arrival_needle_time.Split(':')[1]), int.Parse(item.arrival_needle_time.Split(':')[2])).TotalSeconds;
                            _arrivalneedlemeanlist.Add(time);
                            _arrivalneedlemedianlist.Add(time);
                            arrivaltoneedlecount++;
                        }
                        if (item.activation_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.activation_time.Split(':')[0]), int.Parse(item.activation_time.Split(':')[1]), int.Parse(item.activation_time.Split(':')[2])).TotalSeconds;
                            _activationmeanlist.Add(time);
                            _activationmedianlist.Add(time);
                            activationcount++;
                        }
                        if (item.on_screen_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.on_screen_time.Split(':')[0]), int.Parse(item.on_screen_time.Split(':')[1]), int.Parse(item.on_screen_time.Split(':')[2])).TotalSeconds;
                            _onscreenmeanlist.Add(time);
                            _onscreenmedianlist.Add(time);
                            onscreencount++;
                        }
                        if (item.verbal_order_to_needle_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.verbal_order_to_needle_time.Split(':')[0]), int.Parse(item.verbal_order_to_needle_time.Split(':')[1]), int.Parse(item.verbal_order_to_needle_time.Split(':')[2])).TotalSeconds;
                            _verbalordermeanlist.Add(time);
                            _verbalordermedianlist.Add(time);
                            verbalcount++;
                        }
                        if (item.symptoms_to_needle_time != "")
                        {
                            var time = new TimeSpan(int.Parse(item.symptoms_to_needle_time.Split(':')[0]), int.Parse(item.symptoms_to_needle_time.Split(':')[1]), int.Parse(item.symptoms_to_needle_time.Split(':')[2])).TotalSeconds;
                            _symptommeanlist.Add(time);
                            _symptommedianlist.Add(time);
                            symptomtoneedlecount++;
                        }

                    }
                }
                TimeSpan _bedsidemeantime = new TimeSpan();
                TimeSpan _bedsidemediantime = new TimeSpan();
                TimeSpan _arivaltoneedlemeantime = new TimeSpan();
                TimeSpan _arivaltoneedlemediantime = new TimeSpan();
                TimeSpan _activationmeantime = new TimeSpan();
                TimeSpan _activationmediantime = new TimeSpan();
                TimeSpan _onscreenmeantime = new TimeSpan();
                TimeSpan _onscreenmediantime = new TimeSpan();
                TimeSpan _verbalordermeantime = new TimeSpan();
                TimeSpan _verbalordermediantime = new TimeSpan();
                TimeSpan _symptommeantime = new TimeSpan();
                TimeSpan _symptommediantime = new TimeSpan();
                if (_bedsidemeanlist.Count > 0)
                {
                    double mean = _bedsidemeanlist.Average();
                    _bedsidemeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_bedsidemedianlist.Count > 0)
                {
                    int numbercount = _bedsidemedianlist.Count();
                    int halfindex = _bedsidemedianlist.Count() / 2;
                    var sortednumbers = _bedsidemedianlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _bedsidemediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_arrivalneedlemeanlist.Count > 0)
                {
                    double mean = _arrivalneedlemeanlist.Average();
                    _arivaltoneedlemeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_arrivalneedlemeanlist.Count > 0)
                {
                    int numbercount = _arrivalneedlemeanlist.Count();
                    int halfindex = _arrivalneedlemeanlist.Count() / 2;
                    var sortednumbers = _arrivalneedlemeanlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _arivaltoneedlemediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_activationmeanlist.Count > 0)
                {
                    double mean = _activationmeanlist.Average();
                    _activationmeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_activationmedianlist.Count > 0)
                {
                    int numbercount = _activationmedianlist.Count();
                    int halfindex = _activationmedianlist.Count() / 2;
                    var sortednumbers = _activationmedianlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _activationmediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_onscreenmeanlist.Count > 0)
                {
                    double mean = _onscreenmeanlist.Average();
                    _onscreenmeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_onscreenmedianlist.Count > 0)
                {
                    int numbercount = _onscreenmedianlist.Count();
                    int halfindex = _onscreenmedianlist.Count() / 2;
                    var sortednumbers = _onscreenmedianlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _onscreenmediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_verbalordermeanlist.Count > 0)
                {
                    double mean = _verbalordermeanlist.Average();
                    _verbalordermeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_verbalordermedianlist.Count > 0)
                {
                    int numbercount = _verbalordermedianlist.Count();
                    int halfindex = _verbalordermedianlist.Count() / 2;
                    var sortednumbers = _verbalordermedianlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _verbalordermediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_symptommeanlist.Count > 0)
                {
                    double mean = _symptommeanlist.Average();
                    _symptommeantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                if (_symptommedianlist.Count > 0)
                {
                    int numbercount = _symptommedianlist.Count();
                    int halfindex = _symptommedianlist.Count() / 2;
                    var sortednumbers = _symptommedianlist.OrderBy(x => x);
                    double median;
                    if ((numbercount % 2) == 0)
                    {
                        median = ((sortednumbers.ElementAt(halfindex) + sortednumbers.ElementAt(halfindex - 1))) / 2;
                    }
                    else
                    {
                        median = sortednumbers.ElementAt(halfindex);
                    }
                    _symptommediantime = TimeSpan.FromSeconds(Convert.ToDouble(median));
                }
                if (_bedsidemeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "Bedside Response Time";
                    cls.hospitals = bedsidecount;
                    cls._meantime = _bedsidemeantime;
                    cls._mediantime = _bedsidemediantime;
                    result.Add(cls);
                }
                if (_arivaltoneedlemeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "Arrival To Needle Time";
                    cls.hospitals = arrivaltoneedlecount;
                    cls._meantime = _arivaltoneedlemeantime;
                    cls._mediantime = _arivaltoneedlemediantime;
                    result.Add(cls);
                }
                if (_activationmeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "ED Stroke Alert Activation Time";
                    cls.hospitals = activationcount;
                    cls._meantime = _activationmeantime;
                    cls._mediantime = _activationmediantime;
                    result.Add(cls);
                }
                if (_onscreenmeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "On Screen Time";
                    cls.hospitals = onscreencount;
                    cls._meantime = _onscreenmeantime;
                    cls._mediantime = _onscreenmediantime;
                    result.Add(cls);
                }
                if (_verbalordermeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "Alteplase early mix decision To tPA Time";
                    cls.hospitals = verbalcount;
                    cls._meantime = _verbalordermeantime;
                    cls._mediantime = _verbalordermediantime;
                    result.Add(cls);
                }
                if (_symptommeantime != null)
                {
                    QualityMetricsReportCls cls = new QualityMetricsReportCls();
                    cls.reportname = "Symptom to Needle Time";
                    cls.hospitals = symptomtoneedlecount;
                    cls._meantime = _symptommeantime;
                    cls._mediantime = _symptommediantime;
                    result.Add(cls);
                }
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }
                guids = guids.TrimEnd(',');


                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    reportname = x.reportname,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetBedsideMetrics(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });
                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }

                if (model.WorkFlowType != null)
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }
                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }
                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }
                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }
                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,

                    bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    bedside_response_time_cmp = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                });

                #endregion

                query = query.OrderBy(m => m.bedside_response_time);
                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                int count = 0;
                List<string> bedsidetime = query.Select(x => x.bedside_response_time).ToList();
                if (bedsidetime.Count > 0)
                {
                    foreach (var item in bedsidetime)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }

                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }
                guids = guids.TrimEnd(',');

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetArivalToNeedle(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });
                #region ----- Filters -----
                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }


                if (model.WorkFlowType != null)
                {
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                }
                else
                {
                    List<int> workflowlist = new List<int>();
                    workflowlist.Add(1);
                    workflowlist.Add(3);
                    model.WorkFlowType = workflowlist;
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                }
                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }



                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion
                cases = cases.Where(c => c.ca.cas_patient_type != 4);
                #endregion

                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,
                    arrival_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                    arrival_needle_time_cmp = x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                });

                #endregion

                query = query.OrderBy(m => m.arrival_needle_time);

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> arivaltoneedle = query.Select(x => x.arrival_needle_time).ToList();
                int count = 0;
                if (arivaltoneedle.Count > 0)
                {
                    foreach (var item in arivaltoneedle)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }
                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }
                guids = guids.TrimEnd(',');


                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetVerbalTotPA(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }



                if (model.WorkFlowType != null)
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,
                    verbal_order_to_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),
                    //verbal_order_to_ocopr_order = DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),
                    //verbal_order_to_ocopr_order_cmp = DBHelper.DiffSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),
                });

                #endregion

                query = query.OrderBy(m => m.verbal_order_to_needle_time);

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> verbalorder = query.Select(x => x.verbal_order_to_needle_time).ToList();
                int count = 0;
                if (verbalorder.Count > 0)
                {
                    foreach (var item in verbalorder)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }
                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }

                guids = guids.TrimEnd(',');

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetOnScreen(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }



                if (model.WorkFlowType != null)
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                            .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,

                    on_screen_time = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                    on_screen_time_cmp = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                });

                #endregion

                query = query.OrderBy(m => m.on_screen_time);
                List<QualityMetricsReportCls> final = new List<QualityMetricsReportCls>();
                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> onscreen = query.Select(x => x.on_screen_time).ToList();
                int count = 0;
                if (onscreen.Count > 0)
                {
                    foreach (var item in onscreen)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }
                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }

                guids = guids.TrimEnd(',');
                result.Add(cls);

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetHandleTime(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true && DbFunctions.TruncateTime(ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                                         DbFunctions.TruncateTime(ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate)
                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                navigatorID = ca.cas_created_by,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

               // var test = cases.ToList();

                #region ----- Filters -----

                //if (model.IncludeTime)
                //{
                //    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                //}
                //else
                //{
                //    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                //                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                //}

                //var test = cases.ToList();

                //if (model.WorkFlowType != null)
                //    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                //if (model.CallType != null)
                //    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                //if (model.CallerSource != null)
                //{
                //    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                //}

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_created_by));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                //if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                //{
                //    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                //}


                //if (model.tPA != null && model.tPA.Count > 0)
                //{
                //    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                //}

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,
                    x.navigatorID,
                    handle_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),
                    handle_time_cmp = DBHelper.DiffSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),
                });

                #endregion

                query = query.OrderBy(m => m.handle_time);

                var queryList = query.ToList();

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";
                if (model.Physicians != null)
                {
                    for (int i = 0; i < model.Physicians.Count; i++)
                    {
                        QualityMetricsReportCls cls = new QualityMetricsReportCls();
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        List<string> handletime = queryList.Where(x => 
                        x.navigatorID == model.Physicians[i]).Select(x => x.handle_time).ToList();
                        int count = 0;
                        if (handletime.Count > 0)
                        {
                            foreach (var item in handletime)
                            {
                                if (item != "")
                                {
                                    var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                                    _meanlist.Add(time);
                                    _medianlist.Add(time);
                                    count++;
                                }
                            }
                        }
                        TimeSpan _meantime = new TimeSpan();
                        TimeSpan _mediantime = new TimeSpan();
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
                        cls.hospitals = count;
                        cls._meantime = _meantime;
                        cls._mediantime = _mediantime;
                        cls.Navigator = queryList.Where(x => x.navigatorID == model.Physicians[i]).Select(x => x.navigator).ToList().FirstOrDefault();
                        cls.NavigatorID = model.Physicians[i];
                        if (model.Facilities != null)
                        {
                            foreach (var guid in model.Facilities)
                            {
                                guids += guid + ",";
                            }
                        }

                        guids = guids.TrimEnd(',');
                        cls.timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                        result.Add(cls);
                     }
                }
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    Navigator = x.Navigator,
                    NavigatorID = x.NavigatorID,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = x.timeframe
                }).Where(x => x.hospitalname != 0).OrderBy(x => x.Navigator).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetActivationTime(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }

                if (model.WorkFlowType != null)
                {
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                }

                //else
                //{
                //    List<int> workflowlist = new List<int>();
                //    workflowlist.Add(1);
                //    workflowlist.Add(3);
                //    model.WorkFlowType = workflowlist;
                //    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                //}
                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));

                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));


                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));

                }

                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }

                #endregion
                cases = cases.Where(c => c.ca.cas_patient_type != 4);
                
                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",


                    activation_time = x.ca.cas_patient_type != patientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
                });

                #endregion

                query = query.OrderBy(m => m.activation_time);

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> activation_time = query.Select(x => x.activation_time).ToList();
                int count = 0;
                if (activation_time.Count > 0)
                {
                    foreach (var item in activation_time)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }

                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }
                guids = guids.TrimEnd(',');

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetSymptomstoNeedle(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                            from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                            from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                            from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                            from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                            from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                acceptedInfo_date = accepted.cah_created_date_utc,
                                billingCode = billing_code != null ? billing_code.ucd_title : "",
                                caseType = case_type != null ? case_type.ucd_title : "",
                                caseStatus = case_status != null ? case_status.ucd_title : "",
                                callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

                #region ----- Filters -----

                if (model.IncludeTime)
                {
                    cases = cases.Where(x => x.ca.cas_created_date >= model.StartDate && x.ca.cas_created_date <= model.EndDate);
                }
                else
                {
                    cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));
                }

                if (model.WorkFlowType != null)
                {
                    cases = cases.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                }
                
                if (model.CallType != null)
                    cases = cases.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                if (model.CallerSource != null)
                {
                    cases = cases.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                }

                if (model.CaseStatus != null)
                    cases = cases.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                if (model.CaseType != null)
                    cases = cases.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                if (model.BillingCode != null)
                    cases = cases.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }



                if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                {
                    cases = cases.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                }


                if (model.tPA != null && model.tPA.Count > 0)
                {
                    cases = cases.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                }

                #region TCARE-479
                if (model.eAlert != null && model.eAlert.Count > 0)
                {
                    cases = cases.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                }
                #endregion
                cases = cases.Where(c => c.ca.cas_patient_type == 4);
                
                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    callType = x.ca.cas_call_type != null && x.ca.cas_ctp_key == ((int)CaseType.StrokeAlert) ? ((CallType)x.ca.cas_call_type).ToString() : "",
                    case_number = x.ca.cas_case_number,
                    patient_intial = DBHelper.GetInitials(x.ca.cas_patient),
                    dob = DBHelper.FormatDateTime(x.ca.cas_billing_dob, false),
                    gender = x.ca.cas_metric_patient_gender != null && x.ca.cas_metric_patient_gender != "" ? x.ca.cas_metric_patient_gender == "F" ? "Female" : "Male" : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    x.navigator,
                    symptoms_to_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time),
                });

                #endregion

                query = query.OrderBy(m => m.symptoms_to_needle_time);

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> symptoms_to_needle_time = query.Select(x => x.symptoms_to_needle_time).ToList();
                int count = 0;
                if (symptoms_to_needle_time.Count > 0)
                {

                    foreach (var item in symptoms_to_needle_time)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }

                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }

                guids = guids.TrimEnd(',');

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetPerformanceLicense(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var query = from p in _unitOfWork.ApplicationUsers
                        join f in _unitOfWork.PhysicianLicenseRepository.Query() on p.Id equals f.phl_user_key
                        join s in _unitOfWork.UCL_UCDRepository.Query() on f.phl_license_state.Value equals s.ucd_key
                        where p.IsActive
                        && f.phl_is_active
                        && f.phl_license_state.HasValue
                        && s.ucd_ucl_key == (int)UclTypes.State
                        orderby p.LastName, p.FirstName, s.ucd_title, f.phl_issued_date
                        select new
                        {
                            phyId = p.Id,
                            FullName = p.LastName + " " + p.FirstName,
                            ucd_key = s.ucd_key.ToString(),
                            s.ucd_title,
                            f.phl_issued_date,
                            f.phl_expired_date,
                            f.phl_is_in_use,
                            f.phl_assigned_to_name,
                            f.phl_date_assigned,
                            f.phl_app_started,
                            f.phl_app_submitted_to_board,
                            f.phl_assigned_to_id,
                        };

            #region ----- Filters -----
            if (model.Physicians != null)
            {
                query = query.Where(m => model.Physicians.Contains(m.phyId));
            }
            if (model.Specialist != null && model.Specialist.Count > 0)
            {
                if (model.Specialist[0] != Guid.Empty)
                    query = query.Where(m => model.Specialist.Contains(m.phl_assigned_to_id));
            }
            if (model.states != null)
            {
                List<string> statelist = new List<string>();
                if (model.states != null && model.states.Count > 0)
                {
                    foreach (var item in model.states)
                    {
                        statelist.Add(item.Value.ToString());
                    }
                }
                query = query.Where(m => statelist.Contains(m.ucd_key));
            }
            if (model.DefaultType == "Dateassigned")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.phl_date_assigned) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.phl_date_assigned) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "AppStarted")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.phl_app_started) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.phl_app_started) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "AppsubmittedtotheBoard")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.phl_app_submitted_to_board) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.phl_app_submitted_to_board) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "Licenseissuedate")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.phl_issued_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.phl_issued_date) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "ExpireDate")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.phl_expired_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.phl_expired_date) <= DbFunctions.TruncateTime(model.EndDate));
            }

            #endregion

            #region ----- Calculations -----
            var detail = query.Select(x => new
            {
                phy_Id = x.phyId,
                Phy_Name = x.FullName,
                state_name = x.ucd_title,
                state_key = x.ucd_key,
                date_assigned_to_appstarted = x.phl_app_started.HasValue && x.phl_date_assigned.HasValue ? x.phl_app_started.Value.Day < x.phl_date_assigned.Value.Day ? 0 : DbFunctions.DiffDays(x.phl_date_assigned.Value, x.phl_app_started.Value) : 0,
                appstarted_to_appsubmitted = x.phl_app_submitted_to_board.HasValue && x.phl_app_started.HasValue ? x.phl_app_submitted_to_board.Value.Day < x.phl_app_started.Value.Day ? 0 : DbFunctions.DiffDays(x.phl_app_started.Value, x.phl_app_submitted_to_board.Value) : 0,
                appsubmitted_to_issue_date = x.phl_app_submitted_to_board.HasValue && x.phl_issued_date != null ? x.phl_app_submitted_to_board.Value.Day < x.phl_issued_date.Day ? 0 : DbFunctions.DiffDays(x.phl_issued_date, x.phl_app_submitted_to_board.Value) : 0,
            });

            #endregion
            var PhyLicense = detail.OrderBy(x => x.phy_Id).ToList();
            double dateassigntoappstarted_mean = 0;
            double appstarted_to_appsubmitted_mean = 0;
            double appsubmitted_to_issue_date_mean = 0;
            if (PhyLicense.Count > 0)
            {
                var dateassigntoappstarted = PhyLicense.Select(x => x.date_assigned_to_appstarted).ToList();
                if (dateassigntoappstarted.Count > 0)
                {
                    dateassigntoappstarted_mean = Math.Round(Convert.ToDouble(dateassigntoappstarted.Average()), 2);
                }
                var appstarted_to_appsubmitted = PhyLicense.Select(x => x.appstarted_to_appsubmitted).ToList();
                if (appstarted_to_appsubmitted.Count > 0)
                {
                    appstarted_to_appsubmitted_mean = Math.Round(Convert.ToDouble(appstarted_to_appsubmitted.Average()), 2);
                }
                var appsubmitted_to_issue_date = PhyLicense.Select(x => x.appsubmitted_to_issue_date).ToList();
                if (appstarted_to_appsubmitted.Count > 0)
                {
                    appsubmitted_to_issue_date_mean = Math.Round(Convert.ToDouble(appsubmitted_to_issue_date.Average()), 2);
                }
            }
            var _result = detail.Select(x => new
            {
                date_assigned_to_appstarted_days = x.date_assigned_to_appstarted,
                date_assigned_to_appstarted_mean = dateassigntoappstarted_mean,
                appstarted_to_appsubmitted_days = x.appstarted_to_appsubmitted,
                appstarted_to_appsubmitted_mean = appstarted_to_appsubmitted_mean,
                appsubmitted_to_issue_day = x.appsubmitted_to_issue_date,
                appsubmitted_to_issue_date_mean = appsubmitted_to_issue_date_mean
            });

            return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetPerformanceCredentialing(DataSourceRequest request, QualityMetricsViewModel model)
        {

            var query = from p in _unitOfWork.ApplicationUsers
                        join fp in _unitOfWork.FacilityPhysicianRepository.Query() on p.Id equals fp.fap_user_key
                        join f in _unitOfWork.FacilityRepository.Query() on fp.fap_fac_key equals f.fac_key
                        join stt in _unitOfWork.UCL_UCDRepository.Query() on f.fac_stt_key.Value equals stt.ucd_key
                        where !p.IsDeleted && !fp.fap_hide //552 to only show physicians that are not hide in report
                        orderby p.LastName, p.FirstName, f.fac_name
                        select new
                        {
                            fap_key = fp.fap_key,
                            state = stt.ucd_title,
                            phyId = p.Id,
                            fac_id = f.fac_key,
                            FullName = p.LastName + " " + p.FirstName,
                            f.fac_name,
                            fp.fap_credential_specialist,
                            fp.fap_date_assigned,
                            fp.fap_initial_app_received,
                            fp.fap_app_started,
                            fp.fap_app_submitted_to_hospital,
                            fp.fap_vcaa_date,
                            fp.fap_start_date,
                            fp.fap_Credentials_confirmed_date,
                            fp.fap_end_date,
                            isStartDate = fp.fap_start_date.HasValue,
                            isEndDate = fp.fap_end_date.HasValue,
                            fp.fap_is_on_boarded,
                            fp.fap_onboarded_date,
                            p.IsActive,
                            fp.fap_onboarded_by_name,
                            f.fac_go_live,
                            f.fac_is_active,
                            onBoarded = fp.fap_is_on_boarded && fp.fap_onboarded_date != null ? DBHelper.FormatDateTime(fp.fap_onboarded_date, false) : ""
                        };
            if (model.Facilities != null)
            {
                query = query.Where(m => model.Facilities.Contains(m.fac_id));
            }
            if (model.Physicians != null)
            {
                query = query.Where(m => model.Physicians.Contains(m.phyId));
            }
            if (model.Credentialing != null)
            {
                query = query.Where(m => model.Credentialing.Contains(m.fap_credential_specialist));
            }
            if (model.DefaultType == "Dateassigned")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_date_assigned) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.fap_date_assigned) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "AppStarted")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_app_started) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.fap_app_started) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "InitialAppReceived")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_initial_app_received) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.fap_initial_app_received) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "Appsubmittedtothehospital")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_app_submitted_to_hospital) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.fap_app_submitted_to_hospital) <= DbFunctions.TruncateTime(model.EndDate));
            }
            else if (model.DefaultType == "VCAAdate")
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.fap_vcaa_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                               DbFunctions.TruncateTime(x.fap_vcaa_date) <= DbFunctions.TruncateTime(model.EndDate));
            }

            var detail = query.Select(x => new
            {
                fap_key = x.fap_key,
                phy_Id = x.phyId,
                Phy_Name = x.FullName,
                date_assigned_to_appstarted = x.fap_app_started.HasValue && x.fap_date_assigned.HasValue ? x.fap_app_started.Value < x.fap_date_assigned.Value ? 0 : DbFunctions.DiffDays(x.fap_date_assigned.Value, x.fap_app_started.Value) : 0,
                initial_to_appstarted = x.fap_app_started.HasValue && x.fap_initial_app_received.HasValue ? x.fap_app_started.Value.Day < x.fap_initial_app_received.Value.Day ? 0 : DbFunctions.DiffDays(x.fap_initial_app_received.Value, x.fap_app_started.Value) : 0,
                appstarted_to_app_submitted = x.fap_app_submitted_to_hospital.HasValue && x.fap_app_started.HasValue ? x.fap_app_submitted_to_hospital.Value.Day < x.fap_app_started.Value.Day ? 0 : DbFunctions.DiffDays(x.fap_app_started.Value, x.fap_app_submitted_to_hospital.Value) : 0,
                app_submitted_to_vccaDate = x.fap_vcaa_date.HasValue && x.fap_app_submitted_to_hospital.HasValue ? x.fap_vcaa_date.Value.Day < x.fap_app_submitted_to_hospital.Value.Day ? 0 : DbFunctions.DiffDays(x.fap_app_submitted_to_hospital.Value, x.fap_vcaa_date.Value) : 0,
            });

            var FacilityPhy = detail.OrderBy(x => x.phy_Id).ToList();

            double dateassigntoappstarted_mean = 0;
            double initial_to_appstarted_mean = 0;
            double appstarted_to_app_submitted_mean = 0;
            double app_submitted_to_vccaDate_mean = 0;
            if (FacilityPhy.Count > 0)
            {
                var dateassigntoappstarted = FacilityPhy.Select(x => x.date_assigned_to_appstarted).ToList();
                if (dateassigntoappstarted.Count > 0)
                {
                    dateassigntoappstarted_mean = Math.Round(Convert.ToDouble(dateassigntoappstarted.Average()), 2);
                }
                var initial_to_appstarted = FacilityPhy.Select(x => x.initial_to_appstarted).ToList();
                if (initial_to_appstarted.Count > 0)
                {
                    initial_to_appstarted_mean = Math.Round(Convert.ToDouble(initial_to_appstarted.Average()), 2);
                }
                var appstarted_to_app_submitted = FacilityPhy.Select(x => x.appstarted_to_app_submitted).ToList();
                if (appstarted_to_app_submitted.Count > 0)
                {
                    appstarted_to_app_submitted_mean = Math.Round(Convert.ToDouble(appstarted_to_app_submitted.Average()), 2);
                }
                var app_submitted_to_vccaDate = FacilityPhy.Select(x => x.app_submitted_to_vccaDate).ToList();
                if (app_submitted_to_vccaDate.Count > 0)
                {
                    app_submitted_to_vccaDate_mean = Math.Round(Convert.ToDouble(app_submitted_to_vccaDate.Average()), 2);
                }
            }
            var _result = detail.Select(x => new
            {
                date_assigned_to_appstarted_days = x.date_assigned_to_appstarted,
                date_assigned_to_appstarted_mean = dateassigntoappstarted_mean,
                initial_to_appstarted_days = x.initial_to_appstarted,
                initial_to_appstarted_mean = initial_to_appstarted_mean,
                appstarted_to_app_submitted_day = x.appstarted_to_app_submitted,
                appstarted_to_app_submitted_mean = appstarted_to_app_submitted_mean,
                app_submitted_to_vccaDate_day = x.app_submitted_to_vccaDate,
                app_submitted_to_vccaDate_mean = app_submitted_to_vccaDate_mean
            });
            return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetStatTime(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                                //from billing_code in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.BillingCode && ca.cas_billing_bic_key == x.ucd_key).DefaultIfEmpty()
                                //from case_type in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseType && ca.cas_ctp_key == x.ucd_key).DefaultIfEmpty()
                                //from case_status in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CaseStatus && ca.cas_cst_key == x.ucd_key).DefaultIfEmpty()
                                //from caller_source in context.ucl_data.Where(x => x.ucd_ucl_key == (int)UclTypes.CallerSource && ca.cas_caller_source_key == x.ucd_key).DefaultIfEmpty()

                                //from accepted in context.case_assign_history.Where(x => x.cah_action == "Accepted" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()
                                //from waitingToAccept in context.case_assign_history.Where(x => x.cah_action == "Waiting to Accept" && ca.cas_key == x.cah_cas_key).OrderByDescending(x => x.cah_created_date).Take(1).DefaultIfEmpty()

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 10

                            select (new
                            {
                                ca,
                                navigator = ca.cas_created_by_name,
                                //waiting_to_accept_date = waitingToAccept.cah_created_date_utc,
                                //acceptedInfo_date = accepted.cah_created_date_utc,
                                //billingCode = billing_code != null ? billing_code.ucd_title : "",
                                //caseType = case_type != null ? case_type.ucd_title : "",
                                //caseStatus = case_status != null ? case_status.ucd_title : "",
                                //callerSource = (ca.cas_caller_source_text == "" ? (caller_source != null ? caller_source.ucd_description : "") : ca.cas_caller_source_text)
                            });

                #region ----- Filters -----


                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));

                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_callback_response_by));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    case_number = x.ca.cas_case_number,
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    stat_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_callback_response_time),
                });

                #endregion

                query = query.OrderBy(m => m.stat_time);

                List<QualityMetricsReportCls> result = new List<QualityMetricsReportCls>();
                string guids = "";

                QualityMetricsReportCls cls = new QualityMetricsReportCls();
                List<double> _meanlist = new List<double>();
                List<double> _medianlist = new List<double>();
                List<string> Stattime = query.Select(x => x.stat_time).ToList();
                int count = 0;
                if (Stattime.Count > 0)
                {
                    foreach (var item in Stattime)
                    {
                        if (item != "")
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                            _medianlist.Add(time);
                            count++;
                        }
                    }
                }
                TimeSpan _meantime = new TimeSpan();
                TimeSpan _mediantime = new TimeSpan();
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
                cls.hospitals = count;
                cls._meantime = _meantime;
                cls._mediantime = _mediantime;
                result.Add(cls);
                if (model.Facilities != null)
                {
                    foreach (var guid in model.Facilities)
                    {
                        guids += guid + ",";
                    }
                }

                guids = guids.TrimEnd(',');

                string timeframe = model.StartDate.FormatDate() + " - " + model.EndDate.FormatDate();
                var finalresult = result.Select(x => new
                {
                    id = guids,
                    hospitalname = x.hospitals,
                    mean = (string.Format("{0:00}:{1:00}:{2:00}", (x._meantime.Hours + x._meantime.Days * 24), x._meantime.Minutes, x._meantime.Seconds)),
                    median = (string.Format("{0:00}:{1:00}:{2:00}", (x._mediantime.Hours + x._mediantime.Days * 24), x._mediantime.Minutes, x._mediantime.Seconds)),
                    timecycle = timeframe
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetStatTrendsTime(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_ctp_key == 10

                            select (new
                            {
                                ca,
                            });

                #region ----- Filters -----


                cases = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(model.StartDate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(model.EndDate));

                if (model.Physicians != null)
                    cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_callback_response_by));

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                #endregion
                #region ----- Calculations -----
                var patientType = PatientType.Inpatient.ToInt();
                var query = cases.Select(x => new
                {
                    id = x.ca.cas_key,
                    facility_key = x.ca.cas_fac_key,
                    created_date = x.ca.cas_created_date,
                    case_number = x.ca.cas_case_number,
                    rrc = x.ca.cas_callback_response_by,
                    //rrc = (x.ca.AspNetUser != null && x.ca.cas_callback_response_by == x.ca.AspNetUser.Id) ? x.ca.AspNetUser.FirstName + " " + x.ca.AspNetUser.LastName : "",
                    facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                    stat_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_callback_response_time),
                    notes = x.ca.cas_callback_notes
                });

                #endregion

                query = query.OrderBy(m => m.stat_time);

                List<tPACaseAnalysis> result = new List<tPACaseAnalysis>();
                var Stattime = query.Select(x => new { x.stat_time, x.case_number, x.facility, x.rrc, x.id, x.notes }).ToList();
                List<Model.AspNetUser> users = new List<Model.AspNetUser>();
                if (model.Physicians != null)
                {
                    users = _unitOfWork.ApplicationUsers.Where(x => model.Physicians.Contains(x.Id)).ToList();
                }
                else
                {

                    model.Physicians = Stattime.Select(x => x.rrc).ToList();
                    users = _unitOfWork.ApplicationUsers.Where(x => model.Physicians.Contains(x.Id)).ToList();
                }
                if (Stattime.Count > 0)
                {
                    foreach (var item in Stattime)
                    {

                        if (item.stat_time != "")
                        {
                            tPACaseAnalysis cls = new tPACaseAnalysis();
                            var time = new TimeSpan(int.Parse(item.stat_time.Split(':')[0]), int.Parse(item.stat_time.Split(':')[1]), int.Parse(item.stat_time.Split(':')[2])).TotalMinutes;


                            if (model.TimeCycle == "byall")
                            {
                                cls.DTN = item.stat_time;
                                cls.CaseNumber = Convert.ToInt32(item.case_number);
                                cls.Case_Key = item.id;
                                cls.Facility = item.facility;
                                cls.tPAdelaynotes = item.notes;
                                string rrcname = "";
                                if (users != null)
                                {
                                    var GetName = users.Where(x => x.Id == item.rrc).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                                    if (GetName != null)
                                    {
                                        rrcname = GetName.FirstName + " " + GetName.LastName;
                                    }

                                }

                                cls.Process = rrcname;
                                result.Add(cls);
                            }
                            else
                            {
                                if (time >= 15)
                                {
                                    cls.DTN = item.stat_time;
                                    cls.CaseNumber = Convert.ToInt32(item.case_number);
                                    cls.Case_Key = item.id;
                                    cls.Facility = item.facility;
                                    cls.tPAdelaynotes = item.notes;
                                    string rrcname = "";
                                    if (users != null)
                                    {
                                        var GetName = users.Where(x => x.Id == item.rrc).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                                        if (GetName != null)
                                        {
                                            rrcname = GetName.FirstName + " " + GetName.LastName;
                                        }
                                    }

                                    cls.Process = rrcname;
                                    result.Add(cls);
                                }
                            }

                        }
                    }
                }

                var finalresult = result.Select(x => new
                {
                    id = x.Case_Key,
                    facility = x.Facility,
                    case_number = x.CaseNumber,
                    rrc = x.Process,
                    time = x.DTN,
                    notes = x.tPAdelaynotes
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetCredentialsExpiringCaseslist(DataSourceRequest request, List<Guid> Facilities, List<string> Physicians)
        {
            var result = new CredentialsExpiringCaseListing();
            var query = _unitOfWork.FacilityPhysicianRepository.Query();

            #region --- Filters ----
            if (Facilities != null && Facilities.Count > 0)
            {
                if (Facilities[0] != Guid.Empty)
                    query = query.Where(m => Facilities.Contains(m.fap_fac_key));
            }

            if (Physicians != null && Physicians.Count > 0)
            {
                if (Physicians[0] != string.Empty)
                    query = query.Where(m => Physicians.Contains(m.fap_user_key));
            }
            #endregion

            #region ---- Calculations
            //string facilityTimeZone = BLL.Settings.DefaultTimeZone;
            DateTime currentdate = DateTime.Now.ToEST();
            var cacList = query.Where(x => x.fap_is_active == true && x.AspNetUser.IsActive)
            .Select(x => new
            {
                CaseKey = x.fap_key,
                PhysicianName = x.AspNetUser.FirstName + " " + x.AspNetUser.LastName,
                FacilityName = x.facility.fac_name,
                Days = DbFunctions.DiffDays(currentdate, x.fap_end_date),
                EndDate = x.fap_end_date
            }).OrderByDescending(x => x.CaseKey);
            #endregion

            var list = cacList.ToList();

            foreach (var item in list)
            {
                if (item.Days <= 60 && item.Days >= 0)
                {
                    CredentialsExpiringCase expiringCase = new CredentialsExpiringCase();
                    expiringCase.Fac_Key = item.CaseKey;
                    expiringCase.PhysicianName = item.PhysicianName;
                    expiringCase.FacilityName = item.FacilityName;
                    if (item.EndDate.HasValue)
                    {
                        expiringCase.EndDate = item.EndDate.Value.ToString("MM-dd-yyyy");
                    }
                    else
                    {
                        expiringCase.EndDate = "";
                    }

                    result.CredentialsCases.Add(expiringCase);
                }
            }

            var finalresult = result.CredentialsCases.Select(x => new
            {
                Fac_Key = x.Fac_Key,
                FacilityName = x.FacilityName,
                PhysicianName = x.PhysicianName,
                EndDate = x.EndDate,
            }).OrderBy(x => x.EndDate).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public DataSourceResult GetCasesPendingReviewList(DataSourceRequest request, List<string> QPS, string period)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                var result = new CasesPendingReviewListing();
                var cases = from ca in context.cases
                            join user in context.AspNetUsers on ca.facility.qps_number equals user.Id into name
                            from user in name.DefaultIfEmpty()
                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_response_case_qps_reviewed != 1

                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_key,
                                ca.cas_case_number,
                                ca.facility.fac_name,
                                ca.cas_response_date_consult,
                                ca.cas_billing_date_of_consult,
                                ca.cas_metric_tpa_consult,
                                ca.cas_metric_stamp_time,
                                ca.cas_response_first_atempt,
                                ca.cas_metric_video_start_time,
                                ca.cas_metric_assesment_time,
                                ca.cas_metric_needle_time,
                                ca.cas_response_ts_notification,
                                ca.cas_metric_door_time,
                                ca.cas_metric_tpa_verbal_order_time,
                                ca.cas_created_date,
                                ca.facility.qps_number,
                                qps_name = user.FirstName + " " + user.LastName
                            });

                DateTime currentdate = DateTime.Now.ToEST();
                DateTime olddate = new DateTime();
                if (!string.IsNullOrWhiteSpace(period))
                {
                    if (period == "Last Month")
                    {
                        olddate = currentdate.AddMonths(-1);
                    }
                    else if (period == "Last 2 Months")
                    {
                        olddate = currentdate.AddMonths(-2);
                    }
                    else
                    {
                        olddate = currentdate.AddMonths(-3);
                    }
                }


                cases = cases.Where(x => DbFunctions.TruncateTime(x.cas_created_date) >= DbFunctions.TruncateTime(olddate) &&
                                             DbFunctions.TruncateTime(x.cas_created_date) <= DbFunctions.TruncateTime(currentdate));

                if (!string.IsNullOrWhiteSpace(QPS[0]))
                {
                    cases = cases.Where(x => QPS.Contains(x.qps_number));
                }
                List<caseCalculcation> _list = new List<caseCalculcation>();
                caseCalculcation obj;
                foreach (var item in cases)
                {
                    obj = new caseCalculcation();
                    obj.qps_number = item.qps_name;
                    obj.CaseKey = item.cas_key;
                    obj.CaseNumber = item.cas_case_number;
                    obj.FacilityName = item.fac_name;
                    obj.billingdateofconsult = item.cas_billing_date_of_consult.HasValue ? item.cas_billing_date_of_consult.Value.ToString("MM/dd/yyyy") : "";
                    obj.createddate = item.cas_created_date;
                    TimeSpan ts = new TimeSpan();
                    string abc = "";
                    if (item.cas_metric_needle_time.HasValue && item.cas_metric_door_time.HasValue)
                    {
                        abc = (item.cas_metric_door_time - item.cas_metric_needle_time).FormatTimeSpan();
                        ts = new TimeSpan(Convert.ToInt32(abc.Split(':')[0]), Convert.ToInt32(abc.Split(':')[1]), Convert.ToInt32(abc.Split(':')[2]));
                    }

                    obj.isNeedleTime = item.cas_metric_tpa_consult == true ? item.cas_metric_needle_time > item.cas_metric_door_time ? ts.TotalMinutes > 45 ? true : false : false : false;
                    obj.isNeedleTime45 = item.cas_metric_tpa_consult == true ? item.cas_metric_needle_time > item.cas_metric_door_time ? ts.TotalMinutes > 45 ? DiffBusinessDays(item.cas_created_date, currentdate) > 10 ? true : false : false : false : false;
                    _list.Add(obj);
                }
                /*
                string qps_name = "";

                if (!string.IsNullOrWhiteSpace(QPS[0]))
                {
                    string qpsid = QPS[0];
                    var GetQPSName = _adminService.GetAspNetUsers().Where(m => m.Id == qpsid).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                    if (GetQPSName != null)
                    {
                        qps_name = GetQPSName.FirstName + " " + GetQPSName.LastName;
                    }
                }
                */
                foreach (var item in _list)
                {
                    CasesPendingReview pendingcasses = new CasesPendingReview();
                    if (/*item.isStartToNeedleTime == true || item.isTimeFirstToNeedleTime == true || item.isStartToStamp == true || item.isStartToLoginTime == true || item.isstampToLoginTime == true || item.isarivalTostartTime == true || item.isfirstTimeLoginAttemtToVideostartTime == true || item.isfirstTimeLoginToNIHSSassTime == true ||*/ item.isNeedleTime == true || item.isNeedleTime45 == true)
                    {

                        pendingcasses.CaseKey = item.CaseKey;
                        pendingcasses.FacilityName = item.FacilityName;
                        pendingcasses.TC_CaseNumber = item.CaseNumber;
                        pendingcasses.DateOfConsult = item.billingdateofconsult;
                        /*
                                                if (!string.IsNullOrWhiteSpace(qps_name))
                                                {
                                                    pendingcasses.QPS_Name = qps_name;
                                                }
                                                else
                                                {
                                                */
                        pendingcasses.QPS_Name = item.qps_number;
                        //}

                        if (item.isNeedleTime45 == true)
                        {
                            pendingcasses.ColorRed = true;
                        }
                        else
                        {
                            pendingcasses.ColorRed = false;
                        }

                        result.CasesPendingReview.Add(pendingcasses);
                    }
                }
                var finalresult = result.CasesPendingReview.Select(x => new
                {
                    CaseKey = x.CaseKey,
                    FacilityName = x.FacilityName,
                    DateOfConsult = x.DateOfConsult,
                    QPS_Name = x.QPS_Name,
                    TC_CaseNumber = x.TC_CaseNumber,
                    ColorRed = x.ColorRed,
                }).OrderByDescending(x => x.DateOfConsult).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }

        public DataSourceResult GetCasesCompletedReviewList(DataSourceRequest request, List<string> QPS, string period)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                var result = new CasesPendingReviewListing();
                var cases = from ca in context.cases
                            join user in context.AspNetUsers on ca.facility.qps_number equals user.Id into name
                            from user in name.DefaultIfEmpty()
                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_response_case_qps_reviewed == 1

                            select (new
                            {
                                ca.cas_fac_key,
                                ca.cas_key,
                                ca.cas_case_number,
                                ca.facility.fac_name,
                                ca.cas_response_date_consult,
                                ca.cas_billing_date_of_consult,
                                ca.cas_metric_tpa_consult,
                                ca.cas_metric_stamp_time,
                                ca.cas_response_first_atempt,
                                ca.cas_metric_video_start_time,
                                ca.cas_metric_assesment_time,
                                ca.cas_metric_needle_time,
                                ca.cas_response_ts_notification,
                                ca.cas_metric_door_time,
                                ca.cas_metric_tpa_verbal_order_time,
                                ca.cas_created_date,
                                ca.facility.qps_number,
                                qps_name = user.FirstName + " " + user.LastName
                            });

                DateTime currentdate = DateTime.Now.ToEST();
                DateTime olddate = new DateTime();
                if (!string.IsNullOrWhiteSpace(period))
                {
                    if (period == "Last Month")
                    {
                        olddate = currentdate.AddMonths(-1);
                    }
                    else if (period == "Last 2 Months")
                    {
                        olddate = currentdate.AddMonths(-2);
                    }
                    else
                    {
                        olddate = currentdate.AddMonths(-3);
                    }
                }


                cases = cases.Where(x => DbFunctions.TruncateTime(x.cas_created_date) >= DbFunctions.TruncateTime(olddate) &&
                                             DbFunctions.TruncateTime(x.cas_created_date) <= DbFunctions.TruncateTime(currentdate));



                if (!string.IsNullOrWhiteSpace(QPS[0]))
                {
                    cases = cases.Where(x => QPS.Contains(x.qps_number));
                }
                List<caseCalculcation> _list = new List<caseCalculcation>();
                caseCalculcation obj;
                foreach (var item in cases)
                {
                    obj = new caseCalculcation();
                    obj.qps_number = item.qps_name;
                    obj.CaseKey = item.cas_key;
                    obj.CaseNumber = item.cas_case_number;
                    obj.FacilityName = item.fac_name;
                    obj.billingdateofconsult = item.cas_billing_date_of_consult.HasValue ? item.cas_billing_date_of_consult.Value.ToString("MM/dd/yyyy") : "";
                    obj.createddate = item.cas_created_date;
                    TimeSpan ts = new TimeSpan();
                    string abc = "";
                    if (item.cas_metric_needle_time.HasValue && item.cas_metric_door_time.HasValue)
                    {
                        abc = (item.cas_metric_door_time - item.cas_metric_needle_time).FormatTimeSpan();
                        ts = new TimeSpan(Convert.ToInt32(abc.Split(':')[0]), Convert.ToInt32(abc.Split(':')[1]), Convert.ToInt32(abc.Split(':')[2]));
                    }

                    obj.isNeedleTime = item.cas_metric_tpa_consult == true ? item.cas_metric_needle_time > item.cas_metric_door_time ? ts.TotalMinutes > 45 ? true : false : false : false;
                    obj.isNeedleTime45 = item.cas_metric_tpa_consult == true ? item.cas_metric_needle_time > item.cas_metric_door_time ? ts.TotalMinutes > 45 ? DiffBusinessDays(item.cas_created_date, currentdate) > 10 ? true : false : false : false : false;
                    _list.Add(obj);
                }

                /*
                string qps_name = "";

                if (!string.IsNullOrWhiteSpace(QPS[0]))
                {
                    string qpsid = QPS[0];
                    var GetQPSName = _adminService.GetAspNetUsers().Where(m => m.Id == qpsid).Select(x => new { x.FirstName, x.LastName }).FirstOrDefault();
                    if (GetQPSName != null)
                    {
                        qps_name = GetQPSName.FirstName + " " + GetQPSName.LastName;
                    }
                }
                */

                foreach (var item in _list)
                {
                    CasesPendingReview pendingcasses = new CasesPendingReview();
                    if (/*item.isStartToNeedleTime == true || item.isTimeFirstToNeedleTime == true || item.isStartToStamp == true || item.isStartToLoginTime == true || item.isstampToLoginTime == true || item.isarivalTostartTime == true || item.isfirstTimeLoginAttemtToVideostartTime == true || item.isfirstTimeLoginToNIHSSassTime == true ||*/ item.isNeedleTime == true || item.isNeedleTime45 == true)
                    {

                        pendingcasses.CaseKey = item.CaseKey;
                        pendingcasses.FacilityName = item.FacilityName;
                        pendingcasses.TC_CaseNumber = item.CaseNumber;
                        pendingcasses.DateOfConsult = item.billingdateofconsult;
                        /*
                        if (!string.IsNullOrWhiteSpace(qps_name))
                        {
                            pendingcasses.QPS_Name = qps_name;
                        }
                        else
                        {
                        */
                        pendingcasses.QPS_Name = item.qps_number;
                        // }

                        if (item.isNeedleTime45 == true)
                        {
                            pendingcasses.ColorRed = true;
                        }
                        else
                        {
                            pendingcasses.ColorRed = false;
                        }

                        result.CasesPendingReview.Add(pendingcasses);
                    }
                }
                var finalresult = result.CasesPendingReview.Select(x => new
                {
                    CaseKey = x.CaseKey,
                    FacilityName = x.FacilityName,
                    DateOfConsult = x.DateOfConsult,
                    QPS_Name = x.QPS_Name,
                    TC_CaseNumber = x.TC_CaseNumber,
                    ColorRed = x.ColorRed,
                }).OrderByDescending(x => x.DateOfConsult).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }

        public double DiffBusinessDays(DateTime dateValue1, DateTime dateValue)
        {
            double calcBusinessDays =
       1 + ((dateValue - dateValue1).TotalDays * 5 -
       (dateValue1.DayOfWeek - dateValue.DayOfWeek) * 2) / 7;

            if (dateValue.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (dateValue1.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            double days = Math.Round(calcBusinessDays);

            return days;
        }

        public DataSourceResult GetPhysicianVolumetricReport(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                if (model.TimeFrame == "monthly")
                {
                    model.EndDate = model.EndDate.AddMonths(1).AddDays(-1);
                }
                DateTime querysdate = model.StartDate;
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            join phys in context.AspNetUsers on ca.cas_phy_key equals phys.Id into physicians
                            from phys in physicians.DefaultIfEmpty()
                            where ca.cas_is_active == true && ca.cas_cst_key == 20 && ca.cas_created_date >= querysdate && ca.cas_created_date <= queryedate
                            select (new
                            {
                                PhysicianName = phys.FirstName + " " + phys.LastName,
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_phy_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });

                #region ----- Filters -----

                if (model.Physicians != null)
                {
                    cases = cases.Where(m => model.Physicians.Contains(m.cas_phy_key));
                }

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                //cases = cases.Where(x => x.datetime >= querysdate &&
                //                             x.datetime <= queryedate);
                #endregion
                bool isblast = true;
                if (model.Blast != null)
                {
                    if (model.Blast == "false")
                    {
                        isblast = false;
                    }
                }

                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                var result1 = cases.ToList();
                if (model.TimeFrame == "customrange" || model.TimeFrame == "daily")
                {
                    if (model.DefaultType == "casetype")
                    {
                        DateTime startdate = model.StartDate;
                        DateTime enddate = model.EndDate;

                        for (var i = startdate; startdate <= enddate;)
                        {
                            List<VolumeMetricsReport> report = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforstrokestat = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforothers = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> result = new List<VolumeMetricsReport>();
                            DateTime enddatess = startdate.AddDays(1);
                            DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                            DateTime edate = enddatess.ToUniversalTimeZone(facilityTimeZone);
                            string startdateval = startdate.ToString("MM-dd-yyyy");
                            var resultforstrokestats = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                            resultforstrokestats = resultforstrokestats.Where(x => model.Physicians.Contains(x.cas_phy_key));
                            var resultforother = result1.Where(x => x.datetime >= startdate && x.datetime < enddatess);
                            resultforother = resultforother.Where(x => model.Physicians.Contains(x.cas_phy_key));
                            resultforstrokestat = resultforstrokestats.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                StrokeAlert = model.Blast == null ? x.cas_ctp_key == 9 ? 1 : 0 : x.cas_billing_physician_blast == isblast ? x.cas_ctp_key == 9 ? 1 : 0 : 0,
                                STAT = x.cas_ctp_key == 10 ? 1 : 0,
                            }).ToList();

                            resultforothers = resultforother.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                New = x.cas_ctp_key == 163 || x.cas_ctp_key == 227 ? 1 : 0,
                                FollowUp = x.cas_ctp_key == 164 || x.cas_ctp_key == 228 ? 1 : 0,
                                EEG = x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15 ? 1 : 0,
                            }).ToList();

                            result = resultforstrokestat.Union(resultforothers).ToList();

                            report = result.GroupBy(x => x.PhysicianName).Select(x => new VolumeMetricsReport
                            {
                                TimeCycle = startdateval,
                                PhysicianName = x.Key,
                                StrokeAlert = x.Sum(s => s.StrokeAlert),
                                STAT = x.Sum(s => s.STAT),
                                New = x.Sum(s => s.New),
                                FollowUp = x.Sum(s => s.FollowUp),
                                EEG = x.Sum(s => s.EEG),
                                TotalCases = x.Sum(s => s.StrokeAlert) + x.Sum(s => s.STAT) + x.Sum(s => s.New) + x.Sum(s => s.FollowUp) + x.Sum(s => s.EEG)
                            }).ToList();
                            volumelist.AddRange(report);
                            startdate = startdate.AddDays(1);
                        }
                    }
                    else if (model.DefaultType == "billingtype")
                    {
                        DateTime startdate = model.StartDate;
                        DateTime enddate = model.EndDate;

                        for (var i = startdate; startdate <= enddate;)
                        {
                            List<VolumeMetricsReport> report = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforstrokestat = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforothers = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> result = new List<VolumeMetricsReport>();
                            DateTime enddatess = startdate.AddDays(1);
                            DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                            DateTime edate = enddatess.ToUniversalTimeZone(facilityTimeZone);
                            string startdateval = startdate.ToString("MM-dd-yyyy");
                            var resultforstrokestats = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                            resultforstrokestats = resultforstrokestats.Where(x => model.Physicians.Contains(x.cas_phy_key));
                            var resultforother = result1.Where(x => x.datetime >= startdate && x.datetime < enddatess);
                            resultforother = resultforother.Where(x => model.Physicians.Contains(x.cas_phy_key));
                            resultforstrokestat = resultforstrokestats.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                StrokeAlert = model.Blast == null ? x.cas_billing_bic_key == 1 ? 1 : 0 : x.cas_billing_physician_blast == isblast ? x.cas_billing_bic_key == 1 ? 1 : 0 : 0,
                                STAT = x.cas_billing_bic_key == 2 ? 1 : 0,
                            }).ToList();

                            resultforothers = resultforother.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                New = x.cas_billing_bic_key == 3 ? 1 : 0,
                                FollowUp = x.cas_billing_bic_key == 4 ? 1 : 0,
                                EEG = x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6 ? 1 : 0,
                            }).ToList();

                            result = resultforstrokestat.Union(resultforothers).ToList();

                            report = result.GroupBy(x => x.PhysicianName).Select(x => new VolumeMetricsReport
                            {
                                TimeCycle = startdateval,
                                PhysicianName = x.Key,
                                StrokeAlert = x.Sum(s => s.StrokeAlert),
                                STAT = x.Sum(s => s.STAT),
                                New = x.Sum(s => s.New),
                                FollowUp = x.Sum(s => s.FollowUp),
                                EEG = x.Sum(s => s.EEG),
                                TotalCases = x.Sum(s => s.StrokeAlert) + x.Sum(s => s.STAT) + x.Sum(s => s.New) + x.Sum(s => s.FollowUp) + x.Sum(s => s.EEG)
                            }).ToList();
                            volumelist.AddRange(report);
                            startdate = startdate.AddDays(1);
                        }
                    }
                }
                else if (model.TimeFrame == "monthly")
                {
                    if (model.DefaultType == "casetype")
                    {
                        //var result1 = cases.ToList();
                        DateTime startdate = model.StartDate;
                        DateTime enddate = model.EndDate.ToUniversalTimeZone(facilityTimeZone);

                        for (var i = startdate; startdate <= enddate;)
                        {
                            List<VolumeMetricsReport> report = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforstrokestat = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforothers = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> result = new List<VolumeMetricsReport>();
                            DateTime enddateofmonth = startdate.AddMonths(1);
                            DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                            DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                            string startdateval = startdate.ToString("MMMM yyyy");
                            var resultforstrokestats = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                            resultforstrokestats = resultforstrokestats.Where(x => model.Physicians.Contains(x.cas_phy_key));
                            var resultforother = result1.Where(x => x.datetime >= startdate && x.datetime < enddateofmonth);
                            resultforother = resultforother.Where(x => model.Physicians.Contains(x.cas_phy_key));

                            resultforstrokestat = resultforstrokestats.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                StrokeAlert = model.Blast == null ? x.cas_ctp_key == 9 ? 1 : 0 : x.cas_billing_physician_blast == isblast ? x.cas_ctp_key == 9 ? 1 : 0 : 0,
                                STAT = x.cas_ctp_key == 10 ? 1 : 0,
                            }).ToList();

                            resultforothers = resultforother.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                New = x.cas_ctp_key == 163 || x.cas_ctp_key == 227 ? 1 : 0,
                                FollowUp = x.cas_ctp_key == 164 || x.cas_ctp_key == 228 ? 1 : 0,
                                EEG = x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15 ? 1 : 0,
                            }).ToList();

                            result = resultforstrokestat.Union(resultforothers).ToList();

                            report = result.GroupBy(x => x.PhysicianName).Select(x => new VolumeMetricsReport
                            {
                                TimeCycle = startdateval,
                                PhysicianName = x.Key,
                                StrokeAlert = x.Sum(s => s.StrokeAlert),
                                STAT = x.Sum(s => s.STAT),
                                New = x.Sum(s => s.New),
                                FollowUp = x.Sum(s => s.FollowUp),
                                EEG = x.Sum(s => s.EEG),
                                TotalCases = x.Sum(s => s.StrokeAlert) + x.Sum(s => s.STAT) + x.Sum(s => s.New) + x.Sum(s => s.FollowUp) + x.Sum(s => s.EEG)
                            }).ToList();



                            volumelist.AddRange(report);
                            startdate = startdate.AddMonths(1);
                        }
                    }
                    else if (model.DefaultType == "billingtype")
                    {

                        DateTime startdate = model.StartDate;
                        DateTime enddate = model.EndDate.ToUniversalTimeZone(facilityTimeZone);

                        for (var i = startdate; startdate <= enddate;)
                        {
                            List<VolumeMetricsReport> report = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforstrokestat = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> resultforothers = new List<VolumeMetricsReport>();
                            List<VolumeMetricsReport> result = new List<VolumeMetricsReport>();
                            DateTime enddateofmonth = startdate.AddMonths(1);
                            DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                            DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                            string startdateval = startdate.ToString("MMMM yyyy");
                            var resultforstrokestats = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                            resultforstrokestats = resultforstrokestats.Where(x => model.Physicians.Contains(x.cas_phy_key));

                            var resultforother = result1.Where(x => x.datetime >= startdate && x.datetime < enddateofmonth);
                            resultforother = resultforother.Where(x => model.Physicians.Contains(x.cas_phy_key));

                            resultforstrokestat = resultforstrokestats.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                StrokeAlert = model.Blast == null ? x.cas_billing_bic_key == 1 ? 1 : 0 : x.cas_billing_physician_blast == isblast ? x.cas_billing_bic_key == 1 ? 1 : 0 : 0,
                                STAT = x.cas_billing_bic_key == 2 ? 1 : 0,
                            }).ToList();

                            resultforothers = resultforother.Select(x => new VolumeMetricsReport
                            {
                                PhysicianKey = x.cas_phy_key,
                                PhysicianName = x.PhysicianName,
                                New = x.cas_billing_bic_key == 3 ? 1 : 0,
                                FollowUp = x.cas_billing_bic_key == 4 ? 1 : 0,
                                EEG = x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6 ? 1 : 0,
                            }).ToList();

                            result = resultforstrokestat.Union(resultforothers).ToList();

                            report = result.GroupBy(x => x.PhysicianName).Select(x => new VolumeMetricsReport
                            {
                                TimeCycle = startdateval,
                                PhysicianName = x.Key,
                                StrokeAlert = x.Sum(s => s.StrokeAlert),
                                STAT = x.Sum(s => s.STAT),
                                New = x.Sum(s => s.New),
                                FollowUp = x.Sum(s => s.FollowUp),
                                EEG = x.Sum(s => s.EEG),
                                TotalCases = x.Sum(s => s.StrokeAlert) + x.Sum(s => s.STAT) + x.Sum(s => s.New) + x.Sum(s => s.FollowUp) + x.Sum(s => s.EEG)
                            }).ToList();

                            volumelist.AddRange(report);
                            startdate = startdate.AddMonths(1);
                        }
                    }
                }

                var finalresult = volumelist.Select(x => new
                {
                    physician = x.PhysicianName,
                    date = x.TimeCycle,
                    strokealert = x.StrokeAlert,
                    stat = x.STAT,
                    New = x.New,
                    followup = x.FollowUp,
                    eeg = x.EEG,
                    TotalCases = x.TotalCases,
                }).OrderBy(x => x.physician).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }

        public DataSourceResult GetOperationsOutliersList(DataSourceRequest request, string period)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {

                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            join timestamp in context.case_timestamp on ca.cas_key equals timestamp.cts_cas_key into time
                            from timestamp in time.DefaultIfEmpty()
                            where ca.cas_is_active == true && ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10

                            select (new
                            {
                                cas_key = ca.cas_key,
                                casetypeid = ca.cas_ctp_key,
                                case_number = ca.cas_case_number,
                                fac_name = ca.facility.fac_name,
                                start_time = DBHelper.FormatDateTime(DBHelper.ConvertToFacilityTimeZone(ca.facility.fac_timezone, ca.cas_response_ts_notification), true),
                                physician = ca.cas_history_physician_initial,
                                ResponseTime = ca.cas_ctp_key == 9 ? timestamp.cts_response_sec / 60 : 0,
                                TS_ResponseTime = DBHelper.FormatSeconds(timestamp.cts_response_sec),
                                CallBackResponseTime = ca.cas_ctp_key == 10 ? timestamp.cts_callback_to_tsnotification / 60 : 0,
                                CallBack_Response = DBHelper.FormatSeconds(timestamp.cts_callback_to_tsnotification),
                                ca.cas_created_date
                            });

                DateTime currentdate = DateTime.Now.ToEST();
                DateTime olddate = new DateTime();
                if (!string.IsNullOrWhiteSpace(period))
                {
                    if (period == "Last Month")
                    {
                        olddate = currentdate.AddMonths(-1);
                    }
                    else if (period == "Last 2 Months")
                    {
                        olddate = currentdate.AddMonths(-2);
                    }
                    else if (period == "Last 3 Months")
                    {
                        olddate = currentdate.AddMonths(-3);
                    }
                    else if (period == "CurrentDate")
                    {
                        olddate = currentdate;
                    }
                    else
                    {
                        olddate = currentdate.AddDays(-3);
                    }
                }


                cases = cases.Where(x => DbFunctions.TruncateTime(x.cas_created_date) >= DbFunctions.TruncateTime(olddate) &&
                                             DbFunctions.TruncateTime(x.cas_created_date) <= DbFunctions.TruncateTime(currentdate));

                List<OperationsOutliers> _list = new List<OperationsOutliers>();
                OperationsOutliers obj;
                var result = cases.Where(x => (x.ResponseTime > 10 && x.casetypeid == 9) || (x.CallBackResponseTime > 15 && x.casetypeid == 10)).ToList();
                string casekey = string.Join(",", result.Select(x => x.cas_key).ToList());

                List<UserColorOutliers> ColorList = _unitOfWork.SqlQuery<UserColorOutliers>(string.Format("Exec sp_get_case_color_outliers @casekey = '{0}'", casekey)).ToList();
                foreach (var item in result)
                {

                    obj = new OperationsOutliers();
                    obj.CaseKey = item.cas_key;
                    obj.CaseNumber = item.case_number;
                    obj.CaseType = (item.casetypeid == 9) ? "Stroke Alert" : "STAT Consult";
                    obj.StartTime = item.start_time;
                    obj.FacilityName = item.fac_name;
                    obj.Physician_Initials = item.physician;
                    obj.Physician_Status = PhysicianColors(item.cas_key, item.physician, ColorList);
                    if (item.casetypeid == 9)
                    {
                        obj.TS_Response_Time = item.TS_ResponseTime;
                    }
                    else
                    {
                        obj.CallBack_Response_Time = item.CallBack_Response;
                    }
                    //obj.TS_Response_Time = (item.casetypeid == 9) ? item.TS_ResponseTime : item.CallBack_Response;
                    obj.Created_Date = item.cas_created_date;
                    _list.Add(obj);

                    //else if (item.CallBackResponseTime > 15 && item.casetypeid == 10)
                    //{
                    //    obj = new OperationsOutliers();
                    //    obj.CaseKey = item.cas_key;
                    //    obj.CaseNumber = item.case_number;
                    //    obj.CaseType = "STAT Consult";
                    //    obj.StartTime = item.start_time;
                    //    obj.FacilityName = item.fac_name;
                    //    obj.Physician_Initials = item.physician;
                    //    obj.Physician_Status = PhysicianColors(item.cas_key, item.physician, ColorList);
                    //    obj.CallBack_Response_Time = item.CallBack_Response;
                    //    obj.Created_Date = item.cas_created_date;
                    //    _list.Add(obj);
                    //}
                }

                var finalresult = _list.OrderByDescending(x => x.Created_Date).AsQueryable();
                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }

        public string PhysicianColors(int cas_key, string physician, List<UserColorOutliers> ColorList)
        {
            //using (var context = new Model.TeleSpecialistsContext())
            //{

            //context.Configuration.AutoDetectChangesEnabled = false;
            //context.Configuration.ProxyCreationEnabled = false;
            //context.Configuration.LazyLoadingEnabled = false;
            //var phy_initials = physician.Split('/').ToList();
            //var cases = from userlog in _unitOfWork.PhysicianStatusLogRepository.Query()
            //            where userlog.psl_cas_key == cas_key
            //            join phy_status in _unitOfWork.PhysicianStatusRepository.Query() on userlog.psl_phs_key equals phy_status.phs_key into status
            //            from phy_status in status.DefaultIfEmpty()
            //                //join user in _unitOfWork.UserRepository.Query() on userlog.psl_user_key equals user.Id into users
            //                //from user in users.DefaultIfEmpty()
            //                //where user.IsActive == true && user.IsDeleted == false && user.IsDisable == false
            //            select (new
            //            {
            //                userinitial = userlog.AspNetUser.UserInitial,
            //                usercolor = userlog.psl_phs_key == phy_status.phs_key ? phy_status.phs_color_code : "",
            //                userstatus = userlog.psl_status_name,
            //            });

            //List<string> checklist = new List<string>();
            //var detail = cases.ToList();

            var detail = ColorList.Where(x => x.psl_cas_key == cas_key).FirstOrDefault();
            string html = "<div class='physicianstatusdiv'>";
            if (detail != null)
            {
                string[] status = detail.psl_status_name.Split('/');
                string[] color = detail.psl_status_color.Split('/');
                string[] userIntitial = detail.UserInitial.Split('/');

                for (var i = 0; i < userIntitial.Length; i++)
                {
                    if (color[i] == "")
                    {
                        color[i] = "Black";
                    }
                    if (status[i] == "")
                    {
                        status[i] = "Waiting to Accept";
                    }
                    html += "<span title='" + status[i] + "' style='color: " + color[i] + ";font-weight:bold;'>" + userIntitial[i] + "</span>/";
                }
            }
            string result = html.TrimEnd('/');
            result += "</div>";
            return result;
            //}
        }

        #endregion

        #region Graph Quality Report by Ahmad junaid
        public DataSourceResult GetVolumeGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
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
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----
                if (model.TimeFrame == "monthly")
                {
                    model.EndDate = model.EndDate.AddMonths(1).AddDays(-1);
                }
                DateTime querysdate = model.StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                var casesforstrokestat = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);
                var casesforothers = cases.Where(x => DbFunctions.TruncateTime(x.datetime) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.datetime) <= DbFunctions.TruncateTime(model.EndDate));

                #endregion

                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                int casescount = 0;

                VolumeMetricsReport report = new VolumeMetricsReport();
                report.TimeCycle = model.StartDate.ToString("MM-dd-yyyy") + " - " + model.EndDate.ToString("MM-dd-yyyy");

                if (model.DefaultType == "casetype")
                {
                    if (model.Blast != null)
                    {
                        bool isblast = true;
                        if (model.Blast == "false")
                        {
                            isblast = false;
                        }
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                    }
                    else
                    {
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                    }

                    report.STAT = casesforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                    report.New = casesforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                    report.FollowUp = casesforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                    report.EEG = casesforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                    casescount = report.StrokeAlert + report.STAT + report.New + report.FollowUp + report.EEG;
                }
                else if (model.DefaultType == "billingtype")
                {
                    if (model.Blast != null)
                    {
                        bool isblast = true;
                        if (model.Blast == "false")
                        {
                            isblast = false;
                        }
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                    }
                    else
                    {
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                    }

                    report.STAT = casesforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                    report.New = casesforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                    report.FollowUp = casesforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                    report.EEG = casesforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                    casescount = report.StrokeAlert + report.STAT + report.New + report.FollowUp + report.EEG;
                }
                volumelist.Add(report);

                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    strokealert = x.StrokeAlert,
                    stat = x.STAT,
                    New = x.New,
                    followup = x.FollowUp,
                    eeg = x.EEG,
                    casecount = casescount
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetBedsideMetricsGraph(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            where ca.cas_is_active == true

                            select (new
                            {
                                ca,
                            });

                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();

                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                foreach (var month in last12Months)
                {

                    DateTime date = Convert.ToDateTime(month);

                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);

                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));

                    if (model.WorkFlowType != null)
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }


                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    #endregion
                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        bedside_response_time = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                        bedside_response_time_cmp = x.ca.cas_response_first_atempt < x.ca.cas_metric_stamp_time ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_stamp_time, x.ca.cas_response_first_atempt),
                    });
                    #endregion

                    query = query.OrderBy(m => m.bedside_response_time);
                    List<string> bedsidetime = query.Select(x => x.bedside_response_time).ToList();
                    if (bedsidetime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in bedsidetime)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Bedside Response Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }


                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetArivalToNeedleGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases


                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });


                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }

                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));

                    if (model.WorkFlowType != null)
                    {
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    }
                    else
                    {
                        List<int> workflowlist = new List<int>();
                        workflowlist.Add(1);
                        workflowlist.Add(3);
                        model.WorkFlowType = workflowlist;
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    }
                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }
                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    cases1 = cases1.Where(c => c.ca.cas_patient_type != 4);
                    #endregion

                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        arrival_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_door_time),
                    });

                    #endregion

                    query = query.OrderBy(m => m.arrival_needle_time);

                    List<string> arivaltoneedle = query.Select(x => x.arrival_needle_time).ToList();
                    if (arivaltoneedle.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in arivaltoneedle)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Arrival To Needle Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetVerbalTotPAGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });

                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));

                    if (model.WorkFlowType != null)
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));


                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }


                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    #endregion

                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        //verbal_order_to_ocopr_order = DBHelper.FormatSeconds(x.ca.cas_metric_pa_ordertime, x.ca.cas_metric_tpa_verbal_order_time),
                        verbal_order_to_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_tpa_verbal_order_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_tpa_verbal_order_time),
                    });
                    #endregion

                    query = query.OrderBy(m => m.verbal_order_to_needle_time);

                    List<string> verbalorder = query.Select(x => x.verbal_order_to_needle_time).ToList();
                    if (verbalorder.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in verbalorder)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Alteplase early mix decision To tPA Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetOnScreenGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases


                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });



                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }


                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));


                    if (model.WorkFlowType != null)
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }


                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    #endregion

                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        on_screen_time = x.ca.cas_metric_video_end_time < x.ca.cas_metric_video_start_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_video_end_time, x.ca.cas_metric_video_start_time),
                    });
                    #endregion

                    query = query.OrderBy(m => m.on_screen_time);

                    List<string> onscreen = query.Select(x => x.on_screen_time).ToList();
                    if (onscreen.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in onscreen)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "On Screen Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetHandleTimeGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });



                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Physicians != null && model.Physicians.Count > 0)
                {
                    if (model.Physicians[0] != string.Empty)
                        cases = cases.Where(m => model.Physicians.Contains(m.ca.cas_created_by));
                }
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));

                    if (model.WorkFlowType != null)
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);

                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_created_by));

                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }


                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    #endregion

                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        handle_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_stamp_time),
                    });
                    #endregion

                    query = query.OrderBy(m => m.handle_time);

                    List<string> handletime = query.Select(x => x.handle_time).ToList();
                    if (handletime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in handletime)
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
                        TimeSpan _mediantime = new TimeSpan();
                        string mentime = "00:00:00";
                        string medtime = "00:00:00";
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
                            var mendays = _meantime.Days;
                            var menhours = _meantime.Hours;
                            var menminutes = _meantime.Minutes;
                            var menseconds = _meantime.Seconds;
                            var menmiliseconds = _meantime.Milliseconds;
                            var mendaystohours = 0;
                            if (mendays != 0)
                            {
                                mendaystohours = mendays * 24;
                            }

                            var mentotalhours = mendaystohours + menhours;
                            mentime = mentotalhours + ":" + menminutes + ":" + menseconds;


                            var meddays = _mediantime.Days;
                            var medhours = _mediantime.Hours;
                            var medminutes = _mediantime.Minutes;
                            var medseconds = _mediantime.Seconds;
                            var medmiliseconds = _mediantime.Milliseconds;
                            var meddaystohours = 0;
                            if (meddays != 0)
                            {
                                meddaystohours = meddays * 24;
                            }

                            var medtotalhours = meddaystohours + medhours;
                            medtime = medtotalhours + ":" + medminutes + ":" + medseconds;
                        }
                        catgory.Add(monthName);
                        meanlist.Add(mentime);
                        medianlist.Add(medtime);

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Handle Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("MM/dd/yyyy");//mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetActivationGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });
                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));
                    if (!string.IsNullOrEmpty(facilityAdminId))
                    {
                        var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                                 .Select(m => m.Facility).ToList();

                        cases1 = cases1.Where(m => facilities.Contains(m.ca.cas_fac_key));
                    }

                    if (model.WorkFlowType != null)
                    {
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    }
                    //else
                    //{
                    //    List<int> workflowlist = new List<int>();
                    //    workflowlist.Add(1);
                    //    workflowlist.Add(3);
                    //    model.WorkFlowType = workflowlist;
                    //    cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    //}
                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));

                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));


                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }


                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }

                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    cases1 = cases1.Where(c => c.ca.cas_patient_type != 4);
                    #endregion

                    #region ----- Calculations -----
                    var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        activation_time = x.ca.cas_patient_type != patientType ? x.ca.cas_response_ts_notification < x.ca.cas_metric_door_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_metric_door_time) : "",
                    });
                    #endregion

                    query = query.OrderBy(m => m.activation_time);

                    List<string> onscreen = query.Select(x => x.activation_time).ToList();
                    if (onscreen.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in onscreen)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "ED Stroke Alert Activation Report";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetSymptomstoNeedleGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases
                            where ca.cas_is_active == true
                            select (new
                            {
                                ca
                            });
                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));
                    if (!string.IsNullOrEmpty(facilityAdminId))
                    {
                        var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                                 .Select(m => m.Facility).ToList();
                        cases1 = cases1.Where(m => facilities.Contains(m.ca.cas_fac_key));
                    }
                    if (model.WorkFlowType != null)
                    {
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    }
                    else
                    {
                        List<int> workflowlist = new List<int>();
                        workflowlist.Add(1);
                        workflowlist.Add(3);
                        model.WorkFlowType = workflowlist;
                        cases1 = cases1.Where(m => model.WorkFlowType.Contains((m.ca.cas_patient_type.HasValue ? m.ca.cas_patient_type.Value : -1)) && m.ca.cas_ctp_key == (int)CaseType.StrokeAlert);
                    }
                    if (model.CallType != null)
                        cases1 = cases1.Where(m => model.CallType.Contains((m.ca.cas_call_type.HasValue ? m.ca.cas_call_type.Value : -1)) && m.ca.cas_ctp_key == ((int)CaseType.StrokeAlert));
                    if (model.CallerSource != null)
                    {
                        cases1 = cases1.Where(m => model.CallerSource.Contains((m.ca.cas_caller_source_key.HasValue ? m.ca.cas_caller_source_key.Value : -1)));
                    }
                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.BillingCode != null)
                        cases1 = cases1.Where(m => m.ca.cas_billing_bic_key.HasValue && model.BillingCode.Contains(m.ca.cas_billing_bic_key.Value));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                    if (model.QPSNumbers != null && model.QPSNumbers.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.QPSNumbers.Contains(c.ca.facility.qps_number));//cases = cases.Where(c => c.ca.facility.qps_number.HasValue && model.QPSNumbers.Contains(c.ca.facility.qps_number.Value));
                    }
                    if (model.tPA != null && model.tPA.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.tPA.Contains(c.ca.cas_metric_tpa_consult));
                    }
                    #region TCARE-479
                    if (model.eAlert != null && model.eAlert.Count > 0)
                    {
                        cases1 = cases1.Where(c => model.eAlert.Contains(c.ca.cas_is_ealert));
                    }
                    #endregion
                    //cases1 = cases1.Where(m => m.ca.cas_fac_key == Id);
                    cases1 = cases1.Where(c => c.ca.cas_patient_type == 4);
                    #endregion
                    #region ----- Calculations -----
                    //var patientType = PatientType.Inpatient.ToInt();
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        symptoms_to_needle_time = x.ca.cas_metric_needle_time < x.ca.cas_metric_symptom_onset_during_ed_stay_time ? "00:00:00" : DBHelper.FormatSeconds(x.ca.cas_metric_needle_time, x.ca.cas_metric_symptom_onset_during_ed_stay_time),
                        symptoms_to_needle_time_cmp = x.ca.cas_metric_needle_time_est < x.ca.cas_metric_symptom_onset_during_ed_stay_time_est ? 0 : DBHelper.DiffSeconds(x.ca.cas_metric_needle_time_est, x.ca.cas_metric_symptom_onset_during_ed_stay_time_est),
                    });
                    #endregion

                    query = query.OrderBy(m => m.symptoms_to_needle_time);
                    List<string> symptomsneedle_time = query.Select(x => x.symptoms_to_needle_time).ToList();
                    if (symptomsneedle_time.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in symptomsneedle_time)
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
                        TimeSpan _mediantime = new TimeSpan();
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
                        catgory.Add(monthName);
                        meanlist.Add(_meantime.ToString());
                        medianlist.Add(_mediantime.ToString());

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Symptom to Needle Time Report";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetStatTimeGraphModal(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;

                var cases = from ca in context.cases

                            where ca.cas_is_active == true

                            select (new
                            {
                                ca
                            });



                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                QualityMetricsGraphReport graph = new QualityMetricsGraphReport();
                graph.Mean = "Mean";
                graph.Median = "Median";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                List<string> medianlist = new List<string>();
                int count = 0;
                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.ca.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.ca.cas_fac_key));
                }

                foreach (var month in last12Months)
                {
                    DateTime date = Convert.ToDateTime(month);
                    string monthName = date.ToString("MMMM");

                    DateTime StartDate = new DateTime(date.Year, date.Month, 1);
                    DateTime EndDate = StartDate.AddMonths(1).AddDays(-1);
                    string sdt = StartDate.ToString();
                    string edt = EndDate.ToString();
                    DateTime sdate = Convert.ToDateTime(sdt);
                    DateTime edate = Convert.ToDateTime(edt);
                    #region ----- Filters -----
                    var cases1 = cases.Where(x => DbFunctions.TruncateTime(x.ca.cas_created_date) >= DbFunctions.TruncateTime(sdate) &&
                                         DbFunctions.TruncateTime(x.ca.cas_created_date) <= DbFunctions.TruncateTime(edate));

                    if (model.CaseStatus != null)
                        cases1 = cases1.Where(m => model.CaseStatus.Contains(m.ca.cas_cst_key));
                    if (model.CaseType != null)
                        cases1 = cases1.Where(m => model.CaseType.Contains(m.ca.cas_ctp_key));
                    if (model.Physicians != null)
                        cases1 = cases1.Where(m => model.Physicians.Contains(m.ca.cas_phy_key));

                    #endregion

                    #region ----- Calculations -----
                    var query = cases1.Select(x => new
                    {
                        facility = (x.ca.facility != null && !String.IsNullOrEmpty(x.ca.facility.fac_name)) ? x.ca.facility.fac_name : "",
                        stat_time = DBHelper.FormatSeconds(x.ca.cas_response_ts_notification, x.ca.cas_callback_response_time),
                    });
                    #endregion

                    query = query.OrderBy(m => m.stat_time);

                    List<string> stattime = query.Select(x => x.stat_time).ToList();
                    if (stattime.Count > 0)
                    {
                        List<double> _meanlist = new List<double>();
                        List<double> _medianlist = new List<double>();
                        foreach (var item in stattime)
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
                        TimeSpan _mediantime = new TimeSpan();
                        string mentime = "00:00:00";
                        string medtime = "00:00:00";
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
                            var mendays = _meantime.Days;
                            var menhours = _meantime.Hours;
                            var menminutes = _meantime.Minutes;
                            var menseconds = _meantime.Seconds;
                            var menmiliseconds = _meantime.Milliseconds;
                            var mendaystohours = 0;
                            if (mendays != 0)
                            {
                                mendaystohours = mendays * 24;
                            }

                            var mentotalhours = mendaystohours + menhours;
                            mentime = mentotalhours + ":" + menminutes + ":" + menseconds;


                            var meddays = _mediantime.Days;
                            var medhours = _mediantime.Hours;
                            var medminutes = _mediantime.Minutes;
                            var medseconds = _mediantime.Seconds;
                            var medmiliseconds = _mediantime.Milliseconds;
                            var meddaystohours = 0;
                            if (meddays != 0)
                            {
                                meddaystohours = meddays * 24;
                            }

                            var medtotalhours = meddaystohours + medhours;
                            medtime = medtotalhours + ":" + medminutes + ":" + medseconds;
                        }
                        catgory.Add(monthName);
                        meanlist.Add(mentime);
                        medianlist.Add(medtime);

                    }
                }
                if (catgory.Count > 0)
                {
                    graph.Title = "Stat Response Time";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = new DateTime(date.Year, date.Month, 1);
                    graph.MinDate = mindate.Date.ToString("MM/dd/yyyy");//mindate.Date.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                    graph.MedianCalculation = medianlist;
                }

                List<QualityMetricsGraphReport> list = new List<QualityMetricsGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.MedianCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        public DataSourceResult GetPhysicianVolumetricGraph(DataSourceRequest request, QualityMetricsViewModel model, string facilityAdminId, string Status)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                List<string> _PhyList = new List<string>();
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                var cases = from ca in context.cases

                            where ca.cas_is_active == true && ca.cas_cst_key == 20

                            select (new
                            {
                                physicianname = DBHelper.GetUserFullName(ca.cas_phy_key),
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_phy_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----
                if (model.TimeFrame == "monthly")
                {
                    model.EndDate = model.EndDate.AddMonths(1).AddDays(-1);
                }
                DateTime querysdate = model.StartDate;
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);
                if (model.Physicians != null)
                {
                    cases = cases.Where(m => model.Physicians.Contains(m.cas_phy_key));
                    _PhyList = model.Physicians;
                }
                else
                {
                    _PhyList = cases.Select(x => x.cas_phy_key).Distinct().ToList();
                }

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                cases = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);

                #endregion
                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                List<int> DataList = new List<int>();
                List<string> Category = new List<string>();
                var result1 = cases.ToList();
                if (model.TimeFrame == "customrange" || model.TimeFrame == "daily")
                {
                    DateTime startdate = model.StartDate;
                    DateTime enddate = model.EndDate;

                    for (var i = startdate; startdate <= enddate;)
                    {
                        DateTime enddatess = startdate.AddDays(1);
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddatess.ToUniversalTimeZone(facilityTimeZone);
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        var resultforstrokestat = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                        var resultforothers = result1.Where(x => x.datetime >= startdate && x.datetime < enddatess);
                        report.TimeCycle = startdate.ToString("MM-dd-yyyy");
                        if (model.DefaultType == "casetype")
                        {
                            if (Status == "strokealert")
                            {
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                        report.Name = "StrokeAlert (Blast Excluded)";
                                    }
                                    else if (model.Blast == "true")
                                    {
                                        report.Name = "StrokeAlert (Blast Only)";
                                    }

                                    int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                                    DataList.Add(casls);
                                }
                                else
                                {
                                    report.Name = "StrokeAlert";
                                    int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                                    DataList.Add(casls);
                                }
                            }
                            else if (Status == "stat")
                            {
                                report.Name = "STAT";
                                int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "New")
                            {
                                report.Name = "New";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "followup")
                            {
                                report.Name = "FU";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "eeg")
                            {
                                report.Name = "EEG";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "alltypes")
                            {
                                int casls = 0;
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                    }

                                    casls += resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                                }
                                else
                                {
                                    casls += resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                                }

                                casls += resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                                DataList.Add(casls);
                                report.Name = "All Types";

                            }
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (Status == "strokealert")
                            {
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        report.Name = "StrokeAlert (Blast Excluded)";
                                        isblast = false;
                                    }
                                    else if (model.Blast == "true")
                                    {
                                        report.Name = "StrokeAlert (Blast Only)";
                                    }
                                    int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                                    DataList.Add(casls);
                                }
                                else
                                {
                                    report.Name = "StrokeAlert";
                                    int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                                    DataList.Add(casls);
                                }
                            }
                            else if (Status == "stat")
                            {
                                report.Name = "STAT";
                                int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "New")
                            {
                                report.Name = "New";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "followup")
                            {
                                report.Name = "FU";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "eeg")
                            {
                                report.Name = "EEG";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "alltypes")
                            {
                                int casls = 0;
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                    }

                                    casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                                }
                                else
                                {
                                    casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                                }

                                casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                                DataList.Add(casls);
                                report.Name = "All Types";

                            }
                        }
                        report.xAxisLabel = "Date";
                        Category.Add(startdate.ToString("MM-dd"));
                        volumelist.Add(report);
                        startdate = startdate.AddDays(1);
                    }

                }
                if (model.TimeFrame == "monthly")
                {
                    DateTime startdate = model.StartDate;
                    DateTime enddate = model.EndDate.ToUniversalTimeZone(facilityTimeZone);

                    for (var i = startdate; startdate <= enddate;)
                    {
                        VolumeMetricsReport report = new VolumeMetricsReport();
                        DateTime enddateofmonth = startdate.AddMonths(1);
                        DateTime sdate = startdate.ToUniversalTimeZone(facilityTimeZone);
                        DateTime edate = enddateofmonth.ToUniversalTimeZone(facilityTimeZone);
                        var resultforstrokestat = result1.Where(x => x.datetime >= sdate && x.datetime < edate);
                        var resultforothers = result1.Where(x => x.datetime >= startdate && x.datetime < enddateofmonth);
                        report.TimeCycle = startdate.ToString("MMMM yyyy");
                        if (model.DefaultType == "casetype")
                        {
                            if (Status == "strokealert")
                            {
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                        report.Name = "StrokeAlert (Blast Excluded)";
                                    }
                                    else if (model.Blast == "true")
                                    {
                                        report.Name = "StrokeAlert (Blast Only)";
                                    }

                                    int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                                    DataList.Add(casls);
                                }
                                else
                                {
                                    report.Name = "StrokeAlert";
                                    int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                                    DataList.Add(casls);
                                }
                            }
                            else if (Status == "stat")
                            {
                                report.Name = "STAT";
                                int casls = resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "New")
                            {
                                report.Name = "New";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "followup")
                            {
                                report.Name = "FU";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "eeg")
                            {
                                report.Name = "EEG";
                                int casls = resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "alltypes")
                            {
                                int casls = 0;
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                    }

                                    casls += resultforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                                }
                                else
                                {
                                    casls += resultforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                                }

                                casls += resultforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                                casls += resultforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                                DataList.Add(casls);
                                report.Name = "All Types";

                            }
                        }
                        else if (model.DefaultType == "billingtype")
                        {
                            if (Status == "strokealert")
                            {
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        report.Name = "StrokeAlert (Blast Excluded)";
                                        isblast = false;
                                    }
                                    else if (model.Blast == "true")
                                    {
                                        report.Name = "StrokeAlert (Blast Only)";
                                    }
                                    int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                                    DataList.Add(casls);
                                }
                                else
                                {
                                    report.Name = "StrokeAlert";
                                    int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                                    DataList.Add(casls);
                                }
                            }
                            else if (Status == "stat")
                            {
                                report.Name = "STAT";
                                int casls = resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "New")
                            {
                                report.Name = "New";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "followup")
                            {
                                report.Name = "FU";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "eeg")
                            {
                                report.Name = "EEG";
                                int casls = resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                                DataList.Add(casls);
                            }
                            else if (Status == "alltypes")
                            {
                                int casls = 0;
                                if (model.Blast != null)
                                {
                                    bool isblast = true;
                                    if (model.Blast == "false")
                                    {
                                        isblast = false;
                                    }

                                    casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                                }
                                else
                                {
                                    casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                                }

                                casls += resultforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                                casls += resultforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                                DataList.Add(casls);
                                report.Name = "All Types";

                            }
                        }
                        report.xAxisLabel = "Month";
                        Category.Add(startdate.ToString("MMMM yyyy"));
                        volumelist.Add(report);
                        startdate = startdate.AddMonths(1);
                    }
                }

                #region commented code
                /* if (Status != "")
                 {
                     string Name = "";
                     List<int> datalist = new List<int>();
                     if (Status == "strokealert")
                     {
                         foreach (var item in volumelist)
                         {
                             datalist.Add(item.StrokeAlert);
                             if (model.Blast == "true")
                             {
                                 Name = "StrokeAlert (Blast Only)";
                             }
                             else if (model.Blast == "false")
                             {
                                 Name = "StrokeAlert (Blast Excluded)";
                             }
                             else
                             {
                                 Name = "StrokeAlert";
                             }

                         }
                     }
                     else if (Status == "stat")
                     {
                         foreach (var item in volumelist)
                         {
                             datalist.Add(item.STAT);
                             Name = "STAT";
                         }
                     }
                     else if (Status == "New")
                     {
                         foreach (var item in volumelist)
                         {
                             datalist.Add(item.New);
                             Name = "New";
                         }
                     }
                     else if (Status == "followup")
                     {
                         foreach (var item in volumelist)
                         {
                             datalist.Add(item.FollowUp);
                             Name = "FU";
                         }
                     }
                     else if (Status == "eeg")
                     {
                         foreach (var item in volumelist)
                         {
                             datalist.Add(item.EEG);
                             Name = "EEG";
                         }
                     }
                     VolumeMetricsReport metrics = new VolumeMetricsReport();
                     metrics.Category = Category;
                     metrics.Name = Name;
                     metrics.xAxisLabel = volumelist.Select(x => x.xAxisLabel).FirstOrDefault();
                     metrics.DataList = datalist;
                     volumelist.Add(metrics);
                 }*/

                #endregion
                List<VolumeMetricsReport> finalreportlist = new List<VolumeMetricsReport>();
                if (volumelist.Count > 0)
                {
                    VolumeMetricsReport finalreport = new VolumeMetricsReport();
                    finalreport.Category = Category;
                    finalreport.DataList = DataList;
                    finalreport.Name = volumelist.Select(x => x.Name).FirstOrDefault();
                    finalreport.xAxisLabel = volumelist.Select(x => x.xAxisLabel).FirstOrDefault();
                    finalreportlist.Add(finalreport);
                }
                var finalresult = finalreportlist.AsQueryable();
                //var finalresult = finalreportlist.Select(x => new
                //{
                //    name = x.Name,
                //    xlabel = x.xAxisLabel,
                //    category = x.Category,
                //    datalist = x.DataList,
                //}).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }


        public DataSourceResult GetPhysicianVolumePieChart(QualityMetricsViewModel model, string facilityAdminId, DataSourceRequest request)
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
                                ca.cas_billing_physician_blast,
                                ca.cas_fac_key,
                                ca.cas_phy_key,
                                ca.cas_ctp_key,
                                ca.cas_billing_bic_key,
                                datetime = model.DefaultType == "casetype" ? ca.cas_ctp_key == 9 || ca.cas_ctp_key == 10 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult : ca.cas_billing_bic_key == 1 || ca.cas_billing_bic_key == 2 ? ca.cas_response_ts_notification : ca.cas_billing_date_of_consult,
                            });
                #region ----- Filters -----
                if (model.TimeFrame == "monthly")
                {
                    model.EndDate = model.EndDate.AddMonths(1).AddDays(-1);
                }
                DateTime querysdate = model.StartDate.ToUniversalTimeZone(facilityTimeZone);
                DateTime queryedate = model.EndDate.AddDays(1).ToUniversalTimeZone(facilityTimeZone);

                if (model.Physicians != null)
                {
                    cases = cases.Where(m => model.Physicians.Contains(m.cas_phy_key));
                }

                if (model.Facilities != null && model.Facilities.Count > 0)
                {
                    if (model.Facilities[0] != Guid.Empty)
                        cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                }
                else if (model.states != null && model.states.Count > 0)
                {
                    IQueryable<int?> States = model.states.AsQueryable();
                    model.Facilities = _lookUpService.GetAllFacilityByState(null, States).Select(x => x.fac_key).ToList();
                    if (model.Facilities != null && model.Facilities.Count > 0)
                    {
                        if (model.Facilities[0] != Guid.Empty)
                            cases = cases.Where(m => model.Facilities.Contains(m.cas_fac_key));
                    }
                }
                else if (!string.IsNullOrEmpty(facilityAdminId))
                {
                    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(facilityAdminId)
                                                             .Select(m => m.Facility).ToList();
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key));
                }
                var casesforstrokestat = cases.Where(x => x.datetime >= querysdate &&
                                             x.datetime <= queryedate);
                var casesforothers = cases.Where(x => DbFunctions.TruncateTime(x.datetime) >= DbFunctions.TruncateTime(model.StartDate) &&
                                             DbFunctions.TruncateTime(x.datetime) <= DbFunctions.TruncateTime(model.EndDate));

                #endregion

                List<VolumeMetricsReport> volumelist = new List<VolumeMetricsReport>();
                int casescount = 0;

                VolumeMetricsReport report = new VolumeMetricsReport();
                report.TimeCycle = model.StartDate.ToString("MM-dd-yyyy") + " - " + model.EndDate.ToString("MM-dd-yyyy");

                if (model.DefaultType == "casetype")
                {
                    if (model.Blast != null)
                    {
                        bool isblast = true;
                        if (model.Blast == "false")
                        {
                            isblast = false;
                        }
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_ctp_key == 9 && x.cas_billing_physician_blast == isblast).Count();
                    }
                    else
                    {
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_ctp_key == 9).Count();
                    }

                    report.STAT = casesforstrokestat.Where(x => x.cas_ctp_key == 10).Count();
                    report.New = casesforothers.Where(x => x.cas_ctp_key == 163 || x.cas_ctp_key == 227).Count();
                    report.FollowUp = casesforothers.Where(x => x.cas_ctp_key == 164 || x.cas_ctp_key == 228).Count();
                    report.EEG = casesforothers.Where(x => x.cas_ctp_key == 13 || x.cas_ctp_key == 14 || x.cas_ctp_key == 15).Count();
                    casescount = report.StrokeAlert + report.STAT + report.New + report.FollowUp + report.EEG;
                }
                else if (model.DefaultType == "billingtype")
                {
                    if (model.Blast != null)
                    {
                        bool isblast = true;
                        if (model.Blast == "false")
                        {
                            isblast = false;
                        }
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_billing_bic_key == 1 && x.cas_billing_physician_blast == isblast).Count();
                    }
                    else
                    {
                        report.StrokeAlert = casesforstrokestat.Where(x => x.cas_billing_bic_key == 1).Count();
                    }

                    report.STAT = casesforstrokestat.Where(x => x.cas_billing_bic_key == 2).Count();
                    report.New = casesforothers.Where(x => x.cas_billing_bic_key == 3).Count();
                    report.FollowUp = casesforothers.Where(x => x.cas_billing_bic_key == 4).Count();
                    report.EEG = casesforothers.Where(x => x.cas_billing_bic_key == 5 || x.cas_billing_bic_key == 6).Count();
                    casescount = report.StrokeAlert + report.STAT + report.New + report.FollowUp + report.EEG;
                }
                volumelist.Add(report);

                var finalresult = volumelist.Select(x => new
                {
                    date = x.TimeCycle,
                    strokealert = x.StrokeAlert,
                    stat = x.STAT,
                    New = x.New,
                    followup = x.FollowUp,
                    eeg = x.EEG,
                    casecount = casescount
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        #endregion



        #region shiraz CWH
        public DataSourceResult GetCWHData(DataSourceRequest request, List<Guid> facilities, DateTime FromMonth, DateTime ToMonth)
        {
            string prevFacilityName = "";
            List<CWHReport> volumelist = new List<CWHReport>();
            List<CWHReport> volumelist1 = new List<CWHReport>();
            List<CaseModel> cases = new List<CaseModel>();
            cases = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData2 @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth.AddMonths(1).AddDays(-1))).ToList();
            CWHReport report;
            DateTime enddate = ToMonth.AddMonths(1).AddDays(-1);
            var result = _FacilityService.GeCWHtDetails(FromMonth, enddate);
            if (facilities != null && facilities.Count > 0)
            {
                if (facilities[0] != Guid.Empty)
                    result = result.Where(m => facilities.Contains(m.cwh_fac_id)).ToList();
            }
            foreach (var item in result)
            {
                if (prevFacilityName != item.cwh_fac_name)
                {
                    prevFacilityName = item.cwh_fac_name;
                    report = new CWHReport();
                    //int month_in_digit = item.date.Month;
                    report.fac_name = item.cwh_fac_name;
                    report.fac_Id = item.cwh_fac_id.ToString();

                    var Total_StrokeAlert_Bwt_Date_Specific_Facility = cases.Where(x => x.cas_response_ts_notification >= FromMonth && x.cas_response_ts_notification <= ToMonth.AddMonths(1).AddDays(-1) && x.cas_fac_key == item.cwh_fac_id).Count();
                    double calvalue2 = (double)Total_StrokeAlert_Bwt_Date_Specific_Facility.ToDouble() / cases.Count().ToDouble();
                    bool res2 = Double.IsNaN(calvalue2);
                    if (res2 == true)
                    {
                        report.Total_CWH = "0";
                    }
                    else
                    {
                        report.Total_CWH = Math.Round(calvalue2, 4).ToString();
                    }

                    //report.Total_CWH = calvalue2.ToString();
                    prevFacilityName = item.cwh_fac_name;
                    report.January = (result.Any(x => x.cwh_date.Value.Month == 1 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 1 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.February = (result.Any(x => x.cwh_date.Value.Month == 2 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 2 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.March = (result.Any(x => x.cwh_date.Value.Month == 3 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 3 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.April = (result.Any(x => x.cwh_date.Value.Month == 4 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 4 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.May = (result.Any(x => x.cwh_date.Value.Month == 5 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 5 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.June = (result.Any(x => x.cwh_date.Value.Month == 6 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 6 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.July = (result.Any(x => x.cwh_date.Value.Month == 7 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 7 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.August = (result.Any(x => x.cwh_date.Value.Month == 8 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 8 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.September = (result.Any(x => x.cwh_date.Value.Month == 9 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 9 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.October = (result.Any(x => x.cwh_date.Value.Month == 10 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 10 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.November = (result.Any(x => x.cwh_date.Value.Month == 11 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 11 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    report.December = (result.Any(x => x.cwh_date.Value.Month == 12 && x.cwh_fac_id == item.cwh_fac_id)) ? result.Where(x => x.cwh_date.Value.Month == 12 && x.cwh_fac_id == item.cwh_fac_id).FirstOrDefault().cwh_month_wise_cwh : null;
                    volumelist.Add(report);
                }
                else
                {
                    continue;
                }

            }

            var finalresult = volumelist.Select(x => new
            {
                strokealert = x.StrokeAlert,
                fac_name = x.fac_name,
                January = x.January,
                February = x.February,
                March = x.March,
                April = x.April,
                May = x.May,
                June = x.June,
                July = x.July,
                August = x.August,
                September = x.September,
                October = x.October,
                November = x.November,
                December = x.December,
                Total_CWH = x.Total_CWH,
                fac_Id = x.fac_Id

            }).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }

        public DataSourceResult GetCWHGraph(DataSourceRequest request, CWHReport model)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                List<CaseModel> cases = new List<CaseModel>();
                var StrokeAlert_value = CaseType.StrokeAlert.ToInt();
                string facilityTimeZone = BLL.Settings.DefaultTimeZone;
                DateTime EndDates = model.ToMonth.AddMonths(1);
                cases = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData @StartDate = '{0}',@edate = '{1}'", model.FromMonth, EndDates)).ToList();
                var datelist = Enumerable.Range(0, 12)
                              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
                              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();

                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                CWHGraphReport graph = new CWHGraphReport();
                graph.Mean = "Months";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                DateTime StartDate = Convert.ToDateTime(model.FromMonth);
                DateTime EndDate = Convert.ToDateTime(model.ToMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                DateTime enddate = EndDate;

                for (var i = StartDate; StartDate <= enddate;)
                {
                    DateTime enddateofmonth = StartDate.AddMonths(1);
                    DateTime sdate = StartDate;
                    DateTime edate = enddateofmonth.AddDays(-1);
                    Double Total_stroke = cases.Where(x => x.cas_response_ts_notification >= StartDate && x.cas_response_ts_notification <= edate).Count();
                    var result = cases.Where(x => x.cas_response_ts_notification >= StartDate && x.cas_response_ts_notification <= edate && x.cas_fac_key == new Guid(model.Facilities));
                    var month_in_digit = StartDate.ToString("MMMM");
                    var jan = result.Count().ToString();
                    double calvalue = (double)jan.ToDouble() / Total_stroke;
                    bool res = Double.IsNaN(calvalue);
                    if (MonthName.January.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.January.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.January.ToDescription());
                        }

                    }
                    else if (MonthName.February.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.February.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.February.ToDescription());
                        }
                    }
                    else if (MonthName.March.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.March.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.March.ToDescription());
                        }
                    }
                    else if (MonthName.April.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.April.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.April.ToDescription());
                        }
                    }
                    else if (MonthName.May.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.May.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.May.ToDescription());
                        }
                    }
                    else if (MonthName.June.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.June.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.June.ToDescription());
                        }
                    }
                    else if (MonthName.July.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.July.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.July.ToDescription());
                        }
                    }
                    else if (MonthName.August.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.August.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.August.ToDescription());
                        }
                    }
                    else if (MonthName.September.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.September.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.September.ToDescription());
                        }
                    }
                    else if (MonthName.October.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.October.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.October.ToDescription());
                        }
                    }
                    else if (MonthName.November.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.November.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.November.ToDescription());
                        }
                    }
                    else if (MonthName.December.ToDescription() == month_in_digit)
                    {
                        if (res == true)
                        {
                            meanlist.Add("0");
                            catgory.Add(MonthName.December.ToDescription());
                        }
                        else
                        {
                            meanlist.Add(Math.Round(calvalue, 4).ToString());
                            catgory.Add(MonthName.December.ToDescription());
                        }
                    }
                    StartDate = StartDate.AddMonths(1);
                }




                if (catgory.Count > 0)
                {

                    graph.Title = "CWH Graph";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = model.FromMonth;
                    graph.MinDate = mindate.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                }


                List<CWHGraphReport> list = new List<CWHGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        #endregion


        #region shiraz RCI
        public DataSourceResult GetRCIData(DataSourceRequest request, List<Guid> Physicians, DateTime FromMonth, DateTime ToMonth)
        {
            List<CaseModel> cases = new List<CaseModel>();
            List<CaseModel> cases2 = new List<CaseModel>();
            List<PhysicianModel> phycisions = new List<PhysicianModel>();
            List<RCIReport> volumelist = new List<RCIReport>();
            decimal resultnumber = 0;
            decimal getselectedmonthsRCIvalue = 0;
            cases = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth.AddMonths(1).AddDays(-1))).ToList();
            phycisions = _unitOfWork.SqlQuery<PhysicianModel>(string.Format("Exec UspGetRCIData @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth.AddMonths(1).AddDays(-1))).ToList();
            cases2 = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData3 @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth.AddMonths(1))).ToList();
            if (Physicians != null && Physicians.Count > 0)
            {
                foreach (var item in Physicians)
                {
                    RCIReport report = new RCIReport();
                    DateTime StartDate = Convert.ToDateTime(FromMonth);
                    DateTime EndDate = Convert.ToDateTime(ToMonth).AddMonths(1);
                    report.Physicians_name = _FacilityService.GetPhycisionName(item.ToString());
                    report.Physicians_Id = item.ToString();
                    report.fac_Id = phycisions.Where(x => x.fap_user_key == item.ToString()).Select(x => x.fap_fac_key).FirstOrDefault().ToString();
                    for (var i = StartDate; StartDate < EndDate;)
                    {
                        DateTime edate = StartDate.AddMonths(1).AddDays(-1);
                        int month_in_digit = StartDate.Month;
                        var total_facilities = phycisions.Where(x => x.fap_start_date <= edate && x.fap_user_key == item.ToString()).Select(x => x.fap_fac_key).ToList();
                        foreach (var item_fac in total_facilities)
                        {
                            var res2 = cases2.Where(x => x.cwh_fac_id == item_fac && x.cwh_date == StartDate).Select(o => o.cwh_month_wise_cwh).FirstOrDefault();
                            resultnumber = resultnumber.ToDecimal() + res2.ToDecimal();
                        }
                        switch (month_in_digit)
                        {
                            case 1:
                                var total_no_j = resultnumber * 100;
                                decimal d = total_no_j;
                                int values = (int)d;
                                if (values == 100)
                                {
                                    report.January = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.January = Math.Round(total_no_j, 2).ToString();
                                    resultnumber = 0;
                                    break;
                                }
                            case 2:
                                var total_nof = resultnumber * 100;
                                decimal d2 = total_nof;
                                int values2 = (int)d2;
                                if (values2 == 100)
                                {
                                    report.February = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.February = Math.Round(total_nof, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 3:
                                var total_nom = resultnumber * 100;
                                decimal d3 = total_nom;
                                int values3 = (int)d3;
                                if (values3 == 100)
                                {
                                    report.March = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.March = Math.Round(total_nom, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 4:
                                var total_noap = resultnumber * 100;
                                decimal d4 = total_noap;
                                int values4 = (int)d4;
                                if (values4 == 100)
                                {
                                    report.April = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.April = Math.Round(total_noap, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 5:
                                var total_noma = resultnumber * 100;
                                decimal d5 = total_noma;
                                int values5 = (int)d5;
                                if (values5 == 100)
                                {
                                    report.May = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.May = Math.Round(total_noma, 2).ToString();
                                    resultnumber = 0;
                                }

                                break;
                            case 6:
                                var total_noju = resultnumber * 100;
                                decimal d6 = total_noju;
                                int values6 = (int)d6;
                                if (values6 == 100)
                                {
                                    report.June = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.June = Math.Round(total_noju, 2).ToString();
                                    resultnumber = 0;
                                }

                                break;
                            case 7:
                                var total_noj = resultnumber * 100;
                                decimal d7 = total_noj;
                                int values7 = (int)d7;
                                if (values7 == 100)
                                {
                                    report.July = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.July = Math.Round(total_noj, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 8:
                                var total_noa = resultnumber * 100;
                                decimal d8 = total_noa;
                                int values8 = (int)d8;
                                if (values8 == 100)
                                {
                                    report.August = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.August = Math.Round(total_noa, 2).ToString();
                                    resultnumber = 0;
                                }

                                break;
                            case 9:
                                var total_nos = resultnumber * 100;
                                decimal d9 = total_nos;
                                int values9 = (int)d9;
                                if (values9 == 100)
                                {
                                    report.September = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.September = Math.Round(total_nos, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 10:
                                var total_noo = resultnumber * 100;
                                decimal d10 = total_noo;
                                int values10 = (int)d10;
                                if (values10 == 100)
                                {
                                    report.October = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.October = Math.Round(total_noo, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 11:
                                var total_non = resultnumber * 100;
                                decimal d11 = total_non;
                                int values11 = (int)d11;
                                if (values11 == 100)
                                {
                                    report.November = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.November = Math.Round(total_non, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            case 12:
                                var total_nod = resultnumber * 100;
                                decimal d12 = total_nod;
                                int values12 = (int)d12;
                                if (values12 == 100)
                                {
                                    report.December = "100";
                                    resultnumber = 0;
                                    break;
                                }
                                else
                                {
                                    report.December = Math.Round(total_nod, 2).ToString();
                                    resultnumber = 0;
                                }
                                break;
                            default:
                                break;
                        }
                        StartDate = StartDate.AddMonths(1);
                    }
                    var total_facilities2 = phycisions.Where(x => x.fap_start_date <= ToMonth && x.fap_user_key == item.ToString()).Select(x => x.fap_fac_key).ToList();
                    foreach (var item_fac in total_facilities2)
                    {
                        double calvalue2 = (double)cases.Where(x => x.cas_fac_key == item_fac).Count().ToDouble() / cases.Count().ToDouble();
                        bool res2 = Double.IsNaN(calvalue2);
                        if (res2 == true)
                        {
                            getselectedmonthsRCIvalue = getselectedmonthsRCIvalue.ToDecimal() + 0.ToDecimal();
                        }
                        else
                        {
                            getselectedmonthsRCIvalue = getselectedmonthsRCIvalue.ToDecimal() + Math.Round(calvalue2, 4).ToDecimal();
                        }
                    }
                    //Total RCI
                    var total_rci = getselectedmonthsRCIvalue * 100;

                    decimal d12s = total_rci;
                    int values12s = (int)d12s;
                    if (values12s == 100)
                    {
                        report.Total_RHI = "100";
                        getselectedmonthsRCIvalue = 0;

                    }
                    else
                    {
                        report.Total_RHI = Math.Round(total_rci, 2).ToString();
                        getselectedmonthsRCIvalue = 0;
                    }
                    //end
                    volumelist.Add(report);
                }

            }

            var finalresult = volumelist.Select(x => new
            {
                Physicians_Id = x.Physicians_Id,
                Physicians_name = x.Physicians_name,
                January = x.January,
                February = x.February,
                March = x.March,
                April = x.April,
                May = x.May,
                June = x.June,
                July = x.July,
                August = x.August,
                September = x.September,
                October = x.October,
                November = x.November,
                December = x.December,
                Total_RHI = x.Total_RHI,
                fac_Id = x.fac_Id

            }).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }

        public DataSourceResult GetRCIGraph(DataSourceRequest request, RCIReport model)
        {
            using (var context = new Model.TeleSpecialistsContext())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                context.Configuration.LazyLoadingEnabled = false;
                List<CaseModel> cases = new List<CaseModel>();
                List<CaseModel> cases2 = new List<CaseModel>();
                List<PhysicianModel> phycisions = new List<PhysicianModel>();
                cases = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData @StartDate = '{0}',@edate = '{1}'", model.FromMonth, model.ToMonth.AddMonths(1).AddDays(-1))).ToList();
                phycisions = _unitOfWork.SqlQuery<PhysicianModel>(string.Format("Exec UspGetRCIData @StartDate = '{0}',@edate = '{1}'", model.FromMonth, model.ToMonth.AddMonths(1))).ToList();
                cases2 = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData3 @StartDate = '{0}',@edate = '{1}'", model.FromMonth, model.ToMonth.AddMonths(1))).ToList();
                var datelist = Enumerable.Range(0, 12)
              .Select(i => DateTime.Now.ToEST().AddMonths(i - 12))
              .Select(date => date.ToString("MM/01/yyyy")).ToArray();
                List<string> last12Months = new List<string>();
                DateTime checkdate19 = Convert.ToDateTime("11/30/2019");
                foreach (var item in datelist)
                {
                    DateTime ndate = Convert.ToDateTime(item);
                    if (ndate > checkdate19)
                    {
                        last12Months.Add(item);
                    }
                }
                decimal resultnumber = 0;
                DateTime StartDate = Convert.ToDateTime(model.FromMonth);
                DateTime EndDate = Convert.ToDateTime(model.ToMonth);
                EndDate = EndDate.AddMonths(1).AddDays(-1);
                RCIGraphReport graph = new RCIGraphReport();
                graph.Mean = "Months";
                List<string> catgory = new List<string>();
                List<string> meanlist = new List<string>();
                for (var i = StartDate; StartDate < EndDate;)
                {
                    DateTime edate = StartDate.AddMonths(1).AddDays(-1);
                    int month_in_digit = StartDate.Month;
                    var total_facilities = phycisions.Where(x => x.fap_start_date <= edate && x.fap_user_key == model.Physicians_Id).Select(x => x.fap_fac_key).ToList();
                    foreach (var item_fac in total_facilities)
                    {
                        var res2 = cases2.Where(x => x.cwh_fac_id == item_fac && x.cwh_date == StartDate).Select(o => o.cwh_month_wise_cwh).FirstOrDefault();
                        resultnumber = resultnumber.ToDecimal() + res2.ToDecimal();
                    }
                    switch (month_in_digit)
                    {
                        case 1:
                            catgory.Add(MonthName.January.ToDescription());
                            var total_no_j = resultnumber * 100;
                            decimal d = total_no_j;
                            int values = (int)d;
                            if (values == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_no_j, 2).ToString());
                                resultnumber = 0;
                                break;
                            }
                        case 2:
                            catgory.Add(MonthName.February.ToDescription());
                            var total_nof = resultnumber * 100;
                            decimal d2 = total_nof;
                            int values2 = (int)d2;
                            if (values2 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_nof, 2).ToString());
                                resultnumber = 0;
                                break;
                            }

                        case 3:
                            catgory.Add(MonthName.March.ToDescription());
                            var total_nom = resultnumber * 100;
                            decimal d3 = total_nom;
                            int values3 = (int)d3;
                            if (values3 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_nom, 2).ToString());
                                resultnumber = 0;
                                break;
                            }
                        case 4:
                            catgory.Add(MonthName.April.ToDescription());
                            var total_noap = resultnumber * 100;
                            decimal d4 = total_noap;
                            int values4 = (int)d4;
                            if (values4 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noap, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 5:
                            catgory.Add(MonthName.May.ToDescription());
                            var total_noma = resultnumber * 100;
                            decimal d5 = total_noma;
                            int values5 = (int)d5;
                            if (values5 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noma, 2).ToString());
                                resultnumber = 0;
                                break;
                            }
                        case 6:
                            catgory.Add(MonthName.June.ToDescription());
                            var total_noju = resultnumber * 100;
                            decimal d6 = total_noju;
                            int values6 = (int)d6;
                            if (values6 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noju, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 7:
                            catgory.Add(MonthName.July.ToDescription());
                            var total_noj = resultnumber * 100;
                            decimal d7 = total_noj;
                            int values7 = (int)d7;
                            if (values7 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noj, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 8:
                            catgory.Add(MonthName.August.ToDescription());
                            var total_noa = resultnumber * 100;
                            decimal d8 = total_noa;
                            int values8 = (int)d8;
                            if (values8 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noa, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 9:
                            catgory.Add(MonthName.September.ToDescription());
                            var total_nos = resultnumber * 100;
                            decimal d9 = total_nos;
                            int values9 = (int)d9;
                            if (values9 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_nos, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 10:
                            catgory.Add(MonthName.October.ToDescription());
                            var total_noo = resultnumber * 100;
                            decimal d10 = total_noo;
                            int values10 = (int)d10;
                            if (values10 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_noo, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        case 11:
                            catgory.Add(MonthName.November.ToDescription());
                            var total_non = resultnumber * 100;
                            decimal d11 = total_non;
                            int values11 = (int)d11;
                            if (values11 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_non, 2).ToString());
                                resultnumber = 0;
                            }

                            resultnumber = 0;
                            break;
                        case 12:
                            catgory.Add(MonthName.December.ToDescription());
                            var total_nod = resultnumber * 100;
                            decimal d12 = total_nod;
                            int values12 = (int)d12;
                            if (values12 == 100)
                            {
                                meanlist.Add("100");
                                resultnumber = 0;
                                break;
                            }
                            else
                            {
                                meanlist.Add(Math.Round(total_nod, 2).ToString());
                                resultnumber = 0;
                            }
                            break;
                        default:
                            break;
                    }

                    StartDate = StartDate.AddMonths(1);
                }

                if (catgory.Count > 0)
                {

                    graph.Title = "RCI Graph";
                    DateTime date = Convert.ToDateTime(last12Months.FirstOrDefault());
                    DateTime mindate = model.FromMonth;
                    graph.MinDate = mindate.ToString("yyyy/dd/MM");
                    graph.Category = catgory;
                    graph.MeanCalculation = meanlist;
                }


                List<RCIGraphReport> list = new List<RCIGraphReport>();
                list.Add(graph);
                var finalresult = list.Select(x => new
                {
                    x.Title,
                    x.Mean,
                    x.Median,
                    x.MinDate,
                    x.MeanCalculation,
                    x.Category
                }).AsQueryable();

                return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            }
        }
        #endregion

        public DataSourceResult GetFacilityVolumetricReports(DataSourceRequest request, List<Guid> facilities, DateTime FromMonth, DateTime ToMonth)
        {

            List<CaseModel> cases = new List<CaseModel>();
            List<CWHReport> volumelist = new List<CWHReport>();
            cases = _unitOfWork.SqlQuery<CaseModel>(string.Format("Exec UspGetCWHData2 @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth.AddMonths(1).AddDays(-1))).ToList();

            foreach (var item in facilities)
            {
                CWHReport report = new CWHReport();
                DateTime StartDate = Convert.ToDateTime(FromMonth);
                DateTime EndDate = Convert.ToDateTime(ToMonth).AddMonths(1).AddDays(-1);
                report.fac_name = _FacilityService.GetFacilityNameForreport(item);
                report.fac_Id = item.ToString();
                for (var i = StartDate; StartDate < EndDate;)
                {
                    DateTime edate = StartDate.AddMonths(1).AddDays(-1);
                    var result = cases.Where(x => x.cas_response_ts_notification >= StartDate && x.cas_response_ts_notification <= edate && x.cas_fac_key == item).Count();
                    int month_in_digit = StartDate.Month;
                    switch (month_in_digit)
                    {
                        case 1:
                            report.January = result;
                            break;
                        case 2:
                            report.February = result;
                            break;
                        case 3:
                            report.March = result;
                            break;
                        case 4:
                            report.April = result;
                            break;
                        case 5:
                            report.May = result;
                            break;
                        case 6:
                            report.June = result;
                            break;
                        case 7:
                            report.July = result;
                            break;
                        case 8:
                            report.August = result;
                            break;
                        case 9:
                            report.September = result;
                            break;
                        case 10:
                            report.October = result;
                            break;
                        case 11:
                            report.November = result;
                            break;
                        case 12:
                            report.December = result;
                            break;
                        default:
                            break;
                    }
                    StartDate = StartDate.AddMonths(1);
                }
                volumelist.Add(report);
            }


            var finalresult = volumelist.Select(x => new
            {
                fac_name = x.fac_name,
                January = x.January,
                February = x.February,
                March = x.March,
                April = x.April,
                May = x.May,
                June = x.June,
                July = x.July,
                August = x.August,
                September = x.September,
                October = x.October,
                November = x.November,
                December = x.December,
                fac_Id = x.fac_Id

            }).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }
        
        public List<DailyVolematricReport> GetDailyFacilityVolumetricReports(string cas_fac_key_arrays, DateTime FromMonth, DateTime ToMonth, string cas_fac_Name_array)
        {

            List<DailyVolimatricModel> cases = new List<DailyVolimatricModel>();
            List<DailyVolematricReport> volumelist = new List<DailyVolematricReport>();
            cases = _unitOfWork.SqlQuery<DailyVolimatricModel>(string.Format("Exec UspGetDailyVolimetircdata @StartDate = '{0}',@edate = '{1}'", FromMonth, ToMonth)).ToList();
            string Imagepath = "";
            DateTime StartDate = Convert.ToDateTime(FromMonth);
            for (var i = StartDate; StartDate < ToMonth;)
            {
                DailyVolematricReport report = new DailyVolematricReport();
                foreach (var item in cas_fac_key_arrays.Split(','))
                {
                    var result = cases.Where(x => x.cas_response_ts_notification.Date == StartDate && x.cas_fac_key == new Guid(item)).Count();
                    Imagepath += result + ",";
                }
                report.fac_name = "Date" + "*" + cas_fac_Name_array;
                report.Dates = StartDate.ToShortDateString() + "," + Imagepath;
                volumelist.Add(report);
                Imagepath = "";
                StartDate = StartDate.AddDays(1);
            }
            return volumelist;
        }
        public void PrepareCastingExport(string path, List<DailyVolematricReport> response)
        {
            string filePath = path + ".csv";
            var facname = response.Select(x => x.fac_name).FirstOrDefault();
            string facname_list = "";
            foreach (var item in facname.Split('*'))
            {
                facname_list += item.Replace(",", "-") + ",";
            }
            var dateslist = response.Select(x => x.Dates).ToList();

            using (var file = new StreamWriter(filePath))
            {
                var fac_trim_list = facname_list.Trim(',');
                file.WriteLine(fac_trim_list);
                foreach (var item in dateslist)
                {
                    var trim_value = item.TrimEnd(',');
                    file.WriteLine(trim_value);
                }
            }

        }
        
        public DataSourceResult GetBCIData(DataSourceRequest request, List<Guid> facilities, List<Guid> Physicians)
        {
            //Get Data From Database for cases
            var date = new DateTime(2020, 01, 01);
            var startdate = date.ToString("yyyy-MM-dd");

            //var datess = new DateTime(2020, 10, 01);
            //var currentdate = datess.ToString("yyyy-MM-dd");


            var current_date = DateTime.Now.AddMonths(-1);
            var cd = new DateTime(current_date.Year, current_date.Month, 1);
            var currentdate = cd.AddMonths(1).AddDays(-1);
            //next month first and last date for forcaste db table data getting
            var next_date = DateTime.Now;
            var next_month_start_date = new DateTime(next_date.Year, next_date.Month, 1);
            var next_month_end_date = next_month_start_date.AddMonths(1).AddDays(-1);
            //initialize model listing
            List<BCIViewModel> cases = new List<BCIViewModel>();
            List<GetAllPhycision> _GetAllPhycision = new List<GetAllPhycision>();
            List<Forcast_Data> values_next_month = new List<Forcast_Data>();
            List<BCIReport> volumelist = new List<BCIReport>();
            List<BCIPhysicion> Finallist = new List<BCIPhysicion>();
            BCIReport report;
            BCIPhysicion bCIPhysicion;
            _GetAllPhycision = _unitOfWork.SqlQuery<GetAllPhycision>(string.Format("Exec UspGetAllPhysicion")).ToList();
            values_next_month = _unitOfWork.SqlQuery<Forcast_Data>(string.Format("Exec UspGetForecastData @StartDate = '{0}',@edate = '{1}'", next_month_start_date, next_month_end_date)).ToList();
            cases = _unitOfWork.SqlQuery<BCIViewModel>(string.Format("Exec UspGetCaseDataForBCI @StartDate = '{0}',@edate = '{1}'", startdate, currentdate)).ToList();
            // Step 1
            // Average Video Time Per Facility Per Stroke Alert
            int _recordCount = 0;
            string Time_string_list = "";
            if (facilities != null && facilities.Count > 0)
            {
                if (facilities[0] != Guid.Empty)
                    cases = cases.Where(m => facilities.Contains(m.cas_fac_key)).ToList();
                values_next_month = values_next_month.Where(m => facilities.Contains(new Guid(m.Fac_Id))).ToList();
            }
            foreach (var fac in facilities)
            {
                report = new BCIReport();
                report.fac_name = _FacilityService.GetFacilityNameForreport(fac);
                report.fac_Id = fac.ToString();
                var case_list = cases.Where(x => x.cas_fac_key == fac).ToList();
                foreach (var item in case_list)
                {
                    DateTime? d1 = item.cas_metric_video_start_time;
                    DateTime? d2 = item.cas_metric_video_end_time;
                    DateTime date1 = d1 ?? DateTime.MinValue;
                    DateTime date2 = d2 ?? DateTime.MinValue;
                    TimeSpan Time_value = date2.Subtract(date1);
                    Time_string_list += Time_value + ",";
                }
                List<double> _meanlist = new List<double>();
                var times = Time_string_list.Split(',').ToArray();
                int count = 0;
                foreach (var item in times)
                {
                    if (item != "")
                    {
                        var chk_formt = item.ToString().Split(':')[0];
                        if (chk_formt.Contains("."))
                        {
                            var ts = TimeSpan.Parse(item).TotalSeconds;
                            _meanlist.Add(ts);
                        }
                        else
                        {
                            var time = new TimeSpan(int.Parse(item.Split(':')[0]), int.Parse(item.Split(':')[1]), int.Parse(item.Split(':')[2])).TotalSeconds;
                            _meanlist.Add(time);
                        }
                        count++;
                    }
                }
                TimeSpan _meantime = new TimeSpan();
                if (_meanlist.Count > 0)
                {
                    double mean = _meanlist.Average();
                    _meantime = TimeSpan.FromSeconds(Convert.ToDouble(mean));
                }
                report.Mean_Per_Facilitys = string.Format("{0:00}:{1:00}:{2:00}", (_meantime.Hours + _meantime.Days * 24), _meantime.Minutes, _meantime.Seconds);
                volumelist.Add(report);
                Time_string_list = "";
                //Step 2
                // Demand Next Month Prediction
                foreach (var item in values_next_month)
                {
                    var get_id = fac.ToString();
                    var model_id = item.Fac_Id;
                    if (get_id == model_id)
                    {
                        var _items = volumelist[_recordCount];
                        _items.Next_Month_Data = Math.Round(item.Month_Prediction.ToDouble(), 2).ToString();
                        break;
                    }
                }
                _recordCount = _recordCount + 1;
            }
            var report_values = volumelist.Where(x => facilities.Contains(new Guid(x.fac_Id))).ToList();
            //Step 3
            //Next Month Video Time
            int counter = 0;
            foreach (var item in report_values)
            {
                if (item.Mean_Per_Facilitys != "")
                {
                    var get_mean_data = item.Mean_Per_Facilitys;
                    var time = new TimeSpan(int.Parse(get_mean_data.Split(':')[0]), int.Parse(get_mean_data.Split(':')[1]), int.Parse(get_mean_data.Split(':')[2])).TotalMinutes;
                    var next_month_data = item.Next_Month_Data;
                    double next_month_video_time = (double)time * next_month_data.ToDouble();
                    var _item = volumelist[counter];
                    if (next_month_video_time.ToString() == "∞" || next_month_video_time.ToString() == "NaN")
                    {
                        _item.Next_Month_Video_Time = "0";
                    }
                    else
                    {
                        _item.Next_Month_Video_Time = next_month_video_time.ToString();
                    }
                    counter++;
                }
            }
            // Step 4
            //Time Adjusted Demand Ratio
            double Total_Demand_Video_Time = 0;
            int countes_val = 0;
            double cal_sum = 0;
            double mul_value = 0;
            var Sum_All_Video_Time = report_values.Select(x => x.Next_Month_Video_Time).ToList();
            foreach (var item in Sum_All_Video_Time)
            {
                Total_Demand_Video_Time = Total_Demand_Video_Time.ToDouble() + item.ToDouble();
            }

            foreach (var item in report_values)
            {
                var cal = (double)item.Next_Month_Video_Time.ToDouble() / Total_Demand_Video_Time;
                if (cal.ToString() == "NaN")
                {
                    cal_sum += cal;
                    var items = volumelist[countes_val];
                    items.Time_Adjusted_Demand_Ratio = "0";
                    double get_percenatge = (double)0 * 100;
                    items.Percentage = get_percenatge.ToString();
                    mul_value += get_percenatge;
                    countes_val++;
                }
                else
                {
                    cal_sum += cal;
                    var items = volumelist[countes_val];
                    items.Time_Adjusted_Demand_Ratio = cal.ToString();
                    double get_percenatge = (double)cal * 100;
                    items.Percentage = get_percenatge.ToString();
                    mul_value += get_percenatge;
                    countes_val++;
                }

            }

            // Step 5 

            var chk_phy = _GetAllPhycision.ToList();
            var Physician = _lookUpService.GetPhysicians().Where(m => m.IsActive == true && m.IsStrokeAlert == true)
                      .OrderBy(m => m.LastName)
                      .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                      .ToList();
            if (Physicians != null && Physicians.Count > 0)
            {
                if (facilities[0] != Guid.Empty)
                    Physician = Physician.Where(m => Physicians.Contains(new Guid(m.Value))).ToList();
            }
            foreach (var phy in Physician)
            {
                bCIPhysicion = new BCIPhysicion();
                double Total_phy_video_time = 0;
                var get_onboarded_list = chk_phy.Where(x => x.fap_user_key == phy.Value).ToList();
                if (facilities != null && facilities.Count > 0)
                {
                    if (facilities[0] != Guid.Empty)
                        get_onboarded_list = get_onboarded_list.Where(m => facilities.Contains(m.fap_fac_key)).ToList();
                }
                foreach (var item in get_onboarded_list)
                {
                    var get_video_value = report_values.Where(x => x.fac_Id == item.fap_fac_key.ToString()).Select(m => m.Time_Adjusted_Demand_Ratio).FirstOrDefault();
                    Total_phy_video_time += get_video_value.ToDouble();
                }

                bCIPhysicion.Phy_Name = phy.Text;
                var _vals = Total_phy_video_time * 100;
                bCIPhysicion.Phy_Bci_Value = Math.Round(_vals, 2);
                Finallist.Add(bCIPhysicion);
                Total_phy_video_time = 0;
            }
            //Convert List To Queryable
            var finalresult = Finallist.Select(x => new
            {
                Phy_Name = x.Phy_Name,
                Phy_Bci_Value = x.Phy_Bci_Value

            }).OrderBy(x => x.Phy_Name).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }
        public DataSourceResult GetDailyForecastBydb(DataSourceRequest request, string facilitiess)
        {
            List<Monthly_Forecast> volumelist = new List<Monthly_Forecast>();
            Monthly_Forecast monthly_Forecast;
            var db_list = _unitOfWork.SqlQuery<Forcast_Data>(string.Format("Exec UspGetForecastDataFOorMOnthly")).ToList();
            var _val_fac = db_list.Select(x => x.Fac_Id).ToList();
            string fac_ids = "";
            string final_val = "";
            foreach (var item in db_list)
            {
                if (item.Fac_Id != final_val)
                {
                    monthly_Forecast = new Monthly_Forecast();
                    var zx = db_list.Where(x => x.Fac_Id == item.Fac_Id).Select(p => p.Month_Prediction).ToList();
                    foreach (var fac_value in zx)
                    {
                        if (fac_ids == item.Fac_Id)
                        {
                            monthly_Forecast.Second_Month = Math.Round(Convert.ToDouble(fac_value), 2).ToString();
                            volumelist.Add(monthly_Forecast);
                            fac_ids = "";
                            final_val = item.Fac_Id;
                        }
                        else
                        {
                            monthly_Forecast.Facility_Name = item.Fac_Name;
                            monthly_Forecast.One_Month = Math.Round(Convert.ToDouble(fac_value), 2).ToString();
                            fac_ids = item.Fac_Id;
                        }
                    }
                }

            }
            var finalresult = volumelist.Select(x => new
            {
                Facility_Name = x.Facility_Name,
                One_Month = x.One_Month,
                Second_Month = x.Second_Month
            }).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }
        public DataSourceResult GetMontlyForecastBydb(DataSourceRequest request, string facilitiess)
        {


            List<Monthly_Forecast> volumelist = new List<Monthly_Forecast>();

            Monthly_Forecast monthly_Forecast;


            var db_list = _unitOfWork.SqlQuery<Forcast_Data>(string.Format("Exec UspGetForecastDataFOorMOnthly")).ToList();
            var _val_fac = db_list.Select(x => x.Fac_Id).ToList();
            string fac_ids = "";
            string final_val = "";
            foreach (var item in db_list)
            {
                if (item.Fac_Id != final_val)
                {
                    monthly_Forecast = new Monthly_Forecast();
                    var zx = db_list.Where(x => x.Fac_Id == item.Fac_Id).Select(p => p.Month_Prediction).ToList();
                    foreach (var fac_value in zx)
                    {
                        if (fac_ids == item.Fac_Id)
                        {
                            monthly_Forecast.Second_Month = Math.Round(Convert.ToDouble(fac_value), 2).ToString();
                            volumelist.Add(monthly_Forecast);
                            fac_ids = "";
                            final_val = item.Fac_Id;
                        }
                        else
                        {
                            monthly_Forecast.Facility_Name = item.Fac_Name;
                            monthly_Forecast.One_Month = Math.Round(Convert.ToDouble(fac_value), 2).ToString();
                            fac_ids = item.Fac_Id;
                        }
                    }
                }

            }

            //Convert List To Queryable
            var finalresult = volumelist.Select(x => new
            {
                Facility_Name = x.Facility_Name,
                One_Month = x.One_Month,
                Second_Month = x.Second_Month
            }).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);

        }
        

    }
}
