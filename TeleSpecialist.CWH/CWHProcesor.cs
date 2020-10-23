using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common;
using TeleSpecialists.BLL.Common.Extensions;
using TeleSpecialists.BLL.Common.Helpers;
using TeleSpecialists.BLL.Common.Process;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ModelEx;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialist.CWH
{
    public class CWHProcesor : ProcessorBase
    {
        //private WebScrapper scrapper = null;
        //private object Cookies = null;

        private TeleSpecialists.BLL.Helpers.WebScrapper scrapper = null;
        private FacilityService facilityService;
        private readonly LookupService _lookUpService;
        public CWHProcesor()
        {
            _serviceName = "CWH DATA";
            scrapper = new TeleSpecialists.BLL.Helpers.WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
            facilityService = new FacilityService();
            _lookUpService = new LookupService();
        }
        public void DoWork()
        {
            try
            {
                // create log
                var date = new DateTime(2019, 09, 01);
                var startdate = date.ToString("yyyy-MM-dd");
                _logger.AddLogEntry(_serviceName, "CWH", "Start Date  " + startdate + "", "");

                var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                _logger.AddLogEntry(_serviceName, "CWH", "End Date  " + endDate + "", "");
                cwh_data _cwh;
                List<cwh_data> lstcmh = new List<cwh_data>();
                List<CaseViewModel> _caseList = new List<CaseViewModel>();
                CaseViewModel _case;
                //Delete Last 3 months record
                var datebefore3months = DateTime.Now.AddMonths(-3);
                datebefore3months = new DateTime(datebefore3months.Year, datebefore3months.Month, 1);
                _logger.AddLogEntry(_serviceName, "CWH", "Last 3 months Date  " + datebefore3months + "", "");
                List<SqlParameter> param1 = new List<SqlParameter>();
                param1.Add(new SqlParameter("@start", datebefore3months));
                DataTable dataTable1 = TeleSpecialists.BLL.Common.Helpers.DBHelper.ExecuteSqlDataAdapter("usp_get_last3month_record", param1.ToArray());
                if (dataTable1.Rows.Count > 0)
                {
                    startdate = datebefore3months.ToString("yyyy-MM-dd");
                    _logger.AddLogEntry(_serviceName, "CWH", "Updated Start date  " + startdate + "", "");
                    foreach (var row in dataTable1.AsEnumerable())
                    {
                        _cwh = new cwh_data();
                        _cwh.cwh_key = row.Field<int>("cwh_key");
                        _logger.AddLogEntry(_serviceName, "CWH", "added " + _cwh.cwh_key + " in first table ", "");
                        lstcmh.Add(_cwh);
                    }
                    facilityService.DeletePreviousRecords(lstcmh);
                }
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@StartDate", startdate));
                param.Add(new SqlParameter("@edate", endDate));
                _logger.AddLogEntry(_serviceName, "CWH", _serviceName + " Started", "");
                _logger.AddLogEntry(_serviceName, "CWH", "Fetching Records from database", "");



                var _facilitylist = _lookUpService.GetAllFacility(null)
                                   .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                   .ToList();

                DataTable dataTable = TeleSpecialists.BLL.Common.Helpers.DBHelper.ExecuteSqlDataAdapter("UspGetCWHData", param.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (var row in dataTable.AsEnumerable())
                    {
                        _case = new CaseViewModel();

                        _case.cas_fac_key = row.Field<Guid>("cas_fac_key");
                        _case.cas_ctp_key = row.Field<int>("cas_ctp_key");
                        _case.cas_response_ts_notification = row.Field<DateTime>("cas_response_ts_notification");
                        _caseList.Add(_case);
                    }
                }
                if (_facilitylist != null && _facilitylist.Count > 0)
                {
                    _logger.AddLogEntry(_serviceName, "CWH", "Preparing data to Save Record", "");
                    foreach (var item in _facilitylist)
                    {
                        CWHReport report = new CWHReport();

                        DateTime StartDate = Convert.ToDateTime(startdate);
                        DateTime EndDate = Convert.ToDateTime(endDate).AddMonths(1).AddDays(-1);

                        _cwh = new cwh_data();
                        _cwh.cwh_fac_name = item.Text;
                        _cwh.cwh_fac_id = item.Value;

                        for (var i = StartDate; StartDate < EndDate;)
                        {
                            DateTime edate = StartDate.AddMonths(1).AddDays(-1);
                            var result2 = _caseList.Where(x => x.cas_response_ts_notification >= StartDate && x.cas_response_ts_notification <= edate).Count();
                            var result = _caseList.Where(x => x.cas_response_ts_notification >= StartDate && x.cas_response_ts_notification <= edate && x.cas_fac_key == _cwh.cwh_fac_id).Count();
                            int month_in_digit = StartDate.Month;
                            double calvalue = (double)result.ToDouble() / result2.ToDouble();
                            if (Double.IsNaN(calvalue))
                            {
                                calvalue = 0;
                            }
                            _cwh.cwh_month_wise_cwh = Math.Round(calvalue, 4).ToDouble();
                            _cwh.cwh_date = StartDate;
                            _cwh.cwh_modified_by = "WEB JOB";
                            _cwh.cwh_totalcwh = Math.Round(calvalue, 4).ToDouble();
                            _cwh.cwh_modified_date = DateTime.Now;
                            StartDate = StartDate.AddMonths(1);
                            _logger.AddLogEntry(_serviceName, "CWH", "Saving Record " + _cwh.cwh_fac_name + "" + _cwh.cwh_date + " " + _cwh.cwh_month_wise_cwh + " " + _cwh.cwh_totalcwh + "", "");
                            facilityService.UpdateTableCWHData(_cwh);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting Service main thread", "DoWork");
            }
        }
    }
}






