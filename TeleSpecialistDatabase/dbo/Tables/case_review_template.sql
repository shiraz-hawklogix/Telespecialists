CREATE TABLE [dbo].[case_review_template] (
    [crt_key]              INT            IDENTITY (1, 1) NOT NULL,
    [crt_cas_key]          INT            NULL,
    [crt_template_html]    NVARCHAR (MAX) NULL,
    [crt_finalize_date]    DATETIME       NULL,
    [crt_created_date]     DATETIME       NULL,
    [crt_created_by]       NVARCHAR (128) NULL,
    [crt_created_by_name]  VARCHAR (300)  NULL,
    [crt_modified_date]    DATETIME       NULL,
    [crt_modified_by]      NVARCHAR (128) NULL,
    [crt_modified_by_name] VARCHAR (300)  NULL,
    CONSTRAINT [PK_case_review_template] PRIMARY KEY CLUSTERED ([crt_key] ASC),
    CONSTRAINT [FK_case_review_template_case] FOREIGN KEY ([crt_cas_key]) REFERENCES [dbo].[case] ([cas_key])
);

