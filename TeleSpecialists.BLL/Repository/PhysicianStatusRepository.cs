using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
   public interface IPhysicianStatusRepository : IGenericRepository<physician_status> { }

    public class PhysicianStatusRepository : GenericRepository<physician_status>, IPhysicianStatusRepository
    {
        public PhysicianStatusRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
