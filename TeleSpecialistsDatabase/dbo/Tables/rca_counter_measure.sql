CREATE TABLE [dbo].[rca_counter_measure] (
    [rca_Id]                      INT            IDENTITY (1, 1) NOT NULL,
    [rca_key_id]                  INT            NOT NULL,
    [rca_root_cause]              NVARCHAR (250) NULL,
    [rca_proposed_countermeasure] NVARCHAR (MAX) NULL,
    [rca_responsible_party]       NVARCHAR (MAX) NULL,
    [rca_proposed_due_date]       DATETIME       NULL,
    [rca_rootcause_id]            INT            NULL,
    [rca_completed_date]          DATETIME       NULL,
    CONSTRAINT [PK_rca_counter_measure] PRIMARY KEY CLUSTERED ([rca_Id] ASC),
    CONSTRAINT [FK_rca_counter_measure_case] FOREIGN KEY ([rca_key_id]) REFERENCES [dbo].[case] ([cas_key])
);


GO
ALTER TABLE [dbo].[rca_counter_measure] NOCHECK CONSTRAINT [FK_rca_counter_measure_case];

