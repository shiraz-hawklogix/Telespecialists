using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPhysicianRateRepository : IGenericRepository<physician_shift_rate> { }
    class PhysicianRateRepository : GenericRepository<physician_shift_rate>, IPhysicianRateRepository
    {
        public PhysicianRateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
