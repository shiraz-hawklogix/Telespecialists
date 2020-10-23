using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AlarmTuneService : BaseService
    {
        public alarm_tunes GetDetails(string id)
        {
            var model = _unitOfWork.AlarmTuneRepository.Query()
                                   .FirstOrDefault(m => m.alt_phy_key == id);
            return model;
        }
        public List<alarm_tunes> GetTuneList()
        {
            var model = _unitOfWork.AlarmTuneRepository.Query().ToList();
            return model;
        }
        public void Create(alarm_tunes entity)
        {
            _unitOfWork.AlarmTuneRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(alarm_tunes entity)
        {
            _unitOfWork.AlarmTuneRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public int GetMaxId()
        {
            int id = 0;
            try
            {
                var record = _unitOfWork.AlarmTuneRepository.Query().OrderByDescending(x => x.alt_selected_audio).FirstOrDefault();
                if (record != null)
                    id = Convert.ToInt16(record.alt_selected_audio);

            }
            catch
            {

            }            
            return id;
        }
        public bool isExistRecord(string filename)
        {
            bool status = false;
            var record = _unitOfWork.AlarmTuneRepository.Query().Where(x => x.alt_audio_path == filename).FirstOrDefault();
            if (record != null)
                status = true;
            return status;
        }
        public List<alarm_tunes> GetReocordWithOutSelectedAudio(List<alarm_tunes> alarm_Tunes_list)
        {
            alarm_Tunes_list.ForEach(x => x.alt_selected_audio = "");
            return alarm_Tunes_list;
        }
        public alarm_tunes Detail(int id)
        {
            var model = _unitOfWork.AlarmTuneRepository.Query()
                                   .FirstOrDefault(m => m.alt_key == id);
            return model;
        }
        public void Delete(int id)
        {
            _unitOfWork.AlarmTuneRepository.Delete(id);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

    }
}
