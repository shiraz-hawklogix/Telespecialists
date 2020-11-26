CREATE TABLE [dbo].[user_log] (
    [ul_key]        INT            IDENTITY (1, 1) NOT NULL,
    [ul_username]   NVARCHAR (256) NULL,
    [ul_action]     NVARCHAR (100) NULL,
    [ul_timestamp]  DATETIME       NULL,
    [ul_browser]    NVARCHAR (100) NULL,
    [ul_ip_address] NVARCHAR (100) NULL,
    [ul_host_name]  NVARCHAR (100) NULL,
    CONSTRAINT [PK__user_log__8D8F8824C4B380B9] PRIMARY KEY CLUSTERED ([ul_key] ASC)
);

