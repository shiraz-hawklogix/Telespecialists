CREATE TABLE [dbo].[Timezone] (
    [Id]                         INT            NOT NULL,
    [Identifier]                 NVARCHAR (100) NULL,
    [StandardName]               NVARCHAR (100) NULL,
    [DisplayName]                NVARCHAR (100) NULL,
    [DaylightName]               NVARCHAR (100) NULL,
    [SupportsDaylightSavingTime] BIT            NULL,
    [BaseUtcOffsetSec]           INT            NULL,
    CONSTRAINT [PK_Timezone] PRIMARY KEY CLUSTERED ([Id] ASC)
);

