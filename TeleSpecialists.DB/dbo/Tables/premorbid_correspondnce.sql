USE [dbTeleSpecialistsDev]
GO
/****** Object:  Table [dbo].[premorbid_correspondnce]    Script Date: 11/6/2020 4:42:18 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[premorbid_correspondnce](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[pmc_cas_key] [int] NULL,
	[pmc_cas_premorbid_patient_phone] [varchar](50) NULL,
	[pmc_cas_premorbid_datetime_of_contact] [datetime] NULL,
	[pmc_cas_premorbid_spokewith] [int] NULL,
	[pmc_cas_premorbid_comments] [nvarchar](max) NULL,
	[pmc_cas_premorbid_successful_or_unsuccessful] [int] NULL,
	[pmc_cas_premorbid_completedby] [nvarchar](150) NULL,
	[pmc_cas_patient_satisfaction_video_experience] [int] NULL,
	[pmc_cas_patient_satisfaction_communication] [int] NULL,
	[pmc_cas_willing_todo_interview] [int] NULL,
	[pmc_cas_consent_sent] [int] NULL,
	[pmc_cas_consent_received] [int] NULL,
 CONSTRAINT [PK_premorbid_correspondnce] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[premorbid_correspondnce]  WITH CHECK ADD  CONSTRAINT [FK_premorbid_correspondnce_case] FOREIGN KEY([pmc_cas_key])
REFERENCES [dbo].[case] ([cas_key])
GO
ALTER TABLE [dbo].[premorbid_correspondnce] CHECK CONSTRAINT [FK_premorbid_correspondnce_case]
GO
