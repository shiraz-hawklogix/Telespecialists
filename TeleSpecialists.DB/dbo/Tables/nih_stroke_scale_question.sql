CREATE TABLE [dbo].[nih_stroke_scale_question] (
    [nsq_key]          INT            NOT NULL,
    [nsq_title]        VARCHAR (500)  NOT NULL,
    [nsq_created_by]   NVARCHAR (128) NULL,
    [nsq_created_date] DATETIME       NULL,
    CONSTRAINT [PK_nih_stroke_scale_question] PRIMARY KEY CLUSTERED ([nsq_key] ASC)
);

