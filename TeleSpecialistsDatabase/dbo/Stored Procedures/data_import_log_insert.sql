
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Jan-31>
-- Description:	<Save data log insert>
-- =============================================

CREATE PROCEDURE [dbo].[data_import_log_insert]
(
	@dil_type						VARCHAR(50),
	@dil_request_id					Varchar(50),
	@dil_provider					NVARCHAR(128),
	@dil_message					Varchar(MAX),
	@dil_created_by					NVARCHAR(128)
)
AS
BEGIN
DECLARE @sql NVARCHAR(MAX)

INSERT INTO [data_import_log]
           (
				 [dil_type]
				,[dil_request_id]
				,[dil_provider]
				,[dil_message]
				,[dil_created_by]
				,[dil_created_date]
           )
     VALUES
           (@dil_type ,@dil_request_id, @dil_provider, @dil_message, @dil_created_by, GETDATE())


END