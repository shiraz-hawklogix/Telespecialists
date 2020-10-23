using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICasCancelledTypeRepository : IGenericRepository<case_cancelled_type> { }
    class CasCancelledTypeRepository : GenericRepository<case_cancelled_type>, ICasCancelledTypeRepository
    {
        public CasCancelledTypeRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
