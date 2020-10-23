using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFacilityAvailabilityRateRepository : IGenericRepository<facility_availability_rate> { }
    class FacilityAvailabilityRateRepository : GenericRepository<facility_availability_rate>, IFacilityAvailabilityRateRepository
    {
        public FacilityAvailabilityRateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
