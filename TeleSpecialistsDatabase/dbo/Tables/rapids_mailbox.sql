CREATE TABLE [dbo].[rapids_mailbox] (
    [rpd_key]             INT            IDENTITY (1, 1) NOT NULL,
    [rpd_uid]             VARCHAR (1000) NOT NULL,
    [rpd_date]            DATETIME       NOT NULL,
    [rpd_from]            VARCHAR (500)  NOT NULL,
    [rpd_to]              VARCHAR (MAX)  NULL,
    [rpd_subject]         VARCHAR (500)  NOT NULL,
    [rpd_body]            NVARCHAR (MAX) NOT NULL,
    [rpd_attachments]     INT            NOT NULL,
    [rpd_logs]            VARCHAR (MAX)  NULL,
    [rpd_created_by]      NVARCHAR (128) NOT NULL,
    [rpd_created_date]    DATETIME       NOT NULL,
    [rpd_attachment_html] NVARCHAR (MAX) NULL,
    [rpd_is_read]         BIT            CONSTRAINT [DF_rapids_mailbox_rpd_is_read] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_rapids_mailbox] PRIMARY KEY CLUSTERED ([rpd_key] ASC)
);

