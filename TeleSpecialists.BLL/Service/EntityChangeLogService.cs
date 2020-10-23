
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class EntityChangeLogService : BaseService
    {

        public void Create(entity_change_log log)
        {
            _unitOfWork.EntityChangeLogRepository.Insert(log);
            _unitOfWork.Save();          
        }
    }
}
