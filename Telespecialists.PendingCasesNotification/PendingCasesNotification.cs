using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
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
    public class PendingCasesNotification : ProcessorBase
    {

        public PendingCasesNotification()
        {
            _serviceName = "Send Pending Cases Notifications";
        }

        public void DoWork()
        {           
            try
            {
                bool isEnablePendCasesNotification = false;
                DataTable PendingCasesNotification = DBHelper.ExecuteSqlDataAdapter("sp_enable_pending_cases_notifications", null);
                if(PendingCasesNotification.Rows.Count > 0)
                {
                    isEnablePendCasesNotification = (bool)PendingCasesNotification.Rows[0][0];
                }

                if (isEnablePendCasesNotification)
                {
                    _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");
                    DateTime TimeTo = DateTime.Now.ToEST().AddHours(-1);
                    DateTime TimeFrom = DateTime.Now.ToEST().AddHours(-1).AddMinutes(-5);
                    _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " for " + TimeFrom + " - " + TimeTo, "");
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@TimeFrom", TimeFrom));
                    param.Add(new SqlParameter("@TimeTo", TimeTo));

                    DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("sp_pending_cases_notification", param.ToArray());

                    if (dataTable.Rows.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("");
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            sb.AppendLine(dataTable.Rows[i][0] + " has " + dataTable.Rows[i][1] + " pending cases.");
                        }
                        string accountSid = ConfigurationManager.AppSettings["AccountSid"].ToString();
                        string authToken = ConfigurationManager.AppSettings["AuthKey"].ToString();
                        string FromPhone = ConfigurationManager.AppSettings["FromPhone"].ToString();
                        string ToPhone = ConfigurationManager.AppSettings["ToPhone"].ToString();

                        Console.WriteLine(sb.ToString());
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
                          body: sb.ToString());
                        }
                    }
                    _logger.AddLogEntry(_serviceName, "COMPLETED", _serviceName + " found " + dataTable.Rows.Count + " pending cases.", "");
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
                _logger.AddLogEntry(_serviceName, "COMPLETED", "", "");
            }

        }
    }
}
