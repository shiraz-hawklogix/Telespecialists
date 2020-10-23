using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICallHistoryRepository : IGenericRepository<call_history> { }

    public class CallHistoryRepository : GenericRepository<call_history>, ICallHistoryRepository
    {
        public CallHistoryRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}