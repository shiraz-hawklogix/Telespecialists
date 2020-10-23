CREATE TABLE [dbo].[case_template_stroke_neuro_tpa] (
    [csn_cas_key]                      INT            NOT NULL,
    [csn_impression]                   NVARCHAR (MAX) NULL,
    [csn_acute_stroke]                 VARCHAR (50)   NULL,
    [csn_mechanism_stroke]             VARCHAR (50)   NULL,
    [csn_mechanism_stroke_text]        VARCHAR (MAX)  NULL,
    [csn_tpa_bolus_complications]      INT            NULL,
    [csn_tpa_bolus_complications_text] VARCHAR (MAX)  NULL,
    [csn_nihss_totalscore]             INT            NULL,
    [csn_comment]                      VARCHAR (MAX)  NULL,
    [csn_vitals_bp]                    VARCHAR (MAX)  NULL,
    [csn_vitals_pulse]                 VARCHAR (MAX)  NULL,
    [csn_verbal_consent]               VARCHAR (500)  NULL,
    [csn_additional_recomendations]    VARCHAR (500)  NULL,
    [csn_vitals_blood_glucose]         VARCHAR (MAX)  NULL,
    [csn_created_date]                 DATETIME       NOT NULL,
    [csn_created_by]                   VARCHAR (128)  NOT NULL,
    [csn_created_by_name]              VARCHAR (300)  NOT NULL,
    [csn_modified_by]                  VARCHAR (128)  NULL,
    [csn_modfied_by_name]              VARCHAR (300)  NULL,
    [csn_modified_date]                DATETIME       NULL,
    [csn_patient_family_cosulted]      BIT            NULL,
    [csn_critical_care_was_provided]   BIT            NULL,
    [csn_critical_care_minutes]        INT            DEFAULT ((35)) NULL,
    CONSTRAINT [PK_case_template_stroke_neuro_tpa] PRIMARY KEY CLUSTERED ([csn_cas_key] ASC),
    CONSTRAINT [FK_case_template_stroke_neuro_tpa_case] FOREIGN KEY ([csn_cas_key]) REFERENCES [dbo].[case] ([cas_key])
);



