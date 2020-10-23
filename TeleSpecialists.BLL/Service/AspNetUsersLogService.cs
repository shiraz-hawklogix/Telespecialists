using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AspNetUsersLogService : BaseService
    {
        public IQueryable<AspNetUsers_Log> GetUserLog(string userId, int take)
        {
            var result = _unitOfWork.AspNetUsersLogRepository.Query().Where(x => x.AspNetUsersId == userId).OrderByDescending(c => c.Id).Take(take);
            return result;
        }
        public void Create(AspNetUsers_Log entity)
        {
            _unitOfWork.AspNetUsersLogRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }       
        public void Edit(AspNetUsers_Log entity)
        {
            _unitOfWork.AspNetUsersLogRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
