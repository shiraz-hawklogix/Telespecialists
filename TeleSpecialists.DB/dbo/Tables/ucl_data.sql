CREATE TABLE [dbo].[ucl_data] (
    [ucd_key]           INT            IDENTITY (1, 1) NOT NULL,
    [ucd_title]         NVARCHAR (150) NOT NULL,
    [ucd_description]   NVARCHAR (300) NULL,
    [ucd_is_default]    BIT            NOT NULL,
    [ucd_is_locked]     BIT            CONSTRAINT [DF_ucl_data_ucd_is_locked] DEFAULT ((0)) NOT NULL,
    [ucd_sort_order]    INT            NOT NULL,
    [ucd_unique_id]     VARCHAR (128)  NULL,
    [ucd_ucl_key]       INT            NULL,
    [ucd_is_deleted]    BIT            NOT NULL,
    [ucd_is_active]     BIT            NOT NULL,
    [ucd_created_date]  DATETIME       NULL,
    [ucd_created_by]    NVARCHAR (128) NULL,
    [ucd_modified_date] DATETIME       NULL,
    [ucd_modified_by]   NVARCHAR (128) NULL,
    CONSTRAINT [PK_ucl_data] PRIMARY KEY CLUSTERED ([ucd_key] ASC),
    CONSTRAINT [FK_ucl_data_ucl] FOREIGN KEY ([ucd_ucl_key]) REFERENCES [dbo].[ucl] ([ucl_key]) ON UPDATE CASCADE
);

