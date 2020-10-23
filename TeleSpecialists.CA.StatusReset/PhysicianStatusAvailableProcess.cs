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

namespace TeleSpecialists.CA.StatusAvailable
{
    public class PhysicianStatusAvailableProcess : ProcessorBase
    {
        private WebScrapper scrapper = null;
        private object Cookies = null;
     

        public PhysicianStatusAvailableProcess()
        {
            _serviceName = "Physician Status Available Service";
            scrapper = new WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
        }

        public void StatusAvailableProcess()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");

                var now = DateTime.Now.ToEST();
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@DateEST", now));

                DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("usp_job_status_available", param.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    var physicianUserIds = dataTable.AsEnumerable().Select(x => x["UserId"].ToString()).ToList();
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
