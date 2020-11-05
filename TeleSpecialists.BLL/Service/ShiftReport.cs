using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels.Reports;

namespace TeleSpecialists.BLL.Service
{
    public class ShiftReport : BaseService
    {
        private TeleSpecialistsContext db = new TeleSpecialistsContext();

        public List<PhysicianBillingByShiftViewModel> GetRecords(List<string> physicians,  DateTime startdate, DateTime enddate ,List<PhysicianBillingByShiftViewModel> scheduleList, List<int> caseStatus,
                                                            ShiftType shiftType)
        {
            //enddate = enddate.AddDays(1);
            List<@case> list = new List<@case>();
            foreach(var item in physicians)
            {
                var obj = db.cases.Where(x => x.cas_phy_key == item && 
                (x.cas_ctp_key == 9 || x.cas_ctp_key == 10 || x.cas_ctp_key == 11)
                //x.cas_ctp_key != 12 && x.cas_ctp_key != 13 && x.cas_ctp_key != 14 && x.cas_ctp_key != 15 && x.cas_ctp_key != 16 && x.cas_ctp_key != 227
                  //              && x.cas_ctp_key != 228 && x.cas_ctp_key != 163 && x.cas_ctp_key != 164 && x.cas_ctp_key != 220
                                && DbFunctions.TruncateTime( startdate) <= DbFunctions.TruncateTime( x.cas_physician_assign_date) && DbFunctions.TruncateTime( x.cas_physician_assign_date) <= DbFunctions.TruncateTime( enddate) && x.cas_billing_physician_blast == false && x.cas_cst_key != 140).ToList();
                list.AddRange(obj);
            }
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join s in _unitOfWork.ScheduleRepository.Query() on c.cas_phy_key equals s.uss_user_id
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                //let hus = SqlFunctions.DateAdd("hh", 1, s.uss_time_to_calc)
                                where c.cas_phy_key != null && (c.cas_ctp_key != 163 && c.cas_ctp_key != 164)
                                && c.cas_physician_assign_date != null
                                //&&  DbFunctions.TruncateTime(s.uss_time_from_calc) <= DbFunctions.TruncateTime(c.cas_physician_assign_date)
                                //&& DbFunctions.TruncateTime(s.uss_time_to_calc) >= DbFunctions.TruncateTime(c.cas_physician_assign_date)
                                && s.uss_time_from_calc <= c.cas_physician_assign_date
                                && s.uss_time_to_calc >= c.cas_physician_assign_date

                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startdate)
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(enddate)
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
                                            assign_date = (DateTime)onShiftModel.c.cas_physician_assign_date
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    assign_date = (DateTime)g.Key.assign_date,
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
            HashSet<string> _listids = new HashSet<string>(list.Select(s => s.cas_physician_assign_date.ToString()));
            HashSet<string> _caseids = new HashSet<string>(onShiftCasesList.Select(s => s.assign_date.ToString()));
            var newM = _listids.Except(_caseids).ToList();
            List<@case> difference = new List<@case>();
            foreach (var item in newM)
            {
                var getR = list.Where(x => x.cas_physician_assign_date.ToString() == item).FirstOrDefault();
                difference.Add(getR);    
            }
            foreach (var item in difference)
            {
                //var schedule = db.user_schedule.Where(x => x.uss_user_id == item.cas_phy_key && DbFunctions.TruncateTime( x.uss_time_from_calc) >= DbFunctions.TruncateTime(item.cas_physician_assign_date) && DbFunctions.TruncateTime(x.uss_time_to_calc) <= DbFunctions.TruncateTime(item.cas_physician_assign_date)).FirstOrDefault();
                        //user_Schedules.Where(x => x.uss_user_id == item.cas_phy_key && DbFunctions.TruncateTime(x.uss_date) == DbFunctions.TruncateTime(item.cas_physician_assign_date)).ToList();
                PhysicianBillingByShiftViewModel obj = new PhysicianBillingByShiftViewModel();
                DateTime dt = (DateTime)item.cas_physician_assign_date;
                obj.AssignDate = dt.ToString("M/d/yyy");
                obj.Schedule = "Off (" + dt.ToString("hh:mm tt") + ")";
                //obj.assign_date = null;
                /*
                if (schedule!= null)
                {
                    DateTime dtstart = (DateTime)schedule.uss_time_from_calc;
                    DateTime dtend = (DateTime)schedule.uss_time_to_calc;
                    //obj.Schedule = dtstart.ToString("HH:mm") + " - " + dtend.ToString("HH:mm");
                    obj.Schedule = "OS (" + dt.ToString("hh:mm tt") + ")";
                }
                else
                {
                    obj.Schedule = "OS (" + dt.ToString("hh:mm tt") + ")";
                }
                */
                var _name = _unitOfWork.ApplicationUsers.Where(x => x.Id == item.cas_phy_key).FirstOrDefault();
                obj.Physician = _name.LastName + " " + _name.FirstName;
                obj.PhysicianKey = item.cas_phy_key;
                obj.Open = item.cas_cst_key == (int)CaseStatus.Open ? 1 : 0;
                obj.WaitingToAccept = item.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0;
                obj.Accepted = item.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0;
                obj.Complete = item.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0;
                obj.CC1_StrokeAlert = item.cas_billing_bic_key == 1 ? 1 : 0;
                obj.CC1_STAT = item.cas_billing_bic_key == 2 ? 1 : 0;
                obj.New = item.cas_billing_bic_key == 3 ? 1 : 0;
                obj.FU = item.cas_billing_bic_key == 4 ? 1 : 0;
                obj.EEG = item.cas_billing_bic_key == 5 ? 1 : 0;
                obj.LTM_EEG = item.cas_billing_bic_key == 6 ? 1 : 0;
                obj.TC = item.cas_billing_bic_key == 7 ? 1 : 0;
                obj.Not_Seen = item.cas_billing_bic_key == 8 ? 1 : 0;
                obj.Blast = item.cas_billing_physician_blast ? 1 : 0;
                obj.Total = item.cas_key > 0 ? 1 : 0;
                scheduleList.Add(obj);
            }

