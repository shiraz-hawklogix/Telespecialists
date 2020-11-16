using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.Reports;

namespace TeleSpecialists.BLL.Service
{
    public class RateService : BaseService
    {
        private PhysicianRateService _physicianRateService;
        private PhysicianPercentageRateService _physicianPercentageRateService;
        private PhysicianHolidayRateService _physicianHolidayRateService;

        public RateService()
        {
            _physicianRateService = new PhysicianRateService();
            _physicianPercentageRateService = new PhysicianPercentageRateService();
            _physicianHolidayRateService = new PhysicianHolidayRateService();
        }

        private AspNetUser getUserIdentity(string id)
        {
            return _unitOfWork.UserRepository.Query().FirstOrDefault(x => x.Id == id);
        }

        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.RateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.rat_phy_key equals n.Id into physicians
                               join type in GetUclData(UclTypes.BillingCode) on m.rat_cas_id equals type.ucd_key into CaseTypeEntity
                               from case_type in CaseTypeEntity.DefaultIfEmpty()
                               orderby m.rat_key descending
                               select new
                               {
                                   m.rat_key,
                                   m.rat_phy_key,
                                   m.rat_starting,
                                   m.rat_ending,
                                   m.rat_range,
                                   m.rat_shift_hour,
                                   m.rat_shift_name,
                                   m.rat_start_date,
                                   m.rat_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   CaseType = case_type != null ? case_type.ucd_title : "",
                                   rate = "$" + m.rat_price
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public DataSourceResult GetAllProductivity(DataSourceRequest request, string phy_key)
        {
            var caseTypelist = from m in _unitOfWork.RateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.rat_phy_key equals n.Id into physicians
                               join type in GetUclData(UclTypes.BillingCode) on m.rat_cas_id equals type.ucd_key into CaseTypeEntity
                               from case_type in CaseTypeEntity.DefaultIfEmpty()
                               where m.rat_phy_key == phy_key
                               orderby case_type.ucd_title
                               select new
                               {
                                   m.rat_key,
                                   m.rat_phy_key,
                                   m.rat_starting,
                                   m.rat_ending,
                                   m.rat_range,
                                   m.rat_shift_hour,
                                   m.rat_shift_name,
                                   m.rat_start_date,
                                   m.rat_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   CaseType = case_type != null ? case_type.ucd_title : "",
                                   rate = "$" + m.rat_price
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public physician_rate GetDetails(int id)
        {
            var model = _unitOfWork.RateRepository.Query()
                                   .FirstOrDefault(m => m.rat_key == id);
            return model;
        }
        public string GetPhysicianName(string id)
        {
            var getRecord = _unitOfWork.ApplicationUsers.Where(x => x.Id == id).FirstOrDefault();
            string name = getRecord.FirstName + " " + getRecord.LastName;
            return name;
        }
        public bool IsAlreadyExists(string Phy_key, DateTime Start_date, DateTime End_date, int case_ID, int start_Index, int end_Index, int shiftid)
        {
            return _unitOfWork.RateRepository.Query()
                                                 .Where(m => m.rat_phy_key == Phy_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.rat_start_date) <= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.rat_end_date) >= DbFunctions.TruncateTime(End_date))
                                                 .Where(m => m.rat_cas_id == case_ID)
                                                 .Where(m => m.rat_starting <= start_Index)
                                                 .Where(m => m.rat_ending >= end_Index)
                                                 .Where(m => m.rat_shift_id == shiftid)
                                                 //.Where(m => m.rat_ending >= start_Index)
                                                 .Any();
        }
        public bool IsAlreadyExistsRange(string Phy_key, DateTime Start_date, DateTime End_date, int case_ID, int start_Index, int end_Index, int shiftid)
        {
            return _unitOfWork.RateRepository.Query()
                                                 .Where(m => m.rat_phy_key == Phy_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.rat_start_date) <= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.rat_end_date) >= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => m.rat_cas_id == case_ID)
                                                 //.Where(m => m.rat_starting > end_Index)
                                                 .Where(m => m.rat_ending >= start_Index)
                                                 .Where(m => m.rat_shift_id == shiftid)
                                                 //.Where(m => m.rat_ending >= start_Index)
                                                 .Any();

        }
        //phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_ending >= _strokePlusState && x.rat_starting <= _strokePlusState && x.rat_cas_id == 1 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
        public void Create(physician_rate entity)
        {
            _unitOfWork.RateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_rate entity)
        {
            _unitOfWork.RateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        private physician_shift_rate GetRecordBlast(string id, int shift)
        {
            var obj = _unitOfWork.PhysicianRateRepository.Query()
                .Where(x => x.psr_phy_key == id && x.psr_shift == shift).FirstOrDefault();
            return obj;
        }
        #region Get Physician Rate
        private physician_shift_rate GetRecord(string id, int shift, DateTime dateTime)
        {
            var obj = _unitOfWork.PhysicianRateRepository.Query()
                .Where(x => x.psr_phy_key == id && x.psr_shift == shift && (DbFunctions.TruncateTime(x.psr_start_date) <= DbFunctions.TruncateTime(dateTime) && DbFunctions.TruncateTime(x.psr_end_date) >= DbFunctions.TruncateTime(dateTime))).FirstOrDefault();
            return obj;
        }
        #endregion
        #region Get Schedule of physician
        private List<user_schedule> GetPhysicianSchedule(string userId, DateTime startDate, DateTime endDate)
        {
            // Fetch records from previous day of start date, to sync multidate enteries.
            startDate = startDate.AddDays(-1);
            endDate = endDate.AddDays(1);
            // return schedules
            var query = _unitOfWork.ScheduleRepository.Query()
                .Where(m => m.uss_user_id == userId && DbFunctions.TruncateTime(m.uss_date) >= DbFunctions.TruncateTime(startDate)
                            && (DbFunctions.TruncateTime(m.uss_date) <= DbFunctions.TruncateTime(endDate)));

            return query.OrderBy(m => m.uss_date).ToList();
        }
        #endregion

        private IQueryable<physician_rate> getPhysicianRates(string id)
        {
            var list = _unitOfWork.RateRepository.Query().Where(x => x.rat_phy_key == id);
            return list;
        }
        private IEnumerable<physician_rate> _getPhysicianRates(string id)
        {
            var list = _unitOfWork.RateRepository.Query().Where(x => x.rat_phy_key == id).ToList();
            return list;
        }
        public decimal getBill(string id, DateTime startDate, DateTime endDate)
        {
            decimal _result = 0;
            try
            {
                List<string> physician = new List<string>();
                physician.Add(id);
                //Check Schedule for physician
                var isSchedulesExist = GetPhysicianSchedule(id, startDate, endDate);
                if (isSchedulesExist.Count > 0)
                {
                    #region Calculated DateWise Strokes, New, Fu patients

                    var _listForNewFu = _countTodayNewFu(startDate, endDate, physician, isSchedulesExist);
                    var list = CountToday(startDate, endDate, physician);
                    var countedTotal = (from obj in list
                                        group obj by obj.PhysicianKey into g
                                        select new PhysicianBillingByShiftViewModel
                                        {
                                            AssignDate = g.FirstOrDefault().AssignDate,
                                            CC1_StrokeAlert = g.Sum(x => x.CC1_StrokeAlert),
                                            CC1_STAT = g.Sum(x => x.CC1_STAT),
                                            New = g.Sum(x => x.New),
                                            FU = g.Sum(x => x.FU),
                                            EEG = g.Sum(x => x.EEG),
                                            LTM_EEG = g.Sum(x => x.LTM_EEG),
                                            TC = g.Sum(x => x.TC),
                                            Not_Seen = g.Sum(x => x.Not_Seen),
                                            Blast = g.Sum(x => x.Blast),
                                            Total = g.Sum(x => x.Total),
                                            time_from_calc = g.FirstOrDefault().time_from_calc,
                                            time_to_calc = g.FirstOrDefault().time_to_calc
                                        }).ToList();
                    var _countedTotalNewFu = (from obj in _listForNewFu
                                              group obj by obj.PhysicianKey into g
                                              select new PhysicianBillingByShiftViewModel
                                              {
                                                  AssignDate = g.FirstOrDefault().AssignDate,
                                                  CC1_StrokeAlert = g.Sum(x => x.CC1_StrokeAlert),
                                                  CC1_STAT = g.Sum(x => x.CC1_STAT),
                                                  New = g.Sum(x => x.New),
                                                  FU = g.Sum(x => x.FU),
                                                  EEG = g.Sum(x => x.EEG),
                                                  LTM_EEG = g.Sum(x => x.LTM_EEG),
                                                  TC = g.Sum(x => x.TC),
                                                  Not_Seen = g.Sum(x => x.Not_Seen),
                                                  Blast = g.Sum(x => x.Blast),
                                                  Total = g.Sum(x => x.Total)
                                              }).ToList();
                    var seenPatient = new PhysicianBillingByShiftViewModel();
                    if (_countedTotalNewFu.Count() > 0)
                    {
                        var _defaultVal = _countedTotalNewFu.FirstOrDefault();
                        if (countedTotal.Count == 0)
                        {
                            //countedTotal.Add(_defaultVal);
                            seenPatient = _defaultVal;
                        }
                        else
                        {
                            seenPatient = countedTotal.FirstOrDefault();

                            seenPatient.New = _defaultVal.New;
                            seenPatient.FU = _defaultVal.FU;
                            seenPatient.EEG = _defaultVal.EEG;
                            seenPatient.LTM_EEG = _defaultVal.LTM_EEG;

                        }

                    }
                    else
                    {
                        if (countedTotal.Count > 0)
                        {
                            seenPatient = countedTotal.FirstOrDefault();
                            if (_countedTotalNewFu.Count > 0)
                            {
                                var _defaultVal = _countedTotalNewFu.FirstOrDefault();
                                seenPatient.New = _defaultVal.New;
                                seenPatient.FU = _defaultVal.FU;
                                seenPatient.EEG = _defaultVal.EEG;
                                seenPatient.LTM_EEG = _defaultVal.LTM_EEG;
                            }
                        }
                    }


                    #endregion
                    #region  count billing rates accoding to counted strokes, New, Fu ects
                    var phy_rate_list = _getPhysicianRates(id);

                    decimal foundTotalBill = 0;

                    #endregion
                    // object is Empty
                    if(seenPatient.AssignDate == null)
                    {
                        var objSchedule = isSchedulesExist.Find(x => x.uss_date == startDate);
                        if(objSchedule == null)
                        {
                            var _Schedule = isSchedulesExist.FirstOrDefault();
                            DateTime _start = (DateTime)_Schedule.uss_time_from_calc;
                            DateTime _end = (DateTime)_Schedule.uss_time_to_calc;
                            if (_start.Date != _end.Date)
                                objSchedule = _Schedule;//isSchedulesExist.FirstOrDefault();
                        }
                        string dt = objSchedule.uss_date.ToString("M/d/yyy");
                        seenPatient.AssignDate = dt;
                        seenPatient.time_from_calc = (DateTime)objSchedule.uss_time_from_calc;
                        seenPatient.time_to_calc = (DateTime)objSchedule.uss_time_to_calc;
                    }
                    var userDetail = getUserIdentity(id);
                     bool isStrokeAlert = userDetail.NHAlert;
                    #region Get floor rate of a physician according to the shift
                    foundTotalBill = FloorRate(startDate, id, foundTotalBill, isSchedulesExist, phy_rate_list, seenPatient, isStrokeAlert);
                    _result = foundTotalBill;
                    #endregion
                }

            }
            catch (Exception)
            {
                _result = 0;
            }
            return _result;
        }
        List<PhysicianBillingByShiftViewModel> CountToday(DateTime startDate, DateTime endDate, List<string> physicians)
        {
            List<int> caseStatus = null;
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            List<PhysicianBillingByShiftViewModel> offShiftCasesList = null;

            #region On shift billing
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join s in _unitOfWork.ScheduleRepository.Query() on c.cas_phy_key equals s.uss_user_id
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                // add one hour in both times
                                let time_from_calc = SqlFunctions.DateAdd("hh", -2, s.uss_time_from_calc)
                                let time_to_calc = SqlFunctions.DateAdd("hh", 2, s.uss_time_to_calc)
                                where c.cas_phy_key != null &&
                                (c.cas_billing_bic_key == 1 || c.cas_billing_bic_key == 2 )
                                //&& c.cas_cst_key != 140
                                && c.cas_cst_key == 20
                                && c.cas_billing_physician_blast == false
                                && c.cas_billing_bic_key != null
                                && time_from_calc <= c.cas_physician_assign_date
                                && time_to_calc >= c.cas_physician_assign_date
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
                                    time_from_calc = g.Key.uss_time_from_calc,
                                    time_to_calc = g.Key.uss_time_to_calc
                                }).ToList();

