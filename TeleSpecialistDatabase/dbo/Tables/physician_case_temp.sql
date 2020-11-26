CREATE TABLE [dbo].[physician_case_temp] (
    [pct_key]           UNIQUEIDENTIFIER NOT NULL,
    [pct_phy_key]       NVARCHAR (128)   NOT NULL,
    [pct_cst_key]       INT              NULL,
    [pct_created_date]  DATETIME         NOT NULL,
    [pct_created_by]    NVARCHAR (128)   NOT NULL,
    [pct_modified_date] DATETIME         NULL,
    [pct_modified_by]   NVARCHAR (128)   NULL,
    CONSTRAINT [PK_physician_case_temp] PRIMARY KEY CLUSTERED ([pct_key] ASC)
);

