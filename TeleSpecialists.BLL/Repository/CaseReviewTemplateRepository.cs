using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseReviewTemplateRepository : IGenericRepository<case_review_template>
    {
    }

    public class CaseReviewTemplateRepository : GenericRepository<case_review_template>, ICaseReviewTemplateRepository
    {
        public CaseReviewTemplateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
