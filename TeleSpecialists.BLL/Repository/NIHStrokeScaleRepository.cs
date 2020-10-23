using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface INIHStrokeScaleRepository : IGenericRepository<nih_stroke_scale> { }

    public class NIHStrokeScaleRepository : GenericRepository<nih_stroke_scale>, INIHStrokeScaleRepository
    {
        public NIHStrokeScaleRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
