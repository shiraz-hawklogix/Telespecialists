CREATE TABLE [dbo].[AspNetUser_Detail] (
    [Id]             NVARCHAR (128) NOT NULL,
    [PhotoBase64]    VARCHAR (MAX)  NOT NULL,
    [CreatedBy]      NVARCHAR (128) NOT NULL,
    [CreatedByName]  NVARCHAR (128) NOT NULL,
    [CreatedDate]    DATETIME       CONSTRAINT [DF_AspNetUser_Detail_CreatedDate] DEFAULT (getdate()) NOT NULL,
    [ModifiedByName] NVARCHAR (128) NULL,
    [ModifiedBy]     NVARCHAR (128) NULL,
    [ModifiedDate]   DATETIME       NULL,
    CONSTRAINT [PK_AspNetUser_Detail] PRIMARY KEY CLUSTERED ([Id] ASC)
);

