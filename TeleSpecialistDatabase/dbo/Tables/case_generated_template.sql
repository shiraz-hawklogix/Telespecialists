CREATE TABLE [dbo].[case_generated_template] (
    [cgt_key]              INT            IDENTITY (1, 1) NOT NULL,
    [cgt_cas_key]          INT            NOT NULL,
    [cgt_template_html]    NVARCHAR (MAX) NOT NULL,
    [cgt_finalize_date]    DATETIME       NULL,
    [cgt_created_date]     DATETIME       NOT NULL,
    [cgt_created_by]       NVARCHAR (128) NOT NULL,
    [cgt_created_by_name]  VARCHAR (300)  NOT NULL,
    [cgt_modified_date]    DATETIME       NULL,
    [cgt_modified_by]      NVARCHAR (128) NULL,
    [cgt_modified_by_name] VARCHAR (300)  NULL,
    [cgt_ent_key]          INT            NOT NULL,
    CONSTRAINT [PK_case_generated_template] PRIMARY KEY CLUSTERED ([cgt_key] ASC),
    CONSTRAINT [FK_case_generated_template_case] FOREIGN KEY ([cgt_cas_key]) REFERENCES [dbo].[case] ([cas_key]),
    CONSTRAINT [FK_case_generated_template_entity_type] FOREIGN KEY ([cgt_ent_key]) REFERENCES [dbo].[entity_type] ([ent_key])
);

