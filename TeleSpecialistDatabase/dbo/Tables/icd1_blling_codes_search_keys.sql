CREATE TABLE [dbo].[icd1_blling_codes_search_keys] (
    [id]        INT           IDENTITY (1, 1) NOT NULL,
    [name]      VARCHAR (200) NULL,
    [is_active] BIT           NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

