using System;
using System.Collections.Generic;
using System.Threading;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.BLL.Process
{
    public class MDStaffProcessor : ProcessorBase
    {
        public MDStaffProcessor()
        {
            _serviceName = "MD Staff Import Service";
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
                //using (var _setting = new Service.AppSettingService())
                //{
                //    var entity = _setting.Get();

                //    if (entity.aps_md_staff_last_run.AddSeconds(_sleepInterval) <= DateTime.Now.ToEST())
                //    {
                string requestId = Guid.NewGuid().ToString();

                DoWork(requestId);

                //        _logger.AddLogEntry(_serviceName, "INFO", "Sleeping for " + _sleepInterval.ToString() + " seconds.", "");
                //        entity.aps_md_staff_last_run = entity.aps_md_staff_last_run.AddSeconds(_sleepInterval);

                //        _setting.Edit(entity);
                //        //Thread.Sleep(new TimeSpan(0, 0, _sleepInterval));
                //    }
                //    else
                //    {
                //        _logger.AddLogEntry(_serviceName, "INFO", "Not schedule to run at this time.", "");
                //    }
                //}
                Thread.Sleep(new TimeSpan(0, 1, 0));
            }

            // if house keeper exit the main thread
            _logger.AddLogEntry(_serviceName, "ALERT", "Exiting Service main thread", "IterateForever");
        }
        public void DoWork(string requestId = "")
        {
            try
            {
                using (var _setting = new Service.AppSettingService())
                {
                    var settingsModel = _setting.Get();

                    // to be controlled by azure triggered job timer
                    // if (settingsModel.aps_md_staff_last_run.AddSeconds(_sleepInterval) <= DateTime.Now.ToEST())
                    {
                        // create log
                        _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started (" + requestId + ")", "");

                        // update last run
                        settingsModel.aps_md_staff_last_run = DateTime.Now.ToEST(); // settingsModel.aps_md_staff_last_run.AddSeconds(_sleepInterval);
                        var dict = new Dictionary<string, string>();
                        dict.Add("aps_md_staff_last_run", settingsModel.aps_md_staff_last_run.ToString());
                        DBHelper.UpdateSelectedColumns("aps_key", settingsModel.aps_key.ToString(), "application_setting", dict);

                        _logger.AddLogEntry(_serviceName, "COMPLETED", "Import last updated at (" + settingsModel.aps_md_staff_last_run.ToString() + ")", "");


                        // start processing
                        using (var objService = new MDStaff.MDStaffImport())
                        {
                            if (!objService.IsActive)
                            {
                                if (string.IsNullOrEmpty(requestId)) requestId = Guid.NewGuid().ToString();

                                // start processing
                                objService.GetAll(requestId, Guid.Empty.ToString(), "Data Import Service").Wait();
                            }
                            else
                            {
                                _logger.AddLogEntry(_serviceName, "ALERT", _serviceName + " is disabled.", "");
                            }
                        }

                        //_logger.AddLogEntry(_serviceName, "INFO", "Sleeping for " + _sleepInterval.ToString() + " seconds.", "");

                        

                        //Thread.Sleep(new TimeSpan(0, 0, _sleepInterval));
                    }
                    //else
                    //{
                    //    _logger.AddLogEntry(_serviceName, "INFO", "Not schedule to run at this time.", "");
                    //}
                }
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
    }
}
