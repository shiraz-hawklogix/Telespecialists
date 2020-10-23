using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IRateRepository : IGenericRepository<physician_rate> { }
    class RateRepository : GenericRepository<physician_rate>, IRateRepository
    {
        public RateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
