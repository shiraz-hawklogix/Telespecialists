using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPACGeneratedTemplateRepository : IGenericRepository<pac_case_template>
    {
    }
    public class PACGeneratedTemplateRepository:GenericRepository<pac_case_template>,IPACGeneratedTemplateRepository
    {
        public PACGeneratedTemplateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
