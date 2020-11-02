using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IMenuData : IGenericRepository<component> { }
    public class MenuRepository : GenericRepository<component> , IMenuData
    {
        public MenuRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
