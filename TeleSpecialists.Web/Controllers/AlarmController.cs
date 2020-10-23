using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.Web.Models;
using TeleSpecialists.BLL.ViewModels;


namespace TeleSpecialists.Web.Controllers
{
    public class AlarmController : BaseController
    {
        private readonly AspNetUsersLogService _userLogService;
        private readonly AspNetUsersPasswordResetService _userPassResetService;
        private readonly AlarmService _alarmService;
        private readonly AlarmTuneService _alarmTuneService;
        private user_alarm_setting _user_Alarm_Setting;
        public AlarmController()
        {
            _userLogService = new AspNetUsersLogService();
            _userPassResetService = new AspNetUsersPasswordResetService();
            _alarmService = new AlarmService();
            _alarmTuneService = new AlarmTuneService();
            _user_Alarm_Setting = new user_alarm_setting();
        }
        // GET: Alarm
        public ActionResult Index()
        {
            return View();
        }

        #region HawkLogix Work for alarm setting
        /// <summary>
        /// ////custom alarm setting for physician by HawkLogix
        /// </summary>
        /// <returns></returns>
        /// 
        //[HttpGet]
        public ActionResult AlarmSetting()
        {

            #region Get Tune List
            var tunesList = _alarmTuneService.GetTuneList();
            _user_Alarm_Setting.alarm_list = tunesList;
            #endregion
            var _AlarmSetting = _alarmService.GetDetails(loggedInUser.Id);
            if (_AlarmSetting != null)
            {
                _user_Alarm_Setting.obj_alarm_Setting = _AlarmSetting;
            }
            else
            {
                alarm_setting _Alarm = new alarm_setting();
                // check in app setting table
                if (ApplicationSetting != null)
                {
                    if (ApplicationSetting.aps_tune_is_active == true)
                    {
                        _Alarm.als_selected_audio = ApplicationSetting.aps_selected_audio;
                        ViewBag.selected_audio = ApplicationSetting.aps_audio_file_path + "," + ApplicationSetting.aps_selected_audio;
                    }
                }
                _user_Alarm_Setting.obj_alarm_Setting = _Alarm;
            }
            return GetViewResult(_user_Alarm_Setting);
        }
        // for ajax request 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AlarmSetting(string audio_name,string audio_names)
        {
            alarm_setting _Alarm = new alarm_setting();
            try
            {
                audio_name = audio_names;
                bool Status = false;
                if (ModelState.IsValid)
                {
                    
                    var record = _alarmService.GetDetails(loggedInUser.Id);
                    if (record != null)
                    {
                        _Alarm = record;
                        Status = Update(_Alarm, audio_name);
                    }
                    else
                    {
                        //_Alarm = new alarm_setting();
                        Status = Insert(_Alarm, audio_name);
                    }
                    return ShowSuccessMessageOnly("Default Notifications Tune Successfully Saved", _Alarm);

                }
                return GetErrorResult(_Alarm);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult(_Alarm);
            }
        }

