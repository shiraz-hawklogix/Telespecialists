﻿CREATE TABLE [dbo].[case_template_stroke_tpa] (
    [cts_cas_key]                      INT            NOT NULL,
    [cts_impression]                   NVARCHAR (MAX) NULL,
    [cts_acute_stroke]                 VARCHAR (50)   NULL,
    [cts_mechanism_stroke]             VARCHAR (50)   NULL,
    [cts_mechanism_stroke_text]        VARCHAR (MAX)  NULL,
    [cts_tpa_bolus_complications]      INT            NULL,
    [cts_tpa_bolus_complications_text] VARCHAR (MAX)  NULL,
    [cts_nihss_totalscore]             INT            NULL,
    [cts_comment]                      VARCHAR (MAX)  NULL,
    [cts_vitals_bp]                    VARCHAR (MAX)  NULL,
    [cts_vitals_pulse]                 VARCHAR (MAX)  NULL,
    [cts_verbal_consent]               VARCHAR (500)  CONSTRAINT [DF_Table_1_cas_template_verbal_consent] DEFAULT ((0)) NULL,
    [cts_vitals_blood_glucose]         VARCHAR (MAX)  NULL,
    [cts_created_date]                 DATETIME       NOT NULL,
    [cts_created_by]                   VARCHAR (128)  NOT NULL,
    [cts_created_by_name]              VARCHAR (300)  NOT NULL,
    [cts_modified_by]                  VARCHAR (128)  NULL,
    [cts_modfied_by_name]              VARCHAR (300)  NULL,
    [cts_modified_date]                DATETIME       NULL,
    [cts_patient_family_cosulted]      BIT            NULL,
    [cts_critical_care_was_provided]   BIT            NULL,
    [cts_critical_care_minutes]        INT            DEFAULT ((35)) NULL,
    CONSTRAINT [PK_case_template_stroke_tpa] PRIMARY KEY CLUSTERED ([cts_cas_key] ASC),
    CONSTRAINT [FK_case_template_stroke_tpa_case] FOREIGN KEY ([cts_cas_key]) REFERENCES [dbo].[case] ([cas_key]) ON DELETE CASCADE ON UPDATE CASCADE
);



