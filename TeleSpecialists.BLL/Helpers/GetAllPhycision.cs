using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
  public class GetAllPhycision
    {
        public int fap_key { get; set; }
        public System.Guid fap_fac_key { get; set; }
        public string fap_user_key { get; set; }
        public bool fap_is_on_boarded { get; set; }
        public string Physician_Name { get; set; }
    }
}
