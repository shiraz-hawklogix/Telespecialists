CREATE TABLE [dbo].[nih_stroke_scale] (
    [nss_key]          INT            IDENTITY (1, 1) NOT NULL,
    [nss_title]        VARCHAR (500)  NOT NULL,
    [nss_score]        INT            CONSTRAINT [DF_nih_stroke_scale_nss_score] DEFAULT ((0)) NOT NULL,
    [nss_nsq_key]      INT            NOT NULL,
    [nss_created_by]   NVARCHAR (128) NULL,
    [nss_created_date] DATETIME       NULL,
    [nss_is_active]    BIT            CONSTRAINT [DF_nih_stroke_scale_nss_is_active] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_nih_stroke_scale] PRIMARY KEY CLUSTERED ([nss_key] ASC),
    CONSTRAINT [FK_nih_stroke_scale_nih_stroke_scale_question] FOREIGN KEY ([nss_nsq_key]) REFERENCES [dbo].[nih_stroke_scale_question] ([nsq_key]) ON DELETE CASCADE ON UPDATE CASCADE
);

