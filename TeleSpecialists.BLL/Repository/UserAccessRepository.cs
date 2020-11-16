using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IUserAccessData : IGenericRepository<user_access> { }
    public class UserAccessRepository : GenericRepository<user_access>, IUserAccessData
    {
        public UserAccessRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
 
}
