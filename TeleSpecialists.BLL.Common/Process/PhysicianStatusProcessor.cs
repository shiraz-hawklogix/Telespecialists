using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common.Extensions;
using TeleSpecialists.BLL.Common.Helpers;

namespace TeleSpecialists.BLL.Common.Process
{
    public class PhysicianStatusProcessor : ProcessorBase 
    {
        private WebScrapper scrapper = null;
        private object Cookies = null;
        

        public PhysicianStatusProcessor()
        {
            _serviceName = "Physician Status Unavailable Service";
            scrapper = new WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
        }

        public void ResetUnSchedulePhysicians()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");

                var now = DateTime.Now.ToEST();
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@DateEST", now));
                
                DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("usp_job_status_unavailable", param.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    
                    var physicianUserIds = dataTable.AsEnumerable().Select(x => x["PhysicianID"].ToString()).ToList();
                    string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                    string postData = Newtonsoft.Json.JsonConvert.SerializeObject(physicianUserIds);
                    scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
                }
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "", "");
            }
        }


       

    }
}
