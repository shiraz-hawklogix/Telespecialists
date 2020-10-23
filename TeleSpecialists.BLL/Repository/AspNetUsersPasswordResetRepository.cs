using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAspNetUsersPasswordResetRepository : IGenericRepository<AspNetUsers_PasswordReset> { }

    public class AspNetUsersPasswordResetRepository : GenericRepository<AspNetUsers_PasswordReset>, IAspNetUsersPasswordResetRepository
    {
        public AspNetUsersPasswordResetRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}