﻿CREATE TABLE [dbo].[case_template_stroke_notpa] (
    [ctn_cas_key]                               INT            NOT NULL,
    [ctn_impression]                            VARCHAR (50)   NULL,
    [ctn_impression_text]                       VARCHAR (MAX)  NULL,
    [ctn_mechanism_stroke]                      VARCHAR (50)   NULL,
    [ctn_mechanism_stroke_text]                 VARCHAR (MAX)  NULL,
    [ctn_comment]                               VARCHAR (MAX)  NULL,
    [ctn_antiplatelet_therapy_recommedned]      VARCHAR (50)   NULL,
    [ctn_antiplatelet_therapy_recommedned_text] VARCHAR (MAX)  NULL,
    [ctn_imaging_studies_recommedned]           VARCHAR (50)   NULL,
    [ctn_imaging_studies_recommedned_text]      VARCHAR (MAX)  NULL,
    [ctn_therapies]                             VARCHAR (50)   NULL,
    [ctn_nihss_totalscore]                      INT            NULL,
    [ctn_dysphaghia_screen]                     VARCHAR (50)   NULL,
    [ctn_dvt_prophylaxis]                       VARCHAR (50)   NULL,
    [ctn_dvt_prophylaxis_text]                  NVARCHAR (MAX) NULL,
    [ctn_lipid_panel_obtained]                  BIT            CONSTRAINT [DF_case_template_stroke_notpa_ctn_lipid_panel_obtained] DEFAULT ((0)) NOT NULL,
    [ctn_disposition]                           INT            NULL,
    [ctn_sign_out]                              VARCHAR (50)   NULL,
    [ctn_created_date]                          DATETIME       NOT NULL,
    [ctn_created_by]                            VARCHAR (128)  NOT NULL,
    [ctn_created_by_name]                       VARCHAR (300)  NOT NULL,
    [ctn_modified_by]                           VARCHAR (128)  NULL,
    [ctn_modfied_by_name]                       VARCHAR (300)  NULL,
    [ctn_modified_date]                         DATETIME       NULL,
    [ctn_vitals_bp]                             VARCHAR (MAX)  NULL,
    [ctn_vitals_pulse]                          VARCHAR (MAX)  NULL,
    [ctn_vitals_blood_glucose]                  VARCHAR (MAX)  NULL,
    [ctn_patient_family_cosulted]               BIT            NULL,
    [ctn_critical_care_was_provided]            BIT            NULL,
    [ctn_critical_care_minutes]                 INT            NULL,
    [ctn_family_consent_available]              BIT            NULL,
    [ctn_PMH]                                   VARCHAR (50)   NULL,
    [ctn_anticoagulant_use]                     BIT            NULL,
    [ctn_anticoagulant_use_text]                VARCHAR (MAX)  NULL,
    [ctn_antiplatelet_use]                      BIT            NULL,
    [ctn_antiplatelet_use_text]                 VARCHAR (MAX)  NULL,
    [ctn_NIHSS_comatose]                        INT            NULL,
    [ctn_NIHSS_comatose_text]                   VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_case_template_stroke_notpa] PRIMARY KEY CLUSTERED ([ctn_cas_key] ASC),
    CONSTRAINT [FK_case_template_stroke_notpa_case] FOREIGN KEY ([ctn_cas_key]) REFERENCES [dbo].[case] ([cas_key])
);
