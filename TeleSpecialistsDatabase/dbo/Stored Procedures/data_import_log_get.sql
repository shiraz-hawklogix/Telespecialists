
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Jan-31>
-- Description:	<Save data log insert>
-- =============================================

CREATE PROCEDURE [dbo].[data_import_log_get]
(
	@dil_request_id		Varchar(50)
)
AS
BEGIN
	
	SELECT * FROM [data_import_log] (NOLOCK)
	WHERE dil_request_id = @dil_request_id
	ORDER BY dil_created_date desc

END