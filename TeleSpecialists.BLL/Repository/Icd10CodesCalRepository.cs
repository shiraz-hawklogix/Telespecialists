using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IIcd10CodesCalRepository : IGenericRepository<icd10_billing_codes_calcualtor> { }
    public class Icd10CodesCalRepository : GenericRepository<icd10_billing_codes_calcualtor>, IIcd10CodesCalRepository
    {
        public Icd10CodesCalRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
