using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IUCLRepository : IGenericRepository<ucl> { }
    public class UCLRepository : GenericRepository<ucl>, IUCLRepository
    {
        public UCLRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
