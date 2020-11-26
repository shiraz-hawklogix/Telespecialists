

-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Feb-21>
-- Description:	<Return data for graph>
-- =============================================
CREATE PROCEDURE [dbo].[usp_dashboard_cases_by_type]
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

	;WITH cte AS (
	SELECT cast(cas_created_date as date) AS 'CaseDate', cas_cst_key as CaseStatus, COUNT(*) 'CaseCount' 
	FROM [case] (NOLOCK)
	WHERE cas_created_date >= @startDate
	GROUP BY cast(cas_created_date as date), cas_cst_key

	--order by cast(cas_created_date as date)
	)

	SELECT  DATEPART(d, CaseDate) as CaseDay , cte.CaseCount, ucd_title, cte.CaseDate
	FROM cte
	INNER JOIN ucl_data (NOLOCK) ON ucl_data.ucd_key = cte.CaseStatus
	ORDER BY CaseDate

END