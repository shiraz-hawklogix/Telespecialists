CREATE TABLE [dbo].[facility] (
    [fac_key]                          UNIQUEIDENTIFIER CONSTRAINT [DF_facility_fac_key] DEFAULT (newid()) NOT NULL,
    [fac_name]                         VARCHAR (500)    NOT NULL,
    [fac_timezone]                     VARCHAR (200)    NULL,
    [fac_address_line1]                VARCHAR (200)    NULL,
    [fac_address_line2]                VARCHAR (200)    NULL,
    [fac_city]                         VARCHAR (100)    NULL,
    [fac_zip]                          VARCHAR (20)     NULL,
    [fac_ucd_key_system]               INT              NULL,
    [fac_stt_key]                      INT              NULL,
    [fac_fct_key]                      INT              NULL,
    [fac_cst_key]                      INT              NULL,
    [fac_sct_key]                      INT              NULL,
    [fac_md_staff_source_name]         VARCHAR (500)    NULL,
    [fac_md_staff_reference_source_id] NVARCHAR (128)   NULL,
    [fac_is_active]                    BIT              CONSTRAINT [DF_facility_fac_is_active] DEFAULT ((1)) NOT NULL,
    [fac_created_by]                   NVARCHAR (128)   NOT NULL,
    [fac_created_by_name]              VARCHAR (300)    NULL,
    [fac_created_date]                 DATETIME         CONSTRAINT [DF_facility_fac_created_date] DEFAULT (getdate()) NOT NULL,
    [fac_modified_by]                  NVARCHAR (128)   NULL,
    [fac_modified_by_name]             VARCHAR (300)    NULL,
    [fac_modified_date]                DATETIME         NULL,
    [fac_not_templated_used]           BIT              CONSTRAINT [DF_facility_fac_not_templated_used] DEFAULT ((1)) NOT NULL,
    [fac_go_live]                      BIT              CONSTRAINT [DF_facility_fac_go_live] DEFAULT ((0)) NOT NULL,
    [qps_number]                       INT              NULL,
    [fac_emr]                          NVARCHAR (MAX)   NULL,
    [fac_is_pac]                       BIT              CONSTRAINT [fac_is_pac] DEFAULT ((0)) NOT NULL,
    [fac_ucd_region_key]               INT              NULL,
    [fac_freestanding_fac_key]         UNIQUEIDENTIFIER NULL,
    [fac_emr_portal]                   NVARCHAR (MAX)   NULL,
    [fac_cart_type]                    NVARCHAR (MAX)   NULL,
    [fac_transfer_process]             NVARCHAR (MAX)   NULL,
    [fac_ai_software]                  NVARCHAR (MAX)   NULL,
    [fac_tpa_orderset]                 NVARCHAR (MAX)   NULL,
    [fac_imaging_protocol]             NVARCHAR (MAX)   NULL,
    [fac_ucd_bed_size]                 INT              NULL,
    [fac_cart_numbers]                 NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_facility] PRIMARY KEY CLUSTERED ([fac_key] ASC)
);





