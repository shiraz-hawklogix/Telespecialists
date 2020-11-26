CREATE TABLE [dbo].[data_import_log] (
    [dil_key]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [dil_type]         VARCHAR (50)   NOT NULL,
    [dil_request_id]   VARCHAR (128)  NOT NULL,
    [dil_provider]     VARCHAR (128)  NULL,
    [dil_message]      VARCHAR (MAX)  NOT NULL,
    [dil_created_by]   NVARCHAR (128) NOT NULL,
    [dil_created_date] DATETIME       CONSTRAINT [DF_data_import_log_dil_created_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_data_import_log] PRIMARY KEY CLUSTERED ([dil_key] ASC)
);

