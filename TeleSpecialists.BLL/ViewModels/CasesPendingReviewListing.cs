using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels
{
    public class CasesPendingReviewListing
    {
        public List<CasesPendingReview> CasesPendingReview { get; set; } = new List<CasesPendingReview>();
    }

    public class CasesPendingReview
    {
        public int CaseKey { get; set; }
        public string FacilityName { get; set; }
        public string QPS_Name { get; set; }
        public long? TC_CaseNumber { get; set; }
        public string DateOfConsult { get; set; }
        public bool ColorRed { get; set; }
    }

    public class caseCalculcation
    {
        public string qps_number { get; set; }
        public int CaseKey { get; set; }
        public long? CaseNumber { get; set; }
        public string FacilityName { get; set; }
        public string billingdateofconsult { get; set; }
        public DateTime createddate { get; set; }
        public bool isNeedleTime { get; set; }
        public bool isNeedleTime45 { get; set; }
    }
    public class OperationsOutliers
    {
        public int CaseKey { get; set; }
        public long? CaseNumber { get; set; }
        public string CaseType { get; set; }
        public string StartTime { get; set; }
        public string FacilityName { get; set; }
        public string Physician_Initials { get; set; }
        public string TS_Response_Time { get; set; }
        public string CallBack_Response_Time { get; set; }
        public DateTime Created_Date { get; set; }
    }
    
}
