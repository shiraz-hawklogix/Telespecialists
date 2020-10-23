using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFacilityPhysicianRepository : IGenericRepository<facility_physician> { }
    public class FacilityPhysicianRepository : GenericRepository<facility_physician>, IFacilityPhysicianRepository
    {
        public FacilityPhysicianRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
