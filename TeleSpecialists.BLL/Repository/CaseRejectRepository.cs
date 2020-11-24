using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseRejectRepository : IGenericRepository<case_rejection_reason> { }
    class CaseRejectRepository : GenericRepository<case_rejection_reason>, ICaseRejectRepository
    {
        public CaseRejectRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}