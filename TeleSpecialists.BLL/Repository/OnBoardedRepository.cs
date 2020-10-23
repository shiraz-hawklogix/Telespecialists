

using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IOnBoardedRepository : IGenericRepository<Onboarded> { }
    class OnBoardedRepository : GenericRepository<Onboarded>, IOnBoardedRepository
    {
        public OnBoardedRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
