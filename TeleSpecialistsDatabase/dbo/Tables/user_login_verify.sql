CREATE TABLE [dbo].[user_login_verify] (
    [Id]                       INT            IDENTITY (1, 1) NOT NULL,
    [UserId]                   NVARCHAR (128) NULL,
    [IsTwoFactRememberChecked] BIT            DEFAULT ((0)) NULL,
    [TwoFactVerifyExpiryDate]  DATETIME       NULL,
    [MachineName]              VARCHAR (500)  NULL,
    [MachineIpAddress]         VARCHAR (200)  NULL,
    [BrowserKey]               VARCHAR (500)  NULL,
    [IsLoggedIn]               BIT            DEFAULT ((0)) NULL,
    CONSTRAINT [PK_user_login_verify] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_user_login_verify_User_Id] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);

