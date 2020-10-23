CREATE TABLE [dbo].[pac_case_template](
	[pct_key] [int] IDENTITY(1,1) NOT NULL,
	[pct_pac_key] [int] NULL,
	[pct_template_html] [nvarchar](max) NULL,
	[pct_finalize_date] [datetime] NULL,
	[pct_created_date] [datetime] NULL,
	[pct_created_by] [nvarchar](128) NULL,
	[pct_created_by_name] [varchar](300) NULL,
	[pct_modified_date] [datetime] NULL,
	[pct_modified_by] [nvarchar](128) NULL,
	[pct_modified_by_name] [varchar](300) NULL,
 CONSTRAINT [PK_pac_case_template] PRIMARY KEY CLUSTERED 
(
	[pct_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[pac_case_template]  WITH CHECK ADD  CONSTRAINT [FK_pac_case_template_post_acute_care] FOREIGN KEY([pct_pac_key])
REFERENCES [dbo].[post_acute_care] ([pac_key])
GO

ALTER TABLE [dbo].[pac_case_template] CHECK CONSTRAINT [FK_pac_case_template_post_acute_care]
GO
