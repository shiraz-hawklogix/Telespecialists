using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ITokenRepository : IGenericRepository<token>
    {
    }

    public class TokenRepository : GenericRepository<token>, ITokenRepository
    {
        public TokenRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}