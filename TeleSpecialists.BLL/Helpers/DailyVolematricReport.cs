using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class DailyVolematricReport
    {
        public string fac_name { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
        public string fac_Id { get; set; }


        public string Dates { get; set; }
       

    }
}
