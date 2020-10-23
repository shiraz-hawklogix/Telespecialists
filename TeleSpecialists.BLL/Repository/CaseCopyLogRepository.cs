using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseCopyLogRepository : IGenericRepository<case_copy_log> { }

    public class CaseCopyLogRepository : GenericRepository<case_copy_log>, ICaseCopyLogRepository
    {
        public CaseCopyLogRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
