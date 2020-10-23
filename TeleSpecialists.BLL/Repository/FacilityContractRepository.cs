using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFacilityContractRepository : IGenericRepository<facility_contract> { }

    public class FacilityContractRepository : GenericRepository<facility_contract>, IFacilityContractRepository
    {
        public FacilityContractRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
