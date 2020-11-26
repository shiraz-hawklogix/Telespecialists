CREATE TABLE [dbo].[For_Case] (
    [fcs_key]          INT IDENTITY (1, 1) NOT NULL,
    [refresh_requried] BIT NULL,
    CONSTRAINT [PK_For_Case] PRIMARY KEY CLUSTERED ([fcs_key] ASC)
);