            #region Testing on shift
            /* can be open  after testing
             
            onShiftCasesList = (from onShiftModel in difference
                                join s in user_Schedules on onShiftModel.cas_phy_key equals s.uss_user_id
                                join u in userList on onShiftModel.cas_phy_key equals u.Id
                                group
                                    new { onShiftModel } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(s.uss_date).Value, false),
                                            Schedule = DbFunctions.Right("00" + SqlFunctions.DateName("hour", s.uss_time_from_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", s.uss_time_from_calc.Value), 2)
                                                + " - "
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("hour", s.uss_time_to_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", s.uss_time_to_calc.Value), 2),
                                            Physician = u.LastName + " " + u.FirstName,
                                            PhysicianKey = onShiftModel.cas_phy_key,
                                            uss_time_from_calc = s.uss_time_from_calc.Value,
                                            uss_time_to_calc = s.uss_time_to_calc.Value,
                                            assign_date = onShiftModel.cas_physician_assign_date
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    assign_date = (DateTime)g.Key.assign_date,
                                    Schedule = g.Key.Schedule,
                                    Physician = g.Key.Physician,
                                    PhysicianKey = g.Key.PhysicianKey,
                                    Open = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Open ? 1 : 0),
                                    WaitingToAccept = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0),
                                    Accepted = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0),
                                    Complete = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0),
                                    CC1_StrokeAlert = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 1 ? 1 : 0),
                                    CC1_STAT = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 2 ? 1 : 0),
                                    New = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 3 ? 1 : 0),
                                    FU = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 4 ? 1 : 0),
                                    EEG = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 5 ? 1 : 0),
                                    LTM_EEG = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 6 ? 1 : 0),
                                    TC = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 7 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.onShiftModel.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.onShiftModel.cas_key > 0 ? 1 : 0),
                                }).ToList();

    */

            #endregion
            return scheduleList;
        }


