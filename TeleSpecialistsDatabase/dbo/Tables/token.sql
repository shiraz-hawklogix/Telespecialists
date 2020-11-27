CREATE TABLE [dbo].[token] (
    [tok_key]         INT            IDENTITY (1, 1) NOT NULL,
    [tok_phy_key]     NVARCHAR (128) NULL,
    [tok_phy_token]   NVARCHAR (MAX) NULL,
    [tok_device_type] NVARCHAR (50)  NULL,
    CONSTRAINT [PK_token] PRIMARY KEY CLUSTERED ([tok_key] ASC),
    CONSTRAINT [FK_token_AspNetUsers] FOREIGN KEY ([tok_phy_key]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
ALTER TABLE [dbo].[token] NOCHECK CONSTRAINT [FK_token_AspNetUsers];

