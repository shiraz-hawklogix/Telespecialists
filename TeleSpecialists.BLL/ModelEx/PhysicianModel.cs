using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ModelEx
{
    public class PhysicianModel
    {
        public System.Guid fap_fac_key { get; set; }
        public string fap_user_key { get; set; }
        public bool fap_is_on_boarded { get; set; }
        public int fap_key { get; set; }
        public System.DateTime fap_created_date { get; set; }
        public Nullable<System.DateTime> fap_end_date { get; set; }
        public Nullable<System.DateTime> fap_start_date { get; set; }

        public Nullable<System.DateTime> fap_onboarded_date { get; set; }

    }
}
