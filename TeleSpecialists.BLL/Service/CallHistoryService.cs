using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CallhistoryService : BaseService
    {
        public void Create(call_history entity)
        {
            _unitOfWork.CallHistoryRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public IQueryable<call_history> GetCallInfo(string callId)
        {
            var result = _unitOfWork.CallHistoryRepository.Query().Where(x=>x.chi_call_id == callId);
            return result;
        }
        public void Edit(call_history entity)
        {
            _unitOfWork.CallHistoryRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
