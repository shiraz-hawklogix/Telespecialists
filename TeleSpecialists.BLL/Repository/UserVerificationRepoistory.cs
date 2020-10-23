using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IUserVerificationRepoistory : IGenericRepository<user_login_verify> { }
    public class UserVerificationRepoistory : GenericRepository<user_login_verify>, IUserVerificationRepoistory
    {
        public UserVerificationRepoistory(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
