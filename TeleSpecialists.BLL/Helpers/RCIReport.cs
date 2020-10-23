using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
   public class RCIReport
    {
        public string Physicians_Id { get; set; }
        public string Physicians_name { get; set; }
        public string Physicians { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
        public string fac_Id { get; set; }
        public string January { get; set; }
        public string February { get; set; }
        public string March { get; set; }
        public string April { get; set; }
        public string May { get; set; }
        public string June { get; set; }
        public string July { get; set; }
        public string August { get; set; }
        public string September { get; set; }
        public string October { get; set; }
        public string November { get; set; }
        public string December { get; set; }
        public string Total_RHI { get; set; }
    }
}
