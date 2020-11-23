using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.Helpers
{
   public class CCIReport
    {
        public string Phy_Id { get; set; }
        public string Phy_Name { get; set; }
        public decimal H_L_S { get; set; }
        public decimal PWF_M_HLS  { get; set; }
        public string Fac_Id { get; set; }
        public string Fac_Name { get; set; }
        public string P_V { get; set; }
        public string H_V { get; set; }
        public decimal P_W_F { get; set; }
        public string CCI { get; set; }

    }
}
