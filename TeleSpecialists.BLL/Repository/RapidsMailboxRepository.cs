using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IRapidsMailboxRepository : IGenericRepository<rapids_mailbox> { }
    class RapidsMailboxRepository : GenericRepository<rapids_mailbox>, IRapidsMailboxRepository
    {
        public RapidsMailboxRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
