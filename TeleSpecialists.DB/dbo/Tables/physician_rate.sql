CREATE TABLE [dbo].[physician_rate](
	[rat_key] [int] IDENTITY(1,1) NOT NULL,
	[rat_phy_key] [nvarchar](128) NULL,
	[rat_starting] [int] NULL,
	[rat_ending] [int] NULL,
	[rat_range] [nvarchar](50) NULL,
	[rat_price] [decimal](18, 2) NULL,
	[rat_shift_hour] [int] NULL,
	[rat_shift_id] [int] NULL,
	[rat_shift_name] [nvarchar](150) NULL,
	[rat_cas_id] [int] NULL,
	[rat_created_by] [nvarchar](128) NULL,
	[rat_created_date] [datetime] NULL,
	[rat_created_by_name] [nvarchar](100) NULL,
	[rat_modified_by] [nvarchar](128) NULL,
	[rat_modified_date] [datetime] NULL,
	[rat_modified_by_name] [nvarchar](100) NULL,
	[rat_start_date] [datetime] NULL,
	[rat_end_date] [datetime] NULL,
 CONSTRAINT [PK_Rate] PRIMARY KEY CLUSTERED 
(
	[rat_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO