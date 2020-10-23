using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.Reports
{
    public class PhysicianVolumetricViewModel
    {
        public string PhysicianName { get; set; }
        public bool cas_billing_physician_blast { get; set; }
        public Guid cas_fac_key { get; set; }
        public string cas_phy_key { get; set; }
        public int? cas_ctp_key { get; set; }
        public int? cas_billing_bic_key { get; set; }
        public DateTime? qeury_datetime { get; set; }
    }
}
