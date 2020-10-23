CREATE TABLE [dbo].[quality_goals](
	[qag_key] [int] IDENTITY(1,1) NOT NULL,
	[qag_fac_key] [uniqueidentifier] NULL,
	[qag_time_frame] [nvarchar](100) NULL,
 CONSTRAINT [PK_quality_goals] PRIMARY KEY CLUSTERED 
(
	[qag_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[quality_goals]  WITH CHECK ADD  CONSTRAINT [FK_quality_goals_facility] FOREIGN KEY([qag_fac_key])
REFERENCES [dbo].[facility] ([fac_key])
GO

ALTER TABLE [dbo].[quality_goals] CHECK CONSTRAINT [FK_quality_goals_facility]
GO


