using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.ViewModels;
using System.Data.Entity;
using System.Text;
using TeleSpecialists.BLL.ViewModels.Schedule;
using System.Data.SqlClient;

namespace TeleSpecialists.BLL.Service
{
    public class SchedulerService : BaseService
    {
        private readonly AdminService _adminService = new AdminService();
        #region public-methods
        public KeyValuePair<long, bool> UpdateSchedule(user_schedule model, bool isAllDay)
        {
            model.uss_time_from = ParseScheduleTime(model.uss_time_from_calc.Value.ToString("HH:mm"));
            model.uss_time_to = ParseScheduleTime(model.uss_time_to_calc.Value.ToString("HH:mm"));
            
            if (model.uss_time_to_calc.Value.TimeOfDay < model.uss_time_from_calc.Value.TimeOfDay)
            {
                model.uss_time_to_calc = model.uss_date.AddDays(1).Date.Add(model.uss_time_to_calc.Value.TimeOfDay);
                model.uss_time_to = model.uss_time_to + TimeSpan.TicksPerDay;
            }
            else if (model.uss_time_to_calc.Value.TimeOfDay > model.uss_time_from_calc.Value.TimeOfDay && isAllDay)
            {
                // Set the same date in both field
                model.uss_time_to_calc = new DateTime(model.uss_time_from_calc.Value.Year,
                                                            model.uss_time_from_calc.Value.Month,
                                                            model.uss_time_from_calc.Value.Day,
                                                            model.uss_time_to_calc.Value.Hour,
                                                            model.uss_time_to_calc.Value.Minute,
                                                            model.uss_time_to_calc.Value.Second,
                                                            model.uss_time_to_calc.Value.Millisecond);
            }
            else if (model.uss_time_to_calc.Value.Date > model.uss_time_from_calc.Value.Date)
            {
                model.uss_time_to = model.uss_time_to + TimeSpan.TicksPerDay;
            }

            model.uss_time_to_calc_num = Convert.ToInt64(model.uss_time_to_calc.Value.Year.ToString() + model.uss_time_to_calc.Value.DayOfYear.ToString("000") + model.uss_time_to_calc.Value.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());

            var existingSchdule = _unitOfWork.ScheduleRepository.Query()
                                     .Where(m =>
                                          m.uss_time_from_calc < model.uss_time_to_calc &&
                                          model.uss_time_from_calc < m.uss_time_to_calc &&
                                          m.uss_user_id == model.uss_user_id &&
                                          m.uss_key != model.uss_key)
                                   .ToList();
            if (existingSchdule != null && existingSchdule.Count > 0)
                return new KeyValuePair<long, bool>(0, false);

            _unitOfWork.ScheduleRepository.Update(model);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return new KeyValuePair<long, bool>(model.uss_key, true);
        }

        public List<ScheduleRecordViewModel> GetAllPhyScheduals(bool isGetAllRequeset, string userId, DateTime startDate, DateTime endDate, List<string> Physicians, string phy_type, bool isSuperAdmin, string SchType)
        {
            // Fetch records from previous day of start date, to sync multidate enteries.
            startDate = startDate.AddDays(-1);

            string Start_date = startDate.Year.ToString() + startDate.DayOfYear.ToString("000");
            string END_date = endDate.Year.ToString() + endDate.DayOfYear.ToString("000");

            var sp_start_date = new SqlParameter("startDate", SqlDbType.BigInt) { Value = Start_date };
            var sp_end_date = new SqlParameter("endDate", SqlDbType.BigInt) { Value = END_date };
            var sp_isAdmin = new SqlParameter("isAdmin", SqlDbType.Bit) { Value = isGetAllRequeset };
            var sp_isSuperAdmin = new SqlParameter("isSuperAdmin", SqlDbType.Bit) { Value = isSuperAdmin };
            var sp_userId = new SqlParameter("userId", SqlDbType.VarChar) { Value = userId };
            var sp_SchType = new SqlParameter("SchType", SqlDbType.VarChar) { Value = SchType };
            var query = _unitOfWork.ExecuteStoreProcedure<ScheduleRecordViewModel>("sp_phy_scheduals @startDate, @endDate,@isAdmin,@isSuperAdmin,@userId,@SchType", sp_start_date, sp_end_date, sp_isAdmin, sp_isSuperAdmin, sp_userId, sp_SchType).ToList();
            if (Physicians != null && Physicians.Any())
            {
                query = query.Where(m => !Physicians.Any() || Physicians.Any(id => id == m.UserId)).ToList();
            }
            return query;
        }

        //public List<FacilityList> getAllPhyscianList(string start_date, string end_date)
        //{
        //    var sp_start_date = new SqlParameter("start_date", SqlDbType.DateTime) { Value = start_date };
        //    var sp_end_date = new SqlParameter("end_date", SqlDbType.DateTime) { Value = end_date };
        //    List<FacilityList> _list = _unitOfWork.ExecuteStoreProcedure<FacilityList>("sp_flag_facilitites @start_date, @end_date", sp_start_date, sp_end_date).ToList();
        //    return _list;
        //}

        public bool getAllPhyscianListDay(string start_date)
        {
            DateTime date = Convert.ToDateTime(start_date);
            start_date = date.Year.ToString() + date.DayOfYear.ToString("000");

            var sp_start_date = new SqlParameter("start_date", SqlDbType.BigInt) { Value = start_date };
            List<FacilityListDayWise> _list = _unitOfWork.ExecuteStoreProcedure<FacilityListDayWise>("sp_flag_facilitites_Day @start_date", sp_start_date).ToList();
            return _list.Count() > 0 ? true : false;
        }

        public List<FacilityList> getAllPhyscianList(string start_date, string end_date)
        {
            DateTime Startdate = Convert.ToDateTime(start_date);
            start_date = Startdate.Year.ToString() + Startdate.DayOfYear.ToString("000") + Startdate.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim();

            DateTime ENDDATE = Convert.ToDateTime(end_date);
            end_date = ENDDATE.Year.ToString() + ENDDATE.DayOfYear.ToString("000") + ENDDATE.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim();

            var sp_start_date = new SqlParameter("start_date", SqlDbType.BigInt) { Value = start_date };
            var sp_end_date = new SqlParameter("end_date", SqlDbType.BigInt) { Value = end_date };
            List<FacilityList> _list = _unitOfWork.ExecuteStoreProcedure<FacilityList>("sp_flag_facilitites @start_date, @end_date", sp_start_date, sp_end_date).ToList();
            return _list;
        }

        //public bool getAllPhyscianListDay(string start_date)
        //{
        //    var sp_start_date = new SqlParameter("start_date", SqlDbType.DateTime) { Value = start_date };
        //    List<FacilityListDayWise> _list = _unitOfWork.ExecuteStoreProcedure<FacilityListDayWise>("sp_flag_facilitites_Day @start_date", sp_start_date).ToList();
        //    return _list.Count() > 0 ? true : false;
        //}

        public bool PublishSchedule(int month, int year)
        {
            var sp_month = new SqlParameter("month", SqlDbType.Int) { Value = month };
            var sp_year = new SqlParameter("year", SqlDbType.Int) { Value = year };
            int result = _unitOfWork.ExecuteSqlCommand("sp_update_phy_month_schedules @month,@year", sp_month, sp_year);
            return result > 0 ? true : false; ;
        }

        public bool getCheckSchedulePublishFlag()
        {
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            var result = _unitOfWork.ScheduleRepository.Query().Where(x => x.uss_date.Month == month && x.uss_date.Year == year).Any(x => x.uss_is_publish == false);
            return result;
        }

