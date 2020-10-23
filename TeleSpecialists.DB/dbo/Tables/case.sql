CREATE TABLE [dbo].[case] (
    [cas_key]                                                                              INT              IDENTITY (20000, 1) NOT NULL,
    [cas_ctp_key]                                                                          INT              NOT NULL,
    [cas_associate_id]                                                                     NVARCHAR (128)   NOT NULL,
    [cas_fac_key]                                                                          UNIQUEIDENTIFIER NOT NULL,
    [cas_callback]                                                                         VARCHAR (50)     NULL,
    [cas_cart]                                                                             NVARCHAR (50)    NULL,
    [cas_phy_key]                                                                          NVARCHAR (128)   NULL,
    [cas_cst_key]                                                                          INT              NOT NULL,
    [cas_status_assign_date]                                                               DATETIME         NULL,
    [cas_notes]                                                                            VARCHAR (MAX)    NULL,
    [cas_triage_notes]                                                                     VARCHAR (MAX)    NULL,
    [cas_patient]                                                                          VARCHAR (300)    NULL,
    [cas_case_number]                                                                      BIGINT           NULL,
    [cas_phy_technical_issue_date_est]                                                     DATETIME         NULL,
    [cas_phy_technical_issue_date]                                                         DATETIME         NULL,
    [cas_phy_has_technical_issue]                                                          BIT              CONSTRAINT [DF_case_cas_phy_has_technical_issue] DEFAULT ((0)) NOT NULL,
    [cas_metric_symptom_onset_during_ed_stay]                                              BIT              CONSTRAINT [DF_case_symptom_onset_during_ed_stay] DEFAULT ((0)) NOT NULL,
    [cas_metric_lastwell_date]                                                             DATETIME         NULL,
    [cas_metric_door_time]                                                                 DATETIME         NULL,
    [cas_metric_stamp_time]                                                                DATETIME         NULL,
    [cas_metric_firstlogin_date]                                                           DATETIME         NULL,
    [cas_metric_symptoms]                                                                  NVARCHAR (MAX)   NULL,
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
    [cas_billing_comments]                                                                 VARCHAR (MAX)    NULL,
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
    [cas_call_type]                                                                        INT              NULL,
    [cas_identification_type]                                                              INT              NULL,
    [cas_identification_number]                                                            VARCHAR (50)     NULL,
    [cas_callback_response_time_est]                                                       DATETIME         NULL,
    [cas_callback_response_time]                                                           DATETIME         NULL,
    [cas_metric_has_hemorrhgic_history]                                                    INT              CONSTRAINT [DF_case_cas_metric_has_hemorrhgic_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_recent_anticoagulants]                                                 INT              CONSTRAINT [DF_case_cas_metric_has_recent_anticoagulants] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_major_surgery_history]                                                 INT              CONSTRAINT [DF_case_cas_metric_has_major_surgery_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_has_stroke_history]                                                        INT              CONSTRAINT [DF_case_cas_metric_has_stroke_history] DEFAULT ((3)) NOT NULL,
    [cas_metric_tpa_verbal_order_time]                                                     DATETIME         NULL,
    [cas_metric_tpa_verbal_order_time_est]                                                 DATETIME         NULL,
    [cas_metric_weight]                                                                    FLOAT (53)       NULL,
    [cas_metric_patient_gender]                                                            VARCHAR (20)     NULL,
    [cas_metric_weight_unit]                                                               VARCHAR (20)     NULL,
    [cas_metric_tpaDelay_key]                                                              INT              NULL,
    [cas_metric_non_tpa_reason_key]                                                        INT              NULL,
    [cas_metric_non_tpa_reason_text]                                                       NVARCHAR (MAX)   NULL,
    [cas_metric_ct_head_has_no_acture_hemorrhage]                                          BIT              CONSTRAINT [DF_case_cas_metric_has_no_acture_hemorrhage] DEFAULT ((0)) NOT NULL,
    [cas_metric_ct_head_is_reviewed]                                                       BIT              CONSTRAINT [DF_case_cas_metric_is_reviewed] DEFAULT ((0)) NOT NULL,
    [cas_metric_ct_head_is_not_reviewed]                                                   BIT              CONSTRAINT [DF_case_cas_metric_ct_head_is_not_reviewed] DEFAULT ((1)) NOT NULL,
    [cas_metric_discussed_with_neurointerventionalist]                                     BIT              NULL,
    [cas_metric_physician_notified_of_thrombolytics]                                       BIT              CONSTRAINT [DF_case_cas_metric_physician_notified_of_thrombolytics] DEFAULT ((0)) NULL,
    [cas_metric_physician_recommented_consult_neurointerventionalist]                      BIT              CONSTRAINT [DF_case_cas_metric_physician_recommented_consult_neurointerventionalist] DEFAULT ((0)) NOT NULL,
    [cas_billing_physician_blast]                                                          BIT              CONSTRAINT [DF_case_cas_billing_physician_blast] DEFAULT ((0)) NOT NULL,
    [cas_billing_physician_blast_date]                                                     DATETIME         NULL,
    [cas_billing_physician_blast_date_est]                                                 DATETIME         NULL,
    [cas_is_nav_blast]                                                                     BIT              CONSTRAINT [DF_case_cas_is_nav_blast] DEFAULT ((0)) NOT NULL,
    [cas_pulled_from_rounding]                                                             BIT              CONSTRAINT [DF_case_case_pulled_from_rounding] DEFAULT ((0)) NOT NULL,
    [cas_last_4_of_ssn]                                                                    VARCHAR (50)     NULL,
    [cas_referring_physician]                                                              VARCHAR (500)    NULL,
    [cas_metric_last_seen_normal]                                                          INT              CONSTRAINT [DF_case_cas_metric_last_seen_normal] DEFAULT ((3)) NOT NULL,
    [cas_five9_original_stamp_time]                                                        VARCHAR (50)     NULL,
    [cas_follow_up_date]                                                                   DATE             NULL,
    [cas_metric_is_lastwell_unknown]                                                       BIT              CONSTRAINT [DF_case_cas_is_metric_lastwell_unknown] DEFAULT ((0)) NOT NULL,
    [cas_metric_total_dose]                                                                DECIMAL (18, 10) NULL,
    [cas_metric_bolus]                                                                     DECIMAL (18, 10) NULL,
    [cas_metric_infusion]                                                                  DECIMAL (18, 10) NULL,
    [cas_metric_discard_quantity]                                                          DECIMAL (18, 10) NULL,
    [cas_physician_assign_date]                                                            DATETIME         NULL,
    [cas_is_ealert]                                                                        BIT              CONSTRAINT [DF_case_cas_is_ealert] DEFAULT ((0)) NOT NULL,
    [cas_metric_hpi]                                                                       NVARCHAR (MAX)   NULL,
    [cas_followup_case_key]                                                                INT              NULL,
    [cas_batch_id]                                                                         VARCHAR (128)    NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging]                        BIT              NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging_notes]                  NVARCHAR (MAX)   NULL,
    [cas_metric_radiologist_callback_for_review_of_advance_imaging_date]                   DATETIME         NULL,
    [cas_metric_discussed_with_neurointerventionalist_notes]                               NVARCHAR (MAX)   NULL,
    [cas_metric_discussed_with_neurointerventionalist_date]                                DATETIME         NULL,
    [cas_metric_physician_notified_of_thrombolytics_notes]                                 NVARCHAR (MAX)   NULL,
    [cas_metric_physician_notified_of_thrombolytics_date]                                  DATETIME         NULL,
    [cas_metric_thrombectomy_medical_decision_making]                                      INT              NULL,
    [cas_is_flagged]                                                                       BIT              CONSTRAINT [DF_case_cas_is_flagged] DEFAULT ((0)) NOT NULL,
    [cas_nihss_cannot_completed]                                                           BIT              CONSTRAINT [DF_case_cas_nihss_cannot_completed] DEFAULT ((0)) NOT NULL,
    [cas_metric_ct_head_reviewed_text]                                                     VARCHAR (MAX)    NULL,
    [cas_is_active]                                                                        BIT              CONSTRAINT [DF_case_cas_is_active_1] DEFAULT ((1)) NOT NULL,
    [cas_created_by]                                                                       NVARCHAR (128)   NOT NULL,
    [cas_created_by_name]                                                                  VARCHAR (300)    NULL,
    [cas_created_date]                                                                     DATETIME         CONSTRAINT [DF_case_cas_created_date] DEFAULT (getdate()) NOT NULL,
    [cas_modified_by]                                                                      NVARCHAR (128)   NULL,
    [cas_modified_by_name]                                                                 VARCHAR (300)    NULL,
    [cas_modified_date]                                                                    DATETIME         NULL,
    [cas_caller_source_key]                                                                INT              NULL,
    [cas_caller_source_text]                                                               NVARCHAR (MAX)   NULL,
    [cas_cart_location_key]                                                                INT              NULL,
    [cas_cart_location_text]                                                               NVARCHAR (MAX)   NULL,
    [cas_metric_advance_imaging_cta_head_and_neck]                                         BIT              DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_ct_perfusion]                                              BIT              DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_to_be_reviewed_by_ed_provider_and_nir]                     BIT              DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_is_suggestive_of_large_vessel_occlusion]                   BIT              DEFAULT ((0)) NOT NULL,
    [cas_metric_advance_imaging_reviewed_no_indication_of_large_vessel_occlusive_thrombus] BIT              DEFAULT ((0)) NOT NULL,
    [cas_metric_in_cta_queue]                                                              BIT              CONSTRAINT [DF_case_cas_metric_in_cta_queu] DEFAULT ((0)) NOT NULL,
    [cas_is_partial_update]                                                                BIT              CONSTRAINT [DF_case_cas_is_partial_update] DEFAULT ((0)) NOT NULL,
    [cas_is_flagged_dashboard]                                                             BIT              CONSTRAINT [DF_case_cas_is_flagged_dashboard] DEFAULT ((0)) NOT NULL,
    [cas_work_flow_ids]                                                                    NVARCHAR (150)   NULL,
    [cas_qps_assigned]                                                                     INT              NULL,
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
    [cas_template_deleted_date]                                                            DATETIME         NULL,
    [cas_operations_review_completed]                                                      INT              NULL,
    [cas_review_facility_communication]                                                    NVARCHAR (MAX)   NULL,
    [cas_review_internal_notes]                                                            NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_case] PRIMARY KEY CLUSTERED ([cas_key] ASC),
    CONSTRAINT [FK_case_AspNetUsers] FOREIGN KEY ([cas_response_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_case_AspNetUsers_Navigator] FOREIGN KEY ([cas_associate_id]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_case_AspNetUsers1] FOREIGN KEY ([cas_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_case_facility] FOREIGN KEY ([cas_fac_key]) REFERENCES [dbo].[facility] ([fac_key]) ON DELETE CASCADE ON UPDATE CASCADE
);





 

GO
CREATE NONCLUSTERED INDEX [nci_wi_case_4A6AE2A25C10DB1A04393970F7F90D03]
    ON [dbo].[case]([cas_is_active] ASC, [cas_associate_id] ASC)
    INCLUDE([cas_billing_bic_key], [cas_billing_visit_type], [cas_callback], [cas_cart], [cas_phy_key], [cas_status_assign_date], [cas_case_number], [cas_created_by], [cas_created_date], [cas_cst_key], [cas_ctp_key], [cas_fac_key], [cas_is_ealert], [cas_is_nav_blast], [cas_patient]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_46B3BA66528DB60D81F210BBCDA78671]
    ON [dbo].[case]([cas_is_active] ASC, [cas_associate_id] ASC, [cas_cst_key] ASC)
    INCLUDE([cas_billing_visit_type], [cas_callback], [cas_cart], [cas_patient], [cas_phy_key], [cas_status_assign_date], [cas_case_number], [cas_created_date], [cas_ctp_key], [cas_fac_key], [cas_is_ealert], [cas_is_nav_blast]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_980B6F96109081CF0D313C8160382DE8]
    ON [dbo].[case]([cas_is_active] ASC, [cas_cst_key] ASC, [cas_ctp_key] ASC)
    INCLUDE([cas_billing_bic_key], [cas_billing_dob], [cas_billing_visit_type], [cas_response_ts_notification], [cas_status_assign_date], [cas_metric_pa_ordertime], [cas_metric_stamp_time], [cas_metric_video_end_time], [cas_is_ealert], [cas_patient], [cas_metric_door_time], [cas_phy_key], [cas_metric_lastwell_date], [cas_response_first_atempt], [cas_callback], [cas_metric_patient_gender], [cas_cart], [cas_metric_tpa_verbal_order_time], [cas_case_number], [cas_metric_video_start_time], [cas_created_by], [cas_created_date], [cas_fac_key], [cas_is_nav_blast], [cas_metric_is_lastwell_unknown], [cas_metric_needle_time]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_991B6E465F6D72E570A98383477421F6]
    ON [dbo].[case]([cas_is_active] ASC, [cas_fac_key] ASC, [cas_phy_key] ASC)
    INCLUDE([cas_billing_bic_key], [cas_billing_dob], [cas_billing_visit_type], [cas_response_ts_notification], [cas_status_assign_date], [cas_metric_needle_time], [cas_metric_patient_gender], [cas_metric_tpa_verbal_order_time], [cas_ctp_key], [cas_metric_video_start_time], [cas_is_nav_blast], [cas_patient], [cas_metric_is_lastwell_unknown], [cas_response_first_atempt], [cas_callback], [cas_metric_pa_ordertime], [cas_cart], [cas_metric_stamp_time], [cas_case_number], [cas_metric_video_end_time], [cas_created_by], [cas_created_date], [cas_cst_key], [cas_is_ealert], [cas_metric_door_time], [cas_metric_lastwell_date]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_626DCE8508AD807A5AD0A5E12F5BFBAD]
    ON [dbo].[case]([cas_phy_key] ASC)
    INCLUDE([cas_created_date], [cas_fac_key]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_6AE3952463EC07F5197D20F1318DEBC7]
    ON [dbo].[case]([cas_phy_key] ASC)
    INCLUDE([cas_created_date], [cas_fac_key], [cas_physician_assign_date]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_3ABDD0E82241C8053573E2925F5E62F7]
    ON [dbo].[case]([cas_phy_key] ASC)
    INCLUDE([cas_created_date], [cas_fac_key],[cas_physician_assign_date]);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_3420D016BDD265912F836A8598B6ADE3]
    ON [dbo].[case]([cas_phy_key] ASC, [cas_cst_key] ASC)
    INCLUDE([cas_fac_key], [cas_patient], [cas_physician_assign_date], [cas_created_date]);


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


GO
CREATE UNIQUE NONCLUSTERED INDEX [case_unique_case_number]
    ON [dbo].[case]([cas_case_number] DESC);


GO
CREATE NONCLUSTERED INDEX [nci_wi_case_7D05A4AB024F548C7212BE62151C1DFC]
    ON [dbo].[case]([cas_created_date] ASC)
    INCLUDE([cas_ctp_key], [cas_metric_stamp_time_est], [cas_response_first_atempt], [cas_response_time_physician], [cas_response_ts_notification]);

