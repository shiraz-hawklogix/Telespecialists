-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_job_update_email
	-- Add the parameters for the stored procedure here
	 @ealert EAlertReSender   readonly	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SET NOCOUNT ON;

	UPDATE wcl SET
	wcl_error_mail_date=error_email_date	 
	FROM dbo.web2campaign_log(NOLOCK) wcl  
	INNER JOIN @ealert ON cas_key =  wcl_cas_key
	 
END