using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class OperationOutlier
    {
        public long CaseNumber { get; set; }
        public string CaseType { get; set; }
        public string StartTime { get; set; }
        public string FacilityName { get; set; }
        public string Physician { get; set; }
        public string TS_Response_Time_or_CallBack_Response_Time { get; set; }
    }
}
