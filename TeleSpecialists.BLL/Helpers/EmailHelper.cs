using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Mail;
using System.Configuration;

namespace TeleSpecialists.BLL.Helpers
{
    public class EmailHelper
    {
        public static void ServiceErrorEmail()
        {
            try
            {
                string supportEmails = ConfigurationManager.AppSettings.Get("SupportEmails");
                string serviceName = ConfigurationManager.AppSettings.Get("SMSApplicationName");
                string teleCARESupport = ConfigurationManager.AppSettings.Get("TeleCARESupport");
                string teleCAREWebsite = ConfigurationManager.AppSettings.Get("TeleCAREWebsite");


                string subject = "TeleCARE (" + serviceName + ") Service Notification – Service is Stopped";
                string body = "";

                body += "<p>TeleCARE " + serviceName + " is stopped at " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt").Replace('-', '/') + ". Please take the required action. </p>";
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
                    message.To.Add(supportEmails);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                new Helpers.EventLogger().AddLogEntry("Email Helper", "ERROR", ex.ToString(), "ServiceErrorEmail");
            }
        }

        public static void SendEmail(string to, string subject, string emailText)
        {
            try
            {
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
                    }
                }
            }
            catch (Exception ex)
            {
                new Helpers.EventLogger().AddLogEntry("Email Helper", "ERROR", ex.ToString(), "SendEmail");
            }
        }

        public static void SendTwoFactorAuthEmail(string to, string subject, string emailText)
        {
            try
            {

                if (string.IsNullOrEmpty(subject)) subject = "Two FactorAuth";
                string body = "";

                body += "<p>" + emailText + " </p>";
                //body += "<div>";
                //body += "<br />";
                //body += "<br />";
                //body += $@"<p>Condado Group Inc.
                //                          <br>1301 Burlington Street, Suite 150
                //                          <br>Kansas City, MO 64116
                //                          <br><a href=""{teleCARESupport}"">{teleCARESupport}</a>
                //                          <br><a href=""{teleCAREWebsite}"" target=""_blank"">{teleCAREWebsite}</a>
                //                          </p>";
                //body += "<p>";
                //body += $"Email Confidentiality Notice: The information contained in this transmission is confidential, proprietary or privileged and may be subject to protection under the law. The message is intended for the sole use of the individual or entity to whom it is addressed. If you are not the intended recipient, you are notified that any use, distribution or copying of the message is strictly prohibited and may subject you to criminal or civil penalties. If you received this transmission in error, please contact the sender immediately by replying to <a href=\"mailto:{teleCARESupport}\">{teleCARESupport}</a> and delete the material from any computer.";
                //body += "</p>";
                //body += "</div>";


                using (var message = new MailMessage())
                {
                    message.To.Add(to);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                new Helpers.EventLogger().AddLogEntry("Email Helper", "ERROR", ex.ToString(), "SendTwoFactorAuthEmail");
                throw ex;
            }
        }
    }
}
