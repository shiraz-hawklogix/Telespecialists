CREATE TABLE [dbo].[physician_percentage_rate](
	[ppr_key] [int] IDENTITY(1,1) NOT NULL,
	[ppr_phy_key] [nvarchar](128) NULL,
	[ppr_shift_id] [int] NULL,
	[ppr_shift_name] [nvarchar](50) NULL,
	[ppr_percentage] [decimal](18, 2) NULL,
	[ppr_created_by] [nvarchar](128) NULL,
	[ppr_created_by_name] [nvarchar](100) NULL,
	[ppr_created_date] [datetime] NULL,
	[ppr_modified_by] [nvarchar](128) NULL,
	[ppr_modified_by_name] [nvarchar](100) NULL,
	[ppr_modified_date] [datetime] NULL,
	[ppr_start_date] [datetime] NULL,
	[ppr_end_date] [datetime] NULL,
 CONSTRAINT [PK_physician_percentage_rate] PRIMARY KEY CLUSTERED 
(
	[ppr_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO