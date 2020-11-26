CREATE TABLE [dbo].[mute_firebase_notification] (
    [mfn_key]          INT            IDENTITY (1, 1) NOT NULL,
    [mfn_user_key]     NVARCHAR (128) NULL,
    [mfn_firebase_uid] NVARCHAR (100) NULL,
    [mfn_created_on]   DATETIME       NULL,
    [mfn_start_from]   DATETIME       NULL,
    [mfn_to_end]       DATETIME       NULL,
    CONSTRAINT [PK_mute_firebase_notification] PRIMARY KEY CLUSTERED ([mfn_key] ASC)
);

