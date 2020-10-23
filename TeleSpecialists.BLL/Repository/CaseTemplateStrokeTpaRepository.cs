using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseTemplateStrokeTpaRepository : IGenericRepository<case_template_stroke_tpa> { }
    public class CaseTemplateStrokeTpaRepository : GenericRepository<case_template_stroke_tpa>, ICaseTemplateStrokeTpaRepository
    {
        public CaseTemplateStrokeTpaRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
