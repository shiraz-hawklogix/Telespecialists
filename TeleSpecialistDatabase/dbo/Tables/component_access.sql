CREATE TABLE [dbo].[component_access] (
    [cac_key]        INT            IDENTITY (1, 1) NOT NULL,
    [cac_roleid]     NVARCHAR (128) NULL,
    [cac_com_key]    INT            NULL,
    [cac_addedby]    VARCHAR (300)  NULL,
    [cac_addeddate]  DATETIME       NULL,
    [cac_modifiedby] VARCHAR (300)  NULL,
    [cac_modifiedon] DATETIME       NULL,
    [cac_isAllowed]  BIT            NOT NULL,
    CONSTRAINT [PK_ComponentAccess] PRIMARY KEY CLUSTERED ([cac_key] ASC),
    CONSTRAINT [FK_ComponentAccess_Component_Id] FOREIGN KEY ([cac_com_key]) REFERENCES [dbo].[components] ([com_key])
);