        public KeyValuePair<long, bool> AddSchedule(user_schedule model, bool isAllDay)
        {
            model.uss_time_from = ParseScheduleTime(model.uss_time_from_calc.Value.ToString("HH:mm"));
            model.uss_time_to = ParseScheduleTime(model.uss_time_to_calc.Value.ToString("HH:mm"));

            if (model.uss_time_to_calc.Value.TimeOfDay < model.uss_time_from_calc.Value.TimeOfDay)
            {
                model.uss_time_to = model.uss_time_to + TimeSpan.TicksPerDay;
                model.uss_time_to_calc = model.uss_time_to_calc.Value.AddDays(1);
            }
            else if (model.uss_time_to_calc.Value.TimeOfDay > model.uss_time_from_calc.Value.TimeOfDay && isAllDay)
            {
                // Set the same date in both field
                model.uss_time_to_calc = new DateTime(model.uss_time_from_calc.Value.Year,
                                                            model.uss_time_from_calc.Value.Month,
                                                            model.uss_time_from_calc.Value.Day,
                                                            model.uss_time_to_calc.Value.Hour,
                                                            model.uss_time_to_calc.Value.Minute,
                                                            model.uss_time_to_calc.Value.Second,
                                                            model.uss_time_to_calc.Value.Millisecond);
            }
            else if (model.uss_time_to_calc.Value.Date > model.uss_time_from_calc.Value.Date)
            {
                model.uss_time_to = model.uss_time_to + TimeSpan.TicksPerDay;
            }

            model.uss_time_to_calc_num = Convert.ToInt64(model.uss_time_to_calc.Value.Year.ToString() + model.uss_time_to_calc.Value.DayOfYear.ToString("000") + model.uss_time_to_calc.Value.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());

            var existingSchdule = _unitOfWork.ScheduleRepository.Query()
                                   .Where(m =>
                                          m.uss_time_from_calc < model.uss_time_to_calc &&
                                          model.uss_time_from_calc < m.uss_time_to_calc &&
                                          m.uss_user_id == model.uss_user_id)
                                   .ToList();
            if (existingSchdule != null && existingSchdule.Count > 0)
                return new KeyValuePair<long, bool>(0, false);

            _unitOfWork.ScheduleRepository.Insert(model);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return new KeyValuePair<long, bool>(model.uss_key, true);
        }
        public SchedulerResponseViewModel SaveSchedule(string filePath, string fileName, string loggedinUserId, string loggedinUserName, bool SkipErrors, string impType = "")
        {
            var dataTable = new CSVReader().GetCSVAsDataTable(filePath + fileName);
            if (dataTable != null && dataTable.Rows.Count > 1)
            {
                return ImportSchedule(dataTable, loggedinUserId, loggedinUserName, SkipErrors, impType);
            }

            return new SchedulerResponseViewModel();
        }
        public List<user_schedule> GetAll(bool isGetAllRequeset, string userId, DateTime startDate, DateTime endDate, List<string> Physicians, string phy_type)
        {
            // Fetch records from previous day of start date, to sync multidate enteries.
            startDate = startDate.AddDays(-1);
            // return schedules
            var query = this.GetFilteredSchedule(isGetAllRequeset, userId)
                .Where(m => DbFunctions.TruncateTime(m.uss_date) >= DbFunctions.TruncateTime(startDate)
                            && (DbFunctions.TruncateTime(m.uss_date) <= DbFunctions.TruncateTime(endDate)) && m.uss_is_publish == true);
            if (Physicians != null && Physicians.Any())
            {
                query = query.Where(m => !Physicians.Any() || Physicians.Any(id => id == m.uss_user_id));
            }

            return query.OrderBy(m => m.uss_date).ToList();
        }
        public IQueryable<user_schedule> GetSchedule(DateTime dateTime)
        {
            return _unitOfWork.ScheduleRepository
                              .Query()
                              .Where(m => dateTime >= m.uss_time_from_calc.Value)
                              .Where(m => dateTime <= m.uss_time_to_calc.Value)
                              .Where(m=> m.uss_is_publish == true)
                              ;
        }
        public IQueryable<user_schedule> GetScheduleById(long id)
        {
            return _unitOfWork.ScheduleRepository
                              .Query()
                              .Where(m => m.uss_key == id);
        }
        public bool RemoveSchedule(int id)
        {
            _unitOfWork.ScheduleRepository.Delete(id);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public List<Tuple<string, string, string, string, string, string>> GetScheduledPhysicians(bool isgetAllRequest, string userId, bool onlySchedulePhys = false, string phy_type = "Physician")
        {
            var random = new Random();
            var physicianRole = _adminService.GetRoleByName(UserRoles.Physician.ToDescription());
            var partnerPhysician = _adminService.GetRoleByName(UserRoles.PartnerPhysician.ToDescription());
            var PacPhysician = _adminService.GetRoleByName(UserRoles.AOC.ToDescription());

            #region If  type is Physician
            if(phy_type == "Physician")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId).Where(x=>x.AspNetUser.IsStrokeAlert == true)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false) on s.uss_user_id equals u.Id
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { s.uss_user_id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                        )
                        .Distinct()
                        .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                        .ToList()
                        .Select(x => new Tuple<string, string, string, string, string, string>(x.uss_user_id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                        .ToList();
                }
                else
                {
                    return (from u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsStrokeAlert == true)
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { u.Id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                       )
                       .Distinct()
                       .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                       .ToList()
                       .Select(x => new Tuple<string, string, string, string, string, string>(x.Id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                       .ToList();
                }
            }

            #endregion
            #region If  type is  Pac Physician
            else if (phy_type == "aoc")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false) on s.uss_user_id equals u.Id
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == PacPhysician.Id

                            select new { s.uss_user_id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                        )
                        .Distinct()
                        .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                        .ToList()
                        .Select(x => new Tuple<string, string, string, string, string, string>(x.uss_user_id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                        .ToList();
                }
                else
                {
                    return (from u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false)
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where  r.RoleId == PacPhysician.Id

                            select new { u.Id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                       )
                       .Distinct()
                       .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                       .ToList()
                       .Select(x => new Tuple<string, string, string, string, string, string>(x.Id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                       .ToList();
                }
            }
            #endregion
            #region If  type is Sleep
            else if (phy_type == "sleep")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsSleep == true) on s.uss_user_id equals u.Id
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { s.uss_user_id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                        )
                        .Distinct()
                        .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                        .ToList()
                        .Select(x => new Tuple<string, string, string, string, string, string>(x.uss_user_id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                        .ToList();
                }
                else
                {
                    return (from u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsSleep == true)
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { u.Id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                       )
                       .Distinct()
                       .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                       .ToList()
                       .Select(x => new Tuple<string, string, string, string, string, string>(x.Id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                       .ToList();
                }
            }

            #endregion
            #region If  type is NHAlert
            else if (phy_type == "nhAlert")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId).Where(x=>x.AspNetUser.NHAlert == true)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false) on s.uss_user_id equals u.Id
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { s.uss_user_id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                        )
                        .Distinct()
                        .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                        .ToList()
                        .Select(x => new Tuple<string, string, string, string, string, string>(x.uss_user_id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                        .ToList();
                }
                else
                {
                    return (from u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.NHAlert == true)
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == physicianRole.Id || r.RoleId == partnerPhysician.Id

                            select new { u.Id, u.UserInitial, u.FirstName, u.LastName, u.NPINumber, u.Gender }
                       )
                       .Distinct()
                       .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                       .ToList()
                       .Select(x => new Tuple<string, string, string, string, string, string>(x.Id, string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), string.Format("#{0:X6}", random.Next(0x1000000)), string.Format("{0} {1} - {2}", x.FirstName, x.LastName, x.UserInitial), x.NPINumber ?? string.Empty, x.Gender))
                       .ToList();
                }
            }

