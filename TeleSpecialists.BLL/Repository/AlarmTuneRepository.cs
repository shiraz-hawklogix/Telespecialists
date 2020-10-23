using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAlarmTuneRepository : IGenericRepository<alarm_tunes> { }
    class AlarmTuneRepository : GenericRepository<alarm_tunes>, IAlarmTuneRepository
    {
        public AlarmTuneRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {

        }
    }
}
