using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Common.Helpers
{
    public class EventLogger
    {
        private readonly bool _enableDetailedLogs = false;

        public EventLogger()
        {
            _enableDetailedLogs = ConfigurationManager.AppSettings.Get("EnableDetailedLogs") == "1";
        }

        public long AddLogEntry(string serviceType, string status, string error, string functionName)
        {
            long ret2 = 0;
            try
            {
                error = functionName == string.Empty ? error : functionName + " - " + error;
                if (_enableDetailedLogs)
                {
                    List<SqlParameter> param = new List<SqlParameter>();

                    param.Add(new SqlParameter("@log_service_type", serviceType));
                    param.Add(new SqlParameter("@log_status", status));
                    param.Add(new SqlParameter("@log_error", error));
                    param.Add(new SqlParameter() { ParameterName = "@out", Value = 0, Direction = ParameterDirection.Output });

                    ret2 = DBHelper.ExecuteNonQuery("system_log_insert", param.ToArray());
                }
                Console.ForegroundColor = status == "COMPLETED" ? ConsoleColor.Green : ConsoleColor.White;
                Console.WriteLine(status + " " + error);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;

                // can enable/disabled via SMSEnabled in appSettings
                LogEventWithMonitor(status, error, serviceType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (AddLogEntry): " + ex.Message);

                //error = error + ex.ToString();
                AddLogToFile(serviceType, status, error).Wait();
            }

            return ret2;
        }

        public long AddLogEntryUsingADO(string serviceType, string status, Exception error, string functionName, string customMessage = "")
        {
            long ret2 = 0;
            string entityError = "";
            string errorMessage = "";
            try
            {
                errorMessage = customMessage + " \t " + entityError + Environment.NewLine + error.ToString();
                errorMessage = functionName == string.Empty ? errorMessage : functionName + " - " + errorMessage;

                if (_enableDetailedLogs)
                {
                    List<SqlParameter> param = new List<SqlParameter>();

                    param.Add(new SqlParameter("@log_service_type", serviceType));
                    param.Add(new SqlParameter("@log_status", status));
                    param.Add(new SqlParameter("@log_error", errorMessage));
                    param.Add(new SqlParameter() { ParameterName = "@out", Value = 0, Direction = ParameterDirection.Output });

                    ret2 = DBHelper.ExecuteNonQuery("system_log_insert", param.ToArray());
                }

                Console.WriteLine(errorMessage);
                Console.WriteLine();

                /// can enable/disabled via SMSEnabled in appSettings
                LogEventWithMonitor(status, errorMessage, serviceType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (AddLogEntry): " + ex.ToString());

                errorMessage = errorMessage + ex.ToString();
                AddLogToFile(serviceType, status, errorMessage).Wait();
            }

            return ret2;
        }

        public void LogEventWithMonitor(string logType, string logText, string serviceType)
        {
            if (ConfigurationManager.AppSettings["SMSEnabled"] == "1")
            {
                try
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            int companyKey = 0;
                            int.TryParse(ConfigurationManager.AppSettings["SMSCompanyKey"], out companyKey);

                            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["SMSServiceAddress"]);

                            var postData = string.Format("log_key=0&log_svc_key={0}&log_last_report={1}&log_type={2}&log_text={3}&log_server={4}&log_domain_key={5}&log_application={6}",
                                                        ConfigurationManager.AppSettings["SMSServiceKey"],
                                                        DateTime.Now.ToString("MM/dd/yy H:mm:ss"),
                                                        logType,
                                                        serviceType + " " + logText + "" + (Convert.ToString(companyKey) != "0" ? " DomainID = " + Convert.ToString(companyKey) : ""),
                                                        ConfigurationManager.AppSettings["SMSServerKey"],
                                                        companyKey,
                                                        ConfigurationManager.AppSettings["SMSApplicationName"]);

                            var data = Encoding.ASCII.GetBytes(postData);

                            request.Method = "POST";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;

                            using (var stream = request.GetRequestStream())
                            {
                                stream.Write(data, 0, data.Length);
                            }

                            var response = (HttpWebResponse)request.GetResponse();

                            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        }
                        catch (Exception ex)
                        {
                            AddLogToFile(serviceType, logType, ex.ToString()).Wait();
                        }
                    });
                }
                catch { }
            }
        }


        public async Task AddLogToFile(string serviceType, string status, string errorMessage)
        {
            try
            {
                // generate file name service wise
                string fileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));

                // get directory where to put files
                string directoryPath = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs");

                // create directory is does not exists
                if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);


                var uniencoding = new UnicodeEncoding();
                byte[] result = uniencoding.GetBytes(string.Format("Date: {0}{1}Service: {2}{3}Status: {4}{5}Error: {6}{7}", DateTime.Now.ToString(), Environment.NewLine, serviceType, Environment.NewLine, status, Environment.NewLine, errorMessage, Environment.NewLine));

                using (FileStream SourceStream = File.Open(Path.Combine(directoryPath, fileName), FileMode.OpenOrCreate))
                {
                    SourceStream.Seek(0, SeekOrigin.End);
                    await SourceStream.WriteAsync(result, 0, result.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (AddLogToFile): " + ex.ToString());
            }
        }

        #region ----- ExecuteNonQuery -----
        /// <summary>
        /// Executes a stored procedure that does not return a dataTable and returns the
        /// first output parameter.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure to execute</param>
        /// <param name="arrParam">Parameters required by the stored procedure</param>
        /// <returns>First output parameter</returns>



        #endregion
    }
}
