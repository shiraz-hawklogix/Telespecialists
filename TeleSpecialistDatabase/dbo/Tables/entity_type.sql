CREATE TABLE [dbo].[entity_type] (
    [ent_key]  INT          NOT NULL,
    [ent_name] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_entity_type] PRIMARY KEY CLUSTERED ([ent_key] ASC)
);

