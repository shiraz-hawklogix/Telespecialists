CREATE TABLE [dbo].[web2campaign_log] (
    [wcl_key]                    INT             IDENTITY (1, 1) NOT NULL,
    [wcl_error_code]             VARCHAR (10)    NULL,
    [wcl_error_description]      NVARCHAR (1000) NULL,
    [wcl_raw_result]             NVARCHAR (MAX)  NULL,
    [wcl_user_agent]             VARCHAR (500)   NOT NULL,
    [wcl_browser_name]           VARCHAR (50)    NOT NULL,
    [wcl_created_by]             NVARCHAR (128)  NOT NULL,
    [wcl_created_by_name]        VARCHAR (300)   NOT NULL,
    [wcl_created_date]           DATETIME        NOT NULL,
    [wcl_cas_key]                INT             NULL,
    [wcl_request_url]            VARCHAR (500)   NULL,
    [wcl_request_send_time]      DATETIME        NULL,
    [wcl_response_received_time] DATETIME        NULL,
    [wcl_reprocessed_date]       DATETIME        NULL,
    [wcl_error_mail_date]        DATETIME        NULL,
    [wcl_request_retry_count]    INT             NULL,
    CONSTRAINT [PK_web2campaign_log] PRIMARY KEY CLUSTERED ([wcl_key] ASC)
);







