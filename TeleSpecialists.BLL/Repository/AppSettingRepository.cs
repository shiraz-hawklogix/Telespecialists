using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAppSettingRepository : IGenericRepository<application_setting>
    { 
    }

    public class AppSettingRepository : GenericRepository<application_setting>, IAppSettingRepository
    {
        public AppSettingRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}