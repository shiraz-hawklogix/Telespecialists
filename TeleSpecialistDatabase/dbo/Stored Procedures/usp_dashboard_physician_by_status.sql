

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_dashboard_physician_by_status]
	-- Add the parameters for the stored procedure here
	@filter VARCHAR(50) = ''	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	
	SET @filter = LOWER(@filter)

	SELECT @startDate = CASE WHEN @filter = 'week' THEN  (@currentDate_EST - 7)
							 WHEN @filter = 'month' THEN  (@currentDate_EST - 30)
							 WHEN @filter = 'year' THEN (@currentDate_EST - 364)
							 WHEN @filter = 'today' THEN  @currentDate_EST
						END 
	
	SELECT phs_name AS Title, COUNT(Id) AS TitleCount, phs_color_code AS TitleColor 
	FROM physician_status (NOLOCK)
	INNER JOIN AspNetUsers (NOLOCK) ON status_key = phs_key 
	INNER JOIN user_schedule ON uss_user_id = Id


	--INNER JOIN physician_status_log (NOLOCK) ON psl_user_key = AspNetUsers.Id AND psl_status_name = phs_name

	/*
	FROM AspNetUsers (NOLOCK)
	RIGHT OUTER JOIN physician_status (NOLOCK) ON status_key = phs_key
	INNER JOIN physician_status_log (NOLOCK) ON psl_user_key = AspNetUsers.Id AND psl_status_name = phs_name
	*/

	WHERE 1 = 1
	--and psl_start_date >= @startDate
	AND IsActive = 1
	--and status_change_date >= @startDate
	--and (GETDATE() between uss_time_from_calc and uss_time_to_calc)
	AND (@currentDate_EST BETWEEN uss_time_from_calc AND uss_time_to_calc)

	GROUP BY phs_name, phs_color_code 
	ORDER BY phs_name

END