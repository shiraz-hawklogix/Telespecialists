using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseGeneratedTemplateRepository : IGenericRepository<case_generated_template>
    {
    }

    public class CaseGeneratedTemplateRepository : GenericRepository<case_generated_template>, ICaseGeneratedTemplateRepository
    {
        public CaseGeneratedTemplateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
