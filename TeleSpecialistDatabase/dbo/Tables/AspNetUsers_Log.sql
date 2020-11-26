CREATE TABLE [dbo].[AspNetUsers_Log] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [PasswordHash_Old]  NVARCHAR (MAX) NULL,
    [SecurityStamp_Old] NVARCHAR (MAX) NULL,
    [PasswordHash_New]  NVARCHAR (MAX) NULL,
    [CreatedBy]         NVARCHAR (128) NOT NULL,
    [CreatedByName]     NVARCHAR (128) NOT NULL,
    [CreatedDate]       DATETIME       CONSTRAINT [DF_AspNetUsers_Log_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedByName]    NVARCHAR (128) NULL,
    [ModifiedBy]        NVARCHAR (128) NULL,
    [ModifiedDate]      DATETIME       NULL,
    [AspNetUsersId]     NVARCHAR (128) NULL,
    CONSTRAINT [PK_AspNetUsers_Log] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUsers_Log_AspNetUsers] FOREIGN KEY ([AspNetUsersId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

