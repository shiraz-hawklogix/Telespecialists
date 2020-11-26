CREATE TABLE [dbo].[audit_records] (
    [aud_key]        INT            IDENTITY (1, 1) NOT NULL,
    [aud_username]   NVARCHAR (256) NULL,
    [aud_action]     NVARCHAR (300) NULL,
    [aud_timestamp]  DATETIME       NULL,
    [aud_browser]    NVARCHAR (100) NULL,
    [aud_ip_address] NVARCHAR (100) NULL,
    [aud_host_name]  NVARCHAR (100) NULL,
    [aud_others]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK__audit_re__8DE1CF9618D2FB4E] PRIMARY KEY CLUSTERED ([aud_key] ASC)
);

