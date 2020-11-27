CREATE TABLE [dbo].[firebase_usersemail] (
    [fre_key]            INT            IDENTITY (1, 1) NOT NULL,
    [fre_userId]         NVARCHAR (128) NULL,
    [fre_firstname]      NVARCHAR (128) NULL,
    [fre_lastname]       NVARCHAR (128) NULL,
    [fre_email]          NVARCHAR (150) NULL,
    [fre_firebase_email] NVARCHAR (150) NULL,
    [fre_firebase_uid]   NVARCHAR (200) NULL,
    [fre_profileimg]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_FireBaseUsersEmail] PRIMARY KEY CLUSTERED ([fre_key] ASC)
);

