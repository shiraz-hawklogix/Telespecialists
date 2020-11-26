CREATE TABLE [dbo].[facility_questionnaire_contact_designation] (
    [fqd_key]        INT          NOT NULL,
    [fqd_name]       VARCHAR (50) NOT NULL,
    [fqd_is_active]  BIT          CONSTRAINT [DF_Table_1_fgs_is_active] DEFAULT ((1)) NOT NULL,
    [fqd_sort_order] INT          NULL,
    CONSTRAINT [PK_faclity_questionnaire_contact_designation] PRIMARY KEY CLUSTERED ([fqd_key] ASC)
);

