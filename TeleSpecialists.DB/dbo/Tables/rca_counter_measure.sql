CREATE TABLE [dbo].[rca_counter_measure](
	[rca_Id] [int] IDENTITY(1,1) NOT NULL,
	[rca_key_id] [int] NOT NULL,
	[rca_root_cause] [nvarchar](250) NULL,
	[rca_proposed_countermeasure] [nvarchar](max) NULL,
	[rca_responsible_party] [nvarchar](max) NULL,
	[rca_proposed_due_date] [datetime] NULL,
	[rca_rootcause_id] [int] NULL,
 [rca_completed_date] DATETIME NULL, 
    CONSTRAINT [PK_rca_counter_measure] PRIMARY KEY CLUSTERED 
(
	[rca_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[rca_counter_measure]  WITH CHECK ADD  CONSTRAINT [FK_rca_counter_measure_case] FOREIGN KEY([rca_key_id])
REFERENCES [dbo].[case] ([cas_key])
GO

ALTER TABLE [dbo].[rca_counter_measure] CHECK CONSTRAINT [FK_rca_counter_measure_case]
GO