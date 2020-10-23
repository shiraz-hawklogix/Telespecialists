using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPhysicianLicenseRepository : IGenericRepository<physician_license> { }

    public class PhysicianLicenseRepository : GenericRepository<physician_license>, IPhysicianLicenseRepository
    {
        public PhysicianLicenseRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
