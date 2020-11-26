-- =============================================
-- Author:		<Amir J.>
-- Create date: <2020-April-2>
-- Description:	<To Update Error log from EAlert ReSender Job >
-- =============================================
 
CREATE PROCEDURE [dbo].[usp_job_update_ealert_resender]

@ealert EAlertReSender   readonly	 
AS
BEGIN
	 
	SET NOCOUNT ON;

	UPDATE wcl SET
	wcl_raw_result=raw_result,
	wcl_error_code=error_code,
	wcl_error_description=error_description,
    wcl_reprocessed_date=reprocessed_date,
	wcl_request_retry_count = ISNULL(wcl_request_retry_count,0) + 1
	FROM dbo.web2campaign_log wcl  
	INNER JOIN @ealert ON cas_key =  wcl_cas_key
	

END