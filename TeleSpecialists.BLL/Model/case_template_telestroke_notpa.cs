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
    
    public partial class case_template_telestroke_notpa
    {
        public int ctt_cas_key { get; set; }
        public string ctt_impression { get; set; }
        public string ctt_impression_text { get; set; }
        public string ctt_mechanism_stroke { get; set; }
        public string ctt_mechanism_stroke_text { get; set; }
        public string ctt_comment { get; set; }
        public bool ctt_routine_consultation { get; set; }
        public Nullable<int> ctt_nihss_totalscore { get; set; }
        public string ctt_sign_out { get; set; }
        public System.DateTime ctt_created_date { get; set; }
        public string ctt_created_by { get; set; }
        public string ctt_created_by_name { get; set; }
        public string ctt_modified_by { get; set; }
        public string ctt_modfied_by_name { get; set; }
        public Nullable<System.DateTime> ctt_modified_date { get; set; }
        public string ctt_antiplatelet_therapy_recommedned { get; set; }
        public string ctt_antiplatelet_therapy_recommedned_text { get; set; }
        public string ctt_vitals_bp { get; set; }
        public string ctt_vitals_pulse { get; set; }
        public string ctt_vitals_blood_glucose { get; set; }
        public Nullable<bool> ctt_patient_family_cosulted { get; set; }
        public Nullable<bool> ctt_critical_care_was_provided { get; set; }
        public Nullable<int> ctt_critical_care_minutes { get; set; }
        public string ctt_imaging { get; set; }
        public string ctt_therapies { get; set; }
        public string ctt_other_workup { get; set; }
        public string ctt_neuro_exam { get; set; }
        public string ctt_speech { get; set; }
        public string ctt_language { get; set; }
        public string ctt_face { get; set; }
        public string ctt_facial_sensation { get; set; }
        public string ctt_visual { get; set; }
        public string ctt_extraocular_movement { get; set; }
        public string ctt_motor { get; set; }
        public string ctt_sensation { get; set; }
        public string ctt_coordination { get; set; }
        public string ctt_consented_to_tele { get; set; }
        public string ctt_disposition { get; set; }
        public string ctt_nihss_or_neuro { get; set; }
        public string ctt_cheif_complaints { get; set; }
        public string ctt_exam_free_text { get; set; }
        public Nullable<bool> ctt_family_consent_available { get; set; }
        public string ctt_PMH { get; set; }
        public Nullable<bool> ctt_anticoagulant_use { get; set; }
        public string ctt_anticoagulant_use_text { get; set; }
        public Nullable<bool> ctt_antiplatelet_use { get; set; }
        public string ctt_antiplatelet_use_text { get; set; }
    
        public virtual @case @case { get; set; }
    }
}
