using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialist.CWH
{
    public class CaseViewModel
    {
        public Guid cas_fac_key { get; set; }
        public int cas_ctp_key { get; set; }
        public DateTime cas_response_ts_notification { get; set; }
    }
}
