using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;
namespace TeleSpecialists.BLL.Service
{
  public  class UserProfileServices : BaseService
    {

        public void EditUserProfile(AspNetUser entity)
        {
            _unitOfWork.UserProfileRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
