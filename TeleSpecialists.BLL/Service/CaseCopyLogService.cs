using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseCopyLogService : BaseService
    {
        public void Create(case_copy_log entity)
        { 
            _unitOfWork.CaseCopyLogRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
