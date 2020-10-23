using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPhysicianHolidayRateRepository : IGenericRepository<physician_holiday_rate> { }
    class PhysicianHolidayRateRepository : GenericRepository<physician_holiday_rate>, IPhysicianHolidayRateRepository
    {
        public PhysicianHolidayRateRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
