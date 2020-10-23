using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
    public class CWHReport 
    {
        public int StrokeAlert { get; set; }
        public string fac_name  { get; set; }
        public string Facilities { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
        public string fac_Id { get; set; }
        public double? January { get; set; }
        public double? February { get; set; }
        public double? March { get; set; }
        public double? April { get; set; }
        public double? May { get; set; }
        public double? June { get; set; }
        public double? July { get; set; }
        public double? August { get; set; }
        public double? September { get; set; }
        public double? October { get; set; }
        public double? November { get; set; }
        public double? December { get; set; }
        public string Total_CWH { get; set; }

    }
}
