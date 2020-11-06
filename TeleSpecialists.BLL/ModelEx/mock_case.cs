using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TeleSpecialists.BLL.Extensions;


namespace TeleSpecialists.BLL.Model
{
    [MetadataType(typeof(mock_caseMetaData))]
    public partial class mock_case
    {
        public AspNetUser PhysicianUser
        {
            get
            {
                return this.AspNetUser2;
            }
            set
            {
                this.AspNetUser2 = value;
            }
        }

        public string FacilityTimeZone { get; set; }
        public Nullable<System.DateTime> cas_response_ts_notification_utc { get; set; }
        public string SelectedNIHSQuestionResponse { get; set; }
        public bool IsCaseCompleted { get; set; }
        public string VisibleTabs { get; set; }
        public string FromWaitingToAcceptToAcceptTime { get; set; }

        public string mcas_caller_source_key_title { get; set; }

        public string mcas_metric_stamp_time_formated { get; set; }

        public string mcas_identification_Title { get; set; }

        public mock_CaseInterval mCaseInterval { get; set; }

        public string mcas_cart_location_key_Title { get; set; }

    }

    public class mock_CaseInterval
    {
        private readonly DateTime? start_time;
        private readonly DateTime? stamp_time_est;
        private readonly DateTime? stamp_time;
        private readonly DateTime? login_time;
        private readonly DateTime? door_time_est;
        private readonly DateTime? door_time;
        private readonly DateTime? needle_time;
        private readonly DateTime? callback_time;
        //private readonly ICollection<mock_case_assign_history> case_Assign_History;
        //public mCaseInterval(mock_case cas)
        //{
        //    if (cas != null)
        //    {
        //        start_time = cas.mcas_response_ts_notification ?? null;
        //        stamp_time_est = cas.mcas_metric_stamp_time_est ?? null;
        //        stamp_time = cas.mcas_metric_stamp_time ?? null;
        //        login_time = cas.mcas_response_first_atempt ?? null;
        //        door_time_est = cas.mcas_metric_door_time_est ?? null;
        //        door_time = cas.mcas_metric_door_time ?? null;
        //        needle_time = cas.mcas_metric_needle_time_est ?? null;
        //        callback_time = cas.mcas_callback_response_time_est ?? null;
        //        case_Assign_History = cas.mcase_assign_history;
        //    }
        //    else
        //    {
        //        start_time = stamp_time_est = login_time = door_time_est = needle_time = null;
        //    }

        //}
        /// <summary>
        /// Get the most recent case assign history create date in Utc. for Cancelled and Completed Status
        /// </summary> 
        /// <returns>
        /// Case Assign History Created Date 'cah_created_date_utc', (DateTime?) Null if record not found. 
        /// </returns>
        //private DateTime? GetHistoryCreatedDate()
        //{
        //    string cancelledStatus = CaseStatus.Cancelled.ToDescription().ToLower();
        //    string completedStatus = CaseStatus.Complete.ToDescription().ToLower();

        //    var history = case_Assign_History.Where(m => m.cah_action.ToLower().Trim() == cancelledStatus || m.cah_action.ToLower().Trim() == completedStatus)
        //        .OrderByDescending(m => m.cah_key).FirstOrDefault();

        //    if (history != null)
        //    {
        //        return history.cah_created_date_utc;
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// Interval from 'cas_response_ts_notification' to 'cas_metric_stamp_time_est' 
        ///// </summary>
        //public string StartToStamp
        //{
        //    get
        //    {
        //        TimeSpan? interval = start_time.HasValue && stamp_time_est.HasValue ? (start_time.Value - stamp_time_est.Value) : new TimeSpan();
        //        var start_to_stamp = interval.FormatTimeSpan();

        //        return start_to_stamp;
        //    }
        //}

        ///// <summary>
        ///// Interval from 'cas_metric_stamp_time_est' to  'cas_response_first_atempt' 
        ///// </summary> 
        //public string StampToLogin
        //{
        //    get
        //    {
        //        TimeSpan? interval = stamp_time_est.HasValue && login_time.HasValue ? (login_time.Value - stamp_time_est.Value) : new TimeSpan();
        //        var stamp_to_login = interval.FormatTimeSpan();

        //        return stamp_to_login;
        //    }
        //}

        ///// <summary>
        ///// Total minutes from 'cas_metric_stamp_time_est' to  'cas_response_first_atempt' 
        ///// </summary>
        //public double? StampToLoginMinutes
        //{
        //    get
        //    {
        //        TimeSpan? interval = stamp_time_est.HasValue && login_time.HasValue ? (login_time.Value - stamp_time_est.Value) : new TimeSpan();
        //        var totalMinutes = interval?.TotalMinutes;

