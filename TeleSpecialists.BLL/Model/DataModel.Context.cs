﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class TeleSpecialistsContext : DbContext
    {
        public TeleSpecialistsContext()
            : base("name=TeleSpecialistsContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<alarm_setting> alarm_setting { get; set; }
        public virtual DbSet<alarm_tunes> alarm_tunes { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUser_Detail> AspNetUser_Detail { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers_Log> AspNetUsers_Log { get; set; }
        public virtual DbSet<AspNetUsers_PasswordReset> AspNetUsers_PasswordReset { get; set; }
        public virtual DbSet<call_history> call_history { get; set; }
        public virtual DbSet<case_assign_history> case_assign_history { get; set; }
        public virtual DbSet<case_copy_log> case_copy_log { get; set; }
        public virtual DbSet<case_generated_template> case_generated_template { get; set; }
        public virtual DbSet<case_review_template> case_review_template { get; set; }
        public virtual DbSet<case_template_stroke_neuro_tpa> case_template_stroke_neuro_tpa { get; set; }
        public virtual DbSet<case_template_stroke_notpa> case_template_stroke_notpa { get; set; }
        public virtual DbSet<case_template_stroke_tpa> case_template_stroke_tpa { get; set; }
        public virtual DbSet<case_timestamp> case_timestamp { get; set; }
        public virtual DbSet<contact> contacts { get; set; }
        public virtual DbSet<default_notification_tune> default_notification_tune { get; set; }
        public virtual DbSet<ealert_user_case_type> ealert_user_case_type { get; set; }
        public virtual DbSet<ealert_user_facility> ealert_user_facility { get; set; }
        public virtual DbSet<entity_change_log> entity_change_log { get; set; }
        public virtual DbSet<entity_note> entity_note { get; set; }
        public virtual DbSet<entity_type> entity_type { get; set; }
        public virtual DbSet<facility_contract> facility_contract { get; set; }
        public virtual DbSet<facility_contract_service> facility_contract_service { get; set; }
        public virtual DbSet<nih_stroke_scale> nih_stroke_scale { get; set; }
        public virtual DbSet<nih_stroke_scale_answer> nih_stroke_scale_answer { get; set; }
        public virtual DbSet<nih_stroke_scale_question> nih_stroke_scale_question { get; set; }
        public virtual DbSet<pac_case_template> pac_case_template { get; set; }
        public virtual DbSet<physician_case_temp> physician_case_temp { get; set; }
        public virtual DbSet<physician_license> physician_license { get; set; }
        public virtual DbSet<physician_percentage_rate> physician_percentage_rate { get; set; }
        public virtual DbSet<physician_rate> physician_rate { get; set; }
        public virtual DbSet<physician_shift_rate> physician_shift_rate { get; set; }
        public virtual DbSet<physician_status> physician_status { get; set; }
        public virtual DbSet<physician_status_log> physician_status_log { get; set; }
        public virtual DbSet<physician_status_snooze> physician_status_snooze { get; set; }
        public virtual DbSet<physician_status_snooze_option> physician_status_snooze_option { get; set; }
        public virtual DbSet<rca_counter_measure> rca_counter_measure { get; set; }
        public virtual DbSet<ucl> ucls { get; set; }
        public virtual DbSet<ucl_data> ucl_data { get; set; }
        public virtual DbSet<MDStaffFacility> MDStaffFacilities { get; set; }
        public virtual DbSet<post_acute_care> post_acute_care { get; set; }
        public virtual DbSet<audit_records> audit_records { get; set; }
        public virtual DbSet<case_template_telestroke_notpa> case_template_telestroke_notpa { get; set; }
        public virtual DbSet<physician_holiday_rate> physician_holiday_rate { get; set; }
        public virtual DbSet<user_schedule> user_schedule { get; set; }
        public virtual DbSet<rapids_mailbox> rapids_mailbox { get; set; }
        public virtual DbSet<goals_data> goals_data { get; set; }
        public virtual DbSet<quality_goals> quality_goals { get; set; }
        public virtual DbSet<case_template_statconsult> case_template_statconsult { get; set; }
        public virtual DbSet<case_cancelled_type> case_cancelled_type { get; set; }
        public virtual DbSet<facility_questionnaire_contact> facility_questionnaire_contact { get; set; }
        public virtual DbSet<facility_questionnaire_contact_designation> facility_questionnaire_contact_designation { get; set; }
        public virtual DbSet<facility_questionnaire_pre_live> facility_questionnaire_pre_live { get; set; }
        public virtual DbSet<facility_availability_rate> facility_availability_rate { get; set; }
        public virtual DbSet<facility_rate> facility_rate { get; set; }
        public virtual DbSet<user_schedule_sleep> user_schedule_sleep { get; set; }
        public virtual DbSet<user_schedule_nhalert> user_schedule_nhalert { get; set; }
        public virtual DbSet<@case> cases { get; set; }
        public virtual DbSet<facility> facilities { get; set; }
        public virtual DbSet<cwh_data> cwh_data { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<facility_physician> facility_physician { get; set; }
        public virtual DbSet<diagnosis_codes> diagnosis_codes { get; set; }
        public virtual DbSet<application_setting> application_setting { get; set; }
        public virtual DbSet<user_login_verify> user_login_verify { get; set; }
        public virtual DbSet<firebase_usersemail> firebase_usersemail { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<operationsoutliersTemp> operationsoutliersTemps { get; set; }
        public virtual DbSet<Hospital_Protocols> Hospital_Protocols { get; set; }
        public virtual DbSet<Onboarded> Onboardeds { get; set; }
        public virtual DbSet<web2campaign_log> web2campaign_log { get; set; }
        public virtual DbSet<component_access> component_access { get; set; }
        public virtual DbSet<component> components { get; set; }
        public virtual DbSet<token> tokens { get; set; }
        public virtual DbSet<premorbid_correspondnce> premorbid_correspondnce { get; set; }
        public virtual DbSet<Forcast_Data> Forcast_Data { get; set; }
        public virtual DbSet<mock_case> mock_case { get; set; }
        public virtual DbSet<telecare_counters> telecare_counters { get; set; }
    
        public virtual int usp_new_GetAllPhysiciansByFacility(Nullable<System.Guid> facilityKey, Nullable<int> caseType, Nullable<int> isTimeBetween7and12, Nullable<System.Guid> softSaveGuid)
        {
            var facilityKeyParameter = facilityKey.HasValue ?
                new ObjectParameter("FacilityKey", facilityKey) :
                new ObjectParameter("FacilityKey", typeof(System.Guid));
    
            var caseTypeParameter = caseType.HasValue ?
                new ObjectParameter("CaseType", caseType) :
                new ObjectParameter("CaseType", typeof(int));
    
            var isTimeBetween7and12Parameter = isTimeBetween7and12.HasValue ?
                new ObjectParameter("isTimeBetween7and12", isTimeBetween7and12) :
                new ObjectParameter("isTimeBetween7and12", typeof(int));
    
            var softSaveGuidParameter = softSaveGuid.HasValue ?
                new ObjectParameter("SoftSaveGuid", softSaveGuid) :
                new ObjectParameter("SoftSaveGuid", typeof(System.Guid));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("usp_new_GetAllPhysiciansByFacility", facilityKeyParameter, caseTypeParameter, isTimeBetween7and12Parameter, softSaveGuidParameter);
        }
    
        public virtual ObjectResult<usp_get_cwh_data_Result> usp_get_cwh_data(Nullable<System.DateTime> startDateForAll, Nullable<System.DateTime> edateForAll)
        {
            var startDateForAllParameter = startDateForAll.HasValue ?
                new ObjectParameter("StartDateForAll", startDateForAll) :
                new ObjectParameter("StartDateForAll", typeof(System.DateTime));
    
            var edateForAllParameter = edateForAll.HasValue ?
                new ObjectParameter("edateForAll", edateForAll) :
                new ObjectParameter("edateForAll", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<usp_get_cwh_data_Result>("usp_get_cwh_data", startDateForAllParameter, edateForAllParameter);
        }
    
        public virtual ObjectResult<UspGetCWHData_Result> UspGetCWHData(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetCWHData_Result>("UspGetCWHData", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<UspGetCWHData2_Result> UspGetCWHData2(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetCWHData2_Result>("UspGetCWHData2", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<UspGetCWHData3_Result> UspGetCWHData3(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetCWHData3_Result>("UspGetCWHData3", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<UspGetRCIData_Result> UspGetRCIData(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetRCIData_Result>("UspGetRCIData", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<sp_getMenuAccess_Result> sp_getMenuAccess(string id)
        {
            var idParameter = id != null ?
                new ObjectParameter("Id", id) :
                new ObjectParameter("Id", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_getMenuAccess_Result>("sp_getMenuAccess", idParameter);
        }
    
        public virtual ObjectResult<UspGetAllPhysicion_Result> UspGetAllPhysicion()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetAllPhysicion_Result>("UspGetAllPhysicion");
        }
    
        public virtual ObjectResult<UspGetCaseDataForBCI_Result> UspGetCaseDataForBCI(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetCaseDataForBCI_Result>("UspGetCaseDataForBCI", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<UspGetDailyVolimetircdata_Result> UspGetDailyVolimetircdata(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetDailyVolimetircdata_Result>("UspGetDailyVolimetircdata", startDateParameter, edateParameter);
        }
    
        public virtual ObjectResult<UspGetForecastData_Result> UspGetForecastData(Nullable<System.DateTime> startDate, Nullable<System.DateTime> edate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var edateParameter = edate.HasValue ?
                new ObjectParameter("edate", edate) :
                new ObjectParameter("edate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<UspGetForecastData_Result>("UspGetForecastData", startDateParameter, edateParameter);
        }
    }
}
