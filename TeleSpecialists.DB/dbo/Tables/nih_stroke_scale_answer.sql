CREATE TABLE [dbo].[nih_stroke_scale_answer] (
    [nsa_key]             INT            IDENTITY (1, 1) NOT NULL,
    [nsa_cas_key]         INT            NOT NULL,
    [nsa_nss_key]         INT            NOT NULL,
    [nsa_created_by]      NVARCHAR (128) NULL,
    [nsa_created_date]    DATETIME       NOT NULL,
    [nsa_created_by_name] VARCHAR (300)  NULL,
    [nsa_ent_key]         INT            NULL,
    CONSTRAINT [PK_nih_stroke_scale_answer] PRIMARY KEY CLUSTERED ([nsa_key] ASC),
    CONSTRAINT [FK_nih_stroke_scale_answer_case] FOREIGN KEY ([nsa_cas_key]) REFERENCES [dbo].[case] ([cas_key]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_nih_stroke_scale_answer_entity_type] FOREIGN KEY ([nsa_ent_key]) REFERENCES [dbo].[entity_type] ([ent_key]),
    CONSTRAINT [FK_nih_stroke_scale_answer_nih_stroke_scale_question] FOREIGN KEY ([nsa_nss_key]) REFERENCES [dbo].[nih_stroke_scale] ([nss_key])
);


GO
CREATE NONCLUSTERED INDEX [nci_wi_nih_stroke_scale_answer_63678294E8783FDD31F97036D595E37B]
    ON [dbo].[nih_stroke_scale_answer]([nsa_cas_key] ASC, [nsa_ent_key] ASC)
    INCLUDE([nsa_nss_key]);

