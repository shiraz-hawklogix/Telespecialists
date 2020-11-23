using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class IndirectCasesList
    {
        public int CasKey { get; set; }
        public long? CaseNumber { get; set; }
        public string Facility { get; set; }
        public string CallerSource { get; set; }
    }
}
