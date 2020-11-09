using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPremorbidCorrespondnceRepository : IGenericRepository<premorbid_correspondnce>
    {
    }

    public class PremorbidCorrespondnceRepository : GenericRepository<premorbid_correspondnce>, IPremorbidCorrespondnceRepository
    {
        public PremorbidCorrespondnceRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}