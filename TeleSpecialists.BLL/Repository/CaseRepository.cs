using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseRepository : IGenericRepository<@case>
    {  
    }

    public class CaseRepository : GenericRepository<@case>, ICaseRepository
    {
        public CaseRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}