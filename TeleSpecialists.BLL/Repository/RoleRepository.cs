using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    /// <summary>
    /// Please DO NOT use this class for user insertion
    /// It should only be used for queering physicians
    /// </summary>
    public interface IRoleRepository : IGenericRepository<AspNetRole>
    {
    }

    public class RoleRepository : GenericRepository<AspNetRole>, IRoleRepository
    {
        public RoleRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
