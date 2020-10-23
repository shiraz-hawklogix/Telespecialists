
CREATE TABLE [dbo].[alarm_tunes] (
    [alt_key]              INT            IDENTITY (1, 1) NOT NULL,
    [alt_phy_key]          NVARCHAR (128) NULL,
    [alt_audio_path]       NVARCHAR (150) NULL,
    [alt_selected_audio]   NVARCHAR (100) NULL,
    [alt_file_name]        NVARCHAR (100) NULL,
    [alt_created_by]       NVARCHAR (128) NULL,
    [alt_created_by_name]  NVARCHAR (100) NULL,
    [alt_created_date]     DATETIME       NULL,
    [alt_modified_by]      NVARCHAR (128) NULL,
    [alt_modified_by_name] NVARCHAR (100) NULL,
    [alt_modified_date]    DATETIME       NULL,
    [alt_ent_key]          INT            NULL,
    CONSTRAINT [PK_Alarm_Tunes] PRIMARY KEY CLUSTERED ([alt_key] ASC)
);