        public List<PhysicianBillingByShiftViewModel> GetRecordsForBilling(List<string> physicians, DateTime startdate, DateTime enddate, List<PhysicianBillingByShiftViewModel> scheduleList, List<int> caseStatus,
                                                           ShiftType shiftType)
        {
            //enddate = enddate.AddDays(1);
            List<@case> list = new List<@case>();
            foreach (var item in physicians)
            {
                var obj = db.cases.Where(x => x.cas_phy_key == item &&
                (x.cas_billing_bic_key == 1 || x.cas_billing_bic_key == 2) && x.cas_billing_bic_key != null
                                && DbFunctions.TruncateTime(startdate) <= DbFunctions.TruncateTime(x.cas_physician_assign_date) && DbFunctions.TruncateTime(x.cas_physician_assign_date) <= DbFunctions.TruncateTime(enddate) && x.cas_billing_physician_blast == false && x.cas_cst_key == 20).ToList();
                list.AddRange(obj);
            }
            List<PhysicianBillingByShiftViewModel> onShiftCasesList = null;
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join s in _unitOfWork.ScheduleRepository.Query() on c.cas_phy_key equals s.uss_user_id
                                join u in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals u.Id
                                let time_from_calc = SqlFunctions.DateAdd("hh", -2, s.uss_time_from_calc)
                                let time_to_calc = SqlFunctions.DateAdd("hh", 2, s.uss_time_to_calc)
                                where c.cas_phy_key != null && (c.cas_billing_bic_key == 1 || c.cas_billing_bic_key == 2)
                                && c.cas_physician_assign_date != null
                                && c.cas_billing_bic_key != null
                                && c.cas_cst_key == 20
                                && c.cas_billing_physician_blast == false
                                && time_from_calc <= c.cas_physician_assign_date
                                && time_to_calc >= c.cas_physician_assign_date
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startdate)
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(enddate)
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
                                            assign_date = (DateTime)onShiftModel.c.cas_physician_assign_date
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    assign_date = (DateTime)g.Key.assign_date,
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
            HashSet<string> _listids = new HashSet<string>(list.Select(s => s.cas_physician_assign_date.ToString()));
            HashSet<string> _caseids = new HashSet<string>(onShiftCasesList.Select(s => s.assign_date.ToString()));
            var newM = _listids.Except(_caseids).ToList();
            List<@case> difference = new List<@case>();
            foreach (var item in newM)
            {
                var getR = list.Where(x => x.cas_physician_assign_date.ToString() == item).FirstOrDefault();
                difference.Add(getR);
            }
            foreach (var item in difference)
            {
                PhysicianBillingByShiftViewModel obj = new PhysicianBillingByShiftViewModel();
                DateTime dt = (DateTime)item.cas_physician_assign_date;
                obj.AssignDate = dt.ToString("M/d/yyy");//DBHelper.FormatDateTime(DbFunctions.TruncateTime(dt).Value, false);//dt.ToString("MM/dd/yyyy");
                var isExist = scheduleList.Where(x => x.AssignDate == obj.AssignDate && x.PhysicianKey == item.cas_phy_key).FirstOrDefault();
                if(isExist != null)
                {
                    obj.Schedule = isExist.Schedule;
                }
                else
                {
                    obj.Schedule = "Off (" + dt.ToString("hh:mm tt") + ")";
                }
                var _name = _unitOfWork.ApplicationUsers.Where(x => x.Id == item.cas_phy_key).FirstOrDefault();
                obj.Physician = _name.LastName + " " + _name.FirstName;
                obj.PhysicianKey = item.cas_phy_key;
                obj.Open = item.cas_cst_key == (int)CaseStatus.Open ? 1 : 0;
                obj.WaitingToAccept = item.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0;
                obj.Accepted = item.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0;
                obj.Complete = item.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0;
                obj.CC1_StrokeAlert = item.cas_billing_bic_key == 1 ? 1 : 0;
                obj.CC1_STAT = item.cas_billing_bic_key == 2 ? 1 : 0;
                obj.New = item.cas_billing_bic_key == 3 ? 1 : 0;
                obj.FU = item.cas_billing_bic_key == 4 ? 1 : 0;
                obj.EEG = item.cas_billing_bic_key == 5 ? 1 : 0;
                obj.LTM_EEG = item.cas_billing_bic_key == 6 ? 1 : 0;
                obj.TC = item.cas_billing_bic_key == 7 ? 1 : 0;
                obj.Not_Seen = item.cas_billing_bic_key == 8 ? 1 : 0;
                obj.Blast = item.cas_billing_physician_blast ? 1 : 0;
                obj.Total = item.cas_key > 0 ? 1 : 0;
                scheduleList.Add(obj);
            }

            #region Testing on shift
            /* can be open  after testing
             
            onShiftCasesList = (from onShiftModel in difference
                                join s in user_Schedules on onShiftModel.cas_phy_key equals s.uss_user_id
                                join u in userList on onShiftModel.cas_phy_key equals u.Id
                                group
                                    new { onShiftModel } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(s.uss_date).Value, false),
                                            Schedule = DbFunctions.Right("00" + SqlFunctions.DateName("hour", s.uss_time_from_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", s.uss_time_from_calc.Value), 2)
                                                + " - "
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("hour", s.uss_time_to_calc.Value), 2)
                                                + ":"
                                                + DbFunctions.Right("00" + SqlFunctions.DateName("minute", s.uss_time_to_calc.Value), 2),
                                            Physician = u.LastName + " " + u.FirstName,
                                            PhysicianKey = onShiftModel.cas_phy_key,
                                            uss_time_from_calc = s.uss_time_from_calc.Value,
                                            uss_time_to_calc = s.uss_time_to_calc.Value,
                                            assign_date = onShiftModel.cas_physician_assign_date
                                        } into g
                                select new PhysicianBillingByShiftViewModel
                                {
                                    AssignDate = g.Key.AssignDate,
                                    assign_date = (DateTime)g.Key.assign_date,
                                    Schedule = g.Key.Schedule,
                                    Physician = g.Key.Physician,
                                    PhysicianKey = g.Key.PhysicianKey,
                                    Open = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Open ? 1 : 0),
                                    WaitingToAccept = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.WaitingToAccept ? 1 : 0),
                                    Accepted = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Accepted ? 1 : 0),
                                    Complete = g.Sum(x => x.onShiftModel.cas_cst_key == (int)CaseStatus.Complete ? 1 : 0),
                                    CC1_StrokeAlert = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 1 ? 1 : 0),
                                    CC1_STAT = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 2 ? 1 : 0),
                                    New = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 3 ? 1 : 0),
                                    FU = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 4 ? 1 : 0),
                                    EEG = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 5 ? 1 : 0),
                                    LTM_EEG = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 6 ? 1 : 0),
                                    TC = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 7 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.onShiftModel.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.onShiftModel.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.onShiftModel.cas_key > 0 ? 1 : 0),
                                }).ToList();

    */

            #endregion
            return scheduleList;
        }

    }
}