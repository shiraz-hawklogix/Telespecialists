using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Common.Extensions;

namespace TeleSpecialists.CA.eAlert.Model
{
    public class eAlertUpdateViewModel
    {
        public string case_key { get; set; }
        public string error_description { get; set; }
        public DateTime? reprocessed_date { get; set; } = DateTime.Now.ToEST();
        public string error_code { get; set; } = "TC-Error";
        public string raw_result { get; set; }
        public DateTime? error_email_date { get; set; }
        public int wcl_request_retry_count { get; set; }

    }
}
