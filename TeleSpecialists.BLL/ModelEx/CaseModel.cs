using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ModelEx
{
    public class CaseModel
    {
        public System.Guid cas_fac_key { get; set; }
        public int cas_ctp_key { get; set; }
        public double RowCountsForDate { get; set; }
        public int RowCounts { get; set; }
        public int RowCounts2 { get; set; }
        public Nullable<int> cas_billing_bic_key { get; set; }
        public Nullable<int> cas_patient_type { get; set; }
        public Nullable<System.DateTime> cas_response_ts_notification { get; set; }

        public int cwh_key { get; set; }
        public Nullable<System.Guid> cwh_fac_id { get; set; }
        public string cwh_fac_name { get; set; }
        public Nullable<double> cwh_totalcwh { get; set; }
        public Nullable<double> cwh_month_wise_cwh { get; set; }
        public Nullable<System.DateTime> cwh_date { get; set; }
        public Nullable<System.DateTime> cas_metric_video_start_time { get; set; }
        public Nullable<System.DateTime> cas_metric_video_end_time { get; set; } 
    }

    public class BCIViewModel
    {
        public Nullable<System.DateTime> cas_metric_video_start_time { get; set; }
        public Nullable<System.DateTime> cas_metric_video_end_time { get; set; }
        public System.Guid cas_fac_key { get; set; }

    }

}