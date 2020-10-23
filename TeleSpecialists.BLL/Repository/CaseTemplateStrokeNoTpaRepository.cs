using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseTemplateStrokeNoTpaRepository : IGenericRepository<case_template_stroke_notpa> { }
    public class CaseTemplateStrokeNoTpaRepository : GenericRepository<case_template_stroke_notpa>, ICaseTemplateStrokeNoTpaRepository
    {
        public CaseTemplateStrokeNoTpaRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
