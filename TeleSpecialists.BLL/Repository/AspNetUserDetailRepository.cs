using System;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAspNetUserDetailRepositorty : IGenericRepository<AspNetUser_Detail> { }

    public class AspNetUserDetailRepositorty : GenericRepository<AspNetUser_Detail>, IAspNetUserDetailRepositorty
    {
        public AspNetUserDetailRepositorty(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
