CREATE TABLE [dbo].[components] (
    [com_key]               INT           IDENTITY (1, 1) NOT NULL,
    [com_parentcomponentid] INT           NULL,
    [com_module_name]       VARCHAR (200) NULL,
    [com_page_url]          VARCHAR (200) NULL,
    [com_page_name]         VARCHAR (200) NULL,
    [com_page_title]        VARCHAR (200) NULL,
    [com_page_description]  VARCHAR (200) NULL,
    [com_form_id]           VARCHAR (200) NULL,
    [com_status]            BIT           NOT NULL,
    [com_addedby]           VARCHAR (300) NULL,
    [com_addedon]           DATETIME      NULL,
    [com_modifiedby]        VARCHAR (300) NULL,
    [com_modifiedon]        DATETIME      NULL,
    [com_sortorder]         INT           NULL,
    [com_moduleimage]       VARCHAR (500) NULL,
    CONSTRAINT [PK_Components] PRIMARY KEY CLUSTERED ([com_key] ASC)
);

