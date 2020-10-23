using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseStatusRepository : IGenericRepository<case_status> { }

    public class CaseStatusRepository : GenericRepository<case_status>, ICaseStatusRepository
    {
        public CaseStatusRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
