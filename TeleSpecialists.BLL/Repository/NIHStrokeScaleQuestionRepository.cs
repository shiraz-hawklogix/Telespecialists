using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface INIHStrokeScaleQuestionRepository : IGenericRepository<nih_stroke_scale_question> { }
   
    public class NIHStrokeScaleQuestionRepository : GenericRepository<nih_stroke_scale_question>, INIHStrokeScaleQuestionRepository
    {
        public NIHStrokeScaleQuestionRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
