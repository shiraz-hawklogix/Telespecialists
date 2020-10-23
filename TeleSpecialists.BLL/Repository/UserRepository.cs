using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    /// <summary>
    /// Please DO NOT use this class for user insertion
    /// It should only be used for data import from MD Staff - For Now
    /// </summary>
    public interface IUserRepository : IGenericRepository<AspNetUser>
    { 
    }

    public class UserRepository : GenericRepository<AspNetUser>, IUserRepository
    {
        public UserRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}