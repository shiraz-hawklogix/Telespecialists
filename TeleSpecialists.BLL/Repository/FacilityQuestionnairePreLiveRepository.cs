using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{

    public interface IFacilityQuestionnairePreLiveRepository : IGenericRepository<facility_questionnaire_pre_live>
    {
    }
    public class FacilityQuestionnairePreLiveRepository : GenericRepository<facility_questionnaire_pre_live>, IFacilityQuestionnairePreLiveRepository
    {
        public FacilityQuestionnairePreLiveRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
