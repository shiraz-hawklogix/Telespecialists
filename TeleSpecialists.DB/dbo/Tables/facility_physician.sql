CREATE TABLE [dbo].[facility_physician] (
    [fap_key]                                      INT              IDENTITY (1, 1) NOT NULL,
    [fap_fac_key]                                  UNIQUEIDENTIFIER NOT NULL,
    [fap_user_key]                                 NVARCHAR (128)   NOT NULL,
    [fap_start_date]                               DATETIME         NULL,
    [fap_end_date]                                 DATETIME         NULL,
    [fap_is_on_boarded]                            BIT              CONSTRAINT [DF_facility_physician_fap_is_on_boarded] DEFAULT ((0)) NOT NULL,
    [fap_modified_date]                            DATETIME         NULL,
    [fap_is_override]                              BIT              CONSTRAINT [DF_facility_physician_fap_is_override] DEFAULT ((0)) NOT NULL,
    [fap_override_start]                           DATETIME         NULL,
    [fap_override_hours]                           INT              NULL,
    [fap_onboarding_complete_provider_active_date] DATETIME         NULL,
    [fap_is_active]                                BIT              CONSTRAINT [DF_facility_physician_fap_is_active] DEFAULT ((0)) NOT NULL,
    [fap_created_by]                               NVARCHAR (128)   NOT NULL,
    [fap_created_by_name]                          VARCHAR (300)    NULL,
    [fap_created_date]                             DATETIME         NOT NULL,
    [fap_modified_by]                              NVARCHAR (128)   NULL,
    [fap_modified_by_name]                         VARCHAR (300)    NULL,
    [fap_is_hide_pending_onboarding]               BIT              CONSTRAINT [DF_facility_physician_fap_is_hide_pending_onboarding] DEFAULT ((0)) NOT NULL,
    [fap_onboarded_date]                           DATETIME         NULL,
    [fap_onboarded_by]                             NVARCHAR (128)   NULL,
    [fap_onboarded_by_name]                        VARCHAR (300)    NULL,
    [fap_hide]									   BIT              DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_facility_physician] PRIMARY KEY CLUSTERED ([fap_key] ASC),
    CONSTRAINT [FK_facility_physician_AspNetUsers] FOREIGN KEY ([fap_user_key]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_facility_physician_facility] FOREIGN KEY ([fap_fac_key]) REFERENCES [dbo].[facility] ([fac_key]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [NIX_user_facility]
    ON [dbo].[facility_physician]([fap_user_key] ASC, [fap_fac_key] ASC, [fap_is_active] ASC);

