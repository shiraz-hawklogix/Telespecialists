using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using System.Linq;


using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.BLL.Process
{
    public class RapidsProcessor : ProcessorBase
    {
        public RapidsProcessor()
        {
            _serviceName = "Rapids Service";
        }
        public void StartService()
        {
            _thread = new Thread(IterateForever);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void StopService()
        {
            _shutdownEvent.Set();
            EmailHelper.ServiceErrorEmail();
            _logger.AddLogEntry(_serviceName, "ALERT", "Stopped", "");
            if (!_thread.Join(1))
            {
                // give the thread 3 seconds to stop
                _thread.Abort();
            }
        }

        private void IterateForever()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                DoWork();

                Thread.Sleep(new TimeSpan(0, 1, 0));
            }

            // if house keeper exit the main thread
            _logger.AddLogEntry(_serviceName, "ALERT", "Exiting Service main thread", "IterateForever");
        }

        public void DoWork()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");

                // download new emails
                DownloadEmails();

                // delete emails as per retention policy
                DeleteEmails();
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception, "");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting Service main thread", "DoWork");
            }
        }

        private void DownloadEmails()
        {
            _logger.AddLogEntry(_serviceName, "INPROGRESS", "Downloading emails", "DownloadEmails");

            using (var _setting = new AppSettingService())
            {
                var appSettings = _setting.Get();

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013_SP1);

                service.Credentials = new WebCredentials(appSettings.aps_rapids_email, appSettings.aps_rapids_password);
                service.Url = new Uri(appSettings.aps_rapids_service);
                service.PreAuthenticate = true;

                _logger.AddLogEntry(_serviceName, "INPROGRESS", "Reading O365 mailbox", "DownloadEmails");

                ItemView view = new ItemView(int.MaxValue);
                FindItemsResults<Item> findResults = service.FindItems(WellKnownFolderName.Inbox, SetFilter(), view);

                _logger.AddLogEntry(_serviceName, "INFO", $"There are {findResults.Items?.Count} emails to process", "DownloadEmails");

                // in case if deployed different location as background job
                string rapidsDirectoryUrl = System.Configuration.ConfigurationManager.AppSettings.Get("RapidsDirectoryUrl");

                using (var _rapidsService = new RapidsService())
                {
                    foreach (Item item in findResults.Items)
                    {
                        _logger.AddLogEntry(_serviceName, "INPROGRESS", $"Processing email with Subject = {item.Subject}", "DownloadEmails");

                        var mailMessage = (EmailMessage)item;

                        // format internalMessageId to be used as Id
                        string messageID = mailMessage.InternetMessageId.Replace("<", "").Replace(">", "");
                        Model.rapids_mailbox newEmail = null;


                        if (_rapidsService.FindByUID(messageID))
                        {
                            service.DeleteItems(new List<ItemId> { new ItemId(mailMessage.Id.UniqueId) }, DeleteMode.MoveToDeletedItems, null, null);
                            continue;
                        }

                        // load details
                        item.Load();

                        try
                        {
                            newEmail = new Model.rapids_mailbox
                            {
                                rpd_attachments = item.Attachments.Count,
                                rpd_body = item.Body,
                                rpd_date = item.DateTimeReceived,
                                rpd_created_by = "Import Service",
                                rpd_created_date = DateTime.Now.ToEST(),
                                rpd_from = string.Format("{0} <{1}>", mailMessage.From.Name, mailMessage.From.Address),
                                rpd_subject = item.Subject,
                                rpd_to = item.DisplayTo,
                                rpd_uid = messageID,
                            };

                            // save email into database
                            _rapidsService.Create(newEmail);

                            newEmail.rpd_body = newEmail.rpd_body.Replace("cid:", $"{rapidsDirectoryUrl}/RapidsAttachments/{newEmail.rpd_key}/");

                            #region ----- Save Attachments -----

                            // check if there are any attachments
                            if (item.Attachments != null && item.Attachments.Count > 0)
                            {
                                _logger.AddLogEntry(_serviceName, "INPROGRESS", $"Processing attachments with Subject = {item.Subject}", "DownloadEmails");

                                string parentDirectory = System.Configuration.ConfigurationManager.AppSettings.Get("RapidsDirectory");
                                string attachmentDirectory = Path.Combine(parentDirectory, Convert.ToString(newEmail.rpd_key));
                                string attachmentDirectoryOthers = Path.Combine(attachmentDirectory, "Others");

                                // create directory
                                if (!Directory.Exists(attachmentDirectory)) Directory.CreateDirectory(attachmentDirectory);
                                if (!Directory.Exists(attachmentDirectoryOthers)) Directory.CreateDirectory(attachmentDirectoryOthers);

                                // save attachments
                                foreach (Attachment attach in item.Attachments)
                                {
                                    if (attach.IsInline)
                                    {
                                        ((FileAttachment)attach).Load(Path.Combine(attachmentDirectory, attach.Name));
                                        newEmail.rpd_body = newEmail.rpd_body.Replace(attach.ContentId, attach.Name);
                                    }
                                    else
                                    {
                                        ((FileAttachment)attach).Load(Path.Combine(attachmentDirectoryOthers, attach.Name));
                                        string htmlForAttachment = "<div style=\"text-align: center; display: block; padding: 10px; \">";
                                        string attachmentLink = $"{rapidsDirectoryUrl}/RapidsAttachments/{newEmail.rpd_key}/Others/{attach.Name}";

                                        if (attach.Name.EndsWith(".txt") || attach.Name.EndsWith(".doc") || attach.Name.EndsWith(".docx"))
                                            htmlForAttachment += $"<a href=\"{attachmentLink}\" target=\"_blank\">{attach.Name}</a>";
                                        else
                                            htmlForAttachment += $"<img src=\"{attachmentLink}\" alt=\"{attach.Name}\" />";

                                        htmlForAttachment += "</div>";

                                        if (string.IsNullOrEmpty(newEmail.rpd_attachment_html)) newEmail.rpd_attachment_html = "";

                                        newEmail.rpd_attachment_html = newEmail.rpd_attachment_html + htmlForAttachment;

                                        if (!string.IsNullOrEmpty(attach.ContentId))
                                            newEmail.rpd_attachment_html = newEmail.rpd_attachment_html.Replace(attach.ContentId, attach.Name);
                                    }
                                }
                            }

                            #endregion

                            // delete email once saved into database
                            service.DeleteItems(new List<ItemId> { new ItemId(mailMessage.InternetMessageId) }, DeleteMode.MoveToDeletedItems, null, null);
                        }
                        catch (Exception ex)
                        {
                            if (newEmail != null) { newEmail.rpd_logs += ex.ToString(); }
                        }
                        finally
                        {
                            // save email attachments name changes, if any 
                            _rapidsService.Edit(newEmail);
                        }
                    }
                }
            }
        }

        private static SearchFilter SetFilter()
        {
            List<SearchFilter> searchFilterCollection = new List<SearchFilter>();

            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false));
            searchFilterCollection.Add(new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, true));

            SearchFilter s = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, searchFilterCollection.ToArray());
            return s;
        }

        private void DeleteEmails()
        {
            _logger.AddLogEntry(_serviceName, "INPROGRESS", "Deleting old emails", "DeleteEmails");

            using (var _setting = new AppSettingService())
            {
                var appSettings = _setting.Get();

                // if there is a retention setting defined
                if (appSettings.aps_rapids_retention != null)
                {
                    using (var _rapidsService = new RapidsService())
                    {
                        var retentionStartTime = DateTime.Now.ToEST().Subtract(appSettings.aps_rapids_retention);

                        // get emails older than retention period set
                        var oldEmails = _rapidsService.GetAll().Where(x => x.CreatedDate < retentionStartTime).ToList();

                        if (oldEmails == null)
                        {
                            _logger.AddLogEntry(_serviceName, "INFO", "There are not emails to be removed from database", "DeleteEmails");
                            return;
                        }

                        foreach (var item in oldEmails)
                        {
                            _logger.AddLogEntry(_serviceName, "INPROGRESS", $"Deleting email. ID = {item.Id}", "DeleteEmails");


                            try
                            {
                                // delete images and sub-folders
                                string parentDirectory = System.Configuration.ConfigurationManager.AppSettings.Get("RapidsDirectory");
                                string attachmentDirectory = Path.Combine(parentDirectory, Convert.ToString(item.Id));
                                string attachmentDirectoryOthers = Path.Combine(attachmentDirectory, "Others");

                                _logger.AddLogEntry(_serviceName, "INPROGRESS", $"Deleting images. ID = {item.Id}", "DeleteEmails");

                                if (Directory.Exists(attachmentDirectoryOthers)) Directory.Delete(attachmentDirectoryOthers, true);
                                if (Directory.Exists(attachmentDirectory)) Directory.Delete(attachmentDirectory, true);

                                // save email into database
                                _rapidsService.Delete(item.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.AddLogEntry(_serviceName, "ERROR", $"Error deleting email locally. ID = {item.Id}. {ex.ToString()}", "DeleteEmails");
                            }
                        }
                    }
                }
            }
        }
    }
}
