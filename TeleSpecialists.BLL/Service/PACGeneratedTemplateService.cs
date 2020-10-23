using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class PACGeneratedTemplateService : BaseService
    {
        public pac_case_template GetDetails(int pac_key)
        {
            var model = _unitOfWork.PACGeneratedTemplateRepository.Query()
                                   .FirstOrDefault(m => m.pct_pac_key == pac_key);
            return model;
        }
        public void Create(pac_case_template entity)
        {
            _unitOfWork.PACGeneratedTemplateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(pac_case_template entity)
        {
            _unitOfWork.PACGeneratedTemplateRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
