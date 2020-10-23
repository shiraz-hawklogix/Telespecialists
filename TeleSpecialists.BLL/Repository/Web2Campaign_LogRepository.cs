using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{

    public interface IWeb2Campaign_LogRepository : IGenericRepository<web2campaign_log> { }
    class Web2Campaign_LogRepository : GenericRepository<web2campaign_log>, IWeb2Campaign_LogRepository
    {
        public Web2Campaign_LogRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }

}
