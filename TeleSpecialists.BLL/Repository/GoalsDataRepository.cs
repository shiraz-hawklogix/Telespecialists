using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IGoalsDataRepository : IGenericRepository<goals_data> { }

    public class GoalsDataRepository : GenericRepository<goals_data>, IGoalsDataRepository
    {
        public GoalsDataRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
