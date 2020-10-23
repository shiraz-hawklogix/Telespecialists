using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IAuditRecordsRepository : IGenericRepository<audit_records> { }
    public class AuditRecordsRepository: GenericRepository<audit_records>, IAuditRecordsRepository
    {
        public AuditRecordsRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {

        }
    }
}
