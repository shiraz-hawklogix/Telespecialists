using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using TeleSpecialists.BLL.Common.Helpers;
using TeleSpecialists.BLL.Common.Process;
using TeleSpecialists.BLL.Common.Extensions;
using System.Data.SqlClient;
using System.Data;
using TeleSpecialists.BLL.Common;

namespace TeleSpecialists.CA.StatusMover
{

    public class PhysicianStatusSnoozeViewModel
    {
        public string UserId { get; set; }
        public int phs_key { get; set; }
    }

    public class PhysicianStatusMover : ProcessorBase
    {

        private WebScrapper scrapper = null;
        private object Cookies = null;
        
        public PhysicianStatusMover()
        {
            _serviceName = "Physician Status Mover Service";
            scrapper = new WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
        }
 

        public void MovePhysicianStatuses()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");
                
                var now = DateTime.Now.ToEST();
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@DateEST", now));

                DataSet dataSet = DBHelper.ExecuteSqlDataSet("usp_job_status_mover", param.ToArray());
                var snoozePoupUsersIds = new List<PhysicianStatusSnoozeViewModel>();
                List<string> movedUserIds = new List<string>();
                foreach (DataRow item in dataSet.Tables[0].Rows)
                {
                    snoozePoupUsersIds.Add(new PhysicianStatusSnoozeViewModel { phs_key = (Int32)item["status_key"], UserId = item["UserId"].ToString() });
                }
                foreach (DataRow item in dataSet.Tables[1].Rows)
                {
                    movedUserIds.Add(item["UserId"].ToString());
                }

                if (snoozePoupUsersIds.Count() > 0)
                {
                    RunSnoozePopupCode(snoozePoupUsersIds);
                    snoozePoupUsersIds.Clear();
                }

                if (movedUserIds.Count() > 0)
                {
                    // sending request to signal r controller to update the status of online physician
                    string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                    string postData = Newtonsoft.Json.JsonConvert.SerializeObject(movedUserIds);
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

        private void RunSnoozePopupCode(List<PhysicianStatusSnoozeViewModel> PopupUsers)
        {
            string url = scrapper.baseUrl + "/RPCHandler/ShowPhysicianStatusSnoozePopup?SignalRAuthKey=" + Settings.SignalRAuthKey;
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(PopupUsers);
            scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
        }              

    }
}
