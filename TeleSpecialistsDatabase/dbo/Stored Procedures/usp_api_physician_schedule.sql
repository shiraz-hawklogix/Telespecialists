
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-10>
-- Description:	<Used in Telecare API >
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_physician_schedule]
	-- Add the parameters for the stored procedure here
	@StartDate	DATETIME,		
	@EndDate	DATETIME,		
	@NPI		VARCHAR(50) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	Set @EndDate = (CONVERT(varchar(20), @EndDate, 23) + ' 23:59:59') 

    SELECT  
		NPINumber AS 'NPI', 
		FirstName AS 'FirstName', 
		LastName AS 'LastName', 
		--CONVERT(VARCHAR, uss_time_from_calc,120) AS 'StartTime', 
		--CONVERT(VARCHAR, uss_time_to_calc, 120) AS 'EndTime'
		uss_time_from_calc AS 'StartTime', 
		uss_time_to_calc AS 'EndTime'

	FROM user_schedule
	INNER JOIN AspNetUsers ON AspNetUsers.Id = user_schedule.uss_user_id
	WHERE IsActive = 1
	AND uss_time_from_calc >= @StartDate
	AND uss_time_to_calc <= @EndDate
	AND (ISNULL(@NPI, '') = '' OR NPINumber = @NPI )
	
	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END