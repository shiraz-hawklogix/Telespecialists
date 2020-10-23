using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
namespace TeleSpecialists.BLL.Process
{
    public class PhysicianStatusProcessor : ProcessorBase
    {
        private WebScrapper scrapper = null;
        private object Cookies = null;
        string _service2Name = "Physician Status Reset Service";
        string _service3Name = "UnSchedule Physician Update Service";
        Thread _thread2;
        Thread _thread3;
        public PhysicianStatusProcessor()
        {
            _serviceName = "Physician Status Mover Service";
            scrapper = new WebScrapper();
            scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
        }
        public void StartService()
        {
            _thread = new Thread(IterateForeverStatusMover);
            _thread.IsBackground = true;
            _thread.Start();

            _thread2 = new Thread(IterateForeverStatusReset);
            _thread2.IsBackground = true;
            _thread2.Start();

            _thread3 = new Thread(IterateUnSchedulePhysicianReset);
            _thread3.IsBackground = true;
            _thread3.Start();
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

        #region ----- Status Mover -----

        private void IterateForeverStatusMover()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                // start processing
                this.MovePhysicianStatuses();

                _logger.AddLogEntry(_serviceName, "INFO", "Sleeping for " + _sleepInterval.ToString() + " seconds.", "");
                Thread.Sleep(new TimeSpan(0, 0, _sleepInterval));
            }

            // if exit the main thread
            _logger.AddLogEntry(_serviceName, "ALERT", "Exiting Service main thread", "IterateStatusMoverForever");
        }

        public void MovePhysicianStatuses()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "");

                var snoozePoupUsersIds = new List<PhysicianStatusSnoozeViewModel>();
                var movedUserIds = new List<string>();

                using (var objService = new PhysicianService())
                {
                    objService.GetPhysicianStatusDashboard()
                              .Where(m => m.physician.physician_status.phs_move_status_key != null && m.physician.status_key != m.physician.physician_status.phs_move_status_key)
                              .ToList()
                              .ForEach(item =>
                              {

                                  var status_key = item.physician.status_key;
                                  var isSendSnoozeNotification = false;
                                  var snoozeTime = item.physician.physician_status_snooze.Where(m => m.pss_processed_date == null
                                                                                                     && m.pss_phs_key == status_key
                                                                                                     );

                                  

                                  double snoozeMinutes = 0;
                                  if (snoozeTime.Count() > 0)
                                  {
                                      snoozeMinutes = snoozeTime.Sum(m => m.pss_snooze_time.TotalMinutes);
                                  }

                                  if (item.physician.physician_status.phs_move_threshhold_time.HasValue && item.physician.physician_status.phs_enable_snooze)
                                  {
                                      // this condition will be rechecked after the task is completed. little bit doubt on it
                                      var elapsedTime = DateTime.Now.ToEST() - item.physician.status_change_date_forAll.Value.AddMinutes(snoozeMinutes);
                                      var difference = item.physician.physician_status.phs_move_threshhold_time.Value.TotalMinutes - elapsedTime.TotalMinutes;

                                      var currentSnooze = item.physician.physician_status_snooze.Where(m => m.pss_processed_date == null
                                                                                                   && m.pss_phs_key == status_key
                                                                                                   && m.pss_is_latest_snooze
                                                                                                   )
                                                                                                   .FirstOrDefault();

                                      if (difference <= 2.1 && difference >= 1.5 && currentSnooze == null)
                                      {
                                          isSendSnoozeNotification = true;
                                      }
                                  }

                                  if (isSendSnoozeNotification && snoozeTime.Count() < item.physician.physician_status.phs_max_snooze_count.Value)
                                  {
                                      snoozePoupUsersIds.Add(new PhysicianStatusSnoozeViewModel { phs_key = item.physician.status_key.Value, UserId = item.physician.Id });
                                  }
                                  else
                                  {
                                      if (RunStatusUpdateCode(item, snoozeMinutes))
                                      {
                                          snoozeTime.ToList().ForEach(m =>
                                          {
                                              m.pss_processed_date = DateTime.Now.ToEST();
                                              m.pss_is_latest_snooze = false;
                                              m.pss_modified_by = "physician status move service";
                                              m.pss_modified_by_name = "physician status move service";
                                              m.pss_modified_date = DateTime.Now.ToEST();
                                          });

                                          movedUserIds.Add(item.physician.Id);
                                      }
                                  }

                              });

                    objService.SaveChanges();
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
                _logger.AddLogEntry(_serviceName, "ERROR", exception, "");
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

        private bool RunStatusUpdateCode(PhysicianDashboardViewModel item, double snoozeMinutes)
        {
            var elapsedTime = DateTime.Now.ToEST() - item.physician.status_change_date_forAll.Value.AddMinutes(snoozeMinutes);

            var hasExceededThreshhold = item.physician.physician_status.phs_move_threshhold_time.HasValue ?
                                                                item.physician.physician_status.phs_move_threshhold_time.Value.TotalMilliseconds < elapsedTime.TotalMilliseconds : false;

            if (!hasExceededThreshhold)
                return false;

            item.physician.status_key = item.physician.physician_status.phs_move_status_key.Value;

            #region ---- Applying Rule 3 Define in TCARE-11 -  Physician Status Rules/Changes -----

            var status = item.physician.physician_status.physician_status2.phs_key;
            bool isUpdateStatusDate = true;
            if (item.physician.physician_status.phs_key == PhysicianStatus.Stroke.ToInt() && status == PhysicianStatus.TPA.ToInt())
            {
                isUpdateStatusDate = false;
            }

            #endregion


            if (isUpdateStatusDate)
            {
                item.physician.status_change_date = DateTime.Now.ToEST();
                item.physician.status_change_cas_key = null;               
            }
            item.physician.status_change_date_forAll = DateTime.Now.ToEST();

            using (var objLogService = new PhysicianStatusLogService())
            {
                objLogService.Create(new Model.physician_status_log
                {
                    psl_user_key = item.physician.Id,
                    psl_created_by = "move to status service",
                    psl_created_date = DateTime.Now.ToEST(),
                    psl_phs_key = status,
                    psl_status_name = item.physician.physician_status.physician_status2.phs_name,                    
                });
            }


            return true;
        }


        #endregion

        #region ----- Status Reset -----

        private void IterateForeverStatusReset()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                // start processing
                this.ResetOnSchedulePhysicians();


                _logger.AddLogEntry(_service2Name, "INFO", "Sleeping for " + _sleepInterval.ToString() + " seconds.", "");
                Thread.Sleep(new TimeSpan(0, 0, _sleepInterval));
            }

            // if house keeper exit the main thread
            _logger.AddLogEntry(_service2Name, "ALERT", "Exiting Service main thread", "ResetOnSchedulePhysicians");
        }

        public void ResetOnSchedulePhysicians()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_service2Name, "INPROGRESS", _service2Name + " Started", "");

                using (var objService = new PhysicianService())
                {
                    var physicians = objService.GetPhysiciansForService().ToList();
                    physicians.ForEach(p =>
                    {
                        p.physician.status_key = PhysicianStatus.Available.ToInt();
                        p.physician.status_change_date = DateTime.Now.ToEST();
                        p.physician.status_change_date_forAll = DateTime.Now.ToEST();
                        p.physician.status_change_cas_key = null;

                        using (var objLogService = new PhysicianStatusLogService())
                        {
                            objLogService.Create(new Model.physician_status_log
                            {
                                psl_user_key = p.physician.Id,
                                psl_created_by = "reset physician service",
                                psl_created_date = DateTime.Now.ToEST(),
                                psl_phs_key = PhysicianStatus.Available.ToInt(),
                                psl_start_date = p.schedule.uss_time_from_calc,
                                psl_end_date = p.schedule.uss_time_to_calc,
                                psl_status_name = PhysicianStatus.Available.ToString(),
                                psl_uss_key = p.schedule.uss_key,
                            });
                        }

                    });

                    if (physicians.Count() > 0)
                    {
                        objService.SaveChanges();
                        var physicianUserIds = physicians.Select(m => m.physician.Id).ToList();
                        string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                        string postData = Newtonsoft.Json.JsonConvert.SerializeObject(physicianUserIds);
                        var result = scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
                    }

                    var NhPhysicians = objService.GetNHPhysiciansForService().ToList();

                    NhPhysicians.ForEach(p =>
                    {
                        p.physician.status_key = PhysicianStatus.Available.ToInt();
                        p.physician.status_change_date = DateTime.Now.ToEST();
                        p.physician.status_change_date_forAll = DateTime.Now.ToEST();
                        p.physician.status_change_cas_key = null;

                        using (var objLogService = new PhysicianStatusLogService())
                        {
                            objLogService.Create(new Model.physician_status_log
                            {
                                psl_user_key = p.physician.Id,
                                psl_created_by = "reset physician service",
                                psl_created_date = DateTime.Now.ToEST(),
                                psl_phs_key = PhysicianStatus.Available.ToInt(),
                                psl_start_date = p.schedule.uss_time_from_calc,
                                psl_end_date = p.schedule.uss_time_to_calc,
                                psl_status_name = PhysicianStatus.Available.ToString(),
                                psl_uss_key = p.schedule.uss_key,
                            });
                        }

                    });

                    if (NhPhysicians.Count() > 0)
                    {
                        objService.SaveChanges();
                        var physicianUserIds = NhPhysicians.Select(m => m.physician.Id).ToList();
                        string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                        string postData = Newtonsoft.Json.JsonConvert.SerializeObject(physicianUserIds);
                        var result = scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
                    }
                }

            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_service2Name, "ERROR", exception, "");
            }
            finally
            {
                _logger.AddLogEntry(_service2Name, "COMPLETED", "", "");
            }
        }

        #endregion

        #region ----- UnSchedulePhysicianReset -----

        private void IterateUnSchedulePhysicianReset()
        {
            while (!_shutdownEvent.WaitOne(0))
            {
                // start processing
                this.ResetUnSchedulePhysicians();

                _logger.AddLogEntry(_service3Name, "INFO", "Sleeping for " + _sleepInterval.ToString() + " seconds.", "");
                Thread.Sleep(new TimeSpan(0, 0, _sleepInterval));
            }

            // if house keeper exit the main thread
            _logger.AddLogEntry(_service3Name, "ALERT", "Exiting Service main thread", "IterateUnSchedulePhysicianReset");
        }

        public void ResetUnSchedulePhysicians()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_service3Name, "INPROGRESS", _service3Name + " Started", "");

                using (var objService = new PhysicianService())
                {

                    var now = DateTime.Now.ToEST();
                    var physicians = objService.GetUnSchedulePhysiciansForService()
                                               .ToList();
                    physicians.ForEach(p =>
                    {
                        p.status_key = PhysicianStatus.NotAvailable.ToInt();
                        p.status_change_date = now;
                        p.status_change_cas_key = null;
                        p.status_change_date_forAll = now;

                        using (var objLogService = new PhysicianStatusLogService())
                        {

                            objLogService.Create(new Model.physician_status_log
                            {
                                psl_user_key = p.Id,
                                psl_created_by = "unscheduled physicians service",
                                psl_created_date = DateTime.Now.ToEST(),
                                psl_phs_key = PhysicianStatus.NotAvailable.ToInt(),
                                psl_start_date = DateTime.Now.ToEST(),
                                psl_status_name = PhysicianStatus.NotAvailable.ToString()
                            });
                        }

                    });

                    if (physicians.Count() > 0)
                    {
                        objService.SaveChanges();
                        var physicianUserIds = physicians.Select(m => m.Id).ToList();
                        string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                        string postData = Newtonsoft.Json.JsonConvert.SerializeObject(physicianUserIds);
                        scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
                    }

                    var Nhphysicians = objService.GetNHUnSchedulePhysiciansForService()
                                             .ToList();
                    Nhphysicians.ForEach(p =>
                    {
                        p.status_key = PhysicianStatus.NotAvailable.ToInt();
                        p.status_change_date = now;
                        p.status_change_cas_key = null;
                        p.status_change_date_forAll = now;

                        using (var objLogService = new PhysicianStatusLogService())
                        {

                            objLogService.Create(new Model.physician_status_log
                            {
                                psl_user_key = p.Id,
                                psl_created_by = "unscheduled physicians service",
                                psl_created_date = DateTime.Now.ToEST(),
                                psl_phs_key = PhysicianStatus.NotAvailable.ToInt(),
                                psl_start_date = DateTime.Now.ToEST(),
                                psl_status_name = PhysicianStatus.NotAvailable.ToString()
                            });
                        }

                    });

                    if (Nhphysicians.Count() > 0)
                    {
                        objService.SaveChanges();
                        var physicianUserIds = Nhphysicians.Select(m => m.Id).ToList();
                        string url = scrapper.baseUrl + "/RPCHandler/RefreshPhysicianStatus?SignalRAuthKey=" + Settings.SignalRAuthKey;
                        string postData = Newtonsoft.Json.JsonConvert.SerializeObject(physicianUserIds);
                        scrapper.GetData(url, "POST", postData, ref Cookies, ContentTypes.Json);
                    }


                }
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_service3Name, "ERROR", exception, "");
            }
            finally
            {
                _logger.AddLogEntry(_service3Name, "COMPLETED", "", "");
            }
        }

        #endregion

        //public object AuthenticateTeleSpecialist()
        //{
        //    object cc = new object();
        //    scrapper = new WebScrapper();

        //    scrapper.baseUrl = ConfigurationManager.AppSettings["TeleCAREWebsite"];
        //    string url = scrapper.baseUrl + "/Account/login";
        //    string postdata = $"Username={ConfigurationManager.AppSettings["TeleCareUserName"]}&Password={ConfigurationManager.AppSettings["TeleCarePassword"]}";


        //    string html = scrapper.GetInitialCookies(url, "POST", postdata, out cc);

        //    return cc;
        //}

    }
}