            #region Get record after shift time 

            ShiftReport shiftReport = new ShiftReport();
            var getRecordList = shiftReport.GetRecordsForBilling(physicians, startDate, endDate, onShiftCasesList, caseStatus, TeleSpecialists.BLL.Helpers.ShiftType.All);
            onShiftCasesList = getRecordList;
            #endregion

            #endregion

            #region Off shift Billing

            DateTime _startTime = startDate.AddDays(1);
            DateTime _endTime = endDate.AddDays(-1);

            var offShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                 join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                 where c.cas_phy_key != null
                                 && c.cas_physician_assign_date != null
                                 && (c.cas_billing_bic_key == 1 || c.cas_billing_bic_key == 2)
                                 && c.cas_billing_physician_blast
                                 && c.cas_cst_key == 20
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(_startTime)
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(_endTime)
                                 select new { c, u }).Except(from d in onShiftQuery select new { d.c, d.u });

            if (physicians != null)
                offShiftQuery = offShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            if (caseStatus != null)
                offShiftQuery = offShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            try
            {
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
                                         CC1_STAT = g.Sum(x => x.c.cas_billing_bic_key == 2 ? 1 : 0),
                                         New = null,
                                         FU = null,
                                         EEG = null,
                                         LTM_EEG = null,
                                         TC = null,
                                         Not_Seen = null,
                                         Blast = null,
                                         Total = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0)
                                     }).ToList();
            }
            catch (Exception) { }

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
            #endregion

            return null;
        }
        List<PhysicianBillingByShiftViewModel> _countTodayNewFu(DateTime startDate, DateTime endDate, List<string> physicians, List<user_schedule> scheduleList)
        {
            //List<string> physicians = new List<string>();
            List<int> caseStatus = new List<int>();
            //physicians.Add(id);
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                where c.cas_phy_key != null && (c.cas_billing_bic_key == 3 || c.cas_billing_bic_key == 4 || c.cas_billing_bic_key == 5 || c.cas_billing_bic_key == 6 || c.cas_billing_bic_key == 7 || c.cas_billing_bic_key == 324 || c.cas_billing_bic_key == 325 || c.cas_billing_bic_key == 326)
                                && c.cas_cst_key == 20 && c.cas_billing_date_of_consult != null && c.cas_billing_bic_key != null
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_phy_key)
                                select new { c, u });

            if (physicians != null)
                onShiftQuery = onShiftQuery.Where(x => physicians.Contains(x.c.cas_phy_key));
            //if (caseStatus != null)
            //    onShiftQuery = onShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    new { onShiftModel.c } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.c.cas_billing_date_of_consult).Value, false),
                                            Physician = onShiftModel.u.LastName + " " + onShiftModel.u.FirstName,
                                            PhysicianId = onShiftModel.u.PhysicianId,
                                            PhysicianKey = onShiftModel.c.cas_phy_key,
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
                                    EEG = g.Sum(x => x.c.cas_billing_bic_key == 5 || x.c.cas_billing_bic_key == 324 || x.c.cas_billing_bic_key == 325 ? 1 : 0),
                                    LTM_EEG = g.Sum(x => x.c.cas_billing_bic_key == 6 || x.c.cas_billing_bic_key == 326 ? 1 : 0),
                                    TC = g.Sum(x => x.c.cas_billing_bic_key == 7 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),//g.Sum(x => x.c.cas_key > 0 ? 1 : 0),
                                }).ToList();
            foreach (var item in onShiftCasesList)
            {
                try
                {
                    DateTime _dt = Convert.ToDateTime(item.AssignDate);
                    var indexOf = onShiftCasesList.IndexOf(item);
                    var getRecord = //scheduleList.Find(x => x.uss_date == _dt);
                        _unitOfWork.ScheduleRepository.Query().Where(x => x.uss_user_id == item.PhysicianKey && DbFunctions.TruncateTime(_dt) == DbFunctions.TruncateTime(x.uss_date)).FirstOrDefault();
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
                catch(Exception)
                {

                }
            }
            return onShiftCasesList;
        }
        private decimal CalculatePhysicianBill(IEnumerable<physician_rate> phy_rate_list, PhysicianBillingByShiftViewModel obj, int shiftId, bool isStrokeAlert)
        {
            decimal phy_strokeAlert = 0; decimal phy_new = 0; decimal phy_fu = 0;
            int? _strokePlusState = null; int? _countNew = null;
            if (isStrokeAlert == false)
            {
                _strokePlusState = obj.CC1_StrokeAlert + obj.CC1_STAT;
                _countNew = obj.New;
            }
            else
            {
                if (obj.PhysicianKey == "10d96fd1-9b81-44c8-ad36-5655d849b5cb" || obj.PhysicianKey == "f27b69ad-8a73-47d9-b394-3541737def4b")
                {
                    _strokePlusState = obj.CC1_StrokeAlert + obj.CC1_STAT;
                    _countNew = obj.New;
                }
                else
                {
                    _strokePlusState = obj.CC1_StrokeAlert;
                    _countNew = obj.New + obj.CC1_STAT;
                }
            }
            DateTime dt = Convert.ToDateTime(obj.AssignDate);
            var _cc1Stroke = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_ending >= _strokePlusState && x.rat_starting <= _strokePlusState && x.rat_cas_id == 1 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            if(_cc1Stroke == null)
            {
                _cc1Stroke = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_ending >= _strokePlusState && x.rat_starting <= _strokePlusState && x.rat_cas_id == 2 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            }
            var _new = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_ending >= _countNew && x.rat_starting <= _countNew && x.rat_cas_id == 3  && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            var _fu = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_ending >= obj.FU && x.rat_starting <= obj.FU && x.rat_cas_id == 4  && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            //var _eeg = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 5  && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            //var _LTM_eeg = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 6  && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            //var _tc = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 7  && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();

            if (_cc1Stroke != null)
                phy_strokeAlert = (decimal)_cc1Stroke.rat_price;
            if (_new != null)
                phy_new = (decimal)_new.rat_price;
            if (_fu != null)
                phy_fu = (decimal)_fu.rat_price;
            /*
            if (_eeg != null)
            {
                var _val = (decimal)_eeg.rat_price;
                try {
                    var total = obj.EEG * _val;
                    eeg = (decimal)total;
                } catch { }
                
            }
            if (_LTM_eeg != null)
            {
                var _val = (decimal)_LTM_eeg.rat_price;
                try
                {
                    var total = obj.LTM_EEG * _val;
                    ltm_eeg = (decimal)total;
                }
                catch { }
            }
            if (_tc != null)
            {
                var _val = (decimal)_tc.rat_price;
                try
                {
                    var total = obj.TC * _val;
                    tc = (decimal)total;
                }
                catch { }
                
            }
            */
            var billing = phy_strokeAlert + phy_new + phy_fu; //+ eeg + ltm_eeg + tc;
            var isPercentageFound = _physicianPercentageRateService.GetDetailsByKey(obj.PhysicianKey, dt, shiftId);
            if (isPercentageFound != null)
            {
                var _rate = isPercentageFound.ppr_percentage / 100;
                var bill_percentage = _rate * billing;
                billing += (decimal)bill_percentage;
            }
            return billing;
        }
        private decimal CalculatePhysicianBillForEEG(IEnumerable<physician_rate> phy_rate_list, PhysicianBillingByShiftViewModel obj, int shiftId, bool isStrokeAlert)
        {
            decimal phy_new = 0; decimal phy_fu = 0; decimal eeg = 0; decimal ltm_eeg = 0; decimal tc = 0;
            DateTime dt = Convert.ToDateTime(obj.AssignDate);
            var _eeg = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 5 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            var _LTM_eeg = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 6 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();
            var _tc = phy_rate_list.Where(x => x.rat_shift_id == shiftId && x.rat_cas_id == 7 && x.rat_end_date >= dt && x.rat_start_date <= dt).FirstOrDefault();

            if (_eeg != null)
            {
                var _val = (decimal)_eeg.rat_price;
                try
                {
                    var total = obj.EEG * _val;
                    eeg = (decimal)total;
                }
                catch { }

            }
            if (_LTM_eeg != null)
            {
                var _val = (decimal)_LTM_eeg.rat_price;
                try
                {
                    var total = obj.LTM_EEG * _val;
                    ltm_eeg = (decimal)total;
                }
                catch { }
            }
            if (_tc != null)
            {
                var _val = (decimal)_tc.rat_price;
                try
                {
                    var total = obj.TC * _val;
                    tc = (decimal)total;
                }
                catch { }

            }
            var billing =  phy_new + phy_fu + eeg + ltm_eeg + tc;
            //var isPercentageFound = _physicianPercentageRateService.GetDetailsByKey(obj.PhysicianKey, dt, shiftId);
            //if (isPercentageFound != null)
            //{
            //    var _rate = isPercentageFound.ppr_percentage / 100;
            //    var bill_percentage = _rate * billing;
            //    billing += (decimal)bill_percentage;
            //}
            return billing;
        }
        private decimal CalculatePhysicianBlast(PhysicianBillingByShiftViewModel obj)
        {
            DateTime caseDate = obj.assign_date;
            string date = "07/01/2020";
            DateTime compareDate = Convert.ToDateTime(date);
            decimal phy_strokeAlert = 0;
            var _strokePlusState = obj.CC1_StrokeAlert + obj.CC1_STAT;
            
            if (caseDate < compareDate)
            {
                #region old code for get blast rate
                var getBlastPrice = GetRecordBlast(obj.PhysicianKey, 7);
                if (getBlastPrice != null)
                {
                    var _val = (decimal)getBlastPrice.psr_rate;
                    var total = _strokePlusState * _val;
                    phy_strokeAlert = (decimal)total;
                }
                #endregion
            }
            else
            {
                #region New Code for get Blast Rate
                
                if (_strokePlusState <= 22)
                {
                    var total = _strokePlusState * 350;
                    phy_strokeAlert = (decimal)total;
                }
                else if (_strokePlusState > 22 && _strokePlusState <= 42)
                {
                    var first22Blast = 22 * 350;
                    phy_strokeAlert = first22Blast;
                    var morethan22Blast = _strokePlusState - 22;
                    var total = morethan22Blast * 420;
                    phy_strokeAlert += (decimal)total;
                }
                else if (_strokePlusState > 42)
                {
                    var first22Blast = 22 * 350;
                    phy_strokeAlert = first22Blast;
                    var after22Blast = 20 * 420;
                    phy_strokeAlert += after22Blast;
                    var blasts = _strokePlusState - 42;
                    var inThis = blasts * 500;
                    phy_strokeAlert += (decimal)inThis;
                    
                }
                
                #endregion
            }
            return phy_strokeAlert;
        }
        private decimal FloorRate(DateTime scheduleDate, string id, decimal foundTotalBill, List<user_schedule> listSchedule, IEnumerable<physician_rate> phy_rate_list, PhysicianBillingByShiftViewModel objVeiwModel, bool isStrokeAlert)
        {
            decimal phy_floor_rate = 0;
            // get schedule of physician
            if (listSchedule.Count() > 0)
            {
                int shift_id = 0;
                DateTime startDatetime = objVeiwModel.time_from_calc;//_assignDate.Add(startTimeSpan);
                DateTime endDatetime = objVeiwModel.time_to_calc;//_assignDate.Add(endTimeSpan);
                                                                 //var obj = listSchedule.Find(x => x.uss_date == scheduleDate);
                var obj = listSchedule.Where(x => x.uss_time_from_calc == startDatetime && x.uss_time_to_calc == endDatetime).FirstOrDefault();
                if (obj != null)
                {
                    bool gotoProductivity = true;

                    if (obj.uss_shift_key == 10)
                        gotoProductivity = false;

                    #region Find Rate And Productivity
                    DateTime timeFrom = (DateTime)obj.uss_time_from_calc;
                    DateTime endTime = (DateTime)obj.uss_time_to_calc;
                    string day = timeFrom.DayOfWeek.ToString();
                    var hours = (endTime - timeFrom).TotalHours;
                    hours = Math.Round(hours);
                    //get  time from datetime
                    string _startTimeOnly = timeFrom.ToString("HH:mm");
                    string _endTimeOnly = endTime.ToString("HH:mm");
                    #region Get Physician Custom Rate
                    if (obj.uss_custome_rate != null)
                    {
                        phy_floor_rate = (decimal)obj.uss_custome_rate;//(decimal)getRecord.phr_rate;
                        if (obj.uss_shift_key != null)
                            shift_id = (int)obj.uss_shift_key;//getRecord.phr_shift_key;
                        else
                            shift_id = 1;
                    }
                    #endregion
                    #region find physician shift for rate
                    if (hours >= 9 && timeFrom.Date == endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its weekend day shift
                            var getFloorRate = GetRecord(id, 5, scheduleDate); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }
                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 5, isStrokeAlert);
                    }
                    else if (hours >= 9 && timeFrom.Date != endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its weekend night shift
                            var getFloorRate = GetRecord(id, 6, obj.uss_date); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 6, isStrokeAlert);
                    }
                    else if (hours == 6 && timeFrom.Date == endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its day light weekend
                            var getFloorRate = GetRecord(id, 9, scheduleDate); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 9, isStrokeAlert);
                    }
                    else if (hours == 5 && _startTimeOnly == "19:00" && _endTimeOnly == "00:00" && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its moon light weekend
                            var getFloorRate = GetRecord(id, 8, scheduleDate); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 8, isStrokeAlert);
                    }
                    else if (hours >= 9 && timeFrom.Date == endTime.Date)
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its day shift
                            var getFloorRate = GetRecord(id, 1, obj.uss_date); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 1, isStrokeAlert);
                    }
                    else if (hours >= 9 && timeFrom.Date != endTime.Date)
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its night shift
                            var getFloorRate = GetRecord(id, 2, obj.uss_date); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 2, isStrokeAlert);
                    }
                    else if (hours == 6 && timeFrom.Date == endTime.Date)
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its day light shift
                            var getFloorRate = GetRecord(id, 4, obj.uss_date); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 4, isStrokeAlert);
                    }
                    else if (hours == 5 && _startTimeOnly == "19:00" && _endTimeOnly == "00:00")
                    {
                        if (phy_floor_rate == 0)
                        {
                            // its moon light shift
                            var getFloorRate = GetRecord(id, 3, obj.uss_date); // get floor rate from shift rate table
                            if (getFloorRate != null)
                                phy_floor_rate = (decimal)getFloorRate.psr_rate;
                        }

                        //get value of productivity rate
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, 3, isStrokeAlert);
                    }
                    else
                    {
                        if (gotoProductivity)
                            foundTotalBill = CalculatePhysicianBill(phy_rate_list, objVeiwModel, shift_id, isStrokeAlert);
                    }
                    #endregion
                    // compare  floor rate with physician calculted price
                    if (phy_floor_rate < foundTotalBill)
                        phy_floor_rate = foundTotalBill;

                    // get eeg rate and add in total amount
                    var foundAmount = CalculatePhysicianBillForEEG(phy_rate_list, objVeiwModel, 1, isStrokeAlert);
                    phy_floor_rate += foundAmount;
                    #endregion
                }
                else
                {
                    if (objVeiwModel.EEG != 0 || objVeiwModel.LTM_EEG != 0)
                    {
                        phy_floor_rate = CalculatePhysicianBillForEEG(phy_rate_list, objVeiwModel, 1, isStrokeAlert);
                    }
                }
            }
            return phy_floor_rate;
        }

        #region Get Floor rate when no schedule found

        #endregion

        public List<PhysicianBillingByShiftViewModel> GetPhysicianBillingAmount(DataSourceRequest request,
                                                   List<string> physicians,
                                                   DateTime startDate,
                                                   DateTime endDate,
                                                   List<int> caseStatus,
                                                   ShiftType shiftType, string requestFor = "report")
        {
            DateTime _startTime = startDate.AddDays(-1);
            DateTime _endTime = endDate.AddDays(1);
            decimal _result = 0;
            string id = physicians[0];
            var isSchedulesExist = GetPhysicianSchedule(id, startDate, endDate);
            if(isSchedulesExist.Count == 0 && requestFor == "load")
            {
                return null;
            }
            #region Calculated DateWise Strokes, New, Fu patients
            var _listForNewFu = _countTodayNewFu(_startTime, _endTime, physicians, isSchedulesExist);
            var list = CountToday(_startTime, _endTime, physicians);


            var countedTotal = (from obj in list
                                group
                                    new
                                    {
                                        obj
                                    }
                                 by new
                                 {
                                     AssignDate = obj.AssignDate,
                                     PhysicianKey = obj.PhysicianKey,
                                     Schedule = obj.Schedule
                                 } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Schedule = g.First().obj.Schedule,
                                    Physician = g.First().obj.Physician,
                                    PhysicianKey = g.Key.PhysicianKey,
                                    CC1_StrokeAlert = g.Sum(x => x.obj.CC1_StrokeAlert),
                                    CC1_STAT = g.Sum(x => x.obj.CC1_STAT),
                                    New = g.Sum(x => x.obj.New),
                                    FU = g.Sum(x => x.obj.FU),
                                    EEG = g.Sum(x => x.obj.EEG),
                                    LTM_EEG = g.Sum(x => x.obj.LTM_EEG),
                                    TC = g.Sum(x => x.obj.TC),
                                    Not_Seen = g.Sum(x => x.obj.Not_Seen),
                                    Blast = g.Sum(x => x.obj.Blast),
                                    Total = g.Sum(x => x.obj.Total),
                                    AmountDollar = "$0",
                                    time_from_calc = g.First().obj.time_from_calc,
                                    time_to_calc = g.First().obj.time_to_calc
                                }).ToList();
            var _countedTotalNewFu = (from obj in _listForNewFu
                                      group
                                     new
                                     {
                                         obj
                                     }
                                        by new
                                        {
                                            AssignDate = obj.AssignDate,
                                            PhysicianKey = obj.PhysicianKey,
                                            Schedule = obj.Schedule
                                        } into g
                                      select new PhysicianBillingByShiftViewModel
                                      {
                                          AssignDate = g.Key.AssignDate,
                                          Schedule = g.First().obj.Schedule,
                                          Physician = g.First().obj.Physician,
                                          PhysicianKey = g.Key.PhysicianKey,
                                          CC1_StrokeAlert = g.Sum(x => x.obj.CC1_StrokeAlert),
                                          CC1_STAT = g.Sum(x => x.obj.CC1_STAT),
                                          New = g.Sum(x => x.obj.New),
                                          FU = g.Sum(x => x.obj.FU),
                                          EEG = g.Sum(x => x.obj.EEG),
                                          LTM_EEG = g.Sum(x => x.obj.LTM_EEG),
                                          TC = g.Sum(x => x.obj.TC),
                                          Not_Seen = g.Sum(x => x.obj.Not_Seen),
                                          Blast = g.Sum(x => x.obj.Blast),
                                          Total = g.Sum(x => x.obj.Total),
                                      }).ToList();



            if (isSchedulesExist.Count > 0)
            {
                #region  count billing rates accoding to counted strokes, New, Fu ects
                var phy_rate_list = _getPhysicianRates(id);
                // Query to get default rates of a physician
                var getDefualtRate = (from obj in phy_rate_list
                                      where obj.rat_starting == 1
                                      where obj.rat_start_date >= startDate
                                      where obj.rat_end_date <= endDate
                                      group obj by obj.rat_cas_id into g
                                      select new { rate = g.Sum(x => x.rat_price) }).ToList();
                decimal foundTotalBill = 0;
                if (_countedTotalNewFu.Count() > 0)
                {
                    if (countedTotal.Count == 0)
                    {
                        countedTotal = _countedTotalNewFu;
                    }
                    else
                    {
                        #region Get not matched records from two lists
                        // get different records from two list
                        foreach (var i in _countedTotalNewFu)
                        {
                            var found = countedTotal.Where(x => x.AssignDate == i.AssignDate).FirstOrDefault();
                            if (found == null)
                            {
                                countedTotal.Add(i);
                            }
                        }
                        #endregion
                    }
                    // below foreach is here
                }
                var userDetail = getUserIdentity(id);
                bool isStrokeAlert = userDetail.NHAlert;
               
                //var isStrokeAlert =  UserManager.Users.Where(x => x.Id == id)
                foreach (var item in countedTotal)
                {
                    var _defaultVal = _countedTotalNewFu.Where(x => x.AssignDate == item.AssignDate).FirstOrDefault();
                    if (_defaultVal != null)
                    {
                        //check same date record in list
                        var foundList = countedTotal.Where(x => x.AssignDate == item.AssignDate).ToList();
                        if (foundList.Count() > 1)
                        {
                            //get required result from list
                            var _record = new PhysicianBillingByShiftViewModel();

                            int value = DateCompare(foundList[0].time_from_calc, foundList[1].time_from_calc);
                            // checking 
                            if (value > 0)
                            {
                                Console.Write("date1 is later than date2. ");
                                _record = foundList[1];
                            }

                            else if (value < 0)
                            {
                                Console.Write("date1 is earlier than date2. ");
                                _record = foundList[0];
                            }

                            else
                            {
                                Console.Write("date1 is the same as date2. ");
                                _record = foundList[0];
                            }
                            if (item == _record)
                            {
                                item.New = _defaultVal.New;
                                item.FU = _defaultVal.FU;
                                item.EEG = _defaultVal.EEG;
                                item.LTM_EEG = _defaultVal.LTM_EEG;
                            }

                        }
                        else
                        {
                            item.New = _defaultVal.New;
                            item.FU = _defaultVal.FU;
                            item.EEG = _defaultVal.EEG;
                            item.LTM_EEG = _defaultVal.LTM_EEG;
                        }
                    }
                    var seenPatient = item;
                    #region Get floor rate of a physician according to the shift
                    if (item.time_from_calc == (default(DateTime)) && item.AssignDate != "Blast")
                    {
                        DateTime _dtt = Convert.ToDateTime(item.AssignDate);
                        var objSchedule = isSchedulesExist.Find(x => x.uss_date == _dtt);
                        if (objSchedule != null)
                        {
                            item.time_from_calc = (DateTime)objSchedule.uss_time_from_calc;
                            item.time_to_calc = (DateTime)objSchedule.uss_time_to_calc;
                        }
                    }
                    if (seenPatient.AssignDate == "Blast")
                    {
                        seenPatient.assign_date = endDate;
                        foundTotalBill = CalculatePhysicianBlast(seenPatient);
                        _result = foundTotalBill;
                    }
                    else
                    {
                        DateTime _dt = Convert.ToDateTime(item.AssignDate);
                        foundTotalBill = FloorRate(_dt, id, foundTotalBill, isSchedulesExist, phy_rate_list, item, isStrokeAlert);
                        _result = foundTotalBill;
                    }
                    #endregion
                    item.Amount = _result;
                    item.AmountDollar = "$" + decimal.Round(item.Amount, 2);
                }
                #endregion
            }
            else
            {
                // this code is represent to get amount when no schedule found
                #region this code is represent to get amount when no schedule found
                if (countedTotal.Count > 0)
                {
                    var getBlast = countedTotal.Where(x => x.AssignDate == "Blast").FirstOrDefault();
                    getBlast.assign_date = endDate;
                    decimal foundTotalBill = CalculatePhysicianBlast(getBlast);
                    getBlast.Amount = _result;
                    getBlast.AmountDollar = "$" + decimal.Round(getBlast.Amount, 2);
                    int index = countedTotal.IndexOf(getBlast);
                    countedTotal[index] = getBlast;
                }
                #endregion
            }
            #endregion

            #region Get All Schedule days rate
            if(isSchedulesExist.Count > 0)
            {
                countedTotal = GetAllScheduleBilling(countedTotal, isSchedulesExist, id);
            }
            #endregion

            #region Add Sum Of total Amount with  sorted list
            //  find records for removing from list
            countedTotal = FindAndRemove(startDate, endDate, countedTotal);
            countedTotal = TotalAmount(countedTotal);
            #endregion

            return countedTotal;
        }
        private List<PhysicianBillingByShiftViewModel> TotalAmount(List<PhysicianBillingByShiftViewModel> list)
        {
            PhysicianBillingByShiftViewModel obj = new PhysicianBillingByShiftViewModel();
            decimal total = list.Sum(x => x.Amount);
            obj.AssignDate = "Total Earned";
            obj.Amount = total;
            obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);
            list.Add(obj);
            list = DateWiseOrder(list);
            return list;
        }
        private List<PhysicianBillingByShiftViewModel> DateWiseOrder(List<PhysicianBillingByShiftViewModel> list)
        {
            var getListWithoutdates = list.Where(x => x.AssignDate == "Blast" || x.AssignDate == "Total Earned").ToList();
            var getListOfDates = list.Where(x => x.AssignDate != "Blast" && x.AssignDate != "Total Earned").ToList();
            foreach (var item in getListOfDates)
            {
                string date = item.AssignDate;
                DateTime _dt = Convert.ToDateTime(date);
                item.assign_date = _dt;
            }
            getListOfDates = getListOfDates.OrderBy(x => x.assign_date).ToList();
            getListOfDates.AddRange(getListWithoutdates);
            return getListOfDates;
        }
        private List<PhysicianBillingByShiftViewModel> GetAllScheduleBilling(List<PhysicianBillingByShiftViewModel> list, List<user_schedule> listSchedule, string phy_id)
        {
            var phy_name = GetPhysicianName(phy_id);
            List<PhysicianBillingByShiftViewModel> new_billing_list = new List<PhysicianBillingByShiftViewModel>();
            PhysicianBillingByShiftViewModel obj;
            foreach (var item in listSchedule)
            {
                DateTime dt = item.uss_date;
                string date = dt.ToString("M/d/yyy");
                // get time for format start
                DateTime startDateTime = (DateTime)item.uss_time_from_calc;
                DateTime endDateTime = (DateTime)item.uss_time_to_calc;
                string _startFrom = startDateTime.ToString("HH:mm");
                string _endTo = endDateTime.ToString("HH:mm");
                string schedule = _startFrom + " - " + _endTo;

                //var obj = listSchedule.Find(x => x.uss_date == scheduleDate);
                var isExist = list.Where(x => x.AssignDate == date && x.Schedule == schedule).FirstOrDefault();
                // get time for format end
                //var isExist = list.Where(x => x.AssignDate == date).FirstOrDefault();
                if (isExist == null)
                {
                    obj = new PhysicianBillingByShiftViewModel();
                    obj.AssignDate = date;
                    obj.PhysicianKey = item.uss_user_id;
                    obj.Physician = phy_name;
                    DateTime timeFrom = (DateTime)item.uss_time_from_calc;
                    DateTime timeTo = (DateTime)item.uss_time_to_calc;
                    string _timefromcal = timeFrom.ToString("HH:mm"); //timeFrom.Date.Format_Time();//timeFrom.ToString("HH:mm");
                    string _timetocal = timeTo.ToString("HH:mm"); //timeTo.Date.Format_Time();
                    obj.Schedule = _timefromcal + " - " + _timetocal;
                    decimal floor_rate = 0;

                    floor_rate = FloorRate(item);
                    obj.Amount = floor_rate;
                    obj.AmountDollar = "$" + decimal.Round(floor_rate, 2);
                    // initialize values
                    obj.CC1_STAT = 0;
                    obj.CC1_StrokeAlert = 0;
                    obj.New = 0;
                    obj.FU = 0;
                    obj.LTM_EEG = 0;
                    obj.EEG = 0;
                    obj.TC = 0;
                    new_billing_list.Add(obj);
                }
            }
            list.AddRange(new_billing_list);
            return list;
        }
        private decimal FloorRate(user_schedule obj)
        {
            string id = obj.uss_user_id;
            DateTime scheduleDate = obj.uss_date;
            decimal phy_floor_rate = 0;
            DateTime timeFrom = (DateTime)obj.uss_time_from_calc;
            DateTime endTime = (DateTime)obj.uss_time_to_calc;
            string day = timeFrom.DayOfWeek.ToString();
            var hours = (endTime - timeFrom).TotalHours;
            hours = Math.Round(hours);
            //var getRecord =  _physicianHolidayRateService.GetCustomRate(obj.uss_key);
            string _startTimeOnly = timeFrom.ToString("HH:mm");
            string _endTimeOnly = endTime.ToString("HH:mm");
            if (obj.uss_custome_rate != null)
            {
                phy_floor_rate = (decimal)obj.uss_custome_rate;//(decimal)getRecord.phr_rate;
            }
            else
            {
                #region find physician shift for rate
                if (hours >= 9 && timeFrom.Date == endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                {
                    // its weekend day shift
                    var getFloorRate = GetRecord(id, 5, scheduleDate); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours >= 9 && timeFrom.Date != endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                {
                    // its weekend night shift
                    var getFloorRate = GetRecord(id, 6, obj.uss_date); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours == 6 && timeFrom.Date == endTime.Date && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                {
                    // its day light weekend
                    var getFloorRate = GetRecord(id, 9, scheduleDate); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours == 5 && _startTimeOnly == "19:00" && _endTimeOnly == "00:00" && (day == "Friday" || day == "Saturday" || day == "Sunday"))
                {
                    // its moon light weekend
                    var getFloorRate = GetRecord(id, 8, scheduleDate); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours >= 9 && timeFrom.Date == endTime.Date)
                {
                    // its day shift
                    var getFloorRate = GetRecord(id, 1, obj.uss_date); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours >= 9 && timeFrom.Date != endTime.Date)
                {
                    // its night shift
                    var getFloorRate = GetRecord(id, 2, obj.uss_date); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours == 6 && timeFrom.Date == endTime.Date)
                {
                    // its day light shift
                    var getFloorRate = GetRecord(id, 4, obj.uss_date); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                else if (hours == 5 && _startTimeOnly == "19:00" && _endTimeOnly == "00:00")
                {
                    // its moon light shift
                    var getFloorRate = GetRecord(id, 3, obj.uss_date); // get floor rate from shift rate table
                    if (getFloorRate != null)
                        phy_floor_rate = (decimal)getFloorRate.psr_rate;
                }
                #endregion
            }
            return phy_floor_rate;
        }
        private int DateCompare(DateTime dt1, DateTime dt2)
        {
            int value = DateTime.Compare(dt1, dt2);
            return value;
        }
        private List<PhysicianBillingByShiftViewModel> FindAndRemove(DateTime start, DateTime end, List<PhysicianBillingByShiftViewModel> list)
        {
            // get if two days less found
            DateTime _dt = start.AddDays(-2);
            string _dtDate = _dt.ToString("M/d/yyy");
            start = start.AddDays(-1);
            end = end.AddDays(1);
            string startDate = start.ToString("M/d/yyy");
            string endDate = end.ToString("M/d/yyy");
            List<string> li = new List<string>();
            li.Add(startDate);
            li.Add(endDate);
            li.Add(_dtDate);
            HashSet<string> removeDates = new HashSet<string>(li);
            var resultList = list.Where(m => !removeDates.Contains(m.AssignDate));
            return resultList.ToList();
        }

    }



}
