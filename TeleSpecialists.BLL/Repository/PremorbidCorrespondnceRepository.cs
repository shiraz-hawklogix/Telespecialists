using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IPremorbidCorrespondnceRepository : IGenericRepository<premorbid_correspondnce>
    {
    }

    public class PremorbidCorrespondnceRepository : GenericRepository<premorbid_correspondnce>, IPremorbidCorrespondnceRepository
    {
        public PremorbidCorrespondnceRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}