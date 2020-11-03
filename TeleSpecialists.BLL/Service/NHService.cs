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

namespace TeleSpecialists.BLL.Service
{
    public class NHService : BaseService
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
                                          m.uss_key != model.uss_key && m.AspNetUser.NHAlert == true)
                                   .ToList();
            if (existingSchdule != null && existingSchdule.Count > 0)
                return new KeyValuePair<long, bool>(0, false);

            _unitOfWork.ScheduleRepository.Update(model);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return new KeyValuePair<long, bool>(model.uss_key, true);
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
                                          m.uss_user_id == model.uss_user_id && m.AspNetUser.NHAlert == true)
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
                .Where(x => x.AspNetUser.NHAlert == true)
                .Where(m => DbFunctions.TruncateTime(m.uss_date) >= DbFunctions.TruncateTime(startDate)
                            && (DbFunctions.TruncateTime(m.uss_date) <= DbFunctions.TruncateTime(endDate)));
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
                              .Where(m => m.AspNetUser.NHAlert == true)
                              ;
        }
        public IQueryable<user_schedule> GetScheduleById(long id)
        {
            return _unitOfWork.ScheduleRepository
                              .Query()
                              .Where(m => m.uss_key == id)
                               .Where(m => m.AspNetUser.NHAlert == true);
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
            if (phy_type == "Physician")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsStrokeAlert == true) on s.uss_user_id equals u.Id
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
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsStrokeAlert == true) on s.uss_user_id equals u.Id
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
                    return (from u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.IsStrokeAlert == true)
                            join r in _unitOfWork.UserRoleRepository.Query() on u.Id equals r.UserId
                            where r.RoleId == PacPhysician.Id

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

            #region If type is NH 

            if (phy_type.ToUpper() == "NH")
            {
                if (onlySchedulePhys)
                {
                    return (from s in this.GetFilteredSchedule(isgetAllRequest, userId)
                            join u in _unitOfWork.UserRepository.Query().Where(x => x.IsActive && x.IsDeleted == false && x.NHAlert == true) on s.uss_user_id equals u.Id
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
                                    && u.IsActive && u.IsDeleted == false && u.NHAlert == true
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


        private SchedulerResponseViewModel ImportSchedule(DataTable tableSchedules, string loggedinUserId, string loggedinUserName, bool SkipErrors, string impType)
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
                                                uss_is_publish = (listUsers.Where(x => x.UserInitial.ToLower() == currentColumn.ToLower()).FirstOrDefault().Name == "AOC") ? true : (listUsers.Where(x => x.UserInitial.ToLower() == currentColumn.ToLower()).FirstOrDefault().IsStrokeAlert) ? false : true
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
                    return new SchedulerResponseViewModel { Message = "Schedule is updated.", Success = true };
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
    }
}
