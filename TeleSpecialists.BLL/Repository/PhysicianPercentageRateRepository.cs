using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPhysicianPercentageRateRepository : IGenericRepository<physician_percentage_rate> { }
    class PhysicianPercentageRateRepository : GenericRepository<physician_percentage_rate>, IPhysicianPercentageRateRepository
    {
        public PhysicianPercentageRateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
