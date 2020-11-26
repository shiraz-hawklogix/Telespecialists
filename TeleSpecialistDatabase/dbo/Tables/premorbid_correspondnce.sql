CREATE TABLE [dbo].[premorbid_correspondnce] (
    [Id]                                            INT            IDENTITY (1, 1) NOT NULL,
    [pmc_cas_key]                                   INT            NULL,
    [pmc_cas_premorbid_patient_phone]               VARCHAR (50)   NULL,
    [pmc_cas_premorbid_datetime_of_contact]         DATETIME       NULL,
    [pmc_cas_premorbid_spokewith]                   INT            NULL,
    [pmc_cas_premorbid_comments]                    NVARCHAR (MAX) NULL,
    [pmc_cas_premorbid_successful_or_unsuccessful]  INT            NULL,
    [pmc_cas_premorbid_completedby]                 NVARCHAR (150) NULL,
    [pmc_cas_patient_satisfaction_video_experience] INT            NULL,
    [pmc_cas_patient_satisfaction_communication]    INT            NULL,
    [pmc_cas_willing_todo_interview]                INT            NULL,
    [pmc_cas_consent_sent]                          INT            NULL,
    [pmc_cas_consent_received]                      INT            NULL,
    CONSTRAINT [PK_premorbid_correspondnce] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_premorbid_correspondnce_case] FOREIGN KEY ([pmc_cas_key]) REFERENCES [dbo].[case] ([cas_key])
);

