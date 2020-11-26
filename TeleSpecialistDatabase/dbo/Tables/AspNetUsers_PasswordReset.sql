CREATE TABLE [dbo].[AspNetUsers_PasswordReset] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Code]           NVARCHAR (MAX) NULL,
    [ExpirationTime] DATETIME       NULL,
    [UserName]       NVARCHAR (256) NOT NULL,
    [AspNetUserId]   NVARCHAR (128) NOT NULL,
    [CreatedBy]      NVARCHAR (128) NOT NULL,
    [CreatedByName]  NVARCHAR (128) NOT NULL,
    [CreatedDate]    DATETIME       CONSTRAINT [DF_AspNetUsers_PasswordReset_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedByName] NVARCHAR (128) NULL,
    [ModifiedBy]     NVARCHAR (128) NULL,
    [ModifiedDate]   DATETIME       NULL,
    CONSTRAINT [PK_AspNetUsers_PasswordReset] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUsers_PasswordReset_AspNetUsers] FOREIGN KEY ([AspNetUserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

