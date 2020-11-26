CREATE TABLE [dbo].[contact] (
    [cnt_key]              INT              IDENTITY (1, 1) NOT NULL,
    [cnt_first_name]       VARCHAR (100)    NULL,
    [cnt_last_name]        VARCHAR (100)    NULL,
    [cnt_role]             VARCHAR (50)     NULL,
    [cnt_primary_phone]    VARCHAR (20)     NULL,
    [cnt_mobile_phone]     VARCHAR (20)     NULL,
    [cnt_email]            VARCHAR (100)    NULL,
    [cnt_is_active]        BIT              CONSTRAINT [DF_contact_cnt_is_active] DEFAULT ((0)) NOT NULL,
    [cnt_created_date]     DATETIME         NOT NULL,
    [cnt_created_by]       NVARCHAR (128)   NULL,
    [cnt_modified_date]    DATETIME         NULL,
    [cnt_modified_by]      NVARCHAR (128)   NULL,
    [cnt_fac_key]          UNIQUEIDENTIFIER NOT NULL,
    [cnt_created_by_name]  VARCHAR (300)    NULL,
    [cnt_modified_by_name] VARCHAR (300)    NULL,
    [cnt_role_ucd_key]     INT              NULL,
    [cnt_is_deleted]       BIT              CONSTRAINT [DF_contact_cnt_is_deleted] DEFAULT ((0)) NOT NULL,
    [cnt_extension]        VARCHAR (20)     NULL,
    CONSTRAINT [PK_contact] PRIMARY KEY CLUSTERED ([cnt_key] ASC),
    CONSTRAINT [FK_contact_contact] FOREIGN KEY ([cnt_role_ucd_key]) REFERENCES [dbo].[ucl_data] ([ucd_key]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_contact_facility] FOREIGN KEY ([cnt_fac_key]) REFERENCES [dbo].[facility] ([fac_key]) ON DELETE CASCADE ON UPDATE CASCADE
);

