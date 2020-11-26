CREATE TABLE [dbo].[firebase_users] (
    [frb_key]             INT            IDENTITY (1, 1) NOT NULL,
    [frb_userId]          NVARCHAR (128) NULL,
    [frb_user_firebaseId] NVARCHAR (128) NULL,
    [frb_deviceTokenId]   NVARCHAR (250) NULL,
    CONSTRAINT [PK_FireBaseUsers] PRIMARY KEY CLUSTERED ([frb_key] ASC)
);

