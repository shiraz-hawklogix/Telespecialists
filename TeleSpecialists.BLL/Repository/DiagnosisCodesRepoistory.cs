using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Repository;

namespace TeleSpecialists.BLL
{
    public interface IDiagnosisCodesRepoistory : IGenericRepository<diagnosis_codes> { }
    public class DiagnosisCodesRepoistory : GenericRepository<diagnosis_codes>, IDiagnosisCodesRepoistory
    {
        public DiagnosisCodesRepoistory(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
