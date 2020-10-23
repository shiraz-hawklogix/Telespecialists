using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    
    public interface IFacilityQuestionnaireContactDesignationRepository : IGenericRepository<facility_questionnaire_contact_designation>
    {
    }
    public class FacilityQuestionnaireContactDesignationRepository : GenericRepository<facility_questionnaire_contact_designation>, IFacilityQuestionnaireContactDesignationRepository
    {
        public FacilityQuestionnaireContactDesignationRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
