﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.Models
{
    #region ----- Case Models -----

    public class Cases
    {
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public string MDStaffReferenceSourceID { get; set; }
        public string MDStaffReferenceSourceName { get; set; }
        public int StrokeAlerts { get; set; }
    }

    public class CaseDbModel
    {
        public int cas_key { get; set; }
        public DateTime cas_created_date { get; set; }
        public string phy_name { get; set; }
        public string cas_phy_key { get; set; }
        public int cas_cst_key { get; set; }
        public string cst_name { get; set; }
        public long cas_case_number { get; set; }
        public int cas_ctp_key { get; set; }
        public string ctp_name { get; set; }
        
        public Guid cas_fac_key { get; set; }
        public string fac_name { get; set; }
        public int totalRecords { get; set; }
    }

    public class CaseDetailDbModel
    {
        #region Properties
        public int cas_key { get; set; }
        public int cas_ctp_key { get; set; }
        public string cas_associate_id { get; set; }
        public System.Guid cas_fac_key { get; set; }
        public string cas_callback { get; set; }
        public string cas_cart { get; set; }
        public string cas_phy_key { get; set; }
        public int cas_cst_key { get; set; }
        public string cas_notes { get; set; }
        public string cas_patient { get; set; }
        public Nullable<long> cas_case_number { get; set; }
        public Nullable<System.DateTime> cas_metric_lastwell_date { get; set; }
        public Nullable<System.DateTime> cas_metric_door_time { get; set; }
        public Nullable<System.DateTime> cas_metric_stamp_time { get; set; }
        public Nullable<System.DateTime> cas_metric_firstlogin_date { get; set; }
        public Nullable<System.DateTime> cas_metric_assesment_time { get; set; }
        public Nullable<System.DateTime> cas_metric_pa_ordertime { get; set; }
        public Nullable<System.DateTime> cas_metric_needle_time { get; set; }
        public Nullable<System.DateTime> cas_metric_consult_endtime { get; set; }
        public Nullable<System.DateTime> cas_metric_lastwell_date_est { get; set; }
        public Nullable<System.DateTime> cas_metric_door_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_stamp_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_firstlogin_date_est { get; set; }
        public Nullable<System.DateTime> cas_metric_assesment_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_pa_ordertime_est { get; set; }
        public Nullable<System.DateTime> cas_metric_needle_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_consult_endtime_est { get; set; }
        public string cas_metric_notes { get; set; }
        public Nullable<System.DateTime> cas_billing_date_of_consult { get; set; }
        public string cas_billing_patient_name { get; set; }
        public string cas_billing_mrn_fin { get; set; }
        public Nullable<System.DateTime> cas_billing_dob { get; set; }
        public string cas_billing_visit_type { get; set; }
        public Nullable<int> cas_billing_lod_key { get; set; }
        public Nullable<int> cas_billing_bic_key { get; set; }
        public string cas_billing_diagnosis { get; set; }
        public string cas_billing_notes { get; set; }
        public string cas_billing_tpa_delay_notes { get; set; }
        public Nullable<System.DateTime> cas_response_date_consult { get; set; }
        public string cas_response_reviewer { get; set; }
        public string cas_response_phy_key { get; set; }
        public Nullable<System.DateTime> cas_response_response_time { get; set; }
        public Nullable<System.DateTime> cas_response_ts_notification { get; set; }
        public Nullable<System.DateTime> cas_response_zinc_notification { get; set; }
        public Nullable<System.DateTime> cas_response_time_physician { get; set; }
        public Nullable<System.DateTime> cas_response_first_atempt { get; set; }
        public string cas_response_sa_stat { get; set; }
        public string cas_response_case_research { get; set; }
        public int cas_response_sa_ts_md { get; set; }
        public int cas_response_technical_issues { get; set; }
        public int cas_physician_concurrent_alerts { get; set; }
        public int cas_response_nav_to_ts { get; set; }
        public int cas_response_miscommunication { get; set; }
        public int cas_response_pulled_rounding { get; set; }
        public int cas_response_inaccurate_eta { get; set; }
        public int cas_response_physician_blast { get; set; }
        public int cas_response_rca_tracker { get; set; }
        public int cas_response_review_initiated { get; set; }
        public string cas_response_case_number { get; set; }
        public Nullable<int> cas_billing_bic_key2 { get; set; }
        public Nullable<System.DateTime> cas_metric_video_end_time { get; set; }
        public Nullable<System.DateTime> cas_metric_documentation_end_time { get; set; }
        public Nullable<System.DateTime> cas_metric_video_end_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_documentation_end_time_est { get; set; }
        public Nullable<bool> cas_metric_is_neuro_interventional { get; set; }
        public string cas_mrn_number { get; set; }
        public Nullable<int> cas_navigator_concurrent_alerts { get; set; }
        public int cas_response_tpa_to_minute { get; set; }
        public int cas_response_door_to_needle { get; set; }
        public Nullable<int> cas_patient_type { get; set; }
        public string cas_eta { get; set; }
        public bool cas_metric_tpa_consult { get; set; }
        public Nullable<System.DateTime> cas_status_assign_date { get; set; }
        public Nullable<System.DateTime> cas_metric_video_start_time { get; set; }
        public Nullable<System.DateTime> cas_metric_video_start_time_est { get; set; }
        public string cas_ani { get; set; }
        public string cas_dnis { get; set; }
        public Nullable<System.DateTime> cas_time_stamp { get; set; }
        public string cas_campaign_id { get; set; }
        public string cas_call_id { get; set; }
        public string cas_callback_extension { get; set; }
        public string cas_history_physician_initial_cal { get; set; }
        public string cas_caller { get; set; }
        public bool cas_is_active { get; set; }
        public string cas_created_by { get; set; }
        public string cas_created_by_name { get; set; }
        public System.DateTime cas_created_date { get; set; }
        public string cas_modified_by { get; set; }
        public string cas_modified_by_name { get; set; }
        public Nullable<System.DateTime> cas_modified_date { get; set; }
        public string cas_metric_symptoms { get; set; }
        public Nullable<int> cas_identification_type { get; set; }
        public string cas_identification_number { get; set; }
        public int cas_metric_last_seen_normal { get; set; }
        public int cas_metric_has_hemorrhgic_history { get; set; }
        public int cas_metric_has_recent_anticoagulants { get; set; }
        public int cas_metric_has_major_surgery_history { get; set; }
        public int cas_metric_has_stroke_history { get; set; }
        public Nullable<System.DateTime> cas_metric_tpa_verbal_order_time { get; set; }
        public Nullable<System.DateTime> cas_metric_tpa_verbal_order_time_est { get; set; }
        public Nullable<double> cas_metric_weight { get; set; }
        public string cas_metric_weight_unit { get; set; }
        public Nullable<int> cas_metric_tpaDelay_key { get; set; }
        public Nullable<int> cas_metric_non_tpa_reason_key { get; set; }
        public string cas_metric_non_tpa_reason_text { get; set; }
        public bool cas_metric_ct_head_has_no_acture_hemorrhage { get; set; }
        public bool cas_metric_ct_head_is_reviewed { get; set; }
        public Nullable<bool> cas_metric_discussed_with_neurointerventionalist { get; set; }
        public Nullable<bool> cas_metric_physician_notified_of_thrombolytics { get; set; }
        public bool cas_metric_physician_recommented_consult_neurointerventionalist { get; set; }
        public bool cas_billing_physician_blast { get; set; }
        public bool cas_is_nav_blast { get; set; }
        public bool cas_pulled_from_rounding { get; set; }
        public string cas_five9_original_stamp_time { get; set; }
        public string cas_last_4_of_ssn { get; set; }
        public string cas_referring_physician { get; set; }
        public Nullable<System.DateTime> cas_follow_up_date { get; set; }
        public bool cas_metric_ct_head_is_not_reviewed { get; set; }
        public bool cas_metric_is_lastwell_unknown { get; set; }
        public Nullable<decimal> cas_metric_total_dose { get; set; }
        public Nullable<decimal> cas_metric_bolus { get; set; }
        public Nullable<decimal> cas_metric_infusion { get; set; }
        public Nullable<decimal> cas_metric_discard_quantity { get; set; }
        public Nullable<System.DateTime> cas_billing_physician_blast_date { get; set; }
        public Nullable<System.DateTime> cas_billing_physician_blast_date_est { get; set; }
        public Nullable<System.DateTime> cas_physician_assign_date { get; set; }
        public bool cas_is_ealert { get; set; }
        public string cas_metric_hpi { get; set; }
        public string cas_metric_patient_gender { get; set; }
        public Nullable<int> cas_followup_case_key { get; set; }
        public Nullable<bool> cas_metric_radiologist_callback_for_review_of_advance_imaging { get; set; }
        public string cas_metric_radiologist_callback_for_review_of_advance_imaging_notes { get; set; }
        public Nullable<System.DateTime> cas_metric_radiologist_callback_for_review_of_advance_imaging_date { get; set; }
        public string cas_metric_discussed_with_neurointerventionalist_notes { get; set; }
        public Nullable<System.DateTime> cas_metric_discussed_with_neurointerventionalist_date { get; set; }
        public string cas_metric_physician_notified_of_thrombolytics_notes { get; set; }
        public Nullable<System.DateTime> cas_metric_physician_notified_of_thrombolytics_date { get; set; }
        public string cas_batch_id { get; set; }
        public bool cas_is_flagged { get; set; }
        public bool cas_nihss_cannot_completed { get; set; }
        public string cas_metric_ct_head_reviewed_text { get; set; }
        public Nullable<System.DateTime> cas_phy_technical_issue_date_est { get; set; }
        public Nullable<System.DateTime> cas_phy_technical_issue_date { get; set; }
        public bool cas_phy_has_technical_issue { get; set; }
        public string cas_triage_notes { get; set; }
        public Nullable<int> cas_call_type { get; set; }
        public Nullable<System.DateTime> cas_callback_response_time_est { get; set; }
        public Nullable<System.DateTime> cas_callback_response_time { get; set; }
        public bool cas_metric_symptom_onset_during_ed_stay { get; set; }
        public Nullable<int> cas_caller_source_key { get; set; }
        public string cas_caller_source_text { get; set; }
        public bool cas_metric_in_cta_queue { get; set; }
        public bool cas_is_partial_update { get; set; }
        public bool cas_is_flagged_dashboard { get; set; }
        public Nullable<int> cas_cart_location_key { get; set; }
        public string cas_cart_location_text { get; set; }
        public Nullable<int> cas_metric_thrombectomy_medical_decision_making { get; set; }
        public bool cas_metric_advance_imaging_cta_head_and_neck { get; set; }
        public bool cas_metric_advance_imaging_ct_perfusion { get; set; }
        public bool cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir { get; set; }
        public bool cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion { get; set; }
        public bool cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus { get; set; }
        public string cas_billing_comments { get; set; }
        public string cas_work_flow_ids { get; set; }
        public Nullable<int> cas_qps_assigned { get; set; }
        public string cas_triage_arivalstarttodelay { get; set; }
        public string cas_triage_recognition { get; set; }
        public string cas_triage_strokealertrigger { get; set; }
        public string cas_triage_transportandrooming { get; set; }
        public string cas_ems_arivaltostarttimedelay { get; set; }
        public string cas_ems_poor_identification { get; set; }
        public string cas_inpatient_timefirstlogintonhssstartitme { get; set; }
        public string cas_inpatient_timefirstlogintovideostart { get; set; }
        public string cas_inpatient_arivaltoneedletime { get; set; }
        public string cas_inpatient_related_imaging { get; set; }
        public string cas_inpatient_unenhancedct { get; set; }
        public string cas_inpatient_telemedicineassessmentroom { get; set; }
        public string cas_inpatient_telemedicineasesmentinct { get; set; }
        public string cas_inpatient_bpmanagemntrelated { get; set; }
        public string cas_inpatient_workflowbeforemixing { get; set; }
        public string cas_inpatient_workflowaftermixing { get; set; }
        public string cas_ems_identification_occurred { get; set; }
        public string cas_inpatient_delaysrelated_imaging { get; set; }
        public string cas_inpatient_detection_hypertension { get; set; }
        public string cas_inpatient_poormanagement_hypertension { get; set; }
        public string cas_inpatient_tpaadministration_delays { get; set; }
        public string cas_inpatient_system { get; set; }
        public string cas_inpatient_physician_related { get; set; }
        public string cas_inpatient_centralizedpharmacy_delivery { get; set; }
        public string cas_inpatientdelays_mixing { get; set; }
        public string cas_rca_primarydetail { get; set; }
        public string cas_response_case_qps_assessment { get; set; }
        public Nullable<int> cas_response_case_qps_reviewed { get; set; }
        public Nullable<int> cas_response_case_facility_request_reviewed { get; set; }
        public string cas_history_physician_initial { get; set; }
        public Nullable<System.DateTime> cas_metric_symptom_onset_during_ed_stay_time_est { get; set; }
        public Nullable<System.DateTime> cas_metric_symptom_onset_during_ed_stay_time { get; set; }
        public Nullable<int> TemplateEntityType { get; set; }
        public string cas_operations_review { get; set; }
        public Nullable<System.DateTime> cas_template_deleted_date { get; set; }
        public Nullable<int> cas_operations_review_completed { get; set; }
        public string cas_review_facility_communication { get; set; }
        public string cas_review_internal_notes { get; set; }
        #endregion
    }

    public class CaseCustomDetail
    {
        #region Selected Properties
        public int cas_key { get; set; }
        public int cas_ctp_key { get; set; }
        //public string cas_typeName { get; set; }
        public System.Guid cas_fac_key { get; set; }
        public string fac_name { get; set; }
        public string cas_callback { get; set; }
        public string cas_cart { get; set; }
        public string cas_phy_key { get; set; }
        public int cas_cst_key { get; set; }
        public string cas_patient { get; set; }
        public Nullable<long> cas_case_number { get; set; }
        public string fac_emr { get; set; }
        public string PhysicianName { get; set; }
        public string cas_created_by_name { get; set; }
        public Nullable<System.DateTime> cas_billing_dob { get; set; }
        public string cas_metric_patient_gender { get; set; }
        public string cas_identification_type { get; set; }
        public string cas_identification_number { get; set; }
        public string fac_timezone { get; set; }
        public Nullable<System.DateTime> cas_response_ts_notification { get; set; }
        public Nullable<System.DateTime> cas_metric_door_time_est { get; set; }
        
        #endregion
    }

    public class RapidsDbModel
    {
        public int rpd_key { get; set; }
        public string rpd_uid { get; set; }
        public System.DateTime rpd_date { get; set; }
        public string rpd_from { get; set; }
        public string rpd_to { get; set; }
        public string rpd_subject { get; set; }
        public string rpd_body { get; set; }
        public int rpd_attachments { get; set; }
        public string rpd_attachment_html { get; set; }
        public string rpd_logs { get; set; }
        public bool rpd_is_read { get; set; }
        public string rpd_created_by { get; set; }
        public System.DateTime rpd_created_date { get; set; }
        public int totalRecords { get; set; }
    }

    public class RapidsEmails
    {
        public int Id { get; set; }
        public string user_id { get; set; }
        public System.DateTime date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int Attachements { get; set; }
        public string Attachement_html { get; set; }
        public string Logs { get; set; }
        public bool IsRead { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }

    public class RapidsEmailsResult
    {
        public List<RapidsEmails> cases { get; set; }
        public int totalRecords { get; set; }
        public RapidsEmailsResult()
        {
            cases = new List<RapidsEmails>();
        }
    }

    public class CasesByDay : Cases
    {
        public DateTime Date { get; set; }
    }

    public class CaseDetail
    {
        public DateTime Date { get; set; }
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public string MDStaffReferenceSourceID { get; set; }
        public string MDStaffReferenceSourceName { get; set; }
    }
    

    public class CaseByPhysicianResult
    {
        public List<CaseByPhysician> cases { get; set; }
        public int totalRecords { get; set; }
        public CaseByPhysicianResult()
        {
            cases = new List<CaseByPhysician>();
        }
    }
    public class CaseByPhysician
    {
        public int Id { get; set; }
        public DateTime  CreatedDate { get; set; }
        public long CaseNumber { get; set; }
        public KeyValType CaseType { get; set; }
        public KeyValType CaseStatus { get; set; }
        public KeyValType Facility { get; set; }
        public KeyValType Physician { get; set; }

    }

    public class KeyValType
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    #endregion

    #region ----- Consult Models -----

    public class Consult
    {
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public string MDStaffReferenceSourceID { get; set; }
        public string MDStaffReferenceSourceName { get; set; }
        public int FollowUps { get; set; }
        public int RoutineConsults { get; set; }
        public int Total { get; set; }
    }

    public class ConsultByDay : Consult
    {
        public DateTime Date { get; set; }
    }

    #endregion

    #region ----- Facility Models -----

    public class Teleneuro
    {
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public string MDStaffReferenceSourceID { get; set; }
        public string MDStaffReferenceSourceName { get; set; }
        public string Services { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
    }

    #endregion

    #region ----- Physician -----

    public class Schedule
    {
        public string NPI { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }


    public class Credential
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NPI { get; set; }
        public bool Active { get; set; }
        public string FacilityID { get; set; }
        public string FacilityName { get; set; }
        public string MDStaffReferenceSourceName { get; set; }
        public string MDStaffReferenceSourceID { get; set; }
        public DateTime? MDStaffStartDate { get; set; }
        public DateTime? MDStaffEndDate { get; set; }
        public bool Onboarded { get; set; }
        public bool Override { get; set; }
    }

    public class Licensing
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NPI { get; set; }
        public bool Active { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseType { get; set; }
        public string State { get; set; }
        public bool isValid { get; set; }
    }

    #endregion

    #region User Models
    public class UserModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }


        public string PhoneNumber { get; set; }


        public bool EnableFive9 { get; set; }
        public string MobilePhone { get; set; }
        public string NPINumber { get; set; }
        public string UserInitial { get; set; }
        public string Gender { get; set; }
        public string AddressBlock { get; set; }
        public bool IsActive { get; set; }
        public bool CaseReviewer { get; set; }
        public Nullable<int> status_key { get; set; }
        public Nullable<System.DateTime> status_change_date { get; set; }
        public int CredentialCount { get; set; }
        public double CredentialIndex { get; set; }
        public Nullable<System.DateTime> ContractDate { get; set; }
        public Nullable<int> status_change_cas_key { get; set; }
        public Nullable<System.DateTime> status_change_date_forAll { get; set; }
        public bool IsStrokeAlert { get; set; }
        public bool NHAlert { get; set; }
       
    }
    #endregion
}