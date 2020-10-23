using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseReviewTemplateService : BaseService
    {
        public case_review_template GetDetails(int cas_key)
        {
            // as per current requirement case will have only template for a specific type. 
            var model = _unitOfWork.CaseReviewTemplateRepository.Query()
                                   .FirstOrDefault(m => m.crt_cas_key == cas_key);
            return model;
        }
        public void Create(case_review_template entity)
        {
            _unitOfWork.CaseReviewTemplateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(case_review_template entity)
        {
            _unitOfWork.CaseReviewTemplateRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
