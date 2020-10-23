using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseGeneratedTemplateService : BaseService
    {
        public case_generated_template GetDetails(int cas_key, int entityKey)
        {
            // as per current requirement case will have only template for a specific type. 
            var model = _unitOfWork.CaseGeneratedTemplateRepository.Query()
                                   .FirstOrDefault(m => m.cgt_cas_key == cas_key && m.cgt_ent_key == entityKey);
            return model;
        }
        public void Create(case_generated_template entity)
        {
            _unitOfWork.CaseGeneratedTemplateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(case_generated_template entity)
        {
            _unitOfWork.CaseGeneratedTemplateRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
