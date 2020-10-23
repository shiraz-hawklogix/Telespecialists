using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IRootCauseRepository : IGenericRepository<rca_counter_measure>
    {
    }

    public class RootCauseRepository : GenericRepository<rca_counter_measure>, IRootCauseRepository
    {
        public RootCauseRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}