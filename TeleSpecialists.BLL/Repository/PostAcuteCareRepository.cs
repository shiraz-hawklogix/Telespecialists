using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPostAcuteCareRepository : IGenericRepository<post_acute_care>
    {
    }


    public class PostAcuteCareRepository : GenericRepository<post_acute_care>, IPostAcuteCareRepository
    {
        public PostAcuteCareRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
