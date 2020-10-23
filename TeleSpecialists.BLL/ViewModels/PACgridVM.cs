using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class PACgridVM
    {
        public int? pac_key { get; set; }
        public int? pac_ctp_key { get; set; }
        public Guid pac_fac_key { get; set; }
        public string CaseStatus { get; set; }
        public string casetype { get; set; }
        public string facilityname { get; set; }
        public string PhysicianName { get; set; }
        public int? pac_cst_key { get; set; }
        public string pac_patient { get; set; }
        public string pac_patient_initials { get; set; }
        public string pac_callback { get; set; }
        public string pac_created_by_name { get; set; }
        public string pac_date_of_consult { get; set; }
        public string pac_date_of_completion { get; set; }
        public string pac_referring_physician { get; set; }
        public string pac_created_date { get; set; }
        public string pac_phy_key { get; set; } 
        public int? totalRecords { get; set; } // Property to set Total Records
    }
}
