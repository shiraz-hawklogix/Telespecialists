using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{  
    public interface IFacilityContractServiceRepository : IGenericRepository<facility_contract_service> { }

    public class FacilityContractServiceRepository : GenericRepository<facility_contract_service>, IFacilityContractServiceRepository
    {
        public FacilityContractServiceRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