        //        return totalMinutes;
        //    }
        //}
        ///// <summary>
        ///// Total minutes from 'cas_response_ts_notification' to  'cas_callback_response_time_est' 
        ///// </summary>
        //public double? StartToCallbackMinutes
        //{
        //    get
        //    {
        //        TimeSpan? interval = start_time.HasValue && callback_time.HasValue ? (callback_time.Value - start_time.Value) : new TimeSpan();
        //        var totalMinutes = interval?.TotalMinutes;

        //        return totalMinutes;
        //    }
        //}
        ///// <summary>
        ///// Interval from 'cas_metric_door_time_est' to 'cas_metric_stamp_time_est'
        ///// </summary> 
        //public string ArrivalToStamp
        //{
        //    get
        //    {
        //        TimeSpan? interval = stamp_time_est.HasValue && door_time_est.HasValue ? (stamp_time_est.Value - door_time_est.Value) : new TimeSpan();
        //        var stamp_to_door = interval.FormatTimeSpan();
        //        return stamp_to_door;
        //    }
        //}

        ///// <summary>
        ///// Interval from 'cas_metric_door_time_est' to 'cas_metric_needle_time_est'
        ///// </summary>
        //public string ArrivalToNeedle
        //{

        //    get
        //    {
        //        TimeSpan? interval = needle_time.HasValue && door_time_est.HasValue ? (door_time_est.Value - needle_time.Value) : new TimeSpan();

        //        var door_to_needle = interval.FormatTimeSpan();

        //        return door_to_needle;
        //    }
        //}

        ///// <summary>
        ///// Interval from 'cas_response_ts_notification' to  'cas_response_first_atempt' 
        ///// </summary> 
        //public string StartToLogin
        //{
        //    get
        //    {
        //        TimeSpan? interval = start_time.HasValue && login_time.HasValue ? (login_time.Value - start_time.Value) : new TimeSpan();
        //        var start_to_login = interval.FormatTimeSpan();

        //        return start_to_login;
        //    }
        //}

