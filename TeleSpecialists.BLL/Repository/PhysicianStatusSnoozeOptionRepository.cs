using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{

    public interface IPhysicianStatusSnoozeOptionRepository : IGenericRepository<physician_status_snooze_option>
    {
    }

    public class PhysicianStatusSnoozeOptionRepository : GenericRepository<physician_status_snooze_option>, IPhysicianStatusSnoozeOptionRepository
    {
        public PhysicianStatusSnoozeOptionRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
