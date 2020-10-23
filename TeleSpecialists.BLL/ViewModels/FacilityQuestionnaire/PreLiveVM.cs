using System;
using System.Collections.Generic;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels.FacilityQuestionnaire
{
    public class PreLiveVM
    {
        public Guid FacilityKey { get; set; }
        public facility_questionnaire_pre_live questionnaireModel { get; set; }
        public facility_contract faclityContract { get; set; }
        public facility facilityModel { get; set; }
        public ucl_data  facilityState { get; set; }

        public ucl_data facilityHealthSystem { get; set; }
        public ucl_data bedSizeUCL { get; set; }
        public List<contactVM> contactList { get; set; }
        
    }

    public class contactVM
    {
        public int fqd_key { get; set; }
        public string fqd_name { get; set; }
        public int fqc_key { get; set; }
        public int fqc_fqd_key { get; set; }
        public string fqc_name { get; set; }
        public string fqc_phone { get; set; }
        public string fqc_email { get; set; }


    }
}
