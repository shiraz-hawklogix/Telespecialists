using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IFireBaseUserMailRepository : IGenericRepository<firebase_usersemail> { }
    class FireBaseUserMailRepository : GenericRepository<firebase_usersemail>, IFireBaseUserMailRepository
    {
        public FireBaseUserMailRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
