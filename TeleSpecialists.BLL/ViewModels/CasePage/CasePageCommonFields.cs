using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.CasePage
{
   public abstract class CasePageCommonFields
    {
        public int cas_key { get; set; }
        public string cas_status_assign_date { get; set; }
        public int cas_cst_key { get; set; }
        public long? cas_case_number { get; set; }
        public bool cas_is_flagged { get; set; }
        public bool cas_is_ealert { get; set; }
        public string cas_response_ts_notification { get; set; }
        public string cas_metric_stamp_time_est { get; set; }

        // Dashboard Page fields

        public string ctp_name { get; set; }
        public string fac_name { get; set; }
        public string cas_patient { get; set; }

        public string phy_name { get; set; }
        public string cst_name { get; set; }

        public string Navigator { get; set; }

        public int TotalRecords { get; set; } // Property to set Total Records

        public bool cas_is_flagged_physician { get; set; }
    }
}
