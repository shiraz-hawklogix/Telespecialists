using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IContactRepository : IGenericRepository<contact> { }
    public class ContactRepository : GenericRepository<contact>, IContactRepository
    {
        public ContactRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
