using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICaseTemplateTelestrokeNotpaRepository : IGenericRepository<case_template_telestroke_notpa> { }
    public class CaseTemplateTelestrokeNotpaRepository : GenericRepository<case_template_telestroke_notpa>, ICaseTemplateTelestrokeNotpaRepository
    {
        public CaseTemplateTelestrokeNotpaRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
