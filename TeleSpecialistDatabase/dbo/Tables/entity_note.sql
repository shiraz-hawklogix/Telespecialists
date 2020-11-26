CREATE TABLE [dbo].[entity_note] (
    [etn_key]              INT            IDENTITY (1, 1) NOT NULL,
    [etn_notes]            TEXT           NOT NULL,
    [etn_is_active]        BIT            CONSTRAINT [DF_facility_note_fcn_is_active] DEFAULT ((1)) NOT NULL,
    [etn_created_by]       NVARCHAR (128) NOT NULL,
    [etn_created_date]     DATETIME       NOT NULL,
    [etn_modified_by]      NVARCHAR (128) NULL,
    [etn_modified_date]    DATETIME       NULL,
    [etn_entity_key]       NVARCHAR (128) NOT NULL,
    [etn_ntt_key]          INT            NOT NULL,
    [etn_ent_key]          INT            NOT NULL,
    [etn_display_on_open]  BIT            CONSTRAINT [DF__tmp_ms_xx__etn_d__2B76106F] DEFAULT ((0)) NOT NULL,
    [etn_created_by_name]  NVARCHAR (300) NULL,
    [etn_modified_by_name] NVARCHAR (300) NULL,
    CONSTRAINT [PK_facility_note] PRIMARY KEY CLUSTERED ([etn_key] ASC)
);

