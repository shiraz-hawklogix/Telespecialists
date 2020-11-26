CREATE TABLE [dbo].[case] (
    [cas_key]                                                                              INT              IDENTITY (20000, 1) NOT NULL,
    [cas_ctp_key]                                                                          INT              NOT NULL,
    [cas_associate_id]                                                                     NVARCHAR (128)   NOT NULL,
    [cas_fac_key]                                                                          UNIQUEIDENTIFIER NOT NULL,
    [cas_callback]                                                                         VARCHAR (50)     NULL,
    [cas_cart]                                                                             NVARCHAR (MAX)   NULL,
    [cas_phy_key]                                                                          NVARCHAR (128)   NULL,
    [cas_cst_key]                                                                          INT              NOT NULL,
    [cas_notes]                                                                            VARCHAR (MAX)    NULL,
    [cas_patient]                                                                          VARCHAR (300)    NULL,
    [cas_case_number]                                                                      BIGINT           NULL,
    [cas_metric_lastwell_date]                                                             DATETIME         NULL,
    [cas_metric_door_time]                                                                 DATETIME         NULL,
    [cas_metric_stamp_time]                                                                DATETIME         NULL,
    [cas_metric_firstlogin_date]                                                           DATETIME         NULL,
    [cas_metric_assesment_time]                                                            DATETIME         NULL,
    [cas_metric_pa_ordertime]                                                              DATETIME         NULL,
    [cas_metric_needle_time]                                                               DATETIME         NULL,
    [cas_metric_consult_endtime]                                                           DATETIME         NULL,
    [cas_metric_lastwell_date_est]                                                         DATETIME         NULL,
    [cas_metric_door_time_est]                                                             DATETIME         NULL,
    [cas_metric_stamp_time_est]                                                            DATETIME         NULL,
    [cas_metric_firstlogin_date_est]                                                       DATETIME         NULL,
    [cas_metric_assesment_time_est]                                                        DATETIME         NULL,
    [cas_metric_pa_ordertime_est]                                                          DATETIME         NULL,
    [cas_metric_needle_time_est]                                                           DATETIME         NULL,
    [cas_metric_consult_endtime_est]                                                       DATETIME         NULL,
    [cas_metric_notes]                                                                     VARCHAR (MAX)    NULL,
    [cas_billing_date_of_consult]                                                          DATETIME         NULL,
    [cas_billing_patient_name]                                                             VARCHAR (300)    NULL,
    [cas_billing_mrn_fin]                                                                  NVARCHAR (20)    NULL,
    [cas_billing_dob]                                                                      DATETIME         NULL,
    [cas_billing_visit_type]                                                               VARCHAR (20)     NULL,
    [cas_billing_lod_key]                                                                  INT              NULL,
    [cas_billing_bic_key]                                                                  INT              NULL,
    [cas_billing_diagnosis]                                                                VARCHAR (MAX)    NULL,
    [cas_billing_notes]                                                                    VARCHAR (MAX)    NULL,
    [cas_billing_tpa_delay_notes]                                                          VARCHAR (MAX)    NULL,
    [cas_response_date_consult]                                                            DATETIME         NULL,
    [cas_response_reviewer]                                                                VARCHAR (300)    NULL,
    [cas_response_phy_key]                                                                 NVARCHAR (128)   NULL,
    [cas_response_response_time]                                                           DATETIME         NULL,
    [cas_response_ts_notification]                                                         DATETIME         NULL,
    [cas_response_zinc_notification]                                                       DATETIME         NULL,
    [cas_response_time_physician]                                                          DATETIME         NULL,
    [cas_response_first_atempt]                                                            DATETIME         NULL,
    [cas_response_sa_stat]                                                                 VARCHAR (20)     NULL,
    [cas_response_case_research]                                                           NVARCHAR (MAX)   NULL,
    [cas_response_sa_ts_md]                                                                INT              CONSTRAINT [DF_case_cas_response_sa_ts_md] DEFAULT ((2)) NOT NULL,
    [cas_response_technical_issues]                                                        INT              CONSTRAINT [DF_case_cas_response_technical_issues] DEFAULT ((2)) NOT NULL,
    [cas_physician_concurrent_alerts]                                                      INT              CONSTRAINT [DF_case_cas_response_concurrent_alerts] DEFAULT ((2)) NOT NULL,
    [cas_response_nav_to_ts]                                                               INT              CONSTRAINT [DF_case_cas_response_nav_to_ts] DEFAULT ((2)) NOT NULL,
    [cas_response_miscommunication]                                                        INT              CONSTRAINT [DF_case_cas_response_miscommunication] DEFAULT ((2)) NOT NULL,
    [cas_response_pulled_rounding]                                                         INT              CONSTRAINT [DF_case_cas_response_pulled_rounding] DEFAULT ((2)) NOT NULL,
    [cas_response_inaccurate_eta]                                                          INT              CONSTRAINT [DF_case_cas_response_inaccurate_eta] DEFAULT ((2)) NOT NULL,
    [cas_response_physician_blast]                                                         INT              CONSTRAINT [DF_case_cas_response_physician_blast] DEFAULT ((2)) NOT NULL,
    [cas_response_rca_tracker]                                                             INT              CONSTRAINT [DF_case_cas_response_rca_tracker] DEFAULT ((2)) NOT NULL,
    [cas_response_review_initiated]                                                        INT              CONSTRAINT [DF_case_cas_response_review_initiated] DEFAULT ((2)) NOT NULL,
    [cas_response_case_number]                                                             VARCHAR (50)     NULL,
    [cas_billing_bic_key2]                                                                 INT              NULL,
    [cas_metric_video_end_time]                                                            DATETIME         NULL,
    [cas_metric_documentation_end_time]                                                    DATETIME         NULL,
    [cas_metric_video_end_time_est]                                                        DATETIME         NULL,
    [cas_metric_documentation_end_time_est]                                                DATETIME         NULL,
    [cas_metric_is_neuro_interventional]                                                   BIT              NULL,
    [cas_mrn_number]                                                                       VARCHAR (250)    NULL,
    [cas_navigator_concurrent_alerts]                                                      INT              NULL,
    [cas_response_tpa_to_minute]                                                           INT              CONSTRAINT [DF__case__cas_respon__32816A03] DEFAULT ((3)) NOT NULL,
    [cas_response_door_to_needle]                                                          INT              CONSTRAINT [DF__case__cas_respon__33758E3C] DEFAULT ((3)) NOT NULL,
    [cas_patient_type]                                                                     INT              CONSTRAINT [DF__case__cas_patien__47477CBF] DEFAULT ((1)) NULL,
    [cas_eta]                                                                              VARCHAR (250)    NULL,
    [cas_metric_tpa_consult]                                                               BIT              CONSTRAINT [DF__case__cas_metric__52B92F6B] DEFAULT ((0)) NOT NULL,
    [cas_status_assign_date]                                                               DATETIME         NULL,
    [cas_metric_video_start_time]                                                          DATETIME         NULL,
    [cas_metric_video_start_time_est]                                                      DATETIME         NULL,
    [cas_ani]                                                                              VARCHAR (250)    NULL,
    [cas_dnis]                                                                             VARCHAR (250)    NULL,
    [cas_time_stamp]                                                                       DATETIME         NULL,
    [cas_campaign_id]                                                                      VARCHAR (250)    NULL,
    [cas_call_id]                                                                          VARCHAR (250)    NULL,
    [cas_callback_extension]                                                               VARCHAR (50)     NULL,
    [cas_history_physician_initial_cal]                                                    AS               ([dbo].[GetPhysiciansInitial]([cas_key])),
    [cas_caller]                                                                           VARCHAR (50)     NULL,
    [cas_is_active]                                                                        BIT              CONSTRAINT [DF_case_cas_is_active_1] DEFAULT ((1)) NOT NULL,
    [cas_created_by]                                                                       NVARCHAR (128)   NOT NULL,
    [cas_created_by_name]                                                                  VARCHAR (300)    NULL,
    [cas_created_date]                                                                     DATETIME         CONSTRAINT [DF_case_cas_created_date] DEFAULT (getdate()) NOT NULL,
    [cas_modified_by]                                                                      NVARCHAR (128)   NULL,
    [cas_modified_by_name]                                                                 VARCHAR (300)    NULL,
    [cas_modified_date]                                                                    DATETIME         NULL,
    [cas_metric_symptoms]                                                                  NVARCHAR (MAX)   NULL,
    [cas_identification_type]                                                              INT              NULL,
    [cas_identification_number]                                                            VARCHAR (50)     NULL,
    [cas_metric_last_seen_normal]                                                          INT              CONSTRAINT [DF_case_cas_metric_last_seen_normal] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_hemorrhgic_history]                                                    INT              CONSTRAINT [DF_case_cas_metric_has_hemorrhgic_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_recent_anticoagulants]                                                 INT              CONSTRAINT [DF_case_cas_metric_has_recent_anticoagulants] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_major_surgery_history]                                                 INT              CONSTRAINT [DF_case_cas_metric_has_major_surgery_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_stroke_history]                                                        INT              CONSTRAINT [DF_case_cas_metric_has_stroke_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_tpa_verbal_order_time]                                                     DATETIME         NULL,
    [cas_metric_tpa_verbal_order_time_est]                                                 DATETIME         NULL,
    [cas_metric_weight]                                                                    FLOAT (53)       NULL,
    [cas_metric_weight_unit]                                                               VARCHAR (20)     NULL,
    [cas_metric_tpaDelay_key]                                                              INT              NULL,
    [cas_metric_non_tpa_reason_key]                                                        INT              NULL,
    [cas_metric_non_tpa_reason_text]                                                       NVARCHAR (MAX)   NULL,
    [cas_metric_ct_head_has_no_acture_hemorrhage]                                          BIT              CONSTRAINT [DF_case_cas_metric_has_no_acture_hemorrhage] DEFAULT ((0)) NOT NULL,
    [cas_metric_ct_head_is_reviewed]                                                       BIT              CONSTRAINT [DF_case_cas_metric_is_reviewed] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_to_be_reviewed]                                            BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_to_be_reviewed] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_is_reviewed]                                               BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_is_reviewed] DEFAULT ((1)) NOT NULL,
    [cas_metric_advance_imaging_not_obtained]                                              BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_not_obtained] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_cta_head_checked_obtained]                                 BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_cta_head_checked_obtained] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_ctp_obtained]                                              BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_cta_obtained] DEFAULT ((0)) NOT NULL,
    [cas_metric_discussed_with_neurointerventionalist]                                     BIT              NULL,
    [cas_metric_physician_notified_of_thrombolytics]                                       BIT              CONSTRAINT [DF_case_cas_metric_physician_notified_of_thrombolytics] DEFAULT ((0)) NULL,
    [cas_metric_physician_recommented_consult_neurointerventionalist]                      BIT              CONSTRAINT [DF_case_cas_metric_physician_recommented_consult_neurointerventionalist] DEFAULT ((0)) NOT NULL,
    [cas_billing_physician_blast]                                                          BIT              CONSTRAINT [DF_case_cas_billing_physician_blast] DEFAULT ((0)) NOT NULL,
    [cas_is_nav_blast]                                                                     BIT              CONSTRAINT [DF_case_cas_is_nav_blast] DEFAULT ((0)) NOT NULL,
    [cas_pulled_from_rounding]                                                             BIT              CONSTRAINT [DF_case_case_pulled_from_rounding] DEFAULT ((0)) NOT NULL,
    [cas_five9_original_stamp_time]                                                        VARCHAR (50)     NULL,
    [cas_last_4_of_ssn]                                                                    VARCHAR (50)     NULL,
    [cas_referring_physician]                                                              VARCHAR (500)    NULL,
    [cas_follow_up_date]                                                                   DATE             NULL,
    [cas_metric_ct_head_is_not_reviewed]                                                   BIT              CONSTRAINT [DF_case_cas_metric_ct_head_is_not_reviewed] DEFAULT ((1)) NOT NULL,
    [cas_metric_is_lastwell_unknown]                                                       BIT              CONSTRAINT [DF_case_cas_is_metric_lastwell_unknown] DEFAULT ((0)) NOT NULL,
    [cas_metric_total_dose]                                                                DECIMAL (18, 10) NULL,
    [cas_metric_bolus]                                                                     DECIMAL (18, 10) NULL,
    [cas_metric_infusion]                                                                  DECIMAL (18, 10) NULL,
    [cas_metric_discard_quantity]                                                          DECIMAL (18, 10) NULL,
    [cas_billing_physician_blast_date]                                                     DATETIME         NULL,
    [cas_billing_physician_blast_date_est]                                                 DATETIME         NULL,
    [cas_physician_assign_date]                                                            DATETIME         NULL,
    [cas_is_ealert]                                                                        BIT              CONSTRAINT [DF_case_cas_is_ealert] DEFAULT ((0)) NOT NULL,
    [cas_metric_hpi]                                                                       NVARCHAR (MAX)   NULL,
    [cas_metric_patient_gender]                                                            VARCHAR (20)     NULL,
    [cas_metric_advance_imaging_no_indication_thombus]                                     BIT              CONSTRAINT [DF_case_cas_metric_advance_imaging_no_indication_thombus] DEFAULT ((0)) NOT NULL,
    [cas_followup_case_key]                                                                INT              NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging]                        BIT              NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging_notes]                  NVARCHAR (MAX)   NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging_date]                   DATETIME         NULL,
    [cas_metric_discussed_with_neurointerventionalist_notes]                               NVARCHAR (MAX)   NULL,
    [cas_metric_discussed_with_neurointerventionalist_date]                                DATETIME         NULL,
    [cas_metric_physician_notified_of_thrombolytics_notes]                                 NVARCHAR (MAX)   NULL,
    [cas_metric_physician_notified_of_thrombolytics_date]                                  DATETIME         NULL,
    [cas_batch_id]                                                                         VARCHAR (128)    NULL,
    [cas_is_flagged]                                                                       BIT              CONSTRAINT [DF_case_cas_is_flagged] DEFAULT ((0)) NOT NULL,
    [cas_nihss_cannot_completed]                                                           BIT              CONSTRAINT [DF_case_cas_nihss_cannot_completed] DEFAULT ((0)) NOT NULL,
    [cas_metric_ct_head_reviewed_text]                                                     VARCHAR (MAX)    NULL,
    [cas_phy_technical_issue_date_est]                                                     DATETIME         NULL,
    [cas_phy_technical_issue_date]                                                         DATETIME         NULL,
    [cas_phy_has_technical_issue]                                                          BIT              CONSTRAINT [DF_case_cas_phy_has_technical_issue] DEFAULT ((0)) NOT NULL,
    [cas_triage_notes]                                                                     VARCHAR (MAX)    NULL,
    [cas_call_type]                                                                        INT              NULL,
    [cas_callback_response_time_est]                                                       DATETIME         NULL,
    [cas_callback_response_time]                                                           DATETIME         NULL,
    [cas_metric_symptom_onset_during_ed_stay]                                              BIT              CONSTRAINT [DF_case_symptom_onset_during_ed_stay] DEFAULT ((0)) NOT NULL,
    [cas_caller_source_key]                                                                INT              NULL,
    [cas_caller_source_text]                                                               NVARCHAR (MAX)   NULL,
    [cas_metric_presentation_suggestive]                                                   BIT              CONSTRAINT [DF_case_cas_metric_presentation_suggestive] DEFAULT ((0)) NOT NULL,
    [cas_metric_presentation_is_not_suggestive]                                            BIT              CONSTRAINT [DF_case_cas_metric_presentation_is_not_suggestive] DEFAULT ((0)) NOT NULL,
    [cas_metric_in_cta_queue]                                                              BIT              CONSTRAINT [DF_case_cas_metric_in_cta_queu] DEFAULT ((0)) NOT NULL,
    [cas_is_partial_update]                                                                BIT              CONSTRAINT [DF_case_cas_is_partial_update] DEFAULT ((0)) NOT NULL,
    [cas_is_flagged_dashboard]                                                             BIT              CONSTRAINT [DF_case_cas_is_flagged_dashboard] DEFAULT ((0)) NOT NULL,
    [cas_cart_location_key]                                                                INT              NULL,
    [cas_cart_location_text]                                                               NVARCHAR (MAX)   NULL,
    [cas_metric_thrombectomy_medical_decision_making]                                      INT              NULL,
    [cas_metric_advance_imaging_cta_head_and_neck]                                         BIT              CONSTRAINT [DF__case__cas_metric__61DB776A] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_ct_perfusion]                                              BIT              CONSTRAINT [DF__case__cas_metric__62CF9BA3] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir]                     BIT              CONSTRAINT [DF__case__cas_metric__63C3BFDC] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion]                   BIT              CONSTRAINT [DF__case__cas_metric__64B7E415] DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus] BIT              CONSTRAINT [DF__case__cas_metric__65AC084E] DEFAULT ((0)) NOT NULL,
    [cas_billing_comments]                                                                 VARCHAR (MAX)    NULL,
    [cas_work_flow_ids]                                                                    NVARCHAR (150)   NULL,
    [cas_qps_assigned]                                                                     NVARCHAR (150)   NULL,
    [cas_triage_arivalstarttodelay]                                                        NVARCHAR (250)   NULL,
    [cas_triage_recognition]                                                               NVARCHAR (250)   NULL,
    [cas_triage_strokealertrigger]                                                         NVARCHAR (250)   NULL,
    [cas_triage_transportandrooming]                                                       NVARCHAR (250)   NULL,
    [cas_ems_arivaltostarttimedelay]                                                       NVARCHAR (250)   NULL,
    [cas_ems_poor_identification]                                                          NVARCHAR (250)   NULL,
    [cas_inpatient_timefirstlogintonhssstartitme]                                          NVARCHAR (250)   NULL,
    [cas_inpatient_timefirstlogintovideostart]                                             NVARCHAR (250)   NULL,
    [cas_inpatient_arivaltoneedletime]                                                     NVARCHAR (250)   NULL,
    [cas_inpatient_related_imaging]                                                        NVARCHAR (250)   NULL,
    [cas_inpatient_unenhancedct]                                                           NVARCHAR (250)   NULL,
    [cas_inpatient_telemedicineassessmentroom]                                             NVARCHAR (250)   NULL,
    [cas_inpatient_telemedicineasesmentinct]                                               NVARCHAR (250)   NULL,
    [cas_inpatient_bpmanagemntrelated]                                                     NVARCHAR (250)   NULL,
    [cas_inpatient_workflowbeforemixing]                                                   NVARCHAR (250)   NULL,
    [cas_inpatient_workflowaftermixing]                                                    NVARCHAR (250)   NULL,
    [cas_ems_identification_occurred]                                                      NVARCHAR (250)   NULL,
    [cas_inpatient_delaysrelated_imaging]                                                  NVARCHAR (250)   NULL,
    [cas_inpatient_detection_hypertension]                                                 NVARCHAR (250)   NULL,
    [cas_inpatient_poormanagement_hypertension]                                            NVARCHAR (250)   NULL,
    [cas_inpatient_tpaadministration_delays]                                               NVARCHAR (250)   NULL,
    [cas_inpatient_system]                                                                 NVARCHAR (250)   NULL,
    [cas_inpatient_physician_related]                                                      NVARCHAR (250)   NULL,
    [cas_inpatient_centralizedpharmacy_delivery]                                           NVARCHAR (250)   NULL,
    [cas_inpatientdelays_mixing]                                                           NVARCHAR (250)   NULL,
    [cas_rca_primarydetail]                                                                NVARCHAR (250)   NULL,
    [cas_response_case_qps_assessment]                                                     NVARCHAR (MAX)   NULL,
    [cas_response_case_qps_reviewed]                                                       INT              NULL,
    [cas_response_case_facility_request_reviewed]                                          INT              NULL,
    [cas_history_physician_initial]                                                        VARCHAR (3000)   NULL,
    [cas_metric_symptom_onset_during_ed_stay_time_est]                                     DATETIME         NULL,
    [cas_metric_symptom_onset_during_ed_stay_time]                                         DATETIME         NULL,
    [TemplateEntityType]                                                                   INT              NULL,
    [cas_operations_review]                                                                NVARCHAR (MAX)   NULL,
    [cas_operations_review_completed]                                                      INT              NULL,
    [cas_template_deleted_date]                                                            DATETIME         NULL,
    [cas_review_facility_communication]                                                    NVARCHAR (MAX)   NULL,
    [cas_review_internal_notes]                                                            NVARCHAR (MAX)   NULL,
    [cas_billing_bic_key_initial]                                                          INT              NULL,
    [cas_cancelled_type]                                                                   NVARCHAR (128)   NULL,
    [cas_cancelled_text]                                                                   NVARCHAR (MAX)   NULL,
    [cas_datetime_of_contact]                                                              NVARCHAR (250)   NULL,
    [cas_typeof_correspondence]                                                            NVARCHAR (250)   NULL,
    [cas_contact_comments]                                                                 NVARCHAR (MAX)   NULL,
    [cas_callback_response_by]                                                             NVARCHAR (250)   NULL,
    [cas_callback_notes]                                                                   NVARCHAR (MAX)   NULL,
    [cas_commnets_off]                                                                     BIT              NULL,
    [cas_navigator_notes]                                                                  VARCHAR (MAX)    NULL,
    [cas_HTN]                                                                              BIT              NULL,
    [cas_DM]                                                                               BIT              NULL,
    [cas_HLD]                                                                              BIT              NULL,
    [cas_Afib]                                                                             BIT              NULL,
    [cas_CAD]                                                                              BIT              NULL,
    [cas_Stroke]                                                                           BIT              NULL,
    [cas_anticoagulant_use]                                                                BIT              NULL,
    [cas_anticoagulant_use_text]                                                           VARCHAR (MAX)    NULL,
    [cas_antiplatelet_use]                                                                 BIT              NULL,
    [cas_antiplatelet_use_text]                                                            VARCHAR (MAX)    NULL,
    [cas_rejection_type]                                                                   VARCHAR (128)    NULL,
    [cas_rejection_text]                                                                   VARCHAR (MAX)    NULL,
    [cas_metric_has_time_of_set]                                                           INT              NULL,
    [cas_metric_has_ct_head_hemorrhage]                                                    INT              NULL,
    [cas_metric_has_ischemic_stroke]                                                       INT              NULL,
    [cas_metric_has_severe_head_trauma]                                                    INT              NULL,
    [cas_metric_has_intracranial_surgery]                                                  INT              NULL,
    [cas_metric_has_intracranial_hemorrhage]                                               INT              NULL,
    [cas_metric_has_symptoms_SAH]                                                          INT              NULL,
    [cas_metric_has_GI_malignancy]                                                         INT              NULL,
    [cas_metric_has_coagulopathy_platelets]                                                INT              NULL,
    [cas_metric_has_treatment_LMWH]                                                        INT              NULL,
    [cas_metric_has_use_NOAC]                                                              INT              NULL,
    [cas_metric_has_glycoprotein_IIB]                                                      INT              NULL,
    [cas_metric_has_symptoms_endocarditis]                                                 INT              NULL,
    [cas_metric_has_suspected_aortic_arch]                                                 INT              NULL,
    [cas_metric_has_intracranial_neoplasm]                                                 INT              NULL,
    [cas_navigator_stamp_notes]                                                            VARCHAR (MAX)    NULL,
    [cas_exam_free_text]                                                                   VARCHAR (MAX)    NULL,
    [cas_is_flagged_physician]                                                             BIT              CONSTRAINT [Const_cas_is_flagged_physician] DEFAULT ('false') NOT NULL,
    [cas_metric_ct_head_review]                                                            BIT              NULL,
    [cas_metric_ct_head_review_reason]                                                     INT              NULL,
    [cas_metric_ct_head_review_text]                                                       VARCHAR (MAX)    NULL,
    [cas_metric_advanced_imaging_personally]                                               BIT              NULL,
    [cas_metric_advanced_imaging_personally_review]                                        INT              NULL,
    [cas_metric_advanced_imaging_personally_text]                                          VARCHAR (MAX)    NULL,
    [cas_metric_tpa_ordered_personally]                                                    BIT              NULL,
    [cas_metric_tpa_ordered_personally_review]                                             INT              NULL,
    [cas_metric_tpa_ordered_personally_text]                                               VARCHAR (MAX)    NULL,
    [cas_metric_viz_app_usage]                                                             BIT              NULL,
    [cas_metric_viz_app_usage_review]                                                      INT              NULL,
    [cas_metric_viz_app_usage_text]                                                        VARCHAR (MAX)    NULL,
    [cas_review_qps_reviewed_completed]                                                    DATETIME         NULL,
    [cas_metric_has_morbid_symptoms]                                                       INT              NULL,
    [cas_premorbid_symptoms]                                                               INT              NULL,
    [cas_premorbid_symptoms_text]                                                          VARCHAR (MAX)    NULL,
    [cas_premorbid_completed_by]                                                           VARCHAR (128)    NULL,
    [cas_premorbid_completed_date]                                                         DATETIME         NULL,
    [cas_metric_wakeup_stroke]                                                             BIT              CONSTRAINT [Const_cas_metric_wakeup_stroke] DEFAULT ('false') NOT NULL,
    CONSTRAINT [PK_case] PRIMARY KEY CLUSTERED ([cas_key] ASC),
    CONSTRAINT [FK_case_AspNetUsers] FOREIGN KEY ([cas_response_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_case_AspNetUsers_Navigator] FOREIGN KEY ([cas_associate_id]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_case_AspNetUsers1] FOREIGN KEY ([cas_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_case_facility] FOREIGN KEY ([cas_fac_key]) REFERENCES [dbo].[facility] ([fac_key]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO



CREATE TRIGGER [dbo].[trigger_for_dispatch_save_btn_Update]
   ON [dbo].[case]
   AFTER UPDATE
AS BEGIN
    SET NOCOUNT ON;

	IF UPDATE(cas_phy_key)

	BEGIN

	UPDATE [dbo].[dispatch_save_btn_tbl]
	SET cas_physician_key_new = i.cas_phy_key, row_status = 1
	FROM inserted i 
    WHERE [dbo].[dispatch_save_btn_tbl].cas_key  = i.cas_key 
 
    END 


END

GO



CREATE TRIGGER [dbo].[trigger_for_dispatch_save_btn_Insert]
   ON [dbo].[case]
   AFTER INSERT
AS BEGIN
    SET NOCOUNT ON;

	BEGIN
	INSERT INTO [dbo].[dispatch_save_btn_tbl](
        cas_key, 
        cas_physician_key_old,
		cas_physician_key_new,
		row_status
    )
    SELECT
        i.cas_key,
        i.cas_phy_key,
        i.cas_phy_key,
		1
    FROM
        inserted i
    END 

END

GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create TRIGGER trigger_for_case
   ON  [case]
   AFTER insert, update,delete
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	INSERT INTO [dbo].[case_trigger](
        cas_key, 
        cas_created_date,
        cas_modified_date,
		operation
    )
    SELECT
        i.cas_key,
        i.cas_created_date,
        i.cas_modified_date,
		'INS'
    FROM
        inserted i
    UNION ALL
    SELECT
        d.cas_key,
        d.cas_created_date,
        d.cas_modified_date,
		'DEL'
    FROM
        deleted d;

END

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_sa_ts_md';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_technical_issues';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_physician_concurrent_alerts';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_nav_to_ts';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_miscommunication';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_pulled_rounding';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_inaccurate_eta';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_physician_blast';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_rca_tracker';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Yes: 1, No: 2, NA: 3', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'case', @level2type = N'COLUMN', @level2name = N'cas_response_review_initiated';

