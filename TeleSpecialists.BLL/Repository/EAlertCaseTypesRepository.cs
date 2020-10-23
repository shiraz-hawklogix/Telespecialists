using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IEAlertCaseTypesRepository : IGenericRepository<ealert_user_case_type> { }

    public class EAlertCaseTypesRepository : GenericRepository<ealert_user_case_type>, IEAlertCaseTypesRepository
    {
        public EAlertCaseTypesRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
