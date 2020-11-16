using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IMockCaseRepository : IGenericRepository<mock_case>
    {
    }

    public class MockCaseRepository : GenericRepository<mock_case>, IMockCaseRepository
    {
        public MockCaseRepository(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
