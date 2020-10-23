CREATE TABLE [dbo].[audit_records]
(
	[aud_key]			INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[aud_username]		NVARCHAR(256) NULL,
	[aud_action]		NVARCHAR(300) NULL,
	[aud_timestamp]		DATETIME NULL,
	[aud_browser]		NVARCHAR(100) NULL,
	[aud_ip_address]	NVARCHAR(100) NULL,
	[aud_host_name]		NVARCHAR(100) NULL,
	[aud_others]		NVARCHAR(MAX) NULL,
)
