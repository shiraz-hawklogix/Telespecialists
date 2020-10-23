using System;
using System.Collections.Generic;

namespace TeleSpecialists.BLL.ViewModels
{
    public class PhysicianCaseListing
    {
        public List<PhysicianCTACase> CTACases { get; set; } = new List<PhysicianCTACase>();
        public List<PhysicianQueueCase> QueueCases { get; set; } = new List<PhysicianQueueCase>();
        public List<PhysicianFacility> FacilityList { get; set; } = new List<PhysicianFacility>();
    }

    public class PhysicianCTACase
    {
        public int CaseKey { get; set; }
        public DateTime CaseStartTime { get; set; }
        public string FacilityName { get; set; }
        public bool CTPObtained { get; set; }
        public bool CtaHeadCheckedObtained { get; set; }
        public bool IsReviewed { get; set; }
    }

    public class PhysicianQueueCase
    {
        public int CaseKey { get; set; }
        public DateTime CaseStartTime { get; set; }
        public string FacilityName { get; set; }
        public int CaseStatus { get; set; }
        public string PatientName { get; set; }
    }

    public class PhysicianFacility
    {
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; }
        public int fap_key { get; set; }
        public DateTime? fap_Credentials_confirmed_date { get; set; }
    }
}
