using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class Web2CampaignLogService : BaseService
    {
        public void Create(web2campaign_log entity)
        {
            _unitOfWork.Web2Campaign_LogRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public void Edit(web2campaign_log entity)
        {
            _unitOfWork.Web2Campaign_LogRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public web2campaign_log GetByCaseKey(int cas_key)
        {
            return _unitOfWork.Web2Campaign_LogRepository.Query().FirstOrDefault(m => m.wcl_cas_key == cas_key);
        }
    }
}
