using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    /// <summary>
    /// Please DO NOT use this class for user insertion
    /// It should only be used for queering physicians
    /// </summary>
    public interface IUserRoleRepository : IGenericRepository<AspNetUserRole>
    {
    }

    public class UserRoleRepository : GenericRepository<AspNetUserRole>, IUserRoleRepository
    {
        public UserRoleRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
