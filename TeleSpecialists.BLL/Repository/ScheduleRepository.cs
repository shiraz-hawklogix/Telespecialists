using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IScheduleRepository : IGenericRepository<user_schedule>
    {
    }

    public class ScheduleRepository : GenericRepository<user_schedule>, IScheduleRepository
    {
        public ScheduleRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}