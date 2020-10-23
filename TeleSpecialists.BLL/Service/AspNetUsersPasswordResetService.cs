using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AspNetUsersPasswordResetService : BaseService
    {
        public IQueryable<AspNetUsers_PasswordReset> GetUserLog(string userId)
        {
            var result = _unitOfWork.AspNetUsersPasswordResetRepository.Query().Where(x => x.AspNetUserId == userId).OrderByDescending(c => c.Id);
            return result;
        }        
        public AspNetUsers_PasswordReset GetUserDetails(string userId)
        {
            var model = _unitOfWork.AspNetUsersPasswordResetRepository.Query().Where(m => m.AspNetUserId == userId).OrderByDescending(c => c.Id);
            //var model = _unitOfWork.AspNetUsersPasswordResetRepository.Query().FirstOrDefault(m => m.AspNetUserId == userId);
            return model.FirstOrDefault();
        }
        public void DeletePasswordResetUser(string userId)
        {
            _unitOfWork.AspNetUsersPasswordResetRepository.DeleteRange(_unitOfWork.AspNetUsersPasswordResetRepository.Query().Where(m => m.AspNetUserId == userId));
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Create(AspNetUsers_PasswordReset entity)
        {
            _unitOfWork.AspNetUsersPasswordResetRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }        
        public void Edit(AspNetUsers_PasswordReset entity)
        {
            _unitOfWork.AspNetUsersPasswordResetRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
