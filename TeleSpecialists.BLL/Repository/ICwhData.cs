using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface ICwhData : IGenericRepository<cwh_data> { }
    public class CWHRepository : GenericRepository<cwh_data>, ICwhData
    {
        public CWHRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
