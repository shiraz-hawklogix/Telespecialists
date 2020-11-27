CREATE TABLE [dbo].[facility_contract_service] (
    [fcs_key]             INT              IDENTITY (1, 1) NOT NULL,
    [fcs_srv_key]         INT              NOT NULL,
    [fcs_fct_key]         UNIQUEIDENTIFIER NOT NULL,
    [fcs_created_date]    DATETIME         NOT NULL,
    [fcs_created_by]      NVARCHAR (128)   NOT NULL,
    [fcs_created_by_name] VARCHAR (300)    NOT NULL,
    [fcs_is_active]       BIT              CONSTRAINT [DF_facility_contract_service_fcs_is_active] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_facility_contract_service] PRIMARY KEY CLUSTERED ([fcs_key] ASC),
    CONSTRAINT [FK_facility_contract_service_facility_contract] FOREIGN KEY ([fcs_fct_key]) REFERENCES [dbo].[facility_contract] ([fct_key]) ON DELETE CASCADE ON UPDATE CASCADE
);

