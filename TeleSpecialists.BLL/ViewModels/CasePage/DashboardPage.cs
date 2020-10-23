using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.CasePage
{
   
    public class DashboardPage: CasePageCommonFields
    {
    
        public string callType { get; set; }
        public string callerSource { get; set; }

        public string ResponseTime { get; set; }
        public string TPACandidate { get; set; }
        public string StartToStamp { get; set; }
        public string StartToAccept { get; set; }
        public string CallBackResponseTime { get; set; }

    }
}