        /// <summary>
        /// Identifying whether difference betweeen 'door_time_est' and  'needle_time' is greater then 60 minutes.
        /// </summary>
        public bool IsNeedleTimeTPA
        {
            get
            {
                if (door_time_est.HasValue && needle_time.HasValue)
                {
                    return Math.Abs((door_time_est.Value.Subtract(needle_time.Value)).TotalMinutes) > 45; //ticket 556,change time 60 to 45
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Calculate time elapsed from Stamp Time 'cas_metric_stamp_time' to Case Assign History Created Time in UtcNow
        /// </summary>
        //public string ElapsedFromStamp
        //{
        //    get
        //    {

        //        if (stamp_time != null)
        //        {
        //            var createdDateTime = GetHistoryCreatedDate();

        //            var elaspedtime = createdDateTime - stamp_time;

        //            return elaspedtime.FormatTimeSpan();
        //        }
        //        else
        //        {
        //            return "00:00:00";
        //        }

        //    }
        //}

        /// <summary>
        /// Calculate time elapsed from Arrival Time 'cas_metric_door_time' to Case Assign History Created Time in UtcNow
        /// </summary>
        //public string ElapsedFromArrival
        //{
        //    get
        //    {
        //        if (door_time != null)
        //        {
        //            var createdDateTime = GetHistoryCreatedDate();

        //            var elaspedtime = createdDateTime - door_time;

        //            return elaspedtime.FormatTimeSpan();
        //        }
        //        else
        //        {
        //            return "";
        //        }
        //    }
        //}
    }

    public class mock_caseMetaData
    {
        [Required]
        [Display(Name = "Case Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid case type, and try again.")]
        public int mcas_ctp_key { get; set; }

        [Required]
        [Display(Name = "Facility")]
        public int mcas_fac_key { get; set; }

        [MaxLength(50)]
        [Display(Name = "Callback Phone")]
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Invalid phone number. Please enter valid phone number.")]
        public string mcas_callback { get; set; }

        [Display(Name = "Cart")]
        public string mcas_cart { get; set; }

        [Display(Name = "Physician")]
        public string mcas_phy_key { get; set; }

        [Required]
        [Display(Name = "Status")]
        public int mcas_cst_key { get; set; }

        [Display(Name = "Notes")]
        public string mcas_notes { get; set; }

        [Display(Name = "Patient Name")]
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Use letters only in patient name field.")]
        [MaxLength(300)]
        public string mcas_patient { get; set; }
        //[Display(Name = "DOB")]
        //[DataType(DataType.Date)]
        //public Nullable<System.DateTime> mcas_billing_dob { get; set; }

        [Display(Name = "Last 4 of SSN")]
        //[MaxLength(4, ErrorMessage = "The maximum length of Last 4 of SSN is four digit.")] // commented as per Darcy - 2019-11-06, need to fix it later
        public string mcas_last_4_of_ssn { get; set; }

        // Case Metric
        //[Display(Name = "Last Known Well")]
        //public Nullable<System.DateTime> mcas_metric_lastwell_date_est { get; set; }
        //[Display(Name = "Arrival Time")]
        //public Nullable<System.DateTime> mcas_metric_door_time_est { get; set; }
        //[Display(Name = "Stamp Time")]
        //public Nullable<System.DateTime> mcas_metric_stamp_time { get; set; }
        //[Display(Name = "Time 1st Login")]
        //public Nullable<System.DateTime> mcas_metric_firstlogin_date { get; set; }

        //[Display(Name = "Symptoms")]
        //public string mcas_metric_symptoms { get; set; }

        ////[Display(Name = "NIHSS Start Assessment Time:")]
        ////public Nullable<System.DateTime> cas_metric_assesment_time { get; set; }

        //[Display(Name = "NIHSS Start Assessment Time:")]
        //public Nullable<System.DateTime> mcas_metric_assesment_time_est { get; set; }

        //[Display(Name = "tPA CPOE Order Time")]
        //public Nullable<System.DateTime> mcas_metric_pa_ordertime_est { get; set; }
        //[Display(Name = "Needle Time")]
        //public Nullable<System.DateTime> mcas_metric_needle_time_est { get; set; }
        //[Display(Name = "Consult End Time")]
        //public Nullable<System.DateTime> mcas_metric_consult_endtime { get; set; }

        //[Display(Name = "Video End Time")]
        //public Nullable<System.DateTime> mcas_metric_video_end_time_est { get; set; }
        //[Display(Name = "Documentation End Time")]
        //public Nullable<System.DateTime> mcas_metric_documentation_end_time { get; set; }

        //[Display(Name = "Login Delay Notes")]
        //public string mcas_metric_notes { get; set; }

        //[Display(Name = "Neuro Interventional Case")]
        //public bool mcas_metric_is_neuro_interventional { get; set; }
        // Case Billing
        //[MaxLength(300)]
        //public string mcas_billing_patient_name { get; set; }

        //[MaxLength(20)]
        //public string mcas_billing_mrn_fin { get; set; }

        [Display(Name = "Identification Type:")]
        public int mcas_identification_type { get; set; }

        [Display(Name = "Identification Number:")]
        public string mcas_identification_number { get; set; }

        [Display(Name = "Referring Physician:")]
        //[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Use letters only please")] // commented as per Darcy - 2019-11-06, need to fix it later
        public string mcas_referring_physician { get; set; }

        //[MaxLength(20)]
        //public string mcas_billing_visit_type { get; set; }

        //[Display(Name = "Date of Consult")]
        //public Nullable<System.DateTime> mcas_billing_date_of_consult { get; set; }

        //// Case Metric Response
        //[Display(Name = "Date of Consult:")]
        //public Nullable<System.DateTime> mcas_response_date_consult { get; set; }

        [Display(Name = "Reviewer(s)")]
        [MaxLength(300)]
        public string mcas_response_reviewer { get; set; }

        [Display(Name = "Physician")]
        public string mcas_response_phy_key { get; set; }

        [Display(Name = "Response Time")]
        public Nullable<System.DateTime> mcas_response_response_time { get; set; }

        [Display(Name = "Start Time")]
        public Nullable<System.DateTime> mcas_response_ts_notification { get; set; }

        //[Display(Name = "Zinc Notification")]
        //public Nullable<System.DateTime> mcas_response_zinc_notification { get; set; }

        [Display(Name = "Final Physician Acceptance Time")]
        public Nullable<System.DateTime> mcas_response_time_physician { get; set; }

        [Display(Name = "Time First Login Attempt")]
        public Nullable<System.DateTime> mcas_response_first_atempt { get; set; }

        //[Display(Name = "SA or STAT")]
        //[MaxLength(20)]
        //public string mcas_response_sa_stat { get; set; }

        [Display(Name = "Case Research")]
        public string mcas_response_case_research { get; set; }

        //[Display(Name = "Sa to >1 TS MD")]
        //public int mcas_response_sa_ts_md { get; set; }

        [Display(Name = "Technical Issues")]
        public int mcas_response_technical_issues { get; set; }

        [Display(Name = "Physician Concurrent Alerts")]
        public int mcas_physician_concurrent_alerts { get; set; }

        [Display(Name = "Physician to TS Accept >3")]
        public int mcas_response_nav_to_ts { get; set; }

        [Display(Name = "Miscommunication")]
        public int mcas_response_miscommunication { get; set; }

        [Display(Name = "Pulled from Rounding:")]
        public int mcas_response_pulled_rounding { get; set; }

        [Display(Name = "Inaccurate ETA")]
        public int mcas_response_inaccurate_eta { get; set; }

        [Display(Name = "Physician Blast @8\"?")]
        public int mcas_response_physician_blast { get; set; }

        [Display(Name = "Currently on RCA Tracker/ PI Projects")]
        public int mcas_response_rca_tracker { get; set; }

        [Display(Name = "RCA Initiated:")]
        public int mcas_response_review_initiated { get; set; }

        [Display(Name = "RCA Number:")]
        [MaxLength(50)]
        public string mcas_response_case_number { get; set; }

        //[Display(Name = "MRN Number:")]
        //public string mcas_mrn_number { get; set; }

        //[Display(Name = "Navigator Concurrent Alerts")]
        //public Nullable<int> mcas_navigator_concurrent_alerts { get; set; }

        [Display(Name = "tPA >60 Minutes")]
        public int mcas_response_tpa_to_minute { get; set; }

        [Display(Name = "Arrival Time to Needle Time >60 Minutes")]
        public int mcas_response_door_to_needle { get; set; }

        [Display(Name = "Workflow Type")]
        public string mcas_patient_type { get; set; }

       // [Display(Name = "tPA Candidate:")]
       // public string mcas_metric_tpa_consult { get; set; }

       // [Display(Name = "Video Start Time")]
       // public Nullable<System.DateTime> mcas_metric_video_start_time_est { get; set; }

       // [Display(Name = "Total Dose")]
       // public Nullable<decimal> mcas_metric_total_dose { get; set; }

       // [Display(Name = "Bolus")]
       // public Nullable<decimal> mcas_metric_bolus { get; set; }

       // [Display(Name = "Infusion")]
       // public Nullable<decimal> mcas_metric_infusion { get; set; }

       // [Display(Name = "Discard Quantity")]
       // public Nullable<decimal> mcas_metric_discard_quantity { get; set; }

       // metric new fields
       //[Display(Name = "Last Seen Normal Outside of 4.5 Hours")]
       // public bool mcas_metric_last_seen_normal { get; set; }

       // [Display(Name = "History of Hemorrhagic Complications or Intracranial Hemorrhage")]
       // public bool mcas_metric_has_hemorrhgic_history { get; set; }

       // [Display(Name = "Recent Anticoagulants")]
       // public bool mcas_metric_has_recent_anticoagulants { get; set; }

       // [Display(Name = "History of Recent Major Surgery")]
       // public bool mcas_metric_has_major_surgery_history { get; set; }

       // [Display(Name = "History of Recent Stroke")]
       // public bool mcas_metric_has_stroke_history { get; set; }
       // public Nullable<System.DateTime> mcas_metric_tpa_verbal_order_time { get; set; }

       // [Display(Name = "Alteplase Early Mix Decision Time:")]
       // public Nullable<System.DateTime> mcas_metric_tpa_verbal_order_time_est { get; set; }

       // [Display(Name = "Weight Noted by Staff")]
       // public Nullable<double> mcas_metric_weight { get; set; }

       // [Display(Name = "Unit")]
       // public string mcas_metric_weight_unit { get; set; }

       // [Display(Name = "Reason for tPA Delay:")]
       // public Nullable<int> mcas_metric_tpaDelay_key { get; set; }

       // [Display(Name = "Patient Is Not a Candidate for tPA Administration Because:")]
       // public Nullable<int> mcas_metric_non_tpa_reason_key { get; set; }

       // [Display(Name = "Discussed with Neurointerventionalist?")]
       // public bool mcas_metric_discussed_with_neurointerventionalist { get; set; }

       // [Display(Name = "Physician Blast")]
       // public bool mcas_billing_physician_blast { get; set; }
    }
}
