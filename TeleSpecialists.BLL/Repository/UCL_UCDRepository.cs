using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IUCL_UCDRepository : IGenericRepository<ucl_data> { }
    public class UCL_UCDRepository : GenericRepository<ucl_data>, IUCL_UCDRepository
    {
        public UCL_UCDRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
