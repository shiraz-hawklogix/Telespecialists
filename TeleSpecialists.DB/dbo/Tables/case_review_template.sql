CREATE TABLE [dbo].[case_review_template](
	[crt_key] [int] IDENTITY(1,1) NOT NULL,
	[crt_cas_key] [int] NULL,
	[crt_template_html] [nvarchar](max) NULL,
	[crt_finalize_date] [datetime] NULL,
	[crt_created_date] [datetime] NULL,
	[crt_created_by] [nvarchar](128) NULL,
	[crt_created_by_name] [varchar](300) NULL,
	[crt_modified_date] [datetime] NULL,
	[crt_modified_by] [nvarchar](128) NULL,
	[crt_modified_by_name] [varchar](300) NULL,
 CONSTRAINT [PK_case_review_template] PRIMARY KEY CLUSTERED 
(
	[crt_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[case_review_template]  WITH CHECK ADD  CONSTRAINT [FK_case_review_template_case] FOREIGN KEY([crt_cas_key])
REFERENCES [dbo].[case] ([cas_key])
GO

ALTER TABLE [dbo].[case_review_template] CHECK CONSTRAINT [FK_case_review_template_case]
GO


