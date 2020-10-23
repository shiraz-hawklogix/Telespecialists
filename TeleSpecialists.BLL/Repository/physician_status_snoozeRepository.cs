using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface Iphysician_status_snoozeRepository : IGenericRepository<physician_status_snooze>
    {
    }
    
    public class physician_status_snoozeRepository : GenericRepository<physician_status_snooze>, Iphysician_status_snoozeRepository
    {
        public physician_status_snoozeRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
