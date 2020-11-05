using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IMenuAccessData : IGenericRepository<component_access> { }
    public class MenuAccessRepository : GenericRepository<component_access>, IMenuAccessData
    {
        public MenuAccessRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
