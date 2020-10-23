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
    public class FacilityBillingReportService : BaseService
    {
        #region Facility Report By Billing Added by Axim
        public List<FacilityBillingByAmount> GetFacilityReportByBilling(DataSourceRequest request,
                                                   List<string> facilities,
                                                   DateTime startDate,
                                                   DateTime endDate)
        {
            DateTime _startTime = startDate.AddDays(-1);
            DateTime _endTime = endDate.AddDays(1);
            decimal _result = 0;
            string id = facilities[0];
            var facility = "";

            var _listForNewFu = _FacilitycountTodayNewFu(_startTime, _endTime, facilities, null);
            var list = FacilityCountToday(_startTime, _endTime, facilities);

            var countedTotal = (from obj in list
                                group
                                    new
                                    {
                                        obj
                                    }
                                 by new
                                 {
                                     AssignDate = obj.AssignDate,
                                     FacilityKey = obj.FacilityKey
                                 } into g
                                select new FacilityBillingByAmount
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Facility = g.First().obj.Facility,
                                    FacilityKey = g.Key.FacilityKey,
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
                                    LTM_VIDEO = g.Sum(x => x.obj.LTM_VIDEO),
                                    STAT_EEG = g.Sum(x => x.obj.STAT_EEG),
                                    EEG_MINUTES = g.Sum(x => x.obj.EEG_MINUTES),
                                    CC1_StrokeAlertstring = g.Sum(x => x.obj.CC1_StrokeAlert).ToString(),
                                    CC1_STATstring = g.Sum(x => x.obj.CC1_STAT).ToString(),
                                    Newstring = g.Sum(x => x.obj.New).ToString(),
                                    FUstring = g.Sum(x => x.obj.FU).ToString(),
                                    EEGstring = g.Sum(x => x.obj.EEG).ToString(),
                                    LTM_EEGstring = g.Sum(x => x.obj.LTM_EEG).ToString(),
                                    TCstring = g.Sum(x => x.obj.TC).ToString(),
                                    LTM_VIDEOstring = g.Sum(x => x.obj.LTM_VIDEO).ToString(),
                                    STAT_EEGstring = g.Sum(x => x.obj.STAT_EEG).ToString(),
                                    EEG_MINUTESstring = g.Sum(x => x.obj.EEG_MINUTES).ToString(),
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
                                            FacilityKey = obj.FacilityKey
                                        } into g
                                      select new FacilityBillingByAmount
                                      {
                                          AssignDate = g.Key.AssignDate,
                                          Facility = g.First().obj.Facility,
                                          FacilityKey = g.Key.FacilityKey,
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
                                          LTM_VIDEO = g.Sum(x => x.obj.LTM_VIDEO),
                                          STAT_EEG = g.Sum(x => x.obj.STAT_EEG),
                                          EEG_MINUTES = g.Sum(x => x.obj.EEG_MINUTES),
                                          CC1_StrokeAlertstring = g.Sum(x => x.obj.CC1_StrokeAlert).ToString(),
                                          CC1_STATstring = g.Sum(x => x.obj.CC1_STAT).ToString(),
                                          Newstring = g.Sum(x => x.obj.New).ToString(),
                                          FUstring = g.Sum(x => x.obj.FU).ToString(),
                                          EEGstring = g.Sum(x => x.obj.EEG).ToString(),
                                          LTM_EEGstring = g.Sum(x => x.obj.LTM_EEG).ToString(),
                                          TCstring = g.Sum(x => x.obj.TC).ToString(),
                                          LTM_VIDEOstring = g.Sum(x => x.obj.LTM_VIDEO).ToString(),
                                          STAT_EEGstring = g.Sum(x => x.obj.STAT_EEG).ToString(),
                                          EEG_MINUTESstring = g.Sum(x => x.obj.EEG_MINUTES).ToString(),
                                      }).ToList();

            #region  count billing rates accoding to counted strokes, New, Fu ects
            var fac_rate_list = getFacilityRates(id);
            //var all_fac_rate_list = getAllFacilityRates();
            // Query to get default rates of Facility
            //var getDefualtRate = (from obj in fac_rate_list
            //                      where obj.fct_starting == 1
            //                      where obj.fct_start_date >= startDate
            //                      where obj.fct_end_date <= endDate
            //                      group obj by obj.fct_billing_key into g
            //                      select new { rate = g.Sum(x => x.fct_rate) }).ToList();

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
            }

            foreach (var item in countedTotal)
            {
                facility = item.Facility;

                #region Get floor rate of a physician according to the shift
                DateTime _dt = Convert.ToDateTime(item.AssignDate);
                foundTotalBill = FloorRate(_dt, id, foundTotalBill, fac_rate_list, item);
                _result = foundTotalBill;

                #endregion
                item.Amount = _result;
                item.AmountDollar = "$" + decimal.Round(item.Amount, 2);
            }
            #endregion

            countedTotal = FindAndRemove(startDate, endDate, countedTotal);
            // countedTotal = TotalAmount(countedTotal);

            countedTotal = Aggregates(countedTotal, fac_rate_list, startDate, endDate);
            countedTotal = Availability(countedTotal, startDate, endDate);
            countedTotal = GrandTotal(countedTotal);
            return countedTotal;
        }

        private decimal FloorRate(DateTime scheduleDate, string id, decimal foundTotalBill, IEnumerable<facility_rate> fac_rate_list, FacilityBillingByAmount objVeiwModel)
        {
            decimal phy_floor_rate = 0;

            if (objVeiwModel.EEG != 0 || objVeiwModel.LTM_EEG != 0 || objVeiwModel.CC1_StrokeAlert != 0 || objVeiwModel.New != 0 || objVeiwModel.FU != 0 || objVeiwModel.CC1_STAT != 0 || objVeiwModel.TC != 0 || objVeiwModel.STAT_EEG != 0 || objVeiwModel.LTM_VIDEO != 0 || objVeiwModel.EEG_MINUTES != 0)
            {
                phy_floor_rate = CalculatePhysicianBillForEEG(fac_rate_list, objVeiwModel);
            }

            return phy_floor_rate;
        }
        private facility_rate GetRecord(string id, DateTime dateTime)
        {
            var obj = _unitOfWork.facilityRateRepository.Query()
                .Where(x => x.fct_facility_key == new Guid(id) && (DbFunctions.TruncateTime(x.fct_start_date) <= DbFunctions.TruncateTime(dateTime) && DbFunctions.TruncateTime(x.fct_end_date) >= DbFunctions.TruncateTime(dateTime))).FirstOrDefault();
            return obj;
        }

        private decimal CalculatePhysicianBillForEEG(IEnumerable<facility_rate> phy_rate_list, FacilityBillingByAmount obj)
        {
            decimal fct_new = 0; decimal fct_fu = 0; decimal eeg = 0; decimal ltm_eeg = 0; decimal tc = 0;
            decimal cc1stat = 0; decimal fct_strokeAlert = 0; decimal stat_eeg = 0; decimal ltm_video = 0; decimal eeg_minutes = 0;

            int? _strokePlus = null; int? _countNew = null; int? _cc1statplus = null;

            _strokePlus = obj.CC1_StrokeAlert;
            _countNew = obj.New;
            _cc1statplus = +obj.CC1_STAT;

            DateTime dt = Convert.ToDateTime(obj.AssignDate);

            var _cc1Stroke = phy_rate_list.Where(x => x.fct_billing_key == 1
                                                && x.fct_end_date >= dt &&
                                                x.fct_start_date <= dt).FirstOrDefault();

            var _cc1Stat = phy_rate_list.Where(x => x.fct_billing_key == 2 &&
                                              x.fct_end_date >= dt &&
                                              x.fct_start_date <= dt).FirstOrDefault();

            var _new = phy_rate_list.Where(x => x.fct_billing_key == 3 &&
                                           x.fct_end_date >= dt &&
                                           x.fct_start_date <= dt).FirstOrDefault();

            var _fu = phy_rate_list.Where(x => x.fct_billing_key == 4 &&
                                          x.fct_end_date >= dt &&
                                          x.fct_start_date <= dt).FirstOrDefault();

            var _eeg = phy_rate_list.Where(x => x.fct_billing_key == 5 &&
                                         x.fct_end_date >= dt &&
                                         x.fct_start_date <= dt).FirstOrDefault();

            var _LTM_eeg = phy_rate_list.Where(x => x.fct_billing_key == 6 &&
                                               x.fct_end_date >= dt &&
                                               x.fct_start_date <= dt).FirstOrDefault();

            var _tc = phy_rate_list.Where(x => x.fct_billing_key == 7 &&
                                          x.fct_end_date >= dt &&
                                          x.fct_start_date <= dt).FirstOrDefault();

            var _stat_eeg = phy_rate_list.Where(x => x.fct_billing_key == 325 &&
                                          x.fct_end_date >= dt &&
                                          x.fct_start_date <= dt).FirstOrDefault();

            var _ltm_video = phy_rate_list.Where(x => x.fct_billing_key == 326 &&
                                          x.fct_end_date >= dt &&
                                          x.fct_start_date <= dt).FirstOrDefault();

            var _eeg_minutes = phy_rate_list.Where(x => x.fct_billing_key == 324 &&
                                          x.fct_end_date >= dt &&
                                          x.fct_start_date <= dt).FirstOrDefault();
            ///// EEG Rates
            if (_eeg != null)
            {
                var _val = (decimal)_eeg.fct_rate;
                try
                {
                    var total = obj.EEG * _val;
                    eeg = (decimal)total;
                }
                catch { }
            }

            ///// CC1 STAT Rates
            if (_cc1Stat != null)
            {
                var _val = (decimal)_cc1Stat.fct_rate;
                try
                {
                    var total = obj.CC1_STAT * _val;
                    cc1stat = (decimal)total;
                }
                catch { }
            }

            //// LTM EEG Rates
            if (_LTM_eeg != null)
            {
                var _val = (decimal)_LTM_eeg.fct_rate;
                try
                {
                    var total = obj.LTM_EEG * _val;
                    ltm_eeg = (decimal)total;
                }
                catch { }
            }

            //// TC Rates
            if (_tc != null)
            {
                var _val = (decimal)_tc.fct_rate;
                try
                {
                    var total = obj.TC * _val;
                    tc = (decimal)total;
                }
                catch { }
            }

            //// CC1 Stroke Alert Rates
            if (_cc1Stroke != null)
            {
                var _val = (decimal)_cc1Stroke.fct_rate;
                try
                {
                    var total_stroke = obj.CC1_StrokeAlert * _val;
                    fct_strokeAlert = (decimal)total_stroke;
                }
                catch { }
            }

            //// New Rates
            if (_new != null)
            {
                var _val = (decimal)_new.fct_rate;
                try
                {
                    var total = obj.New * _val;
                    fct_new = (decimal)total;
                }
                catch { }
            }

            //// FU Rates
            if (_fu != null)
            {
                var _val = (decimal)_fu.fct_rate;
                try
                {
                    var total = obj.FU * _val;
                    fct_fu = (decimal)total;
                }
                catch { }
            }

            //// EEG Minutes Rates
            if (_eeg_minutes != null)
            {
                var _val = (decimal)_eeg_minutes.fct_rate;
                try
                {
                    var total = obj.EEG_MINUTES * _val;
                    eeg_minutes = (decimal)total;
                }
                catch { }
            }

            //// LTM Video Rates
            if (_ltm_video != null)
            {
                var _val = (decimal)_ltm_video.fct_rate;
                try
                {
                    var total = obj.LTM_VIDEO * _val;
                    ltm_video = (decimal)total;
                }
                catch { }
            }

            //// STAT EEG Rates
            if (_stat_eeg != null)
            {
                var _val = (decimal)_stat_eeg.fct_rate;
                try
                {
                    var total = obj.STAT_EEG * _val;
                    stat_eeg = (decimal)total;
                }
                catch { }
            }

            var billing = fct_new + fct_fu + eeg + ltm_eeg + tc + fct_strokeAlert + cc1stat + stat_eeg + ltm_video + eeg_minutes;

            return billing;
        }


        private int DateCompare(DateTime dt1, DateTime dt2)
        {
            int value = DateTime.Compare(dt1, dt2);
            return value;
        }
        private List<FacilityBillingByAmount> FindAndRemove(DateTime start, DateTime end, List<FacilityBillingByAmount> list)
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
        private List<FacilityBillingByAmount> TotalAmount(List<FacilityBillingByAmount> list)
        {
            FacilityBillingByAmount obj = new FacilityBillingByAmount();
            decimal total = list.Sum(x => x.Amount);
            obj.Facility = "";
            obj.AssignDate = "Total Bill";
            obj.Amount = total;
            obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);

            list.Add(obj);
            list = DateWiseOrder(list);
            return list;
        }
        private List<FacilityBillingByAmount> Aggregates(List<FacilityBillingByAmount> list, IEnumerable<facility_rate> phy_rate_list, DateTime startDate, DateTime endDate)
        {
            var getList = list.Where(x => x.AssignDate != "Availability Pay").ToList();
            var facilities = getList.Select(x => new { name = x.Facility, id = x.FacilityKey }).Distinct().ToList();


            FacilityBillingByAmount obj;
            for (int i = 0; i < facilities.Count; i++)
            {
                decimal CC1_STAT = 0; decimal CC1_StrokeAlert = 0; decimal NewText = 0; decimal FU = 0; decimal EEG = 0; decimal LTM_EEG = 0;
                decimal TC = 0; decimal EEG_MINUTES = 0; decimal LTM_VIDEO = 0; decimal STAT_EEG = 0;
                obj = new FacilityBillingByAmount();

                var _cc1Stroke = phy_rate_list.Where(x => x.fct_billing_key == 1
                                               && x.fct_end_date >= endDate &&
                                               x.fct_start_date <= startDate).ToList();

                var _cc1Stat = phy_rate_list.Where(x => x.fct_billing_key == 2 &&
                                                  x.fct_end_date >= endDate &&
                                                  x.fct_start_date <= startDate).ToList();

                var _new = phy_rate_list.Where(x => x.fct_billing_key == 3 &&
                                               x.fct_end_date >= endDate &&
                                               x.fct_start_date <= startDate).ToList();

                var _fu = phy_rate_list.Where(x => x.fct_billing_key == 4 &&
                                              x.fct_end_date >= endDate &&
                                              x.fct_start_date <= startDate).ToList();

                var _eeg = phy_rate_list.Where(x => x.fct_billing_key == 5 &&
                                             x.fct_end_date >= endDate &&
                                             x.fct_start_date <= startDate).ToList();

                var _LTM_eeg = phy_rate_list.Where(x => x.fct_billing_key == 6 &&
                                                   x.fct_end_date >= endDate &&
                                                   x.fct_start_date <= startDate).ToList();

                var _tc = phy_rate_list.Where(x => x.fct_billing_key == 7 &&
                                              x.fct_end_date >= endDate &&
                                              x.fct_start_date <= startDate).ToList();

                var _stat_eeg = phy_rate_list.Where(x => x.fct_billing_key == 325 &&
                                              x.fct_end_date >= endDate &&
                                              x.fct_start_date <= startDate).ToList();

                var _ltm_video = phy_rate_list.Where(x => x.fct_billing_key == 326 &&
                                              x.fct_end_date >= endDate &&
                                              x.fct_start_date <= startDate).ToList();

                var _eeg_minutes = phy_rate_list.Where(x => x.fct_billing_key == 324 &&
                                              x.fct_end_date >= endDate &&
                                              x.fct_start_date <= startDate).ToList();

                obj.AssignDate = "Aggregate";
                obj.FacilityKey = facilities[i].id;
                obj.Facility = facilities[i].name;
                obj.CC1_STAT = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.CC1_STAT);
                obj.CC1_StrokeAlert = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.CC1_StrokeAlert);
                obj.New = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.New);
                obj.FU = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.FU);
                obj.EEG = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.EEG);
                obj.LTM_EEG = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.LTM_EEG);
                obj.TC = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.TC);
                obj.STAT_EEG = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.STAT_EEG);
                obj.LTM_VIDEO = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.LTM_VIDEO);
                obj.EEG_MINUTES = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.EEG_MINUTES);
                obj.Amount = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.Amount);
                obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);

                #region Stroke Alert
                if (_cc1Stroke != null && _cc1Stroke.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.CC1_StrokeAlert;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _cc1Stroke.Count; a++)
                        {
                            rate = (decimal)_cc1Stroke[a].fct_rate;
                            if (_cc1Stroke[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                CC1_StrokeAlert = (decimal)total;
                                obj.CC1_StrokeAlertstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_cc1Stroke[a].fct_ending <= count)
                                {
                                    rate = (decimal)_cc1Stroke[a].fct_rate;
                                    total = _cc1Stroke[a].fct_ending * rate;
                                    CC1_StrokeAlert = (decimal)total;
                                    cases = _cc1Stroke[a].fct_ending.ToString();
                                    count = count - _cc1Stroke[a].fct_ending;
                                    obj.CC1_StrokeAlertstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region CC1-Stat
                if (_cc1Stat != null && _cc1Stat.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.CC1_STAT;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _cc1Stat.Count; a++)
                        {
                            rate = (decimal)_cc1Stat[a].fct_rate;
                            if (_cc1Stat[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                CC1_STAT = (decimal)total;
                                obj.CC1_STATstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_cc1Stat[a].fct_ending <= count)
                                {
                                    rate = (decimal)_cc1Stat[a].fct_rate;
                                    total = _cc1Stat[a].fct_ending * rate;
                                    CC1_STAT = (decimal)total;
                                    cases = _cc1Stat[a].fct_ending.ToString();
                                    count = count - _cc1Stat[a].fct_ending;
                                    obj.CC1_STATstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region New
                if (_new != null && _new.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.New;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _new.Count; a++)
                        {
                            rate = (decimal)_new[a].fct_rate;
                            if (_new[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                NewText = (decimal)total;
                                obj.Newstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                obj.Amountstring = "$" + Convert.ToInt16(total).ToString();
                                break;
                            }
                            else
                            {
                                if (_new[a].fct_ending <= count)
                                {
                                    rate = (decimal)_new[a].fct_rate;
                                    total = _new[a].fct_ending * rate;
                                    NewText = (decimal)total;
                                    cases = _new[a].fct_ending.ToString();
                                    count = count - _new[a].fct_ending;
                                    obj.Newstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                    obj.Amountstring = "$" + Convert.ToInt16(total).ToString();
                                }
                            }
                        }
                    }
                }
                #endregion

                #region FU
                if (_fu != null && _fu.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.FU;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _fu.Count; a++)
                        {
                            rate = (decimal)_fu[a].fct_rate;
                            if (_fu[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                FU = (decimal)total;
                                obj.FUstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_fu[a].fct_ending <= count)
                                {
                                    rate = (decimal)_fu[a].fct_rate;
                                    total = _fu[a].fct_ending * rate;
                                    FU = (decimal)total;
                                    cases = _fu[a].fct_ending.ToString();
                                    count = count - _fu[a].fct_ending;
                                    obj.FUstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region EEG
                if (_eeg != null && _eeg.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.EEG;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _eeg.Count; a++)
                        {
                            rate = (decimal)_eeg[a].fct_rate;
                            if (_eeg[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                EEG = (decimal)total;
                                obj.EEGstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_eeg[a].fct_ending <= count)
                                {
                                    rate = (decimal)_eeg[a].fct_rate;
                                    total = _eeg[a].fct_ending * rate;
                                    EEG = (decimal)total;
                                    cases = _eeg[a].fct_ending.ToString();
                                    count = count - _eeg[a].fct_ending;
                                    obj.EEGstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region LTM EEG
                if (_LTM_eeg != null && _LTM_eeg.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.LTM_EEG;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _LTM_eeg.Count; a++)
                        {
                            rate = (decimal)_LTM_eeg[a].fct_rate;
                            if (_LTM_eeg[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                LTM_EEG = (decimal)total;
                                obj.LTM_EEGstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_LTM_eeg[a].fct_ending <= count)
                                {
                                    rate = (decimal)_LTM_eeg[a].fct_rate;
                                    total = _LTM_eeg[a].fct_ending * rate;
                                    LTM_EEG = (decimal)total;
                                    cases = _LTM_eeg[a].fct_ending.ToString();
                                    count = count - _LTM_eeg[a].fct_ending;
                                    obj.LTM_EEGstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region TC
                if (_tc != null && _tc.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.TC;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _tc.Count; a++)
                        {
                            rate = (decimal)_tc[a].fct_rate;
                            if (_tc[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                TC = (decimal)total;
                                obj.TCstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_tc[a].fct_ending <= count)
                                {
                                    rate = (decimal)_tc[a].fct_rate;
                                    total = _tc[a].fct_ending * rate;
                                    TC = (decimal)total;
                                    cases = _tc[a].fct_ending.ToString();
                                    count = count - _tc[a].fct_ending;
                                    obj.TCstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region STAT EEG
                if (_stat_eeg != null && _stat_eeg.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.STAT_EEG;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _stat_eeg.Count; a++)
                        {
                            rate = (decimal)_stat_eeg[a].fct_rate;
                            if (_stat_eeg[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                STAT_EEG = (decimal)total;
                                obj.STAT_EEGstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_stat_eeg[a].fct_ending <= count)
                                {
                                    rate = (decimal)_stat_eeg[a].fct_rate;
                                    total = _stat_eeg[a].fct_ending * rate;
                                    STAT_EEG = (decimal)total;
                                    cases = _stat_eeg[a].fct_ending.ToString();
                                    count = count - _stat_eeg[a].fct_ending;
                                    obj.STAT_EEGstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region LTM Video
                if (_ltm_video != null && _ltm_video.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.LTM_VIDEO;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _ltm_video.Count; a++)
                        {
                            rate = (decimal)_ltm_video[a].fct_rate;
                            if (_ltm_video[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                LTM_VIDEO = (decimal)total;
                                obj.LTM_VIDEOstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_ltm_video[a].fct_ending <= count)
                                {
                                    rate = (decimal)_ltm_video[a].fct_rate;
                                    total = _ltm_video[a].fct_ending * rate;
                                    LTM_VIDEO = (decimal)total;
                                    cases = _ltm_video[a].fct_ending.ToString();
                                    count = count - _ltm_video[a].fct_ending;
                                    obj.LTM_VIDEOstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                #region EEG Minutes
                if (_eeg_minutes != null && _eeg_minutes.Count > 0)
                {
                    decimal rate = 0;
                    int? count = obj.EEG_MINUTES;
                    decimal? total = 0;
                    string cases = "";
                    if (count > 0)
                    {
                        for (int a = 0; a < _eeg_minutes.Count; a++)
                        {
                            rate = (decimal)_eeg_minutes[a].fct_rate;
                            if (_eeg_minutes[a].fct_ending >= count)
                            {
                                total = (count * rate) + total;
                                EEG_MINUTES = (decimal)total;
                                obj.EEG_MINUTESstring += count + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(count) * Convert.ToInt16(rate)).ToString() + '\n';
                                break;
                            }
                            else
                            {
                                if (_eeg_minutes[a].fct_ending <= count)
                                {
                                    rate = (decimal)_eeg_minutes[a].fct_rate;
                                    total = _eeg_minutes[a].fct_ending * rate;
                                    EEG_MINUTES = (decimal)total;
                                    cases = _eeg_minutes[a].fct_ending.ToString();
                                    count = count - _eeg_minutes[a].fct_ending;
                                    obj.EEG_MINUTESstring = cases + " x " + Convert.ToInt16(rate) + " = " + (Convert.ToInt16(cases) * Convert.ToInt16(rate)).ToString() + '\n';
                                }
                            }
                        }
                    }
                }
                #endregion

                obj.Amount = (CC1_StrokeAlert + CC1_STAT + NewText + FU + EEG + LTM_EEG + TC + STAT_EEG + LTM_VIDEO + EEG_MINUTES);
                obj.Amountstring = "$" + Convert.ToInt16(obj.Amount).ToString();
                list.Add(obj);
                list = DateWiseOrder(list);
            }
            return list;
        }
        private List<FacilityBillingByAmount> Availability(List<FacilityBillingByAmount> list, DateTime startDate, DateTime endDate)
        {
            var facilities = list.Select(x => new { name = x.Facility, id = x.FacilityKey }).Distinct().ToList();
            var getRate = from m in _unitOfWork.facilityAvailabilityRateRepository.Query()
                              //where m.far_start_date >= startDate && endDate <= m.far_end_date
                          select (new
                          {
                              shifts = m.far_shifts,
                              rate = m.far_rate,
                              m.far_start_date,
                              m.far_end_date,
                              m.far_fac_key,
                              m.far_recurrence
                          });

            var rates = getRate.ToList();

            FacilityBillingByAmount obj;
            for (int i = 0; i < facilities.Count; i++)
            {
                bool recurring = rates.Where(x => x.far_fac_key == new Guid(facilities[i].id)).Select(x => x.far_recurrence).FirstOrDefault();
                if (recurring)
                {
                    int days = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    var shifts = days * 2;
                    var rate = rates.Where(x => x.far_fac_key == new Guid(facilities[i].id)).Select(x => x.rate.HasValue ? x.rate : 0).FirstOrDefault();
                    obj = new FacilityBillingByAmount();
                    obj.AssignDate = "Availability Pay";
                    obj.FacilityKey = facilities[i].id;
                    obj.Facility = facilities[i].name;
                    obj.Amount = Convert.ToDecimal(Convert.ToInt32(shifts) * Convert.ToInt32(rate));
                    obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);
                    obj.CC1_StrokeAlertstring = Convert.ToInt16(shifts) + " x " + Convert.ToInt16(rate) + " = " + obj.Amount;
                    obj.CC1_STATstring = "";
                    obj.Newstring = "";
                    obj.FUstring = "";
                    obj.EEGstring = "";
                    obj.LTM_EEGstring = "";
                    obj.TCstring = "";
                    obj.LTM_VIDEOstring = "";
                    obj.EEG_MINUTESstring = "";
                    obj.STAT_EEGstring = "";
                    obj.Amountstring = "$" + Convert.ToInt16(obj.Amount).ToString();
                    list.Add(obj);
                }
                else
                {
                    var shifts = rates.Where(x => x.far_fac_key == new Guid(facilities[i].id) && x.far_start_date >= startDate && endDate <= x.far_end_date).Select(x => x.shifts.HasValue ? x.shifts : 0).FirstOrDefault();
                    var rate = rates.Where(x => x.far_fac_key == new Guid(facilities[i].id) && x.far_start_date >= startDate && endDate <= x.far_end_date).Select(x => x.rate.HasValue ? x.rate : 0).FirstOrDefault();
                    obj = new FacilityBillingByAmount();
                    obj.AssignDate = "Availability Pay";
                    obj.FacilityKey = facilities[i].id;
                    obj.Facility = facilities[i].name;
                    obj.Amount = Convert.ToDecimal(Convert.ToInt32(shifts) * Convert.ToInt32(rate));
                    obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);
                    obj.CC1_StrokeAlertstring = Convert.ToInt16(shifts) + " x " + Convert.ToInt16(rate) + " = " + obj.Amount;
                    obj.CC1_STATstring = "";
                    obj.Newstring = "";
                    obj.FUstring = "";
                    obj.EEGstring = "";
                    obj.LTM_EEGstring = "";
                    obj.TCstring = "";
                    obj.LTM_VIDEOstring = "";
                    obj.EEG_MINUTESstring = "";
                    obj.STAT_EEGstring = "";
                    obj.Amountstring = "$" + Convert.ToInt16(obj.Amount).ToString();
                    list.Add(obj);
                }

                list = DateWiseOrder(list);
            }
            return list;
        }

        private List<FacilityBillingByAmount> GrandTotal(List<FacilityBillingByAmount> list)
        {
            var getList = list.Where(x => x.AssignDate == "Availability Pay" || x.AssignDate == "Aggregate").ToList();
            var facilities = list.Select(x => new { name = x.Facility, id = x.FacilityKey }).Distinct().ToList();

            FacilityBillingByAmount obj;
            for (int i = 0; i < facilities.Count; i++)
            {
                obj = new FacilityBillingByAmount();

                obj.AssignDate = "Grand Total";
                obj.FacilityKey = facilities[i].id;
                obj.Facility = facilities[i].name;
                obj.Amount = getList.Where(x => x.Facility == facilities[i].name).Sum(x => x.Amount);
                obj.AmountDollar = "$" + decimal.Round(obj.Amount, 2);
                obj.CC1_StrokeAlertstring = "";
                obj.CC1_STATstring = "";
                obj.Newstring = "";
                obj.FUstring = "";
                obj.EEGstring = "";
                obj.LTM_EEGstring = "";
                obj.TCstring = "";
                obj.LTM_VIDEOstring = "";
                obj.EEG_MINUTESstring = "";
                obj.STAT_EEGstring = "";
                obj.Amountstring = "$" + Convert.ToInt16(obj.Amount);
                list.Add(obj);
                list = DateWiseOrder(list);
            }
            return list;
        }
        private List<FacilityBillingByAmount> DateWiseOrder(List<FacilityBillingByAmount> list)
        {
            var getListWithoutdates = list.Where(x => x.AssignDate == "Availability Pay" || x.AssignDate == "Aggregate" || x.AssignDate == "Grand Total").ToList();
            var getListOfDates = list.Where(x => x.AssignDate != "Availability Pay" && x.AssignDate != "Aggregate" && x.AssignDate != "Grand Total").ToList();
            foreach (var item in getListOfDates)
            {
                string date = item.AssignDate;
                DateTime _dt = Convert.ToDateTime(date);
                item.assign_date = _dt;
            }
            getListOfDates = getListOfDates.OrderBy(x => x.AssignDate).ToList();
            getListOfDates.AddRange(getListWithoutdates);
            return getListOfDates;
        }
        List<FacilityBillingByAmount> FacilityCountToday(DateTime startDate, DateTime endDate, List<string> facilities)
        {
            List<int> caseStatus = null;
            List<FacilityBillingByAmount> onShiftCasesList = null;
            List<FacilityBillingByAmount> offShiftCasesList = null;

            #region On shift billing
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join u in _unitOfWork.FacilityRepository.Query() on c.cas_fac_key equals u.fac_key
                                where c.cas_fac_key != null &&
                                (c.cas_billing_bic_key == 1 || c.cas_billing_bic_key == 2)
                                && c.cas_cst_key == 20
                                && c.cas_billing_physician_blast == false
                                && c.cas_billing_bic_key != null
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_fac_key)
                                select new { c, u });

            if (facilities != null)
                onShiftQuery = onShiftQuery.Where(x => facilities.Contains(x.c.cas_fac_key.ToString()));
            if (caseStatus != null)
                onShiftQuery = onShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    new { onShiftModel.c } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.c.cas_physician_assign_date).Value, false),
                                            Facility = onShiftModel.u.fac_name,
                                            FacilityKey = onShiftModel.c.cas_fac_key.ToString()
                                        } into g
                                select new FacilityBillingByAmount
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Facility = g.Key.Facility,
                                    FacilityKey = g.Key.FacilityKey,
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
                                    STAT_EEG = g.Sum(x => x.c.cas_billing_bic_key == 325 ? 1 : 0),
                                    LTM_VIDEO = g.Sum(x => x.c.cas_billing_bic_key == 326 ? 1 : 0),
                                    EEG_MINUTES = g.Sum(x => x.c.cas_billing_bic_key == 324 ? 1 : 0),
                                    Blast = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.c.cas_key > 0 ? 1 : 0),
                                    CC1_StrokeAlertstring = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0).ToString(),
                                    CC1_STATstring = g.Sum(x => x.c.cas_billing_bic_key == 2 ? 1 : 0).ToString(),
                                    Newstring = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0).ToString(),
                                    FUstring = g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0).ToString(),
                                    EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0).ToString(),
                                    LTM_EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0).ToString(),
                                    TCstring = g.Sum(x => x.c.cas_billing_bic_key == 7 ? 1 : 0).ToString(),
                                    STAT_EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 325 ? 1 : 0).ToString(),
                                    LTM_VIDEOstring = g.Sum(x => x.c.cas_billing_bic_key == 326 ? 1 : 0).ToString(),
                                    EEG_MINUTESstring = g.Sum(x => x.c.cas_billing_bic_key == 324 ? 1 : 0).ToString()
                                }).ToList();
            #endregion

            #region Off shift Billing
            var offShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                 join u in _unitOfWork.FacilityRepository.Query() on c.cas_fac_key equals u.fac_key
                                 where c.cas_fac_key != null
                                 && c.cas_physician_assign_date != null
                                 && c.cas_billing_bic_key == 1
                                 && c.cas_billing_physician_blast
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) >= DbFunctions.TruncateTime(startDate)
                                 && DbFunctions.TruncateTime(c.cas_physician_assign_date) <= DbFunctions.TruncateTime(endDate)
                                 select new { c, u }).Except(from d in onShiftQuery select new { d.c, d.u });

            if (facilities != null)
                offShiftQuery = offShiftQuery.Where(x => facilities.Contains(x.c.cas_fac_key.ToString()));
            if (caseStatus != null)
                offShiftQuery = offShiftQuery.Where(x => caseStatus.Contains(x.c.cas_cst_key));

            if (onShiftCasesList != null && offShiftCasesList != null)
            {
                var result = offShiftCasesList.Concat(onShiftCasesList);
                return result.OrderBy(x => x.FacilityKey).ThenByDescending(x => x.AssignDate != "Blast").ToList();
            }
            if (onShiftCasesList != null)
            {
                return onShiftCasesList.OrderBy(x => x.FacilityKey).ToList();
            }
            if (offShiftCasesList != null)
            {
                return offShiftCasesList.OrderBy(x => x.FacilityKey).ToList();
            }
            #endregion

            return null;
        }
        List<FacilityBillingByAmount> _FacilitycountTodayNewFu(DateTime startDate, DateTime endDate, List<string> facilities, List<user_schedule> scheduleList)
        {
            List<int> caseStatus = new List<int>();
            List<FacilityBillingByAmount> onShiftCasesList = null;
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join u in _unitOfWork.FacilityRepository.Query() on c.cas_fac_key equals u.fac_key
                                where c.cas_fac_key != null && (c.cas_billing_bic_key == 3 || c.cas_billing_bic_key == 4 || c.cas_billing_bic_key == 5 || c.cas_billing_bic_key == 6 || c.cas_billing_bic_key == 7 || c.cas_billing_bic_key == 324 || c.cas_billing_bic_key == 325 || c.cas_billing_bic_key == 326)
                                && c.cas_cst_key == 20 && c.cas_billing_date_of_consult != null && c.cas_billing_bic_key != null
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_fac_key)
                                select new { c, u });

            if (facilities != null)
                onShiftQuery = onShiftQuery.Where(x => facilities.Contains(x.c.cas_fac_key.ToString()));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    new { onShiftModel.c } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.c.cas_billing_date_of_consult).Value, false),
                                            Facility = onShiftModel.u.fac_name,
                                            FacilityKey = onShiftModel.c.cas_fac_key.ToString(),
                                        } into g
                                select new FacilityBillingByAmount
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Facility = g.Key.Facility,
                                    FacilityKey = g.Key.FacilityKey,
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
                                    STAT_EEG = g.Sum(x => x.c.cas_billing_bic_key == 325 ? 1 : 0),
                                    LTM_VIDEO = g.Sum(x => x.c.cas_billing_bic_key == 326 ? 1 : 0),
                                    EEG_MINUTES = g.Sum(x => x.c.cas_billing_bic_key == 324 ? 1 : 0),
                                    Not_Seen = g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0),
                                    Blast = g.Sum(x => x.c.cas_billing_physician_blast ? 1 : 0),
                                    Total = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 8 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 324 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 325 ? 1 : 0) + g.Sum(x => x.c.cas_billing_bic_key == 326 ? 1 : 0),//g.Sum(x => x.c.cas_key > 0 ? 1 : 0),
                                    CC1_StrokeAlertstring = g.Sum(x => x.c.cas_billing_bic_key == 1 ? 1 : 0).ToString(),
                                    CC1_STATstring = g.Sum(x => x.c.cas_billing_bic_key == 2 ? 1 : 0).ToString(),
                                    Newstring = g.Sum(x => x.c.cas_billing_bic_key == 3 ? 1 : 0).ToString(),
                                    FUstring = g.Sum(x => x.c.cas_billing_bic_key == 4 ? 1 : 0).ToString(),
                                    EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 5 ? 1 : 0).ToString(),
                                    LTM_EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 6 ? 1 : 0).ToString(),
                                    TCstring = g.Sum(x => x.c.cas_billing_bic_key == 7 ? 1 : 0).ToString(),
                                    STAT_EEGstring = g.Sum(x => x.c.cas_billing_bic_key == 325 ? 1 : 0).ToString(),
                                    LTM_VIDEOstring = g.Sum(x => x.c.cas_billing_bic_key == 326 ? 1 : 0).ToString(),
                                    EEG_MINUTESstring = g.Sum(x => x.c.cas_billing_bic_key == 324 ? 1 : 0).ToString(),
                                }).ToList();

            return onShiftCasesList;
        }
        private IEnumerable<facility_rate> getFacilityRates(string id)
        {
            var list = _unitOfWork.facilityRateRepository.Query().Where(x => x.fct_facility_key == new Guid(id)).ToList();
            return list;
        }

        private IEnumerable<facility_rate> getAllFacilityRates()
        {
            var list = _unitOfWork.facilityRateRepository.Query().ToList();
            return list;
        }
        #endregion
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.facilityRateRepository.Query()
                               join n in _unitOfWork.FacilityRepository.Query() on m.fct_facility_key equals n.fac_key into facilitity
                               join type in GetUclData(UclTypes.BillingCode) on m.fct_billing_key equals type.ucd_key into CaseTypeEntity
                               from case_type in CaseTypeEntity.DefaultIfEmpty()
                               orderby m.fct_key descending
                               select new
                               {
                                   m.fct_key,
                                   m.fct_facility_key,
                                   m.fct_range,
                                   m.fct_start_date,
                                   m.fct_end_date,
                                   CaseType = case_type != null ? case_type.ucd_title : "",
                                   name = facilitity.FirstOrDefault().fac_name,
                                   rate = "$" + m.fct_rate
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetAllAvailable(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.facilityAvailabilityRateRepository.Query()
                               join n in _unitOfWork.FacilityRepository.Query() on m.far_fac_key equals n.fac_key into facility
                               orderby m.far_key descending
                               select new
                               {
                                   m.far_key,
                                   m.far_fac_key,
                                   m.far_shifts,
                                   m.far_start_date,
                                   name = facility.FirstOrDefault().fac_name,
                                   rate = "$" + m.far_rate,
                                   m.far_recurrence
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public bool IsAlreadyExists(Guid fac_key, DateTime Start_date, DateTime End_date, int case_ID, int start_Index, int end_Index)
        {
            return _unitOfWork.facilityRateRepository.Query()
                                                 .Where(m => m.fct_facility_key == fac_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.fct_start_date) <= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.fct_end_date) >= DbFunctions.TruncateTime(End_date))
                                                 .Where(m => m.fct_billing_key == case_ID)
                                                 .Where(m => m.fct_starting <= start_Index)
                                                 .Where(m => m.fct_ending >= end_Index)
                                                 .Any();
        }
        public bool IsAlreadyExistsRange(Guid fac_key, DateTime Start_date, DateTime End_date, int case_ID, int start_Index, int end_Index)
        {
            return _unitOfWork.facilityRateRepository.Query()
                                                 .Where(m => m.fct_facility_key == fac_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.fct_start_date) <= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.fct_end_date) >= DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => m.fct_billing_key == case_ID)
                                                 .Where(m => m.fct_ending >= start_Index)
                                                 .Any();

        }
        public facility_rate GetDetails(int id)
        {
            var model = _unitOfWork.facilityRateRepository.Query()
                                   .FirstOrDefault(m => m.fct_key == id);
            return model;
        }
        public void Create(facility_rate entity)
        {
            _unitOfWork.facilityRateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(facility_rate entity)
        {
            _unitOfWork.facilityRateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public facility_availability_rate GetDetailsAvail(int id)
        {
            var model = _unitOfWork.facilityAvailabilityRateRepository.Query()
                                   .FirstOrDefault(m => m.far_key == id);
            return model;
        }
        public void Create(facility_availability_rate entity)
        {
            _unitOfWork.facilityAvailabilityRateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(facility_availability_rate entity)
        {
            _unitOfWork.facilityAvailabilityRateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
