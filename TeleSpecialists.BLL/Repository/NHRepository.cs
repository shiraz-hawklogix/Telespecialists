using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface INHRepository : IGenericRepository<user_schedule_nhalert>
    {
    }
    public class NHRepository : GenericRepository<user_schedule_nhalert>, INHRepository
    {
        public NHRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
