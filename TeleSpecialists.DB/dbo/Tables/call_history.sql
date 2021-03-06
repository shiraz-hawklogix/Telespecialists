﻿CREATE TABLE [dbo].[call_history] (
    [chi_key]                    INT            IDENTITY (20000, 1) NOT NULL,
    [chi_call_id]                VARCHAR (500)  NOT NULL,
    [chi_call_object_id]         VARCHAR (500)  NULL,
    [chi_ani]                    VARCHAR (500)  NOT NULL,
    [chi_campaign_name]          VARCHAR (500)  NULL,
    [chi_campaign_id]            VARCHAR (500)  NOT NULL,
    [chi_dnis]                   VARCHAR (500)  NULL,
    [chi_domain_name]            VARCHAR (500)  NULL,
    [chi_customer]               VARCHAR (500)  NULL,
    [chi_start_time_stamp]       DATETIME       NULL,
    [chi_agent]                  VARCHAR (500)  NULL,
    [chi_agent_extension]        VARCHAR (500)  NULL,
    [chi_agent_name]             VARCHAR (500)  NULL,
    [chi_call_result]            VARCHAR (500)  NULL,
    [chi_call_type]              VARCHAR (500)  NULL,
    [chi_call_back_id]           VARCHAR (500)  NULL,
    [chi_call_back_number]       VARCHAR (500)  NULL,
    [chi_comments]               VARCHAR (MAX)  NULL,
    [chi_duration]               INT            NULL,
    [chi_format_api_call_type]   VARCHAR (MAX)  NULL,
    [chi_handle_time]            DATETIME       NULL,
    [chi_session_id]             VARCHAR (500)  NULL,
    [chi_subject]                VARCHAR (500)  NULL,
    [chi_talk_and_hold_duration] INT            NULL,
    [chi_wrap_time]              DATETIME       NULL,
    [chi_is_active]              BIT            NOT NULL,
    [chi_created_by]             NVARCHAR (128) NOT NULL,
    [chi_created_date]           DATETIME       CONSTRAINT [DF_call_history_chi_created_date] DEFAULT (getdate()) NOT NULL,
    [chi_modified_by]            NVARCHAR (128) NULL,
    [chi_modified_date]          DATETIME       NULL,
    [chi_original_stamp_time]    VARCHAR (50)   NULL,
    CONSTRAINT [PK_call_history] PRIMARY KEY CLUSTERED ([chi_key] ASC)
);

