using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAlarmRepository : IGenericRepository<alarm_setting> { }
    class AlarmSettingRepository : GenericRepository<alarm_setting>, IAlarmRepository
    {
        public AlarmSettingRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
