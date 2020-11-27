CREATE TABLE [dbo].[AspNetRoles] (
    [Id]            NVARCHAR (128) NOT NULL,
    [Name]          NVARCHAR (256) NOT NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [Discriminator] NVARCHAR (128) CONSTRAINT [DF__AspNetRol__Discr__29572725] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([Name] ASC);

