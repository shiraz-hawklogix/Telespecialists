CREATE TABLE [dbo].[diagnosis_codes] (
    [code_id]              INT           IDENTITY (1, 1) NOT NULL,
    [diag_cat_parent_id]   INT           NULL,
    [icd_code]             VARCHAR (200) NULL,
    [icd_code_title]       VARCHAR (200) NULL,
    [icd_code_description] VARCHAR (500) NULL,
    [icd_code_impression]  VARCHAR (200) NULL,
    [sort_order]           INT           NULL,
    [date_added]           DATETIME      NULL,
    [date_updated]         DATETIME      NULL,
    [is_active]            BIT           NULL,
    CONSTRAINT [PK_diagnosis_category_codes] PRIMARY KEY CLUSTERED ([code_id] ASC)
);

