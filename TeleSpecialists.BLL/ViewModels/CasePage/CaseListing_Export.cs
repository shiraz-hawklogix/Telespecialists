﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.CasePage
{
    public class CaseListing_Export
    {
        public Nullable<long> cas_case_number { get; set; }
        public System.DateTime cas_created_date { get; set; }
        public string CaseType { get; set; }
        public string FacityName { get; set; }
        public string fac_timezone { get; set; }
        public string AssignedPhysician { get; set; }
        public string Status { get; set; }
        public string Identification_Type { get; set; }
        public string cas_cart { get; set; }
        public string cas_callback { get; set; }
        public string cas_callback_extension { get; set; }
        public string cas_patient { get; set; }
        public Nullable<System.DateTime> cas_billing_dob { get; set; }
        public string cas_caller { get; set; }
        public Nullable<System.DateTime> cas_metric_door_time { get; set; }
        public string cas_identification_number { get; set; }
        public string cas_referring_physician { get; set; }
        public string cas_notes { get; set; }
        public string cas_eta { get; set; }
        public string cas_last_4_of_ssn { get; set; }
        public bool cas_pulled_from_rounding { get; set; }
        public bool cas_is_nav_blast { get; set; }
       public Nullable<System.DateTime> cas_metric_lastwell_date { get; set; }
       public bool cas_metric_is_lastwell_unknown { get; set; }
       public Nullable<System.DateTime> cas_metric_stamp_time { get; set; }
       public Nullable<int> cas_patient_type { get; set; }
       public Nullable<System.DateTime> cas_response_first_atempt { get; set; }
       public Nullable<System.DateTime> cas_metric_video_start_time { get; set; }
       public string cas_metric_symptoms { get; set; }
       public Nullable<System.DateTime> cas_metric_assesment_time { get; set; }
       public int cas_metric_last_seen_normal { get; set; }
       public int cas_metric_has_hemorrhgic_history { get; set; }
       public int cas_metric_has_recent_anticoagulants { get; set; }
       public int cas_metric_has_major_surgery_history { get; set; }
       public int cas_metric_has_stroke_history { get; set; }
       public Nullable<System.DateTime> cas_metric_tpa_verbal_order_time { get; set; }
       public bool cas_metric_tpa_consult { get; set; }
       public string Reason_for_Login_Delay { get; set; }
       public string cas_metric_notes { get; set; }
       
        public Nullable<System.DateTime> cas_metric_needle_time { get; set; }
        public Nullable<double> cas_metric_weight { get; set; }
        public string cas_metric_weight_unit { get; set; }
        public Nullable<decimal> cas_metric_total_dose { get; set; }
        public Nullable<decimal> cas_metric_bolus { get; set; }
        public Nullable<decimal> cas_metric_infusion { get; set; }
        public Nullable<decimal> cas_metric_discard_quantity { get; set; }
        public Nullable<System.DateTime> cas_metric_video_end_time { get; set; }
        public string Reason_for_tpa_Login_Delay { get; set; }
        public string non_tpa_reason { get; set; }
        public string cas_billing_tpa_delay_notes { get; set; }
        public bool cas_metric_ct_head_has_no_acture_hemorrhage { get; set; }
        public bool cas_metric_ct_head_is_reviewed { get; set; }
        public bool cas_metric_ct_head_is_not_reviewed { get; set; }
        public bool? cas_metric_is_neuro_interventional { get; set; }
        public bool? cas_metric_discussed_with_neurointerventionalist { get; set; }
        public bool? cas_metric_physician_notified_of_thrombolytics { get; set; }
        public bool? cas_metric_physician_recommented_consult_neurointerventionalist { get; set; }
        public string Billing_Code { get; set; }
        public Nullable<System.DateTime> cas_billing_date_of_consult { get; set; }
        public string cas_billing_diagnosis { get; set; }
        public string cas_billing_notes { get; set; }
        public string cas_billing_visit_type { get; set; }
        public Nullable<System.DateTime> cas_follow_up_date { get; set; }
        public bool cas_billing_physician_blast { get; set; }
        public Nullable<System.DateTime> cas_response_date_consult { get; set; }
        public Nullable<System.DateTime> cas_response_ts_notification { get; set; }
        public Nullable<System.DateTime> cas_response_time_physician { get; set; }
        public int cas_response_sa_ts_md { get; set; }
        public int cas_navigator_concurrent_alerts { get; set; }
        public int cas_physician_concurrent_alerts { get; set; }
        public int cas_response_miscommunication { get; set; }
        public int cas_response_technical_issues { get; set; }
        public int cas_response_tpa_to_minute { get; set; }
        public int cas_response_door_to_needle { get; set; }
        public Nullable<System.DateTime> cas_metric_stamp_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_needle_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_pa_ordertime_est { get; set; }
        public Nullable<System.DateTime> cas_metric_pa_ordertime { get; set; }
        public string cas_response_reviewer { get; set; }
        public string cas_response_case_research { get; set; }
        public int cas_response_nav_to_ts { get; set; }
        public int cas_response_pulled_rounding { get; set; }
        public Nullable<System.DateTime> cas_phy_technical_issue_date { get; set; }
        public int cas_response_physician_blast { get; set; }
        public int cas_response_review_initiated { get; set; }
        public string cas_response_case_number { get; set; }

    }
}
