using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class StatusSnapshotcls
    {
        public int psl_key { get; set; }
        public string physician_name { get; set;}
        public string physician_status { get; set; }
        public DateTime psl_created_date { get; set; }  
    }
}
