using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface INIHStrokeScaleAnswerRepository : IGenericRepository<nih_stroke_scale_answer> { }
    public class NIHStrokeScaleAnswerRepository : GenericRepository<nih_stroke_scale_answer>, INIHStrokeScaleAnswerRepository
    {
        public NIHStrokeScaleAnswerRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
