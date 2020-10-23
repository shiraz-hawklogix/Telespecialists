using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseTemplateStrokeNeuroTPARepository : IGenericRepository<case_template_stroke_neuro_tpa> { }
    public class CaseTemplateStrokeNeuroTPARepository : GenericRepository<case_template_stroke_neuro_tpa>, ICaseTemplateStrokeNeuroTPARepository
    {
        public CaseTemplateStrokeNeuroTPARepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
