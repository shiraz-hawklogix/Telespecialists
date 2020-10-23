using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    
    public interface IPhysician_Case_TempRepository : IGenericRepository<physician_case_temp> { }

    public class Physician_Case_TempRepository : GenericRepository<physician_case_temp>, IPhysician_Case_TempRepository
    {
        public Physician_Case_TempRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }

}
