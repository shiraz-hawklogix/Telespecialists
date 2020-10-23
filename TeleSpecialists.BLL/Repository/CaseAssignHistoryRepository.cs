using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseAssignHistoryRepository : IGenericRepository<case_assign_history> { }
    public class CaseAssignHistoryRepository : GenericRepository<case_assign_history>, ICaseAssignHistoryRepository
    {
        public CaseAssignHistoryRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
