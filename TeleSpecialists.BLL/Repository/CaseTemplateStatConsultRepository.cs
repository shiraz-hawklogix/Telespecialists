using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseTemplateStatConsultRepository : IGenericRepository<case_template_statconsult> { }

   public  class CaseTemplateStatConsultRepository : GenericRepository<case_template_statconsult>, ICaseTemplateStatConsultRepository
    {
        public CaseTemplateStatConsultRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {

        }
    }
}
