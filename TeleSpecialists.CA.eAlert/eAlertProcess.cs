using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common.Extensions;
using TeleSpecialists.BLL.Common.Helpers;
using TeleSpecialists.BLL.Common.Process;
using TeleSpecialists.CA.eAlert.Model;

namespace TeleSpecialists.CA.eAlert
{
    public class eAlertProcess : ProcessorBase
    {
        private WebScrapper scrapper = null;

        public string five9Number { get; set; }
        public string five9Domain { get; set; }
        public string five9List { get; set; }
        public string five9URL { get; set; }
        public string SendEmailTo { get; set; }
        public int RetryLimit { get; set; }

        public eAlertProcess()
        {
            _serviceName = "eAlert Resender";
            scrapper = new WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
            five9URL = ConfigurationManager.AppSettings["Five9URL"];
            SendEmailTo = ConfigurationManager.AppSettings["SendMailTo"];
        }

        public void eAlertResend()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "eAlertResend");

                var eAlert = new List<eAlertViewModel>();
                var eAlertUpdateList = new List<eAlertUpdateViewModel>();

                // Responsible to return five9 setting and eAlert data to execute for five9 call
                DataSet ds = DBHelper.ExecuteSqlDataSet("usp_ealert_resend", null);

                if (ds == null)
                {
                    _logger.AddLogEntry(_serviceName, "ALERT", "Store Procedure [usp_ealert_resend] returned nothing", "eAlertResend");
                    return;
                }

                // get application setting from datatable
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    five9Domain = Convert.ToString(item["aps_five9_domain"]);
                    five9Number = ClearPhoneFormat(Convert.ToString(item["aps_five9_number_to_dial"]));
                    five9List = Convert.ToString(item["aps_five9_list"]);
                    RetryLimit = Convert.ToInt32(item["aps_eAlert_retry_limt"]);
                }

                if (string.IsNullOrEmpty(five9Domain) || string.IsNullOrEmpty(five9Number))
                {
                    _logger.AddLogEntry(_serviceName, "ALERT", "Missing Five9 Information.", "eAlertResend");
                    return;
                }

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    // prepare eAlert  data to re send to five9
                    foreach (DataRow item in ds.Tables[1].Rows)
                    {
                        eAlert.Add(new eAlertViewModel
                        {
                            case_number = Convert.ToString(item["case_number"]),
                            case_key = Convert.ToString(item["cas_key"]),
                            callback_number = Convert.ToString(item["cas_callback"]),
                            callback_extension = Convert.ToString(item["callback_extension"]),
                            cart = Convert.ToString(item["cart"]),
                            case_type = Convert.ToString(item["case_type"]),
                            facility_name = Convert.ToString(item["facility"]),
                            wcl_request_retry_count = Convert.ToInt32(item["wcl_request_retry_count"])
                        });
                    }
                }
                else
                {
                    _logger.AddLogEntry(_serviceName, "INFO", "There are no alerts to process.", "eAlertResend");
                    return;
                }

                foreach (var item in eAlert)
                {
                    eAlertUpdateList.Add(SendCallRequestToFive9(item, five9Number, five9Domain));
                }

                if (eAlertUpdateList.Count > 0)
                {
                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@ealert", CreateDatatableForUpdateRecord(eAlertUpdateList)));
                    DBHelper.ExecuteNonQuery("usp_job_update_ealert_resender", param.ToArray());
                }


            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "eAlertResend");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "", "eAlertResend");
            }

            _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting function", "eAlertResend");
        }

        /// <summary>
        /// Send call to five9
        /// </summary>
        /// <param name="data"></param>
        /// <param name="f9Number"></param>
        /// <param name="f9Domain"></param>
        /// <returns>get eAlert processed data to update in database</returns>
        public eAlertUpdateViewModel SendCallRequestToFive9(eAlertViewModel data, string f9Number, string f9Domain)
        {
            var eAlertUpdateModel = new eAlertUpdateViewModel();
            eAlertUpdateModel.reprocessed_date = DateTime.Now.ToEST();
            eAlertUpdateModel.wcl_request_retry_count = data.wcl_request_retry_count;
            eAlertUpdateModel.case_key = data.case_key;
            try
            {
                //fin9 complete url to process request
                var five9ApiUrl = five9URL + f9Domain + "&F9list=" + five9List;
                five9ApiUrl += "&F9key=case_number&F9updateCRM=1&case_number=" + data.case_number;
                five9ApiUrl += "&number1=" + f9Number;
                five9ApiUrl += "&case_type=" + data.case_type + "&Facility Name=" + data.facility_name + "&cart=" + data.cart + "&callback_number=" + data.callback_number;
                five9ApiUrl += "&callback_extension=" + data.callback_extension + "&F9CallASAP=0&F9retResults=1";

                _logger.AddLogEntry(_serviceName, "INPROGRESS", five9ApiUrl, "SendCallRequestToFive9");

                //send call to five9
                string result = scrapper.GetData(five9ApiUrl, "POST", null);
                if (!string.IsNullOrEmpty(result))
                {
                    //pars response returned from five9
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(result);

                    var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//td/input");

                    //responbile to create json object
                    StringBuilder json = new StringBuilder();
                    json.Append("{");

                    for (int i = 0; i < htmlNodes.Count; i++)
                    {
                        string id = htmlNodes[i].Attributes["id"].Value;
                        string value = htmlNodes[i].Attributes["value"].Value;
                        if (id == "F9errCode" && value == "0")
                        {

                            eAlertUpdateModel.error_code = value;
                        }
                        else if (id == "F9errCode" && value != "0")
                        {
                            eAlertUpdateModel.error_code = "TC-Error";
                        }
                        else if (id == "F9errDesc")
                        {
                            eAlertUpdateModel.error_description = value;
                        }
                        json.Append(string.Format(" \"{0}\":\"{1}\",", id, value));
                    }

                    // remove ',' at the end of the string
                    if (htmlNodes.Count > 0) { json.Remove(json.Length - 1, 1); }
                    json.Append("}");
                   
                    eAlertUpdateModel.raw_result = json.ToString();
                }

                _logger.AddLogEntry(_serviceName, "COMPLETED", five9ApiUrl, "SendCallRequestToFive9");
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "SendCallRequestToFive9");
            }

            return eAlertUpdateModel;
        }


        /// <summary>
        /// Used for ealert data to send atonce to SP as parameter
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private DataTable CreateDatatableForUpdateRecord(List<eAlertUpdateViewModel> model)
        {
            _logger.AddLogEntry(_serviceName, "INPROGRESS", "In Function", "CreateDatatableForUpdateRecord");

            DataTable dt = new DataTable();
            dt.Columns.Add("raw_result");
            dt.Columns.Add("error_code");
            dt.Columns.Add("error_description");
            dt.Columns.Add("cas_key");            
            dt.Columns.Add("reprocessed_date");
            dt.Columns.Add("error_email_date");

            //prepare datatable
            foreach (var item in model)
            {
                if (item.wcl_request_retry_count < (RetryLimit - 1) && item.error_code.ToString().Trim() != "0")
                    item.reprocessed_date = null;
                              
                    dt.Rows.Add(item.raw_result, item.error_code, item.error_description, item.case_key, item.reprocessed_date, item.error_email_date);

            }

            _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting function", "CreateDatatableForUpdateRecord");

            return dt;
        }



        /// <summary>
        /// Used to clear character from phone number like (,),-
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public string ClearPhoneFormat(string phone)
        {
            if (!string.IsNullOrEmpty(phone))
            {
                phone = phone.Replace("(", string.Empty)
                             .Replace(")", string.Empty)
                             .Replace(" ", string.Empty)
                             .Replace("-", string.Empty);

            }
            return phone;
        }

        /// <summary>
        /// This is responsible to get eAlert detail from database and to update the email eAlert records in database
        /// </summary>
        public void SendEmailOfeAlerts()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", " Started", "SendEmailOfeAlerts");

                var eAlert = new List<eAlertViewModel>();
                var eAlertMailUpdate = new List<eAlertUpdateViewModel>();

                // Responsible to return eAlert data to send an email
                DataTable dataTable = DBHelper.ExecuteSqlDataAdapter("usp_ealert_send_email", null);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.AddLogEntry(_serviceName, "ALERT", "Store Procedure [usp_ealert_send_email] returned nothing", "SendEmailOfeAlerts");
                    return;
                }

                //prepare list of email detail

                foreach (DataRow item in dataTable.Rows)
                {
                    eAlert.Add(new eAlertViewModel
                    {
                        case_number = Convert.ToString(item["case_number"]),
                        case_key = Convert.ToString(item["cas_key"]),
                        callback_number = Convert.ToString(item["cas_callback"]),
                        callback_extension = Convert.ToString(item["callback_extension"]),
                        cart = Convert.ToString(item["cart"]),
                        case_type = Convert.ToString(item["case_type"]),
                        facility_name = Convert.ToString(item["facility"]),
                        error_description = Convert.ToString(item["error_description"]),
                        reprocessed_Date = Convert.ToDateTime(item["wcl_reprocessed_date"]),
                    });
                }

                // get directory name
                string directoryPath = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "EmailTemplates");
                string FilePath = directoryPath + "\\MailTemplate.html";
                StreamReader str = new StreamReader(FilePath);
                string MailText = str.ReadToEnd();
                str.Close();
                // send email               
                foreach (var item in eAlert)
                {
                    string emailText = MailText.Replace("**item.case_number**", item.case_number)
                                                .Replace("**item.callback_number**", item.callback_number)
                                                .Replace("**item.callback_extension**", item.callback_extension)
                                                .Replace("**item.cart**", item.cart)
                                                .Replace("**item.facility_name**", item.facility_name)
                                                .Replace("**item.reprocessed_Date**", Convert.ToString(item.reprocessed_Date))
                                                .Replace("**item.case_type**", item.case_type)
                                                .Replace("**item.error_description**", item.error_description);

                    eAlertMailUpdate.Add(SendEmail(SendEmailTo, "Testing Email", emailText, item.case_key));
                }

                if (eAlertMailUpdate.Count > 0)
                {

                    List<SqlParameter> param = new List<SqlParameter>();
                    param.Add(new SqlParameter("@ealert", CreateDatatableForUpdateRecord(eAlertMailUpdate)));
                    DBHelper.ExecuteNonQuery("usp_job_update_email", param.ToArray());
                }

                _logger.AddLogEntry(_serviceName, "COMPLETED", _serviceName + " In Function", "SendEmail");

            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "SendEmailOfeAlerts");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "", "SendEmailOfeAlerts");
            }

            _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting function", "SendEmailOfeAlerts");

        }


        /// <summary>
        /// This is responsible to send email
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="emailText"></param>
        /// <returns></returns>
        public eAlertUpdateViewModel SendEmail(string to, string subject, string emailText, string casekey)
        {
            var emailUpdateModel = new eAlertUpdateViewModel();
            try
            {

                emailUpdateModel.case_key = casekey;
                emailUpdateModel.error_email_date = DateTime.Now.ToEST();  // will  be replaced by actual email sent time after the mail is sent.

                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", "Sending Email with case key=" + casekey + "", "SendEmail");

                string teleCARESupport = ConfigurationManager.AppSettings.Get("TeleCARESupport");
                string teleCAREWebsite = ConfigurationManager.AppSettings.Get("TeleCAREWebsite");


                if (string.IsNullOrEmpty(subject)) subject = "TeleCARE Notification";
                string body = "";

                body += "<p>" + emailText + " </p>";
                body += "<div>";
                body += "<br />";
                body += "<br />";
                body += $@"<p>Condado Group Inc.
                                          <br>1301 Burlington Street, Suite 150
                                          <br>Kansas City, MO 64116
                                          <br><a href=""{teleCARESupport}"">{teleCARESupport}</a>
                                          <br><a href=""{teleCAREWebsite}"" target=""_blank"">{teleCAREWebsite}</a>
                                          </p>";
                body += "<p>";
                body += $"Email Confidentiality Notice: The information contained in this transmission is confidential, proprietary or privileged and may be subject to protection under the law. The message is intended for the sole use of the individual or entity to whom it is addressed. If you are not the intended recipient, you are notified that any use, distribution or copying of the message is strictly prohibited and may subject you to criminal or civil penalties. If you received this transmission in error, please contact the sender immediately by replying to <a href=\"mailto:{teleCARESupport}\">{teleCARESupport}</a> and delete the material from any computer.";
                body += "</p>";
                body += "</div>";


                using (var message = new MailMessage())
                {
                    message.To.Add(to);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Send(message);
                        emailUpdateModel.error_email_date = DateTime.Now.ToEST();
                    }
                }
                _logger.AddLogEntry(_serviceName, "COMPLETED", "Email sent with case key=" + casekey + "", "SendEmail");
                return emailUpdateModel;
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "eAlertResend");
            }
            return emailUpdateModel;
        }

    }
}
