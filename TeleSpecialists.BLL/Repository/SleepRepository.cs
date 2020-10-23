using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ISleepRepository : IGenericRepository<user_schedule_sleep>
    {
    }
    public  class SleepRepository : GenericRepository<user_schedule_sleep>, ISleepRepository
    {
        public SleepRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
