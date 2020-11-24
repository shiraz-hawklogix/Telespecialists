using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianService : BaseService
    {
        SchedulerService _schedulerService = new SchedulerService();
        SleepService _sleepService = new SleepService();
        NHService _NHService = new NHService();
        public AspNetUser GetDetail(string Id)
        {
            string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            string strPacPhysician = UserRoles.AOC.ToDescription().ToLower();
            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => m.Name.ToLower() == strPartnerPhysician || m.Name.ToLower() == strPhysician || m.Name.ToLower() == strPacPhysician)
                                            .Select(m => m.Id);

            return _unitOfWork.ApplicationUserRoles
                                     .Include(m => m.AspNetUser.physician_status)
                                    .Where(m => physicianRoleIds.Contains(m.RoleId))
                                    .Where(m => m.UserId == Id)
                                    .Select(m => m.AspNetUser)
                                    .FirstOrDefault();
        }

        public AspNetUser GetDetailForDispatch(string Id)
        {
            string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            string strPacPhysician = UserRoles.AOC.ToDescription().ToLower();
            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => m.Name.ToLower() == strPartnerPhysician || m.Name.ToLower() == strPhysician || m.Name.ToLower() == strPacPhysician)
                                            .Select(m => m.Id);

            return _unitOfWork.ApplicationUserRoles
                                     .Include(m => m.AspNetUser.physician_status)
                                    .Where(m => physicianRoleIds.Contains(m.RoleId))
                                    .Where(m => m.UserId == Id)
                                    .Select(m => m.AspNetUser).AsNoTracking()
                                    .FirstOrDefault();
        }

        /// <summary>
        /// Returns only those physicians which are on schedule are not updated by physician status service as available 
        /// or those physicians for which the schedule is updated
        /// It will only ignore those physicians for which the status is changed by the navigator for that day.
        /// </summary>
        /// <returns></returns>
        public List<PhysicianStatusResetServiceModel> GetPhysiciansForService()
        {
            //int available = PhysicianStatus.Available.ToInt();
            var now = DateTime.Now.ToEST();

            // get scheduled physicians 
            var scheduleUsers = _schedulerService.GetSchedule(now).Where(x=>x.AspNetUser.IsStrokeAlert == true);
            var scheduleUserIds = scheduleUsers.Select(m => m.uss_user_id).ToList();

            // intersection on scheduled physicians and physicians
            var result = new List<PhysicianStatusResetServiceModel>();
            var physicians = GetPhysicians().Where(m => scheduleUserIds.Contains(m.Id)).ToList();
            var phyIds = physicians.Select(m => m.Id);


            var twodaysLater = now.AddDays(-1);
            var PhysicianStatusLogQuery = _unitOfWork.PhysicianStatusLogRepository.Query()
                                                     .Where(m => DbFunctions.TruncateTime(m.psl_created_date) >= DbFunctions.TruncateTime(twodaysLater))
                                                     .Where(m => phyIds.Contains(m.psl_user_key))
                                                     .ToList();

            //(from phy in GetPhysicians().Where(m => scheduleUserIds.Contains(m.Id)) select phy).ToList()

            physicians.ForEach(item =>
                        {
                            var user_schedule = scheduleUsers.Where(m => m.uss_user_id == item.Id).FirstOrDefault();
                            //bool isSchuedleStarted = (now - user_schedule.uss_time_from_calc.Value).TotalMinutes >= 2 && now <= user_schedule.uss_time_to_calc;


                            var phyicianLog = PhysicianStatusLogQuery.Where(m => m.psl_user_key == item.Id)
                                                                     .Where(m => m.psl_created_by == "reset physician service")
                                                                     .OrderByDescending(m => m.psl_key)
                                                                     .FirstOrDefault();

                            if (phyicianLog != null)
                            {

                                if ((user_schedule.uss_time_from_calc - phyicianLog.psl_start_date)?.TotalMinutes >= 1)
                                {
                                    result.Add(new PhysicianStatusResetServiceModel { physician = item, schedule = user_schedule });
                                }

                            }
                            else
                            {
                                result.Add(new PhysicianStatusResetServiceModel { physician = item, schedule = user_schedule });
                            }
                        });

            return result;
        }
        public IQueryable<AspNetUser> GetUnSchedulePhysiciansForService()
        {
            var now = DateTime.Now.ToEST();
            var notAvailable = PhysicianStatus.NotAvailable.ToInt();
            var updatedPhysicianList = (from m in _unitOfWork.PhysicianStatusLogRepository.Query()
                                        join sch in _unitOfWork.ScheduleRepository.Query() on m.psl_uss_key equals sch.uss_key
                                        where
                                         DbFunctions.TruncateTime(m.psl_created_date) == DbFunctions.TruncateTime(now)
                                         && m.psl_created_by.ToLower() == "unscheduled physicians service"
                                        select m.psl_user_key).ToList();

            var scheduledPhysicians = _unitOfWork.ScheduleRepository
                                                 .Query()
                                                 .Where(m => now >= m.uss_time_from_calc.Value)
                                                 .Where(m => now <= DbFunctions.AddMinutes(m.uss_time_to_calc.Value, 30))
                                                 .Select(m => m.uss_user_id)
                                                 .Distinct()
                                                 .ToList();

            return GetPhysicians()
                                .Where(m => !scheduledPhysicians.Contains(m.Id))
                                .Where(m => !updatedPhysicianList.Contains(m.Id))
                                .Where(m => m.status_key != notAvailable && m.IsStrokeAlert == true);

        }
        public IQueryable<PhysicianDashboardViewModel> GetPhysicianStatusDashboard(string SortOrder = "")
        {
            var now = DateTime.Now.ToEST();
            var schedule = _schedulerService.GetSchedule(now).Where(x=>x.AspNetUser.IsStrokeAlert == true) .Select(m => m.uss_user_id).ToList();
            var query = GetPhysicians()
                        .Where(m => m.IsActive)// && m.IsSleep != true)
                        .Where(m => schedule.Contains(m.Id))
                        .Select(m => new PhysicianDashboardViewModel
                        {

                            physician = m,
                            ThreshholdTime = m.physician_status.phs_threshhold_time,
                            ElapsedTime = DbFunctions.DiffHours(m.status_change_date, now)
                        });

            switch (SortOrder)
            {
                case "status asc":
                    query = query.OrderBy(m => m.physician.physician_status.phs_sort_order);
                    break;
                case "physician asc":
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
                case "physician desc":
                    query = query.OrderByDescending(m => m.physician.FirstName);
                    break;

                case "elapsedtime asc":
                    query = query.OrderBy(m => m.ElapsedTime);
                    break;
                case "elapsedtime desc":
                    query = query.OrderByDescending(m => m.ElapsedTime);
                    break;
                default:
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
            }

            return query;
        }

        #region Husnain Code Block 
        public IQueryable<PhysicianDashboardViewModel> GetPacPhysicianStatusDashboard(string SortOrder = "")
        {
            var now = DateTime.Now.ToEST();
            var schedule = _schedulerService.GetSchedule(now).Select(m => m.uss_user_id).ToList();
            var query = GetPacPhysicians()
                        .Where(m => m.IsActive)
                        .Where(m => schedule.Contains(m.Id))
                        .Select(m => new PhysicianDashboardViewModel
                        {

                            physician = m,
                            ThreshholdTime = m.physician_status.phs_threshhold_time,
                            ElapsedTime = DbFunctions.DiffHours(m.status_change_date, now)
                        });

            switch (SortOrder)
            {
                case "status asc":
                    query = query.OrderBy(m => m.physician.physician_status.phs_sort_order);
                    break;
                case "physician asc":
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
                case "physician desc":
                    query = query.OrderByDescending(m => m.physician.FirstName);
                    break;

                case "elapsedtime asc":
                    query = query.OrderBy(m => m.ElapsedTime);
                    break;
                case "elapsedtime desc":
                    query = query.OrderByDescending(m => m.ElapsedTime);
                    break;
                default:
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
            }

            return query;
        }

        public IQueryable<PhysicianDashboardViewModel> GetSleepPhysicianStatusDashboard(string SortOrder = "")
        {

            var now = DateTime.Now.ToEST();
            var schedule = _sleepService.GetSchedule(now).Select(m => m.uss_user_id).ToList();
            var query = GetPhysicians()
                        .Where(m => m.IsActive)
                        .Where(m => schedule.Contains(m.Id))
                        .Select(m => new PhysicianDashboardViewModel
                        {

                            physician = m,
                            ThreshholdTime = m.physician_status.phs_threshhold_time,
                            ElapsedTime = DbFunctions.DiffHours(m.status_change_date, now)
                        });

            switch (SortOrder)
            {
                case "status asc":
                    query = query.OrderBy(m => m.physician.physician_status.phs_sort_order);
                    break;
                case "physician asc":
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
                case "physician desc":
                    query = query.OrderByDescending(m => m.physician.FirstName);
                    break;

                case "elapsedtime asc":
                    query = query.OrderBy(m => m.ElapsedTime);
                    break;
                case "elapsedtime desc":
                    query = query.OrderByDescending(m => m.ElapsedTime);
                    break;
                default:
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
            }

            return query;
        }
       
        #endregion

        #region Bilal Code Block 

        public IQueryable<PhysicianDashboardViewModel> GetNHPhysicianStatusDashboard(string SortOrder = "")
        {
            var now = DateTime.Now.ToEST();
            var schedule = _NHService.GetSchedule(now).Select(m => m.uss_user_id).ToList();
            var query = GetPhysicians()
                        .Where(m => m.IsActive && m.NHAlert == true)
                        .Where(m => schedule.Contains(m.Id))
                        .Select(m => new PhysicianDashboardViewModel
                        {

                            physician = m,
                            ThreshholdTime = m.physician_status.phs_threshhold_time,
                            ElapsedTime = DbFunctions.DiffHours(m.status_change_date, now)
                        });

            switch (SortOrder)
            {
                case "status asc":
                    query = query.OrderBy(m => m.physician.physician_status.phs_sort_order);
                    break;
                case "physician asc":
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
                case "physician desc":
                    query = query.OrderByDescending(m => m.physician.FirstName);
                    break;

                case "elapsedtime asc":
                    query = query.OrderBy(m => m.ElapsedTime);
                    break;
                case "elapsedtime desc":
                    query = query.OrderByDescending(m => m.ElapsedTime);
                    break;
                default:
                    query = query.OrderBy(m => m.physician.FirstName);
                    break;
            }

            return query;
        }

        public List<PhysicianStatusResetServiceModel> GetNHPhysiciansForService()
        {
            //int available = PhysicianStatus.Available.ToInt();
            var now = DateTime.Now.ToEST();

            // get scheduled physicians 
            var scheduleUsers = _NHService.GetSchedule(now);
            var scheduleUserIds = scheduleUsers.Select(m => m.uss_user_id).ToList();

            // intersection on scheduled physicians and physicians
            var result = new List<PhysicianStatusResetServiceModel>();
            var physicians = GetPhysicians().Where(m => scheduleUserIds.Contains(m.Id)).ToList();
            var phyIds = physicians.Select(m => m.Id);


            var twodaysLater = now.AddDays(-1);
            var PhysicianStatusLogQuery = _unitOfWork.PhysicianStatusLogRepository.Query()
                                                     .Where(m => DbFunctions.TruncateTime(m.psl_created_date) >= DbFunctions.TruncateTime(twodaysLater))
                                                     .Where(m => phyIds.Contains(m.psl_user_key))
                                                     .ToList();

            //(from phy in GetPhysicians().Where(m => scheduleUserIds.Contains(m.Id)) select phy).ToList()

            physicians.ForEach(item =>
            {
                var user_schedule = scheduleUsers.Where(m => m.uss_user_id == item.Id).FirstOrDefault();
                //bool isSchuedleStarted = (now - user_schedule.uss_time_from_calc.Value).TotalMinutes >= 2 && now <= user_schedule.uss_time_to_calc;


                var phyicianLog = PhysicianStatusLogQuery.Where(m => m.psl_user_key == item.Id)
                                                         .Where(m => m.psl_created_by == "reset physician service")
                                                         .OrderByDescending(m => m.psl_key)
                                                         .FirstOrDefault();

                if (phyicianLog != null)
                {

                    if ((user_schedule.uss_time_from_calc - phyicianLog.psl_start_date)?.TotalMinutes >= 1)
                    {
                        result.Add(new PhysicianStatusResetServiceModel { physician = item, schedule = user_schedule });
                    }

                }
                else
                {
                    result.Add(new PhysicianStatusResetServiceModel { physician = item, schedule = user_schedule });
                }
            });

            return result;
        }

        public IQueryable<AspNetUser> GetNHUnSchedulePhysiciansForService()
        {
            var now = DateTime.Now.ToEST();
            var notAvailable = PhysicianStatus.NotAvailable.ToInt();
            var updatedPhysicianList = (from m in _unitOfWork.PhysicianStatusLogRepository.Query()
                                        join sch in _unitOfWork.NHRepository.Query() on m.psl_uss_key equals sch.uss_key
                                        where
                                         DbFunctions.TruncateTime(m.psl_created_date) == DbFunctions.TruncateTime(now)
                                         && m.psl_created_by.ToLower() == "unscheduled physicians service"
                                        select m.psl_user_key).ToList();

            var scheduledPhysicians = _unitOfWork.ScheduleRepository
                                                 .Query()
                                                 .Where(m => now >= m.uss_time_from_calc.Value && m.AspNetUser.NHAlert == true)
                                                 .Where(m => now <= DbFunctions.AddMinutes(m.uss_time_to_calc.Value, 30))
                                                 .Select(m => m.uss_user_id)
                                                 .Distinct()
                                                 .ToList();

            return GetPhysicians()
                                .Where(m => !scheduledPhysicians.Contains(m.Id))
                                .Where(m => !updatedPhysicianList.Contains(m.Id))
                                .Where(m => m.status_key != notAvailable && m.NHAlert == true);

        }

        #endregion

        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
