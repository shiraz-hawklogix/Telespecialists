using System;
using System.Data.Entity;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AppSettingService : BaseService
    {
        public virtual application_setting Get()
        {
            var settings = _unitOfWork.AppSettingRepository.Query().FirstOrDefault();
            return settings;
        }
        public void Create(application_setting entity)
        {   
            _unitOfWork.AppSettingRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(application_setting entity)
        {
            _unitOfWork.AppSettingRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public application_setting CheckForStatus(string fileid)
        {
            var Query = _unitOfWork.AppSettingRepository.Query()
                 .Where(x => x.aps_selected_audio == fileid).FirstOrDefault();
            return Query;
        }
        public int UpdatePhysicianPendingCasesClearanceDate()
        {
            var appSetting = Get();
            if (appSetting != null)
            {
                var dateFromPendingCases = DateTime.Now.ToEST();
                appSetting.aps_clear_physician_pending_cases_date = dateFromPendingCases;
                Edit(appSetting);

                return 1;
            }
            return 0;
        }

        public int UpdatePhysicianCTAPendingCasesClearanceDate()
        {
            var appSetting = Get();
            if (appSetting != null)
            {
                var dateFromPendingCases = DateTime.Now.ToEST();
                appSetting.aps_clear_physician_cta_cases_date = dateFromPendingCases;
                Edit(appSetting);

                return 1;
            }
            return 0;
        }
    }
}
