CREATE TABLE [dbo].[telecare_counters] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [counter_text]  VARCHAR (100) NULL,
    [counter_value] BIGINT        NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);

