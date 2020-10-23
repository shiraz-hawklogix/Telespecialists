using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFacilityRepository : IGenericRepository<facility> { }

    public class FacilityRepository : GenericRepository<facility>, IFacilityRepository
    {
        public FacilityRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
