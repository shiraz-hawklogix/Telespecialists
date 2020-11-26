CREATE TABLE [dbo].[quality_goals] (
    [qag_key]        INT              IDENTITY (1, 1) NOT NULL,
    [qag_fac_key]    UNIQUEIDENTIFIER NULL,
    [qag_time_frame] NVARCHAR (100)   NULL,
    CONSTRAINT [PK_quality_goals] PRIMARY KEY CLUSTERED ([qag_key] ASC),
    CONSTRAINT [FK_quality_goals_facility] FOREIGN KEY ([qag_fac_key]) REFERENCES [dbo].[facility] ([fac_key])
);


GO
ALTER TABLE [dbo].[quality_goals] NOCHECK CONSTRAINT [FK_quality_goals_facility];

