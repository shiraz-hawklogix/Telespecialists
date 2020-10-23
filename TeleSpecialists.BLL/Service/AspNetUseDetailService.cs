using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AspNetUsersDetailService : BaseService
    {
       
        public void Create(AspNetUser_Detail entity)
        {
            _unitOfWork.AspNetUserDetailRepositorty.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(AspNetUser_Detail entity)
        {
            _unitOfWork.AspNetUserDetailRepositorty.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
