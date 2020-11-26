CREATE TABLE [dbo].[user_access] (
    [user_key]         INT            IDENTITY (1, 1) NOT NULL,
    [user_role_key]    NVARCHAR (128) NULL,
    [user_id]          NVARCHAR (128) NULL,
    [user_com_key]     INT            NULL,
    [user_isAllowed]   BIT            NOT NULL,
    [user_createddate] DATETIME       NULL,
    [user_createdBy]   NVARCHAR (128) NULL,
    [user_updateddate] DATETIME       NULL,
    [user_updatedBy]   NVARCHAR (128) NULL,
    CONSTRAINT [PK_user_access] PRIMARY KEY CLUSTERED ([user_key] ASC)
);

