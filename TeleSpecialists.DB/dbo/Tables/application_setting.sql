﻿CREATE TABLE [dbo].[application_setting] (
    [aps_key]                                              UNIQUEIDENTIFIER CONSTRAINT [DF_application_setting_aps_key] DEFAULT (newid()) NOT NULL,
    [aps_md_login]                                         VARCHAR (500)    NULL,
    [aps_md_password]                                      VARCHAR (500)    NULL,
    [aps_md_instance]                                      VARCHAR (500)    NULL,
    [aps_md_facility]                                      VARCHAR (500)    NULL,
    [aps_md_grant_type]                                    VARCHAR (500)    NULL,
    [aps_md_base_url]                                      VARCHAR (500)    NULL,
    [aps_md_token_url]                                     VARCHAR (500)    NULL,
    [aps_enable_auto_assign_process]                       BIT              CONSTRAINT [DF_application_setting_aps_enable_auto_assign_process] DEFAULT ((0)) NOT NULL,
    [aps_physician_role_id]                                NVARCHAR (128)   NULL,
    [aps_csv_connection_string]                            VARCHAR (500)    NULL,
    [aps_is_md_staff_active]                               BIT              CONSTRAINT [DF_application_setting_aps_is_md_staff_active] DEFAULT ((0)) NOT NULL,
    [aps_md_staff_last_run]                                DATETIME         CONSTRAINT [DF_application_setting_aps_md_staff_last_run] DEFAULT (getdate()) NOT NULL,
    [aps_cas_facility_popup_on_load]                       BIT              CONSTRAINT [DF_application_setting_aps_cas_popup_on_load] DEFAULT ((1)) NOT NULL,
    [aps_modified_by]                                      NVARCHAR (128)   NULL,
    [aps_modified_date]                                    DATETIME         NULL,
    [aps_security_is_lowercase_required]                   BIT              CONSTRAINT [DF_application_setting_aps_security_is_lowercase_required] DEFAULT ((0)) NOT NULL,
    [aps_security_is_uppercase_required]                   BIT              CONSTRAINT [DF_application_setting_aps_security_is_uppercase_required] DEFAULT ((0)) NOT NULL,
    [aps_security_is_number_required]                      BIT              CONSTRAINT [DF_application_setting_aps_security_is_number_required] DEFAULT ((0)) NOT NULL,
    [aps_security_is_non_alphanumeric_required]            BIT              CONSTRAINT [DF_application_setting_aps_security_is_non_alphanumeric_required] DEFAULT ((0)) NOT NULL,
    [aps_security_password_length_value]                   NUMERIC (18)     NULL,
    [aps_security_password_age_value]                      INT              CONSTRAINT [DF_application_setting_aps_security_password_age_value] DEFAULT ((0)) NOT NULL,
    [aps_security_password_history_value]                  NUMERIC (18)     NULL,
    [aps_secuirty_is_reset_password_required]              BIT              CONSTRAINT [DF_application_setting_aps_secuirty_is_reset_password_required] DEFAULT ((0)) NOT NULL,
    [aps_secuirty_is_multi_factor_authentication_required] BIT              CONSTRAINT [DF_application_setting_aps_secuirty_is_multi_factor_authentication_required] DEFAULT ((0)) NOT NULL,
    [aps_five9_domain]                                     VARCHAR (500)    NULL,
    [aps_five9_number_to_dial]                             VARCHAR (50)     NULL,
    [aps_five9_list]                                       VARCHAR (50)     CONSTRAINT [DF_application_setting_aps_five9_list] DEFAULT ('eAlerts') NULL,
    [aps_duplicate_popup_timer]                            INT              NULL,
    [aps_enable_case_auto_save]                            BIT              CONSTRAINT [DF_application_setting_enable_case_auto_save] DEFAULT ((1)) NOT NULL,
    [aps_clear_pending_cases_date]                         DATE             NULL,
    [aps_statusgrid_filter_start_time]                     TIME (7)         NULL,
    [aps_statusgrid_filter_endtime]                        TIME (7)         NULL,
    [aps_clear_physician_pending_cases_date]               DATE             NULL,
    [aps_clear_physician_cta_cases_date]                   DATE             NULL,
    [aps_enable_alarm_setting]                             BIT              CONSTRAINT [DF__applicati__aps_e__1D1C38C9] DEFAULT ((0)) NOT NULL,
    [aps_audio_file_path]                                  NVARCHAR (150)   NULL,
    [aps_selected_audio]                                   NVARCHAR (100)   NULL,
    [aps_tune_is_active]                                   BIT              NULL,
    [aps_rapids_email]                                     VARCHAR (500)    NULL,
    [aps_rapids_password]                                  VARCHAR (500)    NULL,
    [aps_rapids_service]                                   VARCHAR (500)    NULL,
    [aps_rapids_retention]                                 TIME (0)         CONSTRAINT [DF_application_setting_aps_rapids_retention] DEFAULT ('00:30:00') NOT NULL,
    [aps_status_page_interval]                             INT              CONSTRAINT [DF_application_setting_aps_status_page_interval] DEFAULT ((1000)) NOT NULL,
    [aps_enable_snooze_alarm_setting]                      BIT              CONSTRAINT [DF_application_setting_aps_enable_snooze_alarm_setting] DEFAULT ((0)) NOT NULL,
    [aps_snooze_audio_file_path]                           NVARCHAR (50)    NULL,
    [aps_snooze_selected_audio]                            NVARCHAR (100)   NULL,
    [aps_snooze_tune_is_active]                            BIT              CONSTRAINT [DF_application_setting_aps_snooze_tune_is_active] DEFAULT ((0)) NOT NULL,
    [aps_eAlert_retry_limt]                                INT              NULL,
    CONSTRAINT [PK_application_setting] PRIMARY KEY CLUSTERED ([aps_key] ASC)
);









