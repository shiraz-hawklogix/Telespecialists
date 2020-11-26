CREATE TABLE [dbo].[entity_change_log] (
    [ecl_key]             INT            IDENTITY (1, 1) NOT NULL,
    [ecl_changeset]       TEXT           NULL,
    [ecl_created_by]      NVARCHAR (128) NOT NULL,
    [ecl_created_by_name] VARCHAR (300)  NOT NULL,
    [ecl_created_date]    DATETIME       NOT NULL,
    [ecl_entity_key]      NVARCHAR (128) NOT NULL,
    [ecl_type]            VARCHAR (50)   NOT NULL,
    CONSTRAINT [PK_entity_change_log] PRIMARY KEY CLUSTERED ([ecl_key] ASC)
);

