using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
   
    public interface IFacilityQuestionnaireContactRespository : IGenericRepository<facility_questionnaire_contact>
    {
    }
    public class FacilityQuestionnaireContactRespository : GenericRepository<facility_questionnaire_contact>, IFacilityQuestionnaireContactRespository
    {
        public FacilityQuestionnaireContactRespository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
