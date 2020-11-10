//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TeleSpecialists.BLL.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class mock_case
    {
        public int mcas_key { get; set; }
        public int mcas_ctp_key { get; set; }
        public string mcas_associate_id { get; set; }
        public System.Guid mcas_fac_key { get; set; }
        public string mcas_callback { get; set; }
        public string mcas_cart { get; set; }
        public string mcas_phy_key { get; set; }
        public int mcas_cst_key { get; set; }
        public string mcas_notes { get; set; }
        public string mcas_patient { get; set; }
        public Nullable<long> mcas_case_number { get; set; }
        public Nullable<System.DateTime> mcas_response_date_consult { get; set; }
        public string mcas_response_reviewer { get; set; }
        public string mcas_response_phy_key { get; set; }
        public Nullable<System.DateTime> mcas_response_response_time { get; set; }
        public Nullable<System.DateTime> mcas_response_ts_notification { get; set; }
        public Nullable<System.DateTime> mcas_response_time_physician { get; set; }
        public Nullable<System.DateTime> mcas_response_first_atempt { get; set; }
        public string mcas_response_case_research { get; set; }
        public int mcas_response_technical_issues { get; set; }
        public int mcas_physician_concurrent_alerts { get; set; }
        public int mcas_response_nav_to_ts { get; set; }
        public int mcas_response_miscommunication { get; set; }
        public int mcas_response_pulled_rounding { get; set; }
        public int mcas_response_inaccurate_eta { get; set; }
        public int mcas_response_physician_blast { get; set; }
        public int mcas_response_rca_tracker { get; set; }
        public int mcas_response_review_initiated { get; set; }
        public string mcas_response_case_number { get; set; }
        public int mcas_response_tpa_to_minute { get; set; }
        public int mcas_response_door_to_needle { get; set; }
        public Nullable<int> mcas_patient_type { get; set; }
        public string mcas_eta { get; set; }
        public Nullable<System.DateTime> mcas_status_assign_date { get; set; }
        public string mcas_ani { get; set; }
        public string mcas_dnis { get; set; }
        public Nullable<System.DateTime> mcas_time_stamp { get; set; }
        public string mcas_call_id { get; set; }
        public string mcas_callback_extension { get; set; }
        public string mcas_caller { get; set; }
        public bool mcas_is_active { get; set; }
        public string mcas_created_by { get; set; }
        public string mcas_created_by_name { get; set; }
        public System.DateTime mcas_created_date { get; set; }
        public string mcas_modified_by { get; set; }
        public string mcas_modified_by_name { get; set; }
        public Nullable<System.DateTime> mcas_modified_date { get; set; }
        public Nullable<int> mcas_identification_type { get; set; }
        public string mcas_identification_number { get; set; }
        public bool mcas_pulled_from_rounding { get; set; }
        public string mcas_last_4_of_ssn { get; set; }
        public string mcas_referring_physician { get; set; }
        public Nullable<System.DateTime> mcas_follow_up_date { get; set; }
        public Nullable<System.DateTime> mcas_billing_dob { get; set; }
        public Nullable<System.DateTime> mcas_physician_assign_date { get; set; }
        public Nullable<int> mcas_followup_case_key { get; set; }
        public string mcas_batch_id { get; set; }
        public bool mcas_is_flagged { get; set; }
        public Nullable<System.DateTime> mcas_phy_technical_issue_date_est { get; set; }
        public Nullable<System.DateTime> mcas_phy_technical_issue_date { get; set; }
        public bool mcas_phy_has_technical_issue { get; set; }
        public string mcas_triage_notes { get; set; }
        public Nullable<int> mcas_call_type { get; set; }
        public Nullable<System.DateTime> mcas_callback_response_time_est { get; set; }
        public Nullable<System.DateTime> mcas_callback_response_time { get; set; }
        public Nullable<int> mcas_caller_source_key { get; set; }
        public string mcas_caller_source_text { get; set; }
        public bool mcas_is_partial_update { get; set; }
        public bool mcas_is_flagged_dashboard { get; set; }
        public Nullable<int> mcas_cart_location_key { get; set; }
        public string mcas_cart_location_text { get; set; }
        public string mcas_work_flow_ids { get; set; }
        public string mcas_qps_assigned { get; set; }
        public string mcas_cancelled_type { get; set; }
        public string mcas_cancelled_text { get; set; }
        public string mcas_datetime_of_contact { get; set; }
        public string mcas_typeof_correspondence { get; set; }
        public string mcas_contact_comments { get; set; }
        public string mcas_callback_response_by { get; set; }
        public string mcas_callback_notes { get; set; }
        public string mcas_navigator_notes { get; set; }
        public string mcas_rejection_type { get; set; }
        public string mcas_rejection_text { get; set; }
        public bool mcas_is_flagged_physician { get; set; }
        public Nullable<bool> mcas_nurse_identify_members_in_room { get; set; }
        public Nullable<bool> mcas_nursing_verbalize_pre_CT_process { get; set; }
        public Nullable<bool> mcas_nursing_verbalize_post_CT_process { get; set; }
        public Nullable<bool> mcas_nursing_demonstrate_positioninig_cart { get; set; }
        public Nullable<bool> mcas_NIHSS_assessment_booklet_at_bedside { get; set; }
        public Nullable<bool> mcas_nurse_assisst_completion_NIHSS_exam { get; set; }
        public Nullable<bool> mcas_LOC_following_commands { get; set; }
        public Nullable<bool> mcas_visual_field_patient_looking_examiner_periphery { get; set; }
        public Nullable<bool> mcas_EOM_slow_motion { get; set; }
        public Nullable<bool> mcas_facial_symmetry { get; set; }
        public Nullable<bool> mcas_motor_drift_extensor_drift { get; set; }
        public Nullable<bool> mcas_coordination_stretch_finger { get; set; }
        public Nullable<bool> mcas_sensory_broken_Q_tip { get; set; }
        public Nullable<bool> mcas_sensory_taps_bare_skin { get; set; }
        public Nullable<bool> mcas_neglect_unilaterly_then_bilaterally { get; set; }
        public Nullable<bool> mcas_neglect_primary_sensation_visual_neglect { get; set; }
        public Nullable<bool> mcas_neglect_visual_neglect_exam { get; set; }
        public Nullable<bool> mcas_speech_exam_stroke_assessment_booklet { get; set; }
        public Nullable<bool> mcas_language_exam_repetition_words { get; set; }
        public Nullable<bool> mcas_nursing_verbalize_transfer_patient { get; set; }
        public Nullable<bool> mcas_connectivity_issue { get; set; }
        public Nullable<bool> mcas_is_nav_blast { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
        public virtual AspNetUser AspNetUser2 { get; set; }
        public virtual facility facility { get; set; }
    }
}
