CREATE TABLE [dbo].[facility_contract] (
    [fct_key]           UNIQUEIDENTIFIER NOT NULL,
    [fct_start_date]    DATETIME         NULL,
    [fct_end_date]      DATETIME         NULL,
    [fct_cvr_key]       INT              NOT NULL,
    [fct_created_date]  DATETIME         NOT NULL,
    [fct_created_by]    NVARCHAR (128)   NOT NULL,
    [fct_modified_date] DATETIME         NULL,
    [fct_modified_by]   NVARCHAR (128)   NULL,
    [fct_is_active]     BIT              CONSTRAINT [DF_facility_contract_fct_is_active] DEFAULT ((1)) NOT NULL,
    [fct_service_calc]  AS               ([dbo].[GetFacilityContractServices]([fct_key])),
    CONSTRAINT [PK_facility_contract_1] PRIMARY KEY CLUSTERED ([fct_key] ASC),
    CONSTRAINT [FK_facility_contract_facility] FOREIGN KEY ([fct_key]) REFERENCES [dbo].[facility] ([fac_key])
);

