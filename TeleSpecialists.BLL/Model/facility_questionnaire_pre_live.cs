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
    
    public partial class facility_questionnaire_pre_live
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public facility_questionnaire_pre_live()
        {
            this.facility_questionnaire_contact = new HashSet<facility_questionnaire_contact>();
        }
    
        public System.Guid fqp_key { get; set; }
        public bool fqp_is_active { get; set; }
        public string fqp_created_by { get; set; }
        public string fqp_created_by_name { get; set; }
        public System.DateTime fqp_created_date { get; set; }
        public string fqp_modified_by { get; set; }
        public string fqp_modified_by_name { get; set; }
        public Nullable<System.DateTime> fqp_modified_date { get; set; }
        public Nullable<int> fqp_volume_ed_visits { get; set; }
        public Nullable<int> fqp_volume_go_live_annual_strokes { get; set; }
        public Nullable<int> fqp_volume_go_live_ip_neuro_per_day { get; set; }
        public Nullable<int> fqp_volume_go_live_EEG_per_month { get; set; }
        public string fqp_op_emergency_neuro { get; set; }
        public string fqp_op_general_neuro { get; set; }
        public string fqp_op_EEG { get; set; }
        public string fqp_op_EGG_equipment { get; set; }
        public string fqp_op_ct_cta_schedule { get; set; }
        public string fqp_op_mri_schedule { get; set; }
        public string fqp_op_radiology_coverage_for_stroke { get; set; }
        public string fqp_op_nir { get; set; }
        public string fqp_op_weighted_stretchers { get; set; }
        public string fqp_process_ed_stroke_desc { get; set; }
        public string fqp_process_stat_response { get; set; }
        public string fqp_process_timeframe_code_stroke { get; set; }
        public string fqp_process_admin_post_tpa_patients { get; set; }
        public bool fqp_process_rn_nihss_certified { get; set; }
        public string fqp_process_facility_transfer_patient_stroke { get; set; }
        public Nullable<int> fqp_bedsize { get; set; }
        public bool fqp_service_has_telestroke { get; set; }
        public bool fqp_service_has_teleneuro { get; set; }
        public bool fqp_service_has_stat_consult { get; set; }
        public bool fqp_service_has_eeg { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<facility_questionnaire_contact> facility_questionnaire_contact { get; set; }
        public virtual facility facility { get; set; }
    }
}
