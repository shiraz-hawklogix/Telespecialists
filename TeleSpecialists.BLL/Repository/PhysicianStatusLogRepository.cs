using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPhysicianStatusLogRepository : IGenericRepository<physician_status_log> { }
    class PhysicianStatusLogRepository : GenericRepository<physician_status_log>, IPhysicianStatusLogRepository
    {
        public PhysicianStatusLogRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
