using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{

    public interface IEntityChangeLogRepository : IGenericRepository<entity_change_log> { }
    class EntityChangeLogRepository : GenericRepository<entity_change_log>, IEntityChangeLogRepository
    {
        public EntityChangeLogRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
