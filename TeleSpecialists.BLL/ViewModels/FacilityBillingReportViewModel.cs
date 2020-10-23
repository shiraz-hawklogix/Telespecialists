using System.Collections.Generic;

namespace TeleSpecialists.BLL.ViewModels
{
    public class FacilityBillingReportViewModel
    {
        public string Date { get; set; }
        public List<FacilityData> facilities { get; set; }

        public FacilityBillingReportViewModel()
        {
            facilities = new List<FacilityData>();
        }
        public class FacilityData
        {
            public string fac_name { get; set; }
            //public string fac_key { get; set; }
            public Dictionary<string, object> BillingData;
            public FacilityData()
            {
                BillingData = new Dictionary<string, object>();
            }
        }
    }
}
