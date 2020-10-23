using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAspNetUsersLogRepository : IGenericRepository<AspNetUsers_Log> { }

    public class AspNetUsersLogRepository : GenericRepository<AspNetUsers_Log>, IAspNetUsersLogRepository
    {
        public AspNetUsersLogRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}