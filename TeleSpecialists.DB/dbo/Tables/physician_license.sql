﻿CREATE TABLE [dbo].[physician_license] (
    [phl_key]                  UNIQUEIDENTIFIER CONSTRAINT [DF_physician_license_phl_key] DEFAULT (newid()) NOT NULL,
    [phl_user_key]             NVARCHAR (128)   NOT NULL,
    [phl_license_type]         UNIQUEIDENTIFIER NULL,
    [phl_license_state]        INT              NULL,
    [phl_license_number]       VARCHAR (50)     NULL,
    [phl_issued_date]          DATETIME         NOT NULL,
    [phl_expired_date]         DATETIME         NULL,
    [phl_licensure_board_id]   NVARCHAR (128)   NULL,
    [phl_licensure_board_name] VARCHAR (300)    NULL,
    [phl_is_in_use]            BIT              CONSTRAINT [DF_physician_license_phl_In_use] DEFAULT ((0)) NOT NULL,
    [phl_is_active]            BIT              NOT NULL,
    [phl_created_date]         DATETIME         NULL,
    [phl_created_by]           NVARCHAR (128)   NULL,
    [phl_created_by_name]      VARCHAR (300)    NULL,
    [phl_modified_by]          NVARCHAR (128)   NULL,
    [phl_modified_by_name]     VARCHAR (300)    NULL,
    [phl_modified_date]        DATETIME         NULL,
    CONSTRAINT [PK_physician_license] PRIMARY KEY CLUSTERED ([phl_key] ASC),
    CONSTRAINT [FK_physician_license_AspNetUsers] FOREIGN KEY ([phl_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);

