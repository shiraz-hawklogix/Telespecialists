CREATE TYPE [dbo].[EAlertReSender] AS TABLE (
    [raw_result]        NVARCHAR (MAX)  NULL,
    [error_code]        VARCHAR (10)    NULL,
    [error_description] NVARCHAR (1000) NULL,
    [cas_key]           INT             NULL,
    [reprocessed_date]  DATETIME        NULL,
    [error_email_date]  DATETIME        NULL);

