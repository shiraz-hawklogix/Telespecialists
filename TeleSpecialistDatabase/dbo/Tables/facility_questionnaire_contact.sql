CREATE TABLE [dbo].[facility_questionnaire_contact] (
    [fqc_key]             INT              IDENTITY (1, 1) NOT NULL,
    [fqc_fqd_key]         INT              NOT NULL,
    [fqc_name]            VARCHAR (300)    NOT NULL,
    [fqc_phone]           VARCHAR (50)     NULL,
    [fqc_email]           VARCHAR (150)    NULL,
    [fqc_fqp_key]         UNIQUEIDENTIFIER NOT NULL,
    [fqc_created_date]    DATETIME         NOT NULL,
    [fqc_created_by]      NVARCHAR (128)   NOT NULL,
    [fqc_created_by_name] VARCHAR (300)    NOT NULL,
    CONSTRAINT [PK_faclity_questionnaire_contact] PRIMARY KEY CLUSTERED ([fqc_key] ASC),
    CONSTRAINT [FK_faclity_questionnaire_contact_facility_questionnaire_pre_live] FOREIGN KEY ([fqc_fqp_key]) REFERENCES [dbo].[facility_questionnaire_pre_live] ([fqp_key]),
    CONSTRAINT [FK_faclity_questionnaire_contact_faclity_questionnaire_contact_designation] FOREIGN KEY ([fqc_fqd_key]) REFERENCES [dbo].[facility_questionnaire_contact_designation] ([fqd_key])
);

