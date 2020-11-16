using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels.Dispatch
{
    public class DispatchListing
    {
        public int cas_key { get; set; }
        public DateTime cas_created_date { get; set; }
        public string cas_status_assign_date { get; set; }
        public string cas_cart { get; set; }
        public string phy_name { get; set; }
        public int cas_cst_key { get; set; }
        public long? cas_case_number { get; set; }
        public string cas_response_ts_notification { get; set; }
        public string cas_metric_stamp_time_est { get; set; }
        public string FacilityTimeZone { get; set; }
        public string date_of_consult { get; set; }
        
        public string ctp_name { get; set; }
        public string fac_name { get; set; }
        public string cas_patient { get; set; }  
        public string cst_name { get; set; }
        public string Navigator { get; set; }
        public string cas_billing_visit_type { get; set; }
        public string cas_callback { get; set; }
        public int TotalRecords { get; set; } // Property to set Total Records

        public Guid cas_fac_key { get; set; }
        public int cas_ctp_key { get; set; }
        public string cas_phy_key { get; set; }

        public string dateTimeElapsed { get; set; }
        public string timeElapsed { get; set; }
        public string crr_reason { get; set; }

        public Guid physician_key { get; set; }
        public string physician_fullname { get; set; }

        public string CombinedMessage { get; set; }

        public string cas_notes { get; set; }
        public string cas_triage_notes { get; set; }
        public string cas_callback_extension { get; set; }
        public string cas_eta { get; set; }
        


        public List<SelectListItem> PhysicianDD { get; set; }
        public List<SelectListItem> CaseStatusDD { get; set; }
        public List<SelectListItem> ReasonsDD { get; set; }

        public DateTime? case_assign_history_waitingToAccept { get; set; }
        public DateTime? case_assign_history_accepted { get; set; }

        #region without kendo

        //public @case caseModel { get; set; }

        //public List<SelectListItem> CaseTypeDD { get; set; }
        //public List<SelectListItem> PhysicianDD { get; set; }
        //public List<SelectListItem> CaseStatusDD { get; set; }

        //public int cas_key { get; set; }
        //public int cas_ctp_key { get; set; }
        ////public string CaseType { get; set; }
        //public string cas_cart { get; set; }
        //public DateTime cas_status_assign_date { get; set; }
        //public string cas_phy_key { get; set; }
        //public string PhysicianName{ get; set; }
        //public Guid fac_key { get; set; }
        //public string fac_name { get; set; }
        //public int cas_cst_key { get; set; }
        ////public string CaseStatus { get; set; }

        //public Nullable<DateTime> FromWaitingToAcceptToAcceptTime { get; set; }

        #endregion 
    }
}
