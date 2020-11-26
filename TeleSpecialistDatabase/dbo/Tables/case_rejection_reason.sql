CREATE TABLE [dbo].[case_rejection_reason] (
    [crr_key]          INT            IDENTITY (1, 1) NOT NULL,
    [crr_reason]       NVARCHAR (128) NULL,
    [crr_troubleshoot] BIT            NOT NULL,
    [crr_parent_key]   INT            NULL,
    [crr_users]        NVARCHAR (MAX) NULL,
    [crr_created_on]   DATETIME       NULL,
    [crr_created_by]   NVARCHAR (128) NULL,
    [crr_modified_on]  DATETIME       NULL,
    [crr_modified_by]  NVARCHAR (128) NULL,
    CONSTRAINT [PK_case_rejection_reason] PRIMARY KEY CLUSTERED ([crr_key] ASC)
);

