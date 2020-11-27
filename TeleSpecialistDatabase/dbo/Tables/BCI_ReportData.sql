CREATE TABLE [dbo].[BCI_ReportData] (
    [BCI_ID]        INT            IDENTITY (1, 1) NOT NULL,
    [Phy_Name]      NVARCHAR (100) NULL,
    [Phy_Bci_Value] NVARCHAR (100) NULL,
    [Phy_Id]        NVARCHAR (100) NULL,
	[PhysicianID]         NVARCHAR (50) NULL,
    CONSTRAINT [PK_BCI_ReportData] PRIMARY KEY CLUSTERED ([BCI_ID] ASC)
);

