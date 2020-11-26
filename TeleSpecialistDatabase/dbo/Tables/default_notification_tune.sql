CREATE TABLE [dbo].[default_notification_tune] (
    [dnt_key]              INT            IDENTITY (1, 1) NOT NULL,
    [dnt_file_path]        NVARCHAR (150) NULL,
    [dnt_selected_tune]    NVARCHAR (100) NULL,
    [dnt_created_by]       NVARCHAR (128) NULL,
    [dnt_created_by_name]  NVARCHAR (100) NULL,
    [dnt_created_date]     DATETIME       NULL,
    [dnt_modified_by]      NVARCHAR (128) NULL,
    [dnt_modified_by_name] NVARCHAR (100) NULL,
    [dnt_modified_date]    DATETIME       NULL,
    [dnt_is_active]        BIT            NULL,
    CONSTRAINT [PK_default_notification_tune] PRIMARY KEY CLUSTERED ([dnt_key] ASC)
);

