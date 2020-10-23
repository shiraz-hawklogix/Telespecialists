using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
   public class AlarmService : BaseService
    {
        
        public alarm_setting GetDetails(string id)
        {
            var model = _unitOfWork.AlarmRepository.Query()
                                   .FirstOrDefault(m => m.als_phy_key == id);
            return model;
        }
        public void Create(alarm_setting entity)
        {
            _unitOfWork.AlarmRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(alarm_setting entity)
        {
            _unitOfWork.AlarmRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        // check alarm setting show to the physicians or not
        public string ShowAlarmSetting(string id, application_setting appSetting)
        {
            string file_path = "";
            if(appSetting.aps_enable_alarm_setting)
            {
                // process of alarm tune if super admin allow the custom alarm tunes for physician
                var _AlarmSetting = GetDetails(id);
                if (_AlarmSetting != null)
                {
                    file_path = _AlarmSetting.als_audio_path;
                }
                else
                {
                    // check  here db tabel to check defaut tune by admin
                    file_path = _find_DefaultPath(appSetting);
                }
            }
            else
            {
                // if super admin doesn't allow the custom tunes
                file_path = "/Content/sounds/new_case_notification.mp3";//_find_DefaultPath();
            }
            return file_path;
        }
        public string _find_DefaultPath(application_setting  application_Setting)
        {
            string file_path = "";
            var setting_detail = application_Setting;//appSettingService.CheckForStatus();
            if (setting_detail != null)
            {
                if (setting_detail.aps_tune_is_active == true)
                    file_path = setting_detail.aps_audio_file_path;
                else
                    file_path = "/Content/sounds/new_case_notification.mp3";
            }
            else
            {
                file_path = "/Content/sounds/new_case_notification.mp3";
            }
            return file_path;
        }

        public void Delete(string id)
        {
            _unitOfWork.AlarmRepository.DeleteRange(_unitOfWork.AlarmRepository.Query().Where(m => m.als_selected_audio == id));
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
