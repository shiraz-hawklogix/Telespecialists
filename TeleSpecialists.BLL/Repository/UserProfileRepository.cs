using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IUserProfileRepository : IGenericRepository<AspNetUser>
    { 
    }

    public class UserProfileRepository : GenericRepository<AspNetUser>, IUserProfileRepository 
    {
        public UserProfileRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}