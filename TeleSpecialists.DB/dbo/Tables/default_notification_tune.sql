
CREATE TABLE [dbo].[default_notification_tune](
	[dnt_key] [int] IDENTITY(1,1) NOT NULL,
	[dnt_file_path] [nvarchar](150) NULL,
	[dnt_selected_tune] [nvarchar](100) NULL,
	[dnt_created_by] [nvarchar](128) NULL,
	[dnt_created_by_name] [nvarchar](100) NULL,
	[dnt_created_date] [datetime] NULL,
	[dnt_modified_by] [nvarchar](128) NULL,
	[dnt_modified_by_name] [nvarchar](100) NULL,
	[dnt_modified_date] [datetime] NULL,
	[dnt_is_active] [bit] NULL,
 CONSTRAINT [PK_default_notification_tune] PRIMARY KEY CLUSTERED 
(
	[dnt_key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]