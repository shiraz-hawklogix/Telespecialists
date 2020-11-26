CREATE TABLE [dbo].[icd10_billing_codes_calcualtor] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [cod_parent_id]     INT           NULL,
    [cod_name]          VARCHAR (500) NULL,
    [cod_class_name]    VARCHAR (500) NULL,
    [cod_sort_order]    INT           NULL,
    [cod_is_active]     BIT           NULL,
    [cod_added_date]    DATETIME      NULL,
    [cod_modified_date] DATETIME      NULL,
    [cod_linked_id]     VARCHAR (200) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

