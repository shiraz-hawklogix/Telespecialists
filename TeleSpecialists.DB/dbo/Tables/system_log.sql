CREATE TABLE [dbo].[system_log] (
    [log_key]          BIGINT        IDENTITY (1, 1) NOT NULL,
    [log_service_type] VARCHAR (200) NOT NULL,
    [log_status]       VARCHAR (50)  NOT NULL,
    [log_time]         DATETIME      NOT NULL,
    [log_error]        VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_system_log] PRIMARY KEY CLUSTERED ([log_key] ASC)
);

