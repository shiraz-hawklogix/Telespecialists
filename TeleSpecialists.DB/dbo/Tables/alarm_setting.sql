
CREATE TABLE [dbo].[alarm_setting] (
    [als_key]              INT            IDENTITY (1, 1) NOT NULL,
    [als_phy_key]          NVARCHAR (128) NULL,
    [als_audio_path]       NVARCHAR (150) NULL,
    [als_selected_audio]   NVARCHAR (100) NULL,
    [als_file_name]        NVARCHAR (100) NULL,
    [als_created_by]       NVARCHAR (128) NULL,
    [als_created_by_name]  NVARCHAR (100) NULL,
    [als_created_date]     DATETIME       NULL,
    [als_modified_by]      NVARCHAR (128) NULL,
    [als_modified_by_name] NVARCHAR (100) NULL,
    [als_modified_date]    DATETIME       NULL,
    [als_ent_key]          INT            NULL,
    CONSTRAINT [PK_alarm_setting] PRIMARY KEY CLUSTERED ([als_key] ASC)
);

