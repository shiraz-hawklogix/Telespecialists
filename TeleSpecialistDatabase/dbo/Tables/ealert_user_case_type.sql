CREATE TABLE [dbo].[ealert_user_case_type] (
    [ect_key]              INT            IDENTITY (1, 1) NOT NULL,
    [ect_case_type_key]    INT            NOT NULL,
    [ect_user_key]         NVARCHAR (128) NOT NULL,
    [ect_is_active]        BIT            CONSTRAINT [DF_ealert_user_case_type_efa_is_active] DEFAULT ((0)) NOT NULL,
    [ect_is_default]       BIT            CONSTRAINT [DF_ealert_user_case_type_efa_is_default] DEFAULT ((0)) NOT NULL,
    [ect_created_by]       NVARCHAR (128) NOT NULL,
    [ect_created_by_name]  VARCHAR (300)  NULL,
    [ect_created_date]     DATETIME       NOT NULL,
    [ect_modified_by]      NVARCHAR (128) NULL,
    [ect_modified_by_name] VARCHAR (300)  NULL,
    [ect_modified_date]    DATETIME       NULL,
    CONSTRAINT [PK_ealert_user_case_type] PRIMARY KEY CLUSTERED ([ect_key] ASC),
    CONSTRAINT [FK_ealert_user_case_type_AspNetUsers] FOREIGN KEY ([ect_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_ealert_user_case_type_ucl_data] FOREIGN KEY ([ect_case_type_key]) REFERENCES [dbo].[ucl_data] ([ucd_key])
);

