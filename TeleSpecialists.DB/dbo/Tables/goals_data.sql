CREATE TABLE [dbo].[goals_data](
	[gd_key] [int] IDENTITY(1,1) NOT NULL,
	[gd_qag_key] [int] NULL,
	[qag_door_to_TS_notification_ave_minutes] [nvarchar](50) NULL,
	[qag_door_to_TS_notification_median_minutes] [nvarchar](50) NULL,
	[qag_percent10_min_or_less_activation_EMS] [nvarchar](50) NULL,
	[qag_percent10_min_or_less_activation_PV] [nvarchar](50) NULL,
	[qag_percent10_min_or_less_activation_Inpt] [nvarchar](50) NULL,
	[qag_TS_notification_to_response_average_minute] [nvarchar](50) NULL,
	[qag_TS_notification_to_response_median_minute] [nvarchar](50) NULL,
	[qag_percent_TS_at_bedside_grterthan10_minutes] [nvarchar](50) NULL,
	[qag_alteplase_administered] [nvarchar](50) NULL,
	[qag_door_to_needle_average] [nvarchar](50) NULL,
	[qag_door_to_needle_median] [nvarchar](50) NULL,
	[qag_verbal_order_to_administration_average_minutes] [nvarchar](50) NULL,
	[qag_DTN_grter_or_equal_30minutes_percent] [nvarchar](50) NULL,
	[qag_DTN_grter_or_equal_45minutes_percent] [nvarchar](50) NULL,
	[qag_DTN_grter_or_equal_60minutes_percent] [nvarchar](50) NULL,
	[qag_TS_notification_to_needle_grter_or_equal_30minutes_percent] [nvarchar](50) NULL,
	[qag_TS_notification_to_needle_grter_or_equal_45minutes_percent] [nvarchar](50) NULL,
	[qag_TS_notification_to_needle_grter_or_equal_60minutes_percent] [nvarchar](50) NULL,
 CONSTRAINT [PK_goals_data] PRIMARY KEY CLUSTERED 
(
	[gd_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[goals_data]  WITH CHECK ADD  CONSTRAINT [FK_goals_data_quality_goals] FOREIGN KEY([gd_qag_key])
REFERENCES [dbo].[quality_goals] ([qag_key])
GO

ALTER TABLE [dbo].[goals_data] CHECK CONSTRAINT [FK_goals_data_quality_goals]
GO
