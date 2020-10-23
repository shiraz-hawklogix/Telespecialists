using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels
{
    public class MultipleCaseViewModel
    {
        [Required]
        public string cas_fac_key { get; set; }

       
        public string cas_phy_key { get; set; }

      
        public DateTime? cas_billing_date_of_consult { get; set; }

        public List<MultipeCaseData> Cases { get; set; }
        public bool cas_commnets_off { get; set; }

    }

    public class VisitType
    {
        public string ucd_key { get; set; }
        public string ucd_title { get; set; }
    }


    public class MultipeCaseData
    {
        public int Id { get; set; }
        [Required]
        public ucl_data CaseType { get; set; }
        public ucl_data BillingCode { get; set; }
        public DateTime? cas_billing_dob { get; set; }
        public ucl_data IdentificationType { get; set; }
        public VisitType VisitType { get; set; }
        public bool isMarkCompleted { get; set; }
        public string cas_patient_name { get; set; }
        public string cas_identification_number { get; set; }
        public string cas_billing_diagnosis { get; set; }
        public string comments { get; set; }



    }
}