        #region  Set default tune for physicians
        public ActionResult SetDefaultTune()
        {
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult SetDefaultTune(string audio_name)
        {
            try
            {
                bool Status = false;
                if (ModelState.IsValid)
                {
                    alarm_setting _Alarm = new alarm_setting();
                    _Alarm = _alarmService.GetDetails(loggedInUser.Id);
                    if (_Alarm != null)
                    {
                        Status = Update(_Alarm, audio_name);
                    }
                    else
                    {
                        Status = Insert(_Alarm, audio_name);
                    }
                    return ShowSuccessMessageOnly("Default Notifications Tune Successfully Saved",_Alarm);
                }
                return GetErrorResult("");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return GetErrorResult("");
            }
        }
        // partial view of SetDefaultTune 
        public ActionResult _alarm()
        {
            #region Get Tune List
            var tunesList = _alarmTuneService.GetTuneList();
            _user_Alarm_Setting.alarm_list = tunesList;
            #endregion
            var _AlarmSetting = _alarmService.GetDetails(loggedInUser.Id);
            if (_AlarmSetting != null)
            {
                _user_Alarm_Setting.obj_alarm_Setting = _AlarmSetting;
            }
            else
            {
                alarm_setting _Alarm = new alarm_setting();
                // check in app setting table
                if (ApplicationSetting != null)
                {
                    if (ApplicationSetting.aps_tune_is_active == true)
                    {
                        _Alarm.als_selected_audio = ApplicationSetting.aps_selected_audio;
                    }
                }
                _user_Alarm_Setting.obj_alarm_Setting = _Alarm;
            }
            return GetViewResult(_user_Alarm_Setting);
        }
        #endregion

        public ActionResult _refreshAlarm()
        {
            if (loggedInUser != null)
            {
                bool _isTuneAllow = false;
                _isTuneAllow = ApplicationSetting.aps_enable_alarm_setting;
                if (_isTuneAllow)
                {
                    string _id = loggedInUser.Id;
                    string filepath = _alarmService.ShowAlarmSetting(_id, ApplicationSetting);
                    ViewBag.file_path = filepath;
                }
                else
                {
                    string filepath = _alarmService._find_DefaultPath(ApplicationSetting);
                    ViewBag.file_path = filepath;
                }
            }
            return GetViewResult();
        }

        public ActionResult AddAlarmTune()
        {
            alarm_tunes _Alarm = new alarm_tunes();
            return GetViewResult(_Alarm);
        }
        [HttpPost]
        public ActionResult AddAlarmTune(alarm_tunes _alarmTunes, HttpPostedFileBase fileUpload, string fileName)
        {
            try
            {
                if (ModelState.IsValid && fileUpload != null)
                {
                    string _filname = "/Content/sounds/" + fileUpload.FileName;
                    bool isExistbefore = _alarmTuneService.isExistRecord(_filname);
                    if (isExistbefore == false)
                    {
                        bool isSave = SaveFile(fileUpload);
                        if (isSave)
                        {
                            _alarmTunes.alt_phy_key = loggedInUser.Id;
                            var _value = getId();
                            _alarmTunes.alt_audio_path = _filname;
                            if (string.IsNullOrEmpty(fileName))
                                _alarmTunes.alt_file_name = Path.GetFileNameWithoutExtension(fileUpload.FileName);
                            else
                                _alarmTunes.alt_file_name = fileName;
                            _alarmTunes.alt_selected_audio = _value.ToString();
                            _alarmTunes.alt_created_by = loggedInUser.Id;
                            _alarmTunes.alt_created_by_name = loggedInUser.FullName;
                            _alarmTunes.alt_created_date = DateTime.Now;
                            //_alarmTuneService.Create(_alarmTunes);
                            //var alarm_tune_list = _alarmTuneService.GetTuneList();
                            alarm_tunes tunes = savealarmtune(_alarmTunes);

                            return ShowSuccessMessageOnly("success", tunes);
                        }
                        else
                        {
                            return ShowSuccessMessageOnly("error", _alarmTunes);
                        }
                    }
                    else
                    {
                        return ShowSuccessMessageOnly("exist", _alarmTunes);
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ShowSuccessMessageOnly("error", _alarmTunes);
            }
            return ShowSuccessMessageOnly("error", _alarmTunes);
        }

        alarm_tunes savealarmtune(alarm_tunes alarm)
        {
            _alarmTuneService.Create(alarm);
            return alarm;
        }

        private bool SaveFile(HttpPostedFileBase postedfile)
        {
            try
            {
                postedfile.SaveAs(Server.MapPath("/Content/sounds/" + postedfile.FileName));
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        private int getId()
        {
            var id = _alarmTuneService.GetMaxId();
            id += 1;
            return id;
        }
       
        #region Alarm Tune Removal
        public ActionResult AlarmTuneRemove(alarm_tunes alarm_Tunes)
        {
            var GetAll = _alarmTuneService.GetTuneList();
            return View(GetAll);
        }
        public JsonResult DeleteAlarmFromApp(int id)
        {
            try
            {
                var GetRecord = _alarmTuneService.Detail(id);
                if (GetRecord != null)
                {
                    using (var _appSettingService = new AppSettingService())
                    {
                        // check tune if its set to default
                        var isThisDefault = _appSettingService.CheckForStatus(GetRecord.alt_selected_audio); //_defaultNotificationTuneService.CheckForStatus(GetRecord.alt_selected_audio);
                        if (isThisDefault != null)
                        {
                            isThisDefault.aps_tune_is_active = false;
                            isThisDefault.aps_modified_by = loggedInUser.Id;
                            isThisDefault.aps_modified_date = DateTime.Now;
                            isThisDefault.aps_audio_file_path = null;
                            isThisDefault.aps_selected_audio = null;
                            _appSettingService.Edit(isThisDefault);
                        }
                    }
                    _alarmTuneService.Delete(id);
                    _alarmService.Delete(GetRecord.alt_selected_audio);
                    // delete file from folder
                    bool isDeleted = DeleteFile(GetRecord.alt_audio_path);
                }
                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json("error", JsonRequestBehavior.AllowGet);
            }
        }
        bool DeleteFile(string file)
        {

            try
            {
                if (System.IO.File.Exists(Server.MapPath(file)))
                {
                    System.IO.File.Delete(Server.MapPath(file));
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }
        public ActionResult DeleteModal(int id)
        {
            ViewBag.id = id;
            return GetViewResult();
        }
        #endregion

        private bool Insert(alarm_setting _Alarm, string audio_name)
        {
            string[] file_arr = audio_name.Split(',');
            _Alarm.als_audio_path = file_arr[0];
            _Alarm.als_selected_audio = file_arr[1];
            _Alarm.als_phy_key = loggedInUser.Id;
            _Alarm.als_file_name = file_arr[0];
            _Alarm.als_created_by = loggedInUser.Id;
            _Alarm.als_created_by_name = loggedInUser.FullName;
            _Alarm.als_created_date = DateTime.Now.ToEST();
            _alarmService.Create(_Alarm);
            return true;
        }
        private bool Update(alarm_setting _Alarm, string audio_name)
        {
            string[] file_name = audio_name.Split(',');
            _Alarm.als_audio_path = file_name[0];
            _Alarm.als_file_name = file_name[0];
            _Alarm.als_selected_audio = file_name[1];
            _Alarm.als_modified_by = loggedInUser.Id;
            _Alarm.als_modified_by_name = loggedInUser.FullName;
            _Alarm.als_modified_date = DateTime.Now.ToEST();
            _alarmService.Edit(_Alarm);
            return true;
        }
        #endregion
        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _alarmService?.Dispose();
                    _alarmTuneService?.Dispose();
                    _userLogService?.Dispose();
                    _userPassResetService?.Dispose();
                    
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}