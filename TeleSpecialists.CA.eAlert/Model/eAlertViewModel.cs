using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA.eAlert.Model
{
    public class eAlertViewModel
    {
        public string case_number { get; set; }
        public string case_key { get; set; }
        public string case_type { get; set; }
        public string facility_name { get; set; }
        public string callback_number { get; set; }
        public string callback_extension { get; set; }
        public string cart { get; set; }
        public string error_description { get; set; }
        public DateTime? reprocessed_Date { get; set; }
        public int wcl_request_retry_count { get; set; }

    }
}
