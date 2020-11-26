CREATE TABLE [dbo].[pac_case_template] (
    [pct_key]              INT            IDENTITY (1, 1) NOT NULL,
    [pct_pac_key]          INT            NULL,
    [pct_template_html]    NVARCHAR (MAX) NULL,
    [pct_finalize_date]    DATETIME       NULL,
    [pct_created_date]     DATETIME       NULL,
    [pct_created_by]       NVARCHAR (128) NULL,
    [pct_created_by_name]  VARCHAR (300)  NULL,
    [pct_modified_date]    DATETIME       NULL,
    [pct_modified_by]      NVARCHAR (128) NULL,
    [pct_modified_by_name] VARCHAR (300)  NULL,
    CONSTRAINT [PK_pac_case_template] PRIMARY KEY CLUSTERED ([pct_key] ASC),
    CONSTRAINT [FK_pac_case_template_post_acute_care] FOREIGN KEY ([pct_pac_key]) REFERENCES [dbo].[post_acute_care] ([pac_key])
);


GO
ALTER TABLE [dbo].[pac_case_template] NOCHECK CONSTRAINT [FK_pac_case_template_post_acute_care];

