﻿CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                        NVARCHAR (128) NOT NULL,
    [UserName]                  NVARCHAR (256) NOT NULL,
    [FirstName]                 VARCHAR (256)  NULL,
    [LastName]                  VARCHAR (256)  NULL,
    [Email]                     NVARCHAR (256) NULL,
    [EmailConfirmed]            BIT            NOT NULL,
    [PasswordHash]              NVARCHAR (MAX) NULL,
    [SecurityStamp]             NVARCHAR (MAX) NULL,
    [PhoneNumber]               NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed]      BIT            NOT NULL,
    [TwoFactorEnabled]          BIT            NOT NULL,
    [LockoutEndDateUtc]         DATETIME       NULL,
    [LockoutEnabled]            BIT            NOT NULL,
    [AccessFailedCount]         INT            NOT NULL,
    [EnableFive9]               BIT            CONSTRAINT [DF_AspNetUsers_EnableFive9] DEFAULT ((0)) NOT NULL,
    [MobilePhone]               NVARCHAR (MAX) NULL,
    [NPINumber]                 VARCHAR (10)   NULL,
    [UserInitial]               VARCHAR (6)    NULL,
    [Gender]                    VARCHAR (10)   NULL,
    [AddressBlock]              VARCHAR (500)  NULL,
    [ContractDate]              DATETIME       NULL,
    [IsActive]                  BIT            CONSTRAINT [DF__AspNetUse__IsAct__1995C0A8] DEFAULT ((1)) NOT NULL,
    [CreatedBy]                 NVARCHAR (128) NOT NULL,
    [CreatedByName]             NVARCHAR (128) NOT NULL,
    [CreatedDate]               DATETIME       CONSTRAINT [DF_AspNetUsers_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedByName]            NVARCHAR (128) NULL,
    [ModifiedBy]                NVARCHAR (128) NULL,
    [ModifiedDate]              DATETIME       NULL,
    [CaseReviewer]              BIT            CONSTRAINT [DF__AspNetUse__CaseR__1DF06171] DEFAULT ((0)) NOT NULL,
    [status_key]                INT            NULL,
    [status_change_date]        DATETIME       NULL,
    [status_change_date_forAll] DATETIME       NULL,
    [status_change_cas_key]     INT            NULL,
    [CredentialCount]           INT            CONSTRAINT [DF_AspNetUsers_CredentialCount] DEFAULT ((0)) NOT NULL,
    [CredentialIndex]           FLOAT (53)     CONSTRAINT [DF_AspNetUsers_CredentialIndex] DEFAULT ((0)) NOT NULL,
    [APISecretKey]              VARCHAR (50)   NULL,
    [APIPassword]               VARCHAR (1000) NULL,
    [IsEEG]                     BIT            CONSTRAINT [DF_AspNetUsers_IsEEG] DEFAULT ((0)) NOT NULL,
    [IsStrokeAlert]             BIT            DEFAULT ((0)) NOT NULL,
    [RequirePasswordReset]      BIT            CONSTRAINT [DF_AspNetUsers_CredentialCount1] DEFAULT ((1)) NOT NULL,
    [PasswordExpirationDate]    DATETIME       NULL,
    [IsDeleted]                 BIT            CONSTRAINT [DF_AspNetUsers_IsDeleted] DEFAULT ((0)) NOT NULL,
	[NHAlert]                 BIT            CONSTRAINT [DF_AspNetUsers_NHAlert] DEFAULT ((0)) NOT NULL,
	[IsDisable]                 BIT            CONSTRAINT [DF_AspNetUsers_IsDisable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUsers_physician_status] FOREIGN KEY ([status_key]) REFERENCES [dbo].[physician_status] ([phs_key])

);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([UserName] ASC);

