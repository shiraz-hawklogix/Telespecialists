CREATE TABLE [dbo].[case_cancelled_type] (
    [cct_key]         INT            IDENTITY (1, 1) NOT NULL,
    [cct_name]        NVARCHAR (128) NULL,
    [cct_created_on]  DATETIME       NULL,
    [cct_created_by]  NVARCHAR (128) NULL,
    [cct_modified_on] DATETIME       NULL,
    [cct_modified_by] NVARCHAR (128) NULL,
    CONSTRAINT [PK_case_cancelled_type] PRIMARY KEY CLUSTERED ([cct_key] ASC)
);

