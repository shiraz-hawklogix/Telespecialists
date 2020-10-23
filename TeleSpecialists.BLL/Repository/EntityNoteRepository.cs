using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IEntityNoteRepository : IGenericRepository<entity_note> { }

    public class EntityNoteRepository : GenericRepository<entity_note>, IEntityNoteRepository
    {
        public EntityNoteRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
