using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFacilityRateRepository : IGenericRepository<facility_rate> { }
    class FacilityRateRepository : GenericRepository<facility_rate>, IFacilityRateRepository
    {
        public FacilityRateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