            #endregion

            return null;
        }
        public void PrepareDownloadSample(string path)
        {
            var activePhysicians = (from u in _unitOfWork.ApplicationUsers
                                    join ur in _unitOfWork.ApplicationUserRoles on u.Id equals ur.UserId
                                    join r in _unitOfWork.ApplicationRoles on ur.RoleId equals r.Id

                                    where !string.IsNullOrEmpty(u.UserInitial)
                                    && u.IsActive && u.IsDeleted == false && u.IsStrokeAlert == true
                                    && (r.Name == "Physician" || r.Name == "Partner Physician")

                                    select u.UserInitial
                                    )
                                    .OrderBy(x => x)
                                    .ToList();

            string filePath = Path.Combine(path, "Schedule-Template.csv");
            // write data in file
            using (var file = new StreamWriter(filePath))
            {
                // header line
                file.WriteLine("Date," + string.Join(",", activePhysicians));

                // one line for each day
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                    file.WriteLine(DateTime.Now.ToString("MM/" + day.ToString() + "/yyyy") + ",07:00-18:00");
            }
        }


        public void PrepareScheduleExport(string path,  DateTime StartDate, DateTime EndDate, List<ScheduleExportVM> response)
        {

            string filePath = path + ".csv"; //Path.Combine(path, ".csv");
            var phyInitials = response.Select(x => x.Title).Distinct().ToList();
            DateTime DateOfcurrentMonth = StartDate.AddMonths(1);
            DateTime TempDate = new DateTime();
            using (var file = new StreamWriter(filePath))
            {
                for (int x = 0; x <= 40; x++)
                {
                    file.WriteLine(" " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " " + " ");
                }
            }
            // write data in file
            using (var file = new StreamWriter(filePath))
            {
                // header line
                file.WriteLine("Date," + string.Join(",", phyInitials));
                String diff2 = (EndDate - StartDate).TotalDays.ToString();
                if (diff2.ToInt() > 10) // Just a random check for days greater than a week OR Month View
                {
                    //Month
                    // one line for each day
                    for (int day = 1; day <= DateTime.DaysInMonth(DateOfcurrentMonth.Year, DateOfcurrentMonth.Month); day++)
                    {
                        TempDate = new DateTime(DateOfcurrentMonth.Year, DateOfcurrentMonth.Month, day);
                        StringBuilder builder = new StringBuilder();
                        foreach (var pinitial in phyInitials)
                        {
                            ScheduleExportVM tempobj = response.Where(row => row.ScheduleDate.ToDateTime() == TempDate && row.Title == pinitial).FirstOrDefault();
                            if (tempobj != null)
                            {
                                DateTime? startTime = tempobj.Start.ToDateTime();
                                DateTime? endTime = tempobj.End.ToDateTime();

                                var temptimeing = startTime.Value.Hour + ":" + startTime.Value.Minute + "-" + endTime.Value.Hour + ":" + endTime.Value.Minute;
                                // add by husnain
                                DateTime _startdate = (DateTime)startTime;
                                DateTime _enddate = (DateTime)endTime;
                                string _start = _startdate.ToString("HH:mm");
                                string _end = _enddate.ToString("HH:mm");
                                var _temptimeing = _start + "-" + _end;
                                // end
                                builder.Append(_temptimeing).Append(",");
                            }
                            else
                            {
                                builder.Append(" ").Append(",");
                            }


                        }
                        file.WriteLine(DateOfcurrentMonth.ToString("MM/" + day.ToString() + "/yyyy") + "," + builder.ToString());

                    }
                }
                else
                {
                    //Less than Month
                    int dayscounter = 0;
                    // one line for each day
                    for (int day = 1; day <= diff2.ToInt() + 1; day++)
                    {
                        TempDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day);
                        StringBuilder builder = new StringBuilder();
                        foreach (var pinitial in phyInitials)
                        {
                            ScheduleExportVM tempobj = response.Where(row => row.ScheduleDate.ToDateTime() == TempDate.AddDays(dayscounter) && row.Title == pinitial).FirstOrDefault();
                            if (tempobj != null)
                            {
                                DateTime? startTime = tempobj.Start.ToDateTime();
                                DateTime? endTime = tempobj.End.ToDateTime();

                                var temptimeing = startTime.Value.Hour + ":" + startTime.Value.Minute + "-" + endTime.Value.Hour + ":" + endTime.Value.Minute;
                                // add by husnain
                                DateTime _startdate = (DateTime)startTime;
                                DateTime _enddate = (DateTime)endTime;
                                string _start = _startdate.ToString("HH:mm");
                                string _end = _enddate.ToString("HH:mm");
                                var _temptimeing = _start + "-" + _end;
                                // end
                                builder.Append(_temptimeing).Append(",");
                            }
                            else
                            {
                                builder.Append(" ").Append(",");
                            }

                        }
                        file.WriteLine(StartDate.ToString("MM/" + TempDate.AddDays(dayscounter).Day.ToString() + "/yyyy") + "," + builder.ToString());
                        dayscounter++;
                    }
                }

            }
        }

        #endregion

        #region private-methods
        private long ParseScheduleTime(string strTime)
        {
            long ticks = -1;
            int hours = 0;
            TimeSpan convertedTime;

            if (int.TryParse(strTime, out hours))
            {
                if (TimeSpan.TryParse(strTime + ":00", out convertedTime))
                    return convertedTime.Ticks;
            }
            else if (TimeSpan.TryParse(strTime, out convertedTime))
            {
                return convertedTime.Ticks;
            }
            return ticks;
        }

       
        private SchedulerResponseViewModel ImportSchedule(DataTable tableSchedules, string loggedinUserId, string loggedinUserName, bool SkipErrors,  string impType)
        {
            var listColumns = new List<string>();
            SchedulerResponseViewModel response = new SchedulerResponseViewModel();

            // generate columns list
            foreach (DataColumn col in tableSchedules.Columns) { if (!listColumns.Contains(col.ColumnName) && col.ColumnName.ToLower() != "date") listColumns.Add(col.ColumnName); }

            // if there are column to import
            if (listColumns.Count() == 0) return response;

            // get user's info based on initials in CSV
            var listUsers = _unitOfWork.ApplicationUsers
                                       .Where(x => listColumns.Contains(x.UserInitial))
                                       .Select(x => new
                                       {
                                           x.UserInitial,
                                           x.Id,
                                           x.FirstName,
                                           x.LastName,
                                           x.IsStrokeAlert,
                                           x.NHAlert,
                                           x.AspNetUserRoles.Where(y => y.UserId == y.AspNetUser.Id).FirstOrDefault().AspNetRole.Name
                                       })
                                       .ToList();
            var distinctScheduleUserIds = listUsers.Select(x => x.Id).ToList();
            // temp schedules
            var listSchedule = new List<user_schedule>();
            var currentTime = DateTime.Now.ToEST();

            // process all columns
            int rowCount = 2; // first row is header that's why initializing it from 2
            int colCount = 1;

            foreach (DataRow row in tableSchedules.Rows)
            {
                colCount = 1;
                DateTime schDate = new DateTime();
                bool hasValidDate = true;
                if (string.IsNullOrEmpty(row["Date"].ToString()))
                    continue;

                try { schDate = Convert.ToDateTime(row["Date"]); } catch { hasValidDate = false; }
                if (hasValidDate)
                {
                    foreach (string currentColumn in listColumns)
                    {
                        string columnData = Convert.ToString(row[currentColumn]);
                        if (!string.IsNullOrEmpty(currentColumn) && !string.IsNullOrEmpty(columnData))
                        {
                            var splitShift = columnData.Split('|');
                            foreach (string schItem in splitShift)
                            {
                                string[] times = null;
                                string userId = null;
                                try
                                {
                                    try { if (!string.IsNullOrEmpty(schItem.Trim())) { times = schItem.Trim().Split('-').Where(m => !string.IsNullOrEmpty(m)).ToArray(); } } catch { response.ParseErrors.Add($"Invalid Time {schItem} on Row {rowCount} and Column {colCount}"); }
                                    if (userId == null)
                                    {
                                        try { userId = listUsers.Where(x => x.UserInitial.ToLower() == currentColumn.ToLower()).FirstOrDefault()?.Id; } catch { response.ParseErrors.Add($"Invalid Initial {currentColumn} on Row {rowCount} and Column {colCount}"); }
                                    }

                                    if (userId != null && times != null)
                                    {
                                        if (times.Length == 2)
                                        {
                                            var schEntry = new user_schedule
                                            {
                                                uss_date = schDate,
                                                uss_user_id = userId,
                                                uss_time_from = ParseScheduleTime(times[0]),//TimeSpan.Parse(times[0]).Ticks,
                                                uss_time_to = ParseScheduleTime(times[1]), //TimeSpan.Parse(times[1]).Ticks,
                                                uss_is_active = true,
                                                uss_created_by = loggedinUserId,
                                                uss_created_by_name = loggedinUserName,
                                                uss_created_date = currentTime,
                                                uss_is_publish = (listUsers.Where(x => x.UserInitial.ToLower() == currentColumn.ToLower()).FirstOrDefault().Name == "AOC") ? true : (listUsers.Where(x => x.UserInitial.ToLower() == currentColumn.ToLower()).FirstOrDefault().NHAlert) ? true : impType == "aoc" ? true : false
                                            };

                                            

                                            // next day's time check 
                                            if (schEntry.uss_time_to < schEntry.uss_time_from)
                                                schEntry.uss_time_to = schEntry.uss_time_to + TimeSpan.TicksPerDay;

                                            schEntry.uss_time_from_calc = schEntry.uss_date.Date.AddTicks(schEntry.uss_time_from);
                                            schEntry.uss_time_to_calc = schEntry.uss_date.Date.AddTicks(schEntry.uss_time_to);

                                            DateTime TimeFrom = schEntry.uss_date.Date.AddTicks(schEntry.uss_time_from);
                                            DateTime TimeTo = schEntry.uss_date.Date.AddTicks(schEntry.uss_time_to);

                                            schEntry.uss_date_num = Convert.ToInt64(schDate.Year.ToString() + schDate.DayOfYear.ToString("000"));
                                            schEntry.uss_time_from_calc_num = Convert.ToInt64(TimeFrom.Year.ToString() + TimeFrom.DayOfYear.ToString("000") + TimeFrom.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());
                                            schEntry.uss_time_to_calc_num = Convert.ToInt64(TimeTo.Year.ToString() + TimeTo.DayOfYear.ToString("000") + TimeTo.TimeOfDay.ToString().Replace(":", "").Substring(0, 4).Trim());

                                            listSchedule.Add(schEntry);
                                        }
                                        else
                                        {
                                            response.ParseErrors.Add($"Invalid Time {schItem} on Row {rowCount} and Column {colCount}");
                                            response.Success = false;
                                        }
                                    }
                                }
                                catch
                                {
                                    response.ParseErrors.Add($"Parse Data issue on Row {rowCount} and Column {colCount}");
                                }
                            }
                        }
                        colCount++;
                    }
                }
                else
                {
                    response.Success = false;
                    response.ParseErrors.Add($"Invalid Date {row["Date"].ToString()} on Row {rowCount} and Column {colCount}");
                }
                rowCount++;
            }
            if (listSchedule != null && listSchedule.Count() > 0 && response.Success || (listSchedule != null && listSchedule.Count() > 0 && SkipErrors))
            {
                try
                {
                    var distinctDates = listSchedule.Select(x => x.uss_date).Distinct().ToList();
                    var oldEntries = _unitOfWork.ScheduleRepository.Query().Where(x => distinctDates.Any(y => y == x.uss_date)).ToList();

                    #region Get Only Required Physician
                    PhysicianDictionary physicianDictionary = new PhysicianDictionary();
                    var ids = physicianDictionary.GetRecordAsList(impType);
                    var foundIds = _adminService.GetAllUsersIds(ids);
                    HashSet<string> resIds = new HashSet<string>(oldEntries.Select(s => s.uss_user_id.ToString()));
                    var matchIds = resIds.Intersect(foundIds).ToList();
                    //var _oldEntries = oldEntries.Where(x => matchIds.Contains(x.uss_user_id)).ToList();
                    var _oldEntries = oldEntries.Where(x => matchIds.Contains(x.uss_user_id)).Where(x => distinctScheduleUserIds.Contains(x.uss_user_id)).ToList();
                    #endregion
                    _unitOfWork.BeginTransaction();
                    // remove old entries 
                    if (_oldEntries != null && _oldEntries.Count() > 0)
                    {
                        _unitOfWork.ScheduleRepository.DeleteRange(_oldEntries);
                        _unitOfWork.Save();
                    }
                    // add schedule entries
                    if (listSchedule.Count() > 0)
                    {
                        _unitOfWork.ScheduleRepository.InsertRange(listSchedule);
                        _unitOfWork.Save();
                    }
                    _unitOfWork.Commit();
                    //return new SchedulerResponseViewModel { Message = "Schedule is updated.", Success = true };
                    return new SchedulerResponseViewModel { Message = impType == "aoc" ? "Schedule saved." : (listSchedule.Any(x => x.uss_is_publish == false)) ? "Schedule saved. Publish from Schedule page." : "Schedule saved.", Success = true };
                }
                catch (Exception ex)
                {
                    try { _unitOfWork.Rollback(); }
                    catch
                    {
                        throw ex;
                    }
                }
                finally { listSchedule = null; }
            }
            return response;
        }
        private IQueryable<user_schedule> GetFilteredSchedule(bool isGetAllRequeset, string userId)
        {
            var query = _unitOfWork.ScheduleRepository.Query().Where(m => m.AspNetUser.IsActive && m.AspNetUser.IsDeleted == false);
            // if not admin then return only physician own schedule
            if (!isGetAllRequeset)
                query = query.Where(x => x.uss_user_id == userId);

            return query;
        }
        #endregion

        #region Code for Paoc import
        public void PrepareDownloadSampleImp(string path)
        {
            var activePhysicians = (from u in _unitOfWork.ApplicationUsers
                                    join ur in _unitOfWork.ApplicationUserRoles on u.Id equals ur.UserId
                                    join r in _unitOfWork.ApplicationRoles on ur.RoleId equals r.Id
                                    where !string.IsNullOrEmpty(u.UserInitial)
                                    && u.IsActive && u.IsDeleted == false
                                    && (r.Name == "aoc")
                                    select u.UserInitial
                                    )
                                    .OrderBy(x => x)
                                    .ToList();

            string filePath = Path.Combine(path, "Schedule-Template.csv");
            // write data in file
            using (var file = new StreamWriter(filePath))
            {
                // header line
                file.WriteLine("Date," + string.Join(",", activePhysicians));

                // one line for each day
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                    file.WriteLine(DateTime.Now.ToString("MM/" + day.ToString() + "/yyyy") + ",07:00-18:00");
            }
        }
        #endregion
        #region Code for Sleep Import
        public void PrepareDownloadSampleSlp(string path)
        {
            var activePhysicians = (from u in _unitOfWork.ApplicationUsers
                                    join ur in _unitOfWork.ApplicationUserRoles on u.Id equals ur.UserId
                                    join r in _unitOfWork.ApplicationRoles on ur.RoleId equals r.Id
                                    where !string.IsNullOrEmpty(u.UserInitial)
                                    && u.IsActive && u.IsDeleted == false && u.IsSleep == true
                                    && (r.Name == "Physician" || r.Name == "Partner Physician")

                                    select u.UserInitial
                                    )
                                    .OrderBy(x => x)
                                    .ToList();

            string filePath = Path.Combine(path, "Schedule-Template.csv");
            // write data in file
            using (var file = new StreamWriter(filePath))
            {
                // header line
                file.WriteLine("Date," + string.Join(",", activePhysicians));

                // one line for each day
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                    file.WriteLine(DateTime.Now.ToString("MM/" + day.ToString() + "/yyyy") + ",07:00-18:00");
            }
        }
        #endregion
        #region Code for NH Import
        public void PrepareDownloadSampleNH(string path)
        {
            var activePhysicians = (from u in _unitOfWork.ApplicationUsers
                                    join ur in _unitOfWork.ApplicationUserRoles on u.Id equals ur.UserId
                                    join r in _unitOfWork.ApplicationRoles on ur.RoleId equals r.Id
                                    where !string.IsNullOrEmpty(u.UserInitial)
                                    && u.IsActive && u.IsDeleted == false && u.NHAlert
                                    && (r.Name == "Physician" || r.Name == "Partner Physician")

                                    select u.UserInitial
                                    )
                                    .OrderBy(x => x)
                                    .ToList();

            string filePath = Path.Combine(path, "Schedule-Template.csv");
            // write data in file
            using (var file = new StreamWriter(filePath))
            {
                // header line
                file.WriteLine("Date," + string.Join(",", activePhysicians));

                // one line for each day
                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); day++)
                    file.WriteLine(DateTime.Now.ToString("MM/" + day.ToString() + "/yyyy") + ",07:00-18:00");
            }
        }
        #endregion
    }

    public class PhysicianDictionary
    {
        Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        public PhysicianDictionary()
        {
            keyValuePairs.Add("0029737b-f013-4e0b-8a31-1b09524194f9", "Physician");
            keyValuePairs.Add("55C69965-83C0-4F87-B1C7-436340856C15", "aoc");
            keyValuePairs.Add("684c8b74-216a-48bb-a9c1-c9cd4c1014fc", "PartnerPhysician");
        }
        public Dictionary<string,string> GetList()
        {
            return keyValuePairs;
        }
        public string GetRecord(string physician)
        {
            var record = keyValuePairs.Where(x => x.Value == physician).FirstOrDefault();
            return record.Key;
        }
        public List<string> GetRecordAsList(string physician)
        {
            List<string> list = new List<string>();
            var record = keyValuePairs.Where(x => x.Value == physician).FirstOrDefault();
            list.Add(record.Key);
            if(physician == "Physician")
            {
                var _li = keyValuePairs.Where(x => x.Value == "PartnerPhysician").FirstOrDefault();
                list.Add(_li.Key);
            }
            return list;
        }
        
    }
}
