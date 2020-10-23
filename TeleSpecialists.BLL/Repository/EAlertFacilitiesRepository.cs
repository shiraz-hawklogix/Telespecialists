using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IEAlertFacilitiesRepository : IGenericRepository<ealert_user_facility> { }

    public class EAlertFacilitiesRepository : GenericRepository<ealert_user_facility>, IEAlertFacilitiesRepository
    {
        public EAlertFacilitiesRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
