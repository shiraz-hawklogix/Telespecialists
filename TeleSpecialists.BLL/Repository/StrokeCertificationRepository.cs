using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IStrokeCertificationRepository : IGenericRepository<stroke_certification> { }

    public class StrokeCertificationRepository : GenericRepository<stroke_certification>, IStrokeCertificationRepository
    {
        public StrokeCertificationRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
