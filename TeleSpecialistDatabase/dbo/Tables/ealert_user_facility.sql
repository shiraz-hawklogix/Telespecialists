CREATE TABLE [dbo].[ealert_user_facility] (
    [efa_key]              INT              IDENTITY (1, 1) NOT NULL,
    [efa_fac_key]          UNIQUEIDENTIFIER NOT NULL,
    [efa_user_key]         NVARCHAR (128)   NOT NULL,
    [efa_is_active]        BIT              CONSTRAINT [DF_ealert_userFacility_efa_is_active_1] DEFAULT ((0)) NOT NULL,
    [efa_is_default]       BIT              CONSTRAINT [DF_ealert_userFacility_efa_default_1] DEFAULT ((0)) NOT NULL,
    [efa_created_by]       NVARCHAR (128)   NOT NULL,
    [efa_created_by_name]  VARCHAR (300)    NULL,
    [efa_created_date]     DATETIME         NOT NULL,
    [efa_modified_by]      NVARCHAR (128)   NULL,
    [efa_modified_by_name] VARCHAR (300)    NULL,
    [efa_modified_date]    DATETIME         NULL,
    CONSTRAINT [PK_ealert_userFacility_1] PRIMARY KEY CLUSTERED ([efa_key] ASC),
    CONSTRAINT [FK_ealert_user_facility_AspNetUsers] FOREIGN KEY ([efa_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]),
    CONSTRAINT [FK_ealert_user_facility_facility] FOREIGN KEY ([efa_fac_key]) REFERENCES [dbo].[facility] ([fac_key])
);

