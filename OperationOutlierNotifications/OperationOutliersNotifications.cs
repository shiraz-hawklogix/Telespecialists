using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using TeleSpecialists.BLL.Common.Extensions;
using TeleSpecialists.BLL.Common.Helpers;
using TeleSpecialists.BLL.Common.Process;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Telespecialists.PendingCasesNotification
{
    public class OperationOutliersNotifications : ProcessorBase
    {
        public OperationOutliersNotifications()
        {
            _serviceName = "Send Operation Outliers Notifications";
        }
        public void DoWork()
        {
            try
            {
                List<InsertIntoOperationOutlier> _listOutliers = new List<InsertIntoOperationOutlier>();
                var isEnableOPerationOutlierNotification = ConfigurationManager.AppSettings["isServiceEnabled"].ToString();
                if (isEnableOPerationOutlierNotification.ToLower().Trim() == "true")
                {
                    
                    DateTime TimeTo = DateTime.Now.ToEST();
                    DateTime TimeFrom = new DateTime(TimeTo.Year, TimeTo.Month, TimeTo.Day, 0, 0, 0);
                    if(TimeTo.Hour >= 6 && TimeTo.Hour <= 24)
                    {
                        //_logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");
                        //_logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " for " + TimeFrom + " - " + TimeTo, "");
                        List<SqlParameter> param = new List<SqlParameter>();
                        param.Add(new SqlParameter("@case_start_date_time", TimeFrom));
                        param.Add(new SqlParameter("@case_end_date_time", TimeTo));

                        DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("sp_send_text_notification_operation_outliers", param.ToArray());

                        if (dataTable.Rows.Count > 0)
                        {
                            InsertIntoOperationOutlier objOutliers;
                            StringBuilder sb = new StringBuilder();

                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                bool isYellowCase = false;
                                string case_number = dataTable.Rows[i]["cas_case_number"].ToString();
                                string fac_name = dataTable.Rows[i]["fac_name"].ToString();
                                DateTime? cas_created_date = (DateTime)dataTable.Rows[i]["cas_created_date"];
                                DateTime? cas_modified_date = null;
                                if (!string.IsNullOrEmpty(dataTable.Rows[i]["cas_modified_date"].ToString()))
                                {
                                    cas_modified_date = (DateTime)dataTable.Rows[i]["cas_modified_date"];
                                }
                                string cas_case_assign_phy_initial = dataTable.Rows[i]["phy_name"].ToString();
                                objOutliers = new InsertIntoOperationOutlier();
                                string[] Physicains = cas_case_assign_phy_initial.Split('/');
                                if (Physicains.Length > 2)
                                {
                                    isYellowCase = true;
                                }
                                if (dataTable.Rows[i]["ctp_name"].ToString().ToLower() == "stroke alert")
                                {
                                    string[] ResponseTime = dataTable.Rows[i]["ResponseTime"].ToString().Split(':');
                                    double hours = Convert.ToDouble(ResponseTime[0]);
                                    double mins = Convert.ToDouble(ResponseTime[1]);
                                    if (ResponseTime.Length > 2)
                                    {
                                        mins += Convert.ToDouble(ResponseTime[2]) / 60;
                                    }

                                    if (hours > 0 || mins > 10)
                                    {
                                        objOutliers.cas_case_number = case_number;
                                        objOutliers.cas_created_date = cas_created_date;
                                        objOutliers.cas_modified_date = cas_modified_date;
                                        objOutliers.cas_case_fac_name = fac_name;
                                        objOutliers.cas_case_assign_phy_initial = cas_case_assign_phy_initial;
                                        objOutliers.isAdded = false;
                                        objOutliers.cas_case_type = "stroke alert";

                                        if (isYellowCase)
                                        {
                                            objOutliers.cas_case_color = "PURPLE";
                                            bool isExist = CheckCaseNumberExist(objOutliers);
                                            if (!isExist)
                                            {
                                                objOutliers.isAdded = true;
                                                sb.AppendLine("Case Number: " + case_number + ", Facility Name: " + fac_name + ", Assigned Physician: " + cas_case_assign_phy_initial + ", Case Color: " + objOutliers.cas_case_color);
                                            }
                                        }
                                        else
                                        {
                                            objOutliers.cas_case_color = "PINK";
                                            bool isExist = CheckCaseNumberExist(objOutliers);
                                            if (!isExist)
                                            {
                                                objOutliers.isAdded = true;
                                                sb.AppendLine("Case Number: " + case_number + ", Facility Name: " + fac_name + ", Assigned Physician: " + cas_case_assign_phy_initial + ", Case Color: " + objOutliers.cas_case_color);
                                            }
                                        }
                                    }
                                }
                                else if (dataTable.Rows[i]["ctp_name"].ToString().ToLower() == "stat consult")
                                {
                                    string[] CallBackResponseTime = dataTable.Rows[i]["CallBackResponseTime"].ToString().Split(':');
                                    double hours = Convert.ToDouble(CallBackResponseTime[0]);
                                    double mins = Convert.ToDouble(CallBackResponseTime[1]);
                                    if (CallBackResponseTime.Length > 2)
                                    {
                                        mins += Convert.ToDouble(CallBackResponseTime[2]) / 60;
                                    }

                                    if (hours > 0 || mins > 15)
                                    {
                                        objOutliers.cas_case_number = case_number;
                                        objOutliers.cas_created_date = cas_created_date;
                                        objOutliers.cas_modified_date = cas_modified_date;
                                        objOutliers.cas_case_fac_name = fac_name;
                                        objOutliers.cas_case_assign_phy_initial = cas_case_assign_phy_initial;
                                        objOutliers.isAdded = false;
                                        objOutliers.cas_case_type = "stat consult";

                                        if (isYellowCase)
                                        {
                                            objOutliers.cas_case_color = "PURPLE";
                                            bool isExist = CheckCaseNumberExist(objOutliers);
                                            if (!isExist)
                                            {
                                                objOutliers.isAdded = true;
                                                sb.AppendLine("Case Number: " + case_number + ", Facility Name: " + fac_name + ", Assigned Physician: " + cas_case_assign_phy_initial + ", Case Color: " + objOutliers.cas_case_color);
                                            }
                                        }
                                        else
                                        {
                                            objOutliers.cas_case_color = "PINK";
                                            bool isExist = CheckCaseNumberExist(objOutliers);
                                            if (!isExist)
                                            {
                                                objOutliers.isAdded = true;
                                                sb.AppendLine("Case Number: " + case_number + ", Facility Name: " + fac_name + ", Assigned Physician: " + cas_case_assign_phy_initial + ", Case Color: " + objOutliers.cas_case_color);
                                            }
                                        }
                                    }
                                }

                                if (objOutliers != null && objOutliers.isAdded)
                                {
                                    _listOutliers.Add(objOutliers);
                                }
                            }

                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                sendOperationOutlierNotifications(sb.ToString());
                                InsertIntoOperationOutlierLogTable(_listOutliers);
                                _logger.AddLogEntry(_serviceName, "COMPLETED", _serviceName + " found " + _listOutliers.Count + " outlier notifications.", "");
                            }
                        }
                        
                    }                     
                }
                else
                {
                    _logger.AddLogEntry(_serviceName, "STOPPED", _serviceName + " Stopped.", "");
                }
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "");
            }
            finally
            {
                //_logger.AddLogEntry(_serviceName, "COMPLETED", "", "");                
            }
        }

        public bool sendOperationOutlierNotifications(string messageBody)
        {
            try
            {
                string accountSid = ConfigurationManager.AppSettings["AccountSid"].ToString();
                string authToken = ConfigurationManager.AppSettings["AuthKey"].ToString();
                string FromPhone = ConfigurationManager.AppSettings["FromPhone"].ToString();
                string ToPhone = ConfigurationManager.AppSettings["ToPhone"].ToString();

                TwilioClient.Init(accountSid, authToken);
                var restClient = new TwilioRestClient(accountSid, authToken);

                string[] phoneNumbers = ToPhone.Split(';');
                foreach (var item in phoneNumbers)
                {
                    if (string.IsNullOrEmpty(item))
                    {
                        continue;
                    }
                    var message = MessageResource.Create(
                  to: new PhoneNumber(item),
                  from: new PhoneNumber(FromPhone),
                  body: messageBody);
                }
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "");
                return false;
            }

            return true;

        }

        public bool CheckCaseNumberExist(InsertIntoOperationOutlier objOutliers)
        {
            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@cas_case_number", objOutliers.cas_case_number));
                param.Add(new SqlParameter("@cas_case_color", objOutliers.cas_case_color));
                param.Add(new SqlParameter("@cas_case_type", objOutliers.cas_case_type));

                DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("sp_check_case_number_in_operation_outlier_log", param.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    bool isExist = (bool)dataTable.Rows[0][0];

                    return isExist;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;

        }

        public bool InsertIntoOperationOutlierLogTable(List<InsertIntoOperationOutlier> _listOutliers)
        {
            try
            {
                foreach (var item in _listOutliers)
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@cas_case_number", item.cas_case_number));
                    param.Add(new SqlParameter("@cas_case_type", item.cas_case_type));
                    param.Add(new SqlParameter("@cas_case_color", item.cas_case_color));
                    param.Add(new SqlParameter("@cas_created_date", item.cas_created_date));
                    param.Add(new SqlParameter("@cas_modified_date", item.cas_modified_date));
                    param.Add(new SqlParameter("@cas_case_fac_name", item.cas_case_fac_name));
                    param.Add(new SqlParameter("@cas_case_assign_phy_initial", item.cas_case_assign_phy_initial));

                    int result = DBHelper.ExecuteNonQuery("sp_insert_case_number_in_operation_outlier_log", param.ToArray());
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public class InsertIntoOperationOutlier
        {
            public string cas_case_number { get; set; }
            public string cas_case_fac_name { get; set; }
            public string cas_case_type { get; set; }
            public string cas_case_color { get; set; }
            public string cas_case_assign_phy_initial { get; set; }
            public bool isAdded { get; set; }
            public DateTime? cas_created_date { get; set; }
            public DateTime? cas_modified_date { get; set; }
        }
    }
}
