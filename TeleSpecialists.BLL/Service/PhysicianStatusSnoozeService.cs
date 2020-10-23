using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianStatusSnoozeService : BaseService
    {
        public void Create(physician_status_snooze entity)
        {

            _unitOfWork.PhysicianStatusSnoozeRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_status_snooze entity)
        {


            _unitOfWork.PhysicianStatusSnoozeRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public physician_status_snooze GetCurrentSnooze(int phs_key, string userId)
        {
            return _unitOfWork.PhysicianStatusSnoozeRepository.Query()
                                                               .FirstOrDefault(m => m.pss_phs_key == phs_key
                                                                      && m.pss_user_key == userId
                                                                      && m.pss_is_latest_snooze == true
                                                                      && m.pss_is_active
                                                                     );
        }
        public List<physician_status_snooze> GetUnProcessedRecords(string userId)
        {
            var list = _unitOfWork.PhysicianStatusSnoozeRepository.Query()
                                                                  .Where(m =>
                                                                   m.pss_user_key == userId
                                                                   && m.pss_processed_date == null
                                                                  ).ToList();

            return list;
        }
        public void UpdateExistingRecords(int phs_key, string userId)
        {
            var list = _unitOfWork.PhysicianStatusSnoozeRepository.Query()
                                                              .Where(m => m.pss_phs_key == phs_key
                                                                     && m.pss_user_key == userId
                                                                     && m.pss_is_latest_snooze == false
                                                                     && m.pss_processed_date == null
                                                                     && m.pss_is_active
                                                                    ).ToList().Where(m => m.pss_snooze_time != new System.TimeSpan(0, 0, 0, 0)).ToList();
            list.ForEach(m =>
            {
                m.pss_is_latest_snooze = false;
            });


            if (list.Count() > 0)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
        }
        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
