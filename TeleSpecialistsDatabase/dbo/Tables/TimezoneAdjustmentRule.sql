CREATE TABLE [dbo].[TimezoneAdjustmentRule] (
    [Id]                                     INT           NOT NULL,
    [TimezoneId]                             INT           NULL,
    [RuleNo]                                 INT           NULL,
    [DateStart]                              DATETIME2 (7) NULL,
    [DateEnd]                                DATETIME2 (7) NULL,
    [DaylightTransitionStartIsFixedDateRule] BIT           NULL,
    [DaylightTransitionStartMonth]           INT           NULL,
    [DaylightTransitionStartDay]             INT           NULL,
    [DaylightTransitionStartWeek]            INT           NULL,
    [DaylightTransitionStartDayOfWeek]       INT           NULL,
    [DaylightTransitionStartTimeOfDay]       TIME (7)      NULL,
    [DaylightTransitionEndIsFixedDateRule]   BIT           NULL,
    [DaylightTransitionEndMonth]             INT           NULL,
    [DaylightTransitionEndDay]               INT           NULL,
    [DaylightTransitionEndWeek]              INT           NULL,
    [DaylightTransitionEndDayOfWeek]         INT           NULL,
    [DaylightTransitionEndTimeOfDay]         TIME (7)      NULL,
    [DaylightDeltaSec]                       INT           NULL,
    CONSTRAINT [PK_TimezoneAdjustmentRule] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TimezoneAdjustmentRule_Timezone] FOREIGN KEY ([TimezoneId]) REFERENCES [dbo].[Timezone] ([Id])
);


GO
ALTER TABLE [dbo].[TimezoneAdjustmentRule] NOCHECK CONSTRAINT [FK_TimezoneAdjustmentRule_Timezone];

