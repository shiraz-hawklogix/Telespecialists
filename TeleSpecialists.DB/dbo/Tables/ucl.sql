CREATE TABLE [dbo].[ucl] (
    [ucl_key]       INT           NOT NULL,
    [ucl_type]      NVARCHAR (50) NOT NULL,
    [ucl_title]     VARCHAR (50)  NOT NULL,
    [ucl_is_active] BIT           CONSTRAINT [DF_ucl_ulc_is_active] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ucl] PRIMARY KEY CLUSTERED ([ucl_key] ASC)
);

