using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IQualityGoalsRepository : IGenericRepository<quality_goals> { }

    public class QualityGoalsRepository : GenericRepository<quality_goals>, IQualityGoalsRepository
    {
        public QualityGoalsRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
