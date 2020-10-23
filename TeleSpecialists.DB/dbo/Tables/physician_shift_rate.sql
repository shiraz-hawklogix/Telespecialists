CREATE TABLE [dbo].[physician_shift_rate](
	[psr_key] [int] IDENTITY(1,1) NOT NULL,
	[psr_phy_key] [nvarchar](128) NULL,
	[psr_shift] [int] NULL,
	[psr_shift_name] [nvarchar](50) NULL,
	[psr_rate] [decimal](18, 2) NULL,
	[psr_created_by] [nvarchar](128) NULL,
	[psr_created_date] [datetime] NULL,
	[psr_created_by_name] [nvarchar](100) NULL,
	[psr_modified_by] [nvarchar](128) NULL,
	[psr_modified_date] [datetime] NULL,
	[psr_modified_by_name] [nvarchar](100) NULL,
	[psr_start_date] [datetime] NULL,
	[psr_end_date] [datetime] NULL,
 CONSTRAINT [PK_physician_shift_rate] PRIMARY KEY CLUSTERED 
(
	[psr_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[physician_rate]  WITH CHECK ADD  CONSTRAINT [FK_Rate_Rate] FOREIGN KEY([rat_phy_key])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[physician_rate] CHECK CONSTRAINT [FK_Rate_Rate]
GO