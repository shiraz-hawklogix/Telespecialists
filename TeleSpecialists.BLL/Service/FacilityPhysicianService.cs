using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using System;
using TeleSpecialists.BLL.Extensions;
using System.Data.Entity;
using TeleSpecialists.BLL.Helpers;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels;
using System.Data.SqlClient;
using System.Data;

namespace TeleSpecialists.BLL.Service
{
    public class FacilityPhysicianService : BaseService
    {
        private readonly AdminService _adminService = new AdminService();
        private readonly PhysicianStatusService _physicianStatusService = new PhysicianStatusService();
        private readonly SchedulerService _schedulerService = new SchedulerService();
        private readonly FacilityService _facilityService = new FacilityService();
        private readonly CaseService _caseService = new CaseService();
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var query = _unitOfWork.FacilityPhysicianRepository.Query();

            foreach (var filter in request.Filter.Filters)
            {
                if (filter.Value != null)
                {
                    if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                    {
                        query = query.WhereEquals(filter.Field, filter.Value);
                    }
                }
            }

            request.Filter = null;

            // .Where(m => m.fap_is_active)
            var caseTypelist = query.Select(m => new
            {
                phy_first_name = m.AspNetUser.FirstName,
                phy_last_name = m.AspNetUser.LastName,
                phy_email = m.AspNetUser.Email,
                m.fap_key,
                m.fap_fac_key,
                fap_phy_key = m.fap_user_key,
                phy_is_active = m.AspNetUser.IsActive,
                fap_is_active = m.fap_is_active,
                fap_start_date = m.fap_start_date,
                fap_end_date = m.fap_end_date,
                m.fap_is_on_boarded,
                m.fap_onboarded_date,
                m.fap_onboarded_by_name
            })
            .OrderBy(m => m.phy_first_name);

            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public IQueryable<AspNetUser> GetAvailableAllPhysicians(Guid fac_key, int fap_key = 0)
        {
            var facilityPhysicians = _unitOfWork.FacilityPhysicianRepository.Query()
                                                .Where(m => m.fap_is_active && m.fap_fac_key == fac_key)
                                                .Where(m => m.fap_key != fap_key)
                                                .Select(m => new { m.fap_user_key });



            var availablePhysicians = from phy in GetPhysicians()
                                      join fac in facilityPhysicians on phy.Id equals fac.fap_user_key into phy_fac
                                      from fac in phy_fac.DefaultIfEmpty()
                                      where
                                      fac == null
                                      //&& phy.IsActive
                                      orderby phy.FirstName
                                      select phy;

            var test = availablePhysicians.ToList();
            return availablePhysicians;
        }

        public IQueryable<AspNetUser> GetAvailablePhysicians(Guid fac_key, int fap_key = 0)
        {
            var facilityPhysicians = _unitOfWork.FacilityPhysicianRepository.Query()
                                                .Where(m => m.fap_is_active && m.fap_fac_key == fac_key)
                                                .Where(m => m.fap_key != fap_key)
                                                .Select(m => new { m.fap_user_key });



            var availablePhysicians = from phy in GetPhysicians()
                                      join fac in facilityPhysicians on phy.Id equals fac.fap_user_key into phy_fac
                                      from fac in phy_fac.DefaultIfEmpty()
                                      where
                                      fac == null
                                      && phy.IsActive
                                      orderby phy.FirstName
                                      select phy;

            var test = availablePhysicians.ToList();
            return availablePhysicians;
        }

        public IQueryable<facility_physician> GetAvailablePhysiciansByFacility(List<Guid> Facility)
        {
            string checkfacility = Facility[0].ToString();
            if (checkfacility == "00000000-0000-0000-0000-000000000000")
            {
                var query = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                            join n in _unitOfWork.UserRepository.Query() on m.fap_user_key equals n.Id
                            select m;

                return query;
            }
            else
            {
                var query = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                            join n in _unitOfWork.UserRepository.Query() on m.fap_user_key equals n.Id
                            select m;
                query = query.Where(x => Facility.Contains(x.fap_fac_key));

                return query;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fac_key">Facility Key</param>
        /// <param name="applyScheduleFilter">Boolean to filter the physician based on schedule</param>
        /// <param name="excludePartnerPhysicians">Boolean to  exclude partner physicians</param>
        /// <returns></returns>
        public IQueryable<AspNetUser> GetPhysiciansByFacility(Guid? fac_key, bool applyScheduleFilter, int? casType = 0)
        {
            var currentDate = DateTime.Now.ToEST();
            var maxDate = DateTime.MaxValue;
            var defaultStatus = _physicianStatusService.GetDefault();
            int nullPhysicianStatusOrder = defaultStatus != null ? defaultStatus.phs_assignment_priority.HasValue ? defaultStatus.phs_assignment_priority.Value : int.MaxValue : int.MaxValue;
            var facility = _facilityService.GetDetails(fac_key.Value);
            var licenseQuery = (_unitOfWork.PhysicianLicenseRepository.Query()
                                          .Where(m => m.phl_is_active)
                                          .Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                          .Where(m => m.phl_license_state == null /*|| facility.fac_stt_key == null*/ || m.phl_license_state == facility.fac_stt_key)
                                          .Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
                                          ).Select(m => m.phl_user_key).Distinct();
            var physicianRole = _adminService.GetRoleByName(UserRoles.Physician.ToDescription());
            var partnerPhysician = _adminService.GetRoleByName(UserRoles.PartnerPhysician.ToDescription());
            var schedule = _schedulerService.GetSchedule(currentDate).Select(m => m.uss_user_id).ToList();
            var physiciansQuery = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                                  join n in licenseQuery on m.fap_user_key equals n
                                  join phy in GetPhysicians() on m.fap_user_key equals phy.Id
                                  where
                                  m.fap_fac_key == fac_key
                                  && m.AspNetUser.IsActive
                                  && m.AspNetUser.IsDeleted == false
                                  && m.fap_is_active
                                  && m.fap_is_on_boarded
                                  && (applyScheduleFilter == false || schedule.Contains(m.fap_user_key))
                                  select m;

            var busyPhysicianIds = getBusyPhysicians(fac_key.ToString(), currentDate);

            //var busyPhysicianIds = getBusyPhysicians(physiciansQuery.Select(m => m.AspNetUser.Id).ToList());

            var physicians = physiciansQuery.Select(m => new
            {
                m.AspNetUser,
                IsBusy = busyPhysicianIds.Contains(m.AspNetUser.Id) ? 1 : 0,
                RoleOrder = (m.AspNetUser.AspNetUserRoles.FirstOrDefault() != null ? m.AspNetUser.AspNetUserRoles.FirstOrDefault().RoleId : "") == physicianRole.Id ? 1 : 2
            });

            physicians = physicians
                            .OrderBy(m => m.IsBusy)
                            .ThenBy(m => (m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_assignment_priority : nullPhysicianStatusOrder))
                            .ThenBy(m => m.AspNetUser.CredentialIndex)
                            .ThenBy(m => m.RoleOrder)
                            .ThenBy(m => m.AspNetUser.status_change_date == null ? maxDate : m.AspNetUser.status_change_date);

            if (CaseType.StatEEG.ToInt() == casType || CaseType.RoutineEEG.ToInt() == casType || CaseType.LongTermEEG.ToInt() == casType)
                return physicians.Where(c => c.AspNetUser.IsEEG).Select(s => s.AspNetUser);
            else
                return physicians.Select(m => m.AspNetUser);
        }

        public IQueryable<PhysicianStatusViewModel> GetAllPhysiciansByFacility(application_setting appSetting, Guid? fac_key, Guid? softSaveGuid, int? casType = 0)
        {
            var currentDate = DateTime.Now.ToEST();
            int isTimeBetween7and12 = 0;


            if (appSetting.aps_statusgrid_filter_start_time.HasValue && appSetting.aps_statusgrid_filter_endtime.HasValue)
            {
                var startDate = currentDate.Date.AddDays(appSetting.aps_statusgrid_filter_start_time.Value.Hours == 0 ? 1 : 0).AddTicks(appSetting.aps_statusgrid_filter_start_time.Value.Ticks);
                var endDate = currentDate.Date.AddDays(appSetting.aps_statusgrid_filter_endtime.Value.Hours == 0 ? 1 : 0).AddTicks(appSetting.aps_statusgrid_filter_endtime.Value.Ticks);

                isTimeBetween7and12 = currentDate >= startDate && currentDate <= endDate ? 1 : 0;
            }

            // prepare query
            string query = "";
            if (softSaveGuid.HasValue)
                query = string.Format("Exec usp_new_GetAllPhysiciansByFacility '{0}', {1}, {2}, '{3}'", fac_key, casType, isTimeBetween7and12, softSaveGuid.ToString());
            else
                query = string.Format("Exec usp_new_GetAllPhysiciansByFacility '{0}', {1}, {2}", fac_key, casType, isTimeBetween7and12);


            // return results
            return _unitOfWork.SqlQuery<PhysicianStatusViewModel>(query).AsQueryable();


            #region ----- Optimization -----

            /*
            var maxDate = DateTime.MaxValue;
            var defaultStatus = _physicianStatusService.GetDefault();
            int nullPhysicianStatusOrder = defaultStatus != null ? defaultStatus.phs_assignment_priority.HasValue ? defaultStatus.phs_assignment_priority.Value : int.MaxValue : int.MaxValue;
            var facility = _facilityService.GetDetails(fac_key.Value);

            var licenseQuery = (_unitOfWork.PhysicianLicenseRepository.Query()
                                        .Where(m => m.phl_is_active)
                                        .Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                        .Where(m => m.phl_license_state == null  || m.phl_license_state == facility.fac_stt_key)
                                        .Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
                                )
                                .Select(m => m.phl_user_key)
                                .Distinct();

            var physicianRole = _adminService.GetRoleByName(UserRoles.Physician.ToDescription());
            var partnerPhysician = _adminService.GetRoleByName(UserRoles.PartnerPhysician.ToDescription());
            //var schedule = _schedulerService.GetSchedule(currentDate).Select(m => m.uss_user_id).ToList();
            var shceduleQuery = _unitOfWork.ScheduleRepository.Query().Where(x => (currentDate >= x.uss_time_from_calc && currentDate <= x.uss_time_to_calc));
            var physiciansQuery = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                                  join n in licenseQuery on m.fap_user_key equals n
                                  join phy in GetPhysicians() on m.fap_user_key equals phy.Id
                                  join sch in shceduleQuery on m.fap_user_key equals sch.uss_user_id into scheduleEntity
                                  from scheduleRecords in scheduleEntity.DefaultIfEmpty()
                                  where
                                  m.fap_fac_key == fac_key
                                  && m.AspNetUser.IsActive
                                  && m.AspNetUser.IsDeleted == false
                                  && m.fap_is_active
                                  && m.fap_is_on_boarded
                                  //&& (applyScheduleFilter == false || schedule.Contains(m.fap_user_key))
                                  select new
                                  {
                                      user = m,
                                      scheduleExist = scheduleRecords != null ? true : false,
                                      currentSchedule = scheduleRecords
                                  };

            var busyPhysicianIds = getBusyPhysicians(physiciansQuery.Select(m => m.user.AspNetUser.Id).Distinct().ToList());
            double maxCredentialIndex = 100;
            if (physiciansQuery.Count() > 0)
                maxCredentialIndex = physiciansQuery.Max(m => m.user.AspNetUser.CredentialIndex);
            // we always need to greater date as second parameter
            var physicians = physiciansQuery.Select(m => new
            {
                m.user.AspNetUser,
                m.scheduleExist,
                // Id =  m.user.AspNetUser.Id,
                // dateToCompare = m.user.AspNetUser.status_change_date,
                // currentDate = currentDate,
                // MDiff = DbFunctions.DiffMinutes(m.user.AspNetUser.status_change_date, currentDate),
                IsBusy = busyPhysicianIds.Contains(m.user.AspNetUser.Id) ? 1 : 0,
                IsAvailableForMoreThan90M = m.user.AspNetUser.status_change_date == null ? 2 : DbFunctions.DiffMinutes(m.user.AspNetUser.status_change_date, currentDate) >= 90
                                                                                                && m.user.AspNetUser.status_key == (int)PhysicianStatus.Available
                                                                                                && m.user.AspNetUser.CredentialIndex >= maxCredentialIndex
                                                                                                && isTimeBetween7and12
                                                                                                 ? 1 : 2,
                IsLessThan60MLeft = !m.scheduleExist ? 2 : DbFunctions.DiffMinutes(currentDate, m.currentSchedule.uss_time_to_calc) <= 60 && DbFunctions.DiffMinutes(currentDate, m.currentSchedule.uss_time_to_calc) >= 1
                                                         && m.user.AspNetUser.status_key == (int)PhysicianStatus.Available ? 1 : 2,
                RoleOrder = (m.user.AspNetUser.AspNetUserRoles.FirstOrDefault() != null ? m.user.AspNetUser.AspNetUserRoles.FirstOrDefault().RoleId : "") == physicianRole.Id ? 1 : 2
            }).Distinct();

            //var adminHeller = physicians.Where(m => m.Id == "6c6371ed-9958-44a5-8a60-90ab481be8e5").FirstOrDefault();
            //     var testData = physicians.ToList();
            physicians = physicians
                            .OrderBy(m => m.IsBusy)
                            .ThenBy(m => m.IsLessThan60MLeft)
                            .ThenBy(m => m.IsAvailableForMoreThan90M)
                            .ThenBy(m => (m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_assignment_priority : nullPhysicianStatusOrder))
                            .ThenBy(m => m.AspNetUser.CredentialIndex)
                            .ThenBy(m => m.RoleOrder)
                            .ThenBy(m => m.AspNetUser.status_change_date == null ? maxDate : m.AspNetUser.status_change_date);

            //     var testOrderedData = physicians.ToList();

            if (CaseType.StatEEG.ToInt() == casType || CaseType.RoutineEEG.ToInt() == casType || CaseType.LongTermEEG.ToInt() == casType)
                physicians = physicians.Where(c => c.AspNetUser.IsEEG);

            return physicians.Select(m => new PhysicianStatusViewModel
            {
                isScheduled = m.scheduleExist,
                Name = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                Id = m.AspNetUser.Id,
                CreatedDate = DBHelper.FormatDateTime(m.AspNetUser.CreatedDate, true),
                StatusChangeDate = m.AspNetUser.status_change_date.HasValue ? DBHelper.FormatDateTime(m.AspNetUser.status_change_date.Value, true) : "",
                CredentialIndex = m.AspNetUser.CredentialIndex,
                MobilePhone = m.AspNetUser.MobilePhone,
                PhoneNumber = m.AspNetUser.PhoneNumber,
                IsAvailableStatus = m.AspNetUser.physician_status != null ? true : false,
                StatusColorCode = m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_color_code : "",
                StatusName = m.AspNetUser.physician_status != null ? m.AspNetUser.physician_status.phs_name : "",
            });

            */

            #endregion
        }

        public IQueryable<facility> GetAllFacilities()
        {
    

            var facilities = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                             join contact in _unitOfWork.ContactRepository.Query() on m.fap_fac_key equals contact.cnt_fac_key into facilityContactJoin
                             from facilityContact in facilityContactJoin.DefaultIfEmpty()
                             where 
                              m.fap_is_active
                             && m.fap_is_on_boarded
                             select m.facility;

            return facilities.Distinct();
        }
        public IQueryable<facility> GetPhsicianFacilities(string phy_key, string phoneNumber)
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
                             && m.fap_is_active
                             && m.fap_is_on_boarded
                             && physicianLicenseStates.Contains(m.facility.fac_stt_key)
                             && (phoneNumber == "" || (facilityContact.cnt_is_active && facilityContact.cnt_is_deleted == false && facilityContact.cnt_mobile_phone == phoneNumber))
                             select m.facility;

            return facilities.Distinct();
        }
        public IQueryable<AspNetUser> GetAllPhysicians()
        {
            var physicians = GetPhysicians()
                                        .Where(m => m.IsActive)
                                        .Where(m => m.IsDeleted == false)
                                        .OrderBy(m => m.FirstName);
            return physicians;
        }
        public facility_physician GetDetails(int id)
        {
            var model = _unitOfWork.FacilityPhysicianRepository.Query().AsNoTracking()
                                   .FirstOrDefault(m => m.fap_key == id);
            return model;
        }
        public facility_physician GetDetailsByFacKey(Guid id, string userkey)
        {
            var model = _unitOfWork.FacilityPhysicianRepository.Query().AsNoTracking()
                                   .Where(m => m.fap_fac_key == id && m.fap_user_key == userkey).FirstOrDefault();
            return model;
        }
        public bool Delete(facility_physician entity)
        {
            _unitOfWork.FacilityPhysicianRepository.Delete(entity.fap_key);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }

        public bool IsAlreadyExists(facility_physician entity)
        {
            return _unitOfWork.FacilityPhysicianRepository.Query()
                                                 .Where(m => m.fap_is_active)
                                                 .Where(m => m.fap_fac_key == entity.fap_fac_key)
                                                 .Where(m => m.fap_user_key == entity.fap_user_key)
                                                 .Where(m => m.fap_key != entity.fap_key)
                                                 .Any();
        }
        public void Create(facility_physician entity)
        {
            entity.fap_start_date = DateTime.Now.ToEST();
            _unitOfWork.FacilityPhysicianRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public void Edit(facility_physician entity, bool commitChange = true)
        {

            var facilityPhsycian = _unitOfWork.FacilityPhysicianRepository.Query().Where(x => x.fap_key == entity.fap_key).FirstOrDefault();
            if (facilityPhsycian != null)
            {
                facilityPhsycian.fap_UserName = entity.fap_UserName;
                facilityPhsycian.fap_is_on_boarded = entity.fap_is_on_boarded;
                facilityPhsycian.fap_onboarded_by = entity.fap_onboarded_by;
                facilityPhsycian.fap_onboarded_date = DateTime.Now.ToEST();
                facilityPhsycian.fap_onboarded_by_name = entity.fap_onboarded_by_name;
                facilityPhsycian.fap_onboarded_date = DateTime.Now;

                _unitOfWork.FacilityPhysicianRepository.Update(facilityPhsycian);
                if (commitChange)
                {
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }
            }
        }
     
        public void EditForMultiple(IEnumerable<facility_physician> entity)
        {
            _unitOfWork.FacilityPhysicianRepository.UpdateRange(entity);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            
        }

        public DataSourceResult GetPhysicianFacilities(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                               join n in GetUclData(UclTypes.State) on m.facility.fac_stt_key equals n.ucd_key into FacilityStates
                               from state in FacilityStates.DefaultIfEmpty()
                               orderby m.facility.fac_name
                               select new
                               {
                                   m.facility.fac_name,
                                   m.facility.fac_city,
                                   stt_code = state != null ? state.ucd_title : "",
                                   m.facility.fac_zip,
                                   m.facility.fac_timezone,
                                   m.fap_key,
                                   m.fap_fac_key,
                                   fap_user_key = m.fap_user_key,
                                   m.fap_is_active,
                                   m.fap_start_date,
                                   m.fap_end_date,
                                   m.fap_is_on_boarded,
                                   m.fap_onboarding_complete_provider_active_date,
                                   m.fap_onboarded_date,
                                   m.fap_onboarded_by_name,
                                   m.fap_hide
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetPhysicianFacilitiesForPhy(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                               join n in GetUclData(UclTypes.State) on m.facility.fac_stt_key equals n.ucd_key into FacilityStates
                               from state in FacilityStates.DefaultIfEmpty()
                               where
                                m.fap_is_on_boarded == true
                               && m.fap_is_active == true
                               && m.fap_start_date != null
                               && m.fap_end_date != null && m.fap_hide == false && m.facility.fac_go_live
                               orderby m.facility.fac_name
                               select new
                               {
                                   m.facility.fac_name,
                                   m.facility.fac_city,
                                   stt_code = state != null ? state.ucd_title : "",
                                   m.facility.fac_zip,
                                   m.facility.fac_timezone,
                                   m.fap_key,
                                   m.fap_fac_key,
                                   fap_user_key = m.fap_user_key,
                                   m.fap_is_active,
                                   m.fap_start_date,
                                   m.fap_end_date,
                                   m.fap_is_on_boarded,
                                   m.fap_onboarding_complete_provider_active_date,
                                   m.fap_onboarded_date,
                                   m.fap_onboarded_by_name,
                                   m.fap_hide
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }


        public IQueryable<facility> GetAvailableFacilities(string phy_key, int fap_key = 0)
        {
            var PhysicianFacilities = _unitOfWork.FacilityPhysicianRepository.Query()
                                                .Where(m => m.fap_is_active && m.fap_user_key == phy_key)
                                                .Where(m => m.fap_key != fap_key)
                                                .Select(m => new { m.fap_fac_key });

            var availableFacilities = from fac in _unitOfWork.FacilityRepository.Query()
                                      join p in PhysicianFacilities on fac.fac_key equals p.fap_fac_key into phy_fac
                                      from p in phy_fac.DefaultIfEmpty()
                                      where
                                      p == null
                                      //TCARE-416
                                      //&& fac.fac_is_active 
                                      orderby fac.fac_name
                                      select fac;

            return availableFacilities;
        }

        private List<string> getBusyPhysicians(string fac_key, DateTime CurrentDate)
        {
            List<string> result = new List<string>();

            var sp_fac_key = new SqlParameter("fac_key", SqlDbType.VarChar) { Value = fac_key };
            var sp_current_date = new SqlParameter("CurrentDate", SqlDbType.DateTime) { Value = CurrentDate };
            result = _unitOfWork.ExecuteStoreProcedure<string>("sp_get_busy_physicians_ids @fac_key, @CurrentDate", sp_fac_key, sp_current_date).ToList();

            return result;
        }

        private List<string> getBusyPhysicians(List<string> physiciansIds)
        {
            var physiciansLastCases = _caseService.GetPhysiciansLastCases(physiciansIds).ToList().OrderBy(x => x.cas_physician_assign_date).ToList();
            var result = new List<string>();
            if (physiciansLastCases != null)
            {
                foreach (var currentPhy in physiciansIds)
                {
                    var isBusy = physiciansLastCases.Any(x => x.cas_phy_key == currentPhy && x.cas_cst_key == CaseStatus.WaitingToAccept.ToInt());
                    if (isBusy)
                        result.Add(currentPhy);
                }
            }
            return result;
        }

        public int SetPhyPendingOnboardindFacDate()
        {
            try
            {
                var phyFacList = GetPhsicianNonBoardedFacilities().ToList();
                foreach (var facPhy in phyFacList)
                {
                    var dbFacPhy = GetDetails(facPhy.fap_key);
                    dbFacPhy.fap_is_hide_pending_onboarding = true;
                    Edit(dbFacPhy);
                }
                return 1;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private IQueryable<facility_physician> GetPhsicianNonBoardedFacilities()
        {

            var currentDate = DateTime.Now.ToEST();
            var maxDate = DateTime.MaxValue;
            var defaultStatus = _physicianStatusService.GetDefault();
            var physicianLicenseStates = (_unitOfWork.PhysicianLicenseRepository.Query()
                                          .Where(m => m.phl_is_active)
                                          .Where(m => DbFunctions.TruncateTime(currentDate) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                          .Where(m => m.phl_expired_date == null || DbFunctions.TruncateTime(currentDate) <= DbFunctions.TruncateTime(m.phl_expired_date))
                                          ).Select(m => m.phl_license_state).ToList();

            var facilityPhy = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                              join contact in _unitOfWork.ContactRepository.Query() on m.fap_fac_key equals contact.cnt_fac_key into facilityContactJoin
                              from facilityContact in facilityContactJoin.DefaultIfEmpty()
                              where
                               m.fap_is_on_boarded == false
                              && m.fap_is_hide_pending_onboarding == false
                              && physicianLicenseStates.Contains(m.facility.fac_stt_key)
                              && m.facility.fac_go_live
                              select m;

            return facilityPhy;
        }
        public DataSourceResult GetAllPhysicianPassword(DataSourceRequest request, List<Guid> Facilities, List<string> Physicians)
        {
            #region Query
            var result = (from m in _unitOfWork.FacilityPhysicianRepository.Query()
                          join n in _unitOfWork.UserRepository.Query() on m.fap_user_key equals n.Id
                          select new
                          {
                              m.fap_key,
                              m.fap_fac_key,
                              m.fap_user_key,
                              username = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                              facname = m.facility.fac_name,
                              userpassword = m.fap_UserName
                          });
            #endregion

            #region filters
            if (Facilities != null && Facilities.Count > 0)
            {
                if (Facilities[0] != Guid.Empty)
                    result = result.Where(m => Facilities.Contains(m.fap_fac_key));
            }

            if (Physicians != null && Physicians.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(Physicians[0]))
                    result = result.Where(m => Physicians.Contains(m.fap_user_key));
            }
            #endregion
            var finalresult = result.Select(x => new
            {
                id = x.fap_key,
                username = x.username,
                facname = x.facname,
                userpassword = x.userpassword
            }).OrderBy(x => x.username).AsQueryable();

            return finalresult.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public void updatePhysicianPassword(PhysicianViewModel model, bool commitChange = true)
        {
            var facilityPhsycian = _unitOfWork.FacilityPhysicianRepository.Query().Where(x => x.fap_key == model.id).FirstOrDefault();
            if (facilityPhsycian != null)
            {
                facilityPhsycian.fap_UserName = model.userpassword;
                _unitOfWork.FacilityPhysicianRepository.Update(facilityPhsycian);
            }
            if (commitChange)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }

        }
    }
}