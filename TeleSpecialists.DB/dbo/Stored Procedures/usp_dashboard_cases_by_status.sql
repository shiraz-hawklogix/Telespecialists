-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_dashboard_cases_by_status]
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

	SELECT ucd_title AS Title, COUNT([case].cas_key) AS TitleCount 
	FROM [case] (NOLOCK)
	RIGHT OUTER JOIN ucl_data (NOLOCK) ON ucd_key = cas_cst_key
	WHERE ucd_title IN ('Open', 'Waiting to Accept', 'Accepted', 'Complete','Cancelled')
	AND cas_created_date >= @startDate
	GROUP BY ucd_title
	ORDER BY CASE 
				WHEN ucd_title = 'Open' THEN 1
				WHEN ucd_title = 'Waiting to Accept' THEN 2
				WHEN ucd_title = 'Accepted' THEN 3
				WHEN ucd_title = 'Complete' THEN 4
				WHEN ucd_title = 'Cancelled' THEN 5
				ELSE 10 
			END 
END
