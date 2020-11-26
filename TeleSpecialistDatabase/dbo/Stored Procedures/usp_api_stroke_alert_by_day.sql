
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-13>
-- Description:	<Telecare API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_stroke_alert_by_day]
	-- Add the parameters for the stored procedure here
	@StartDate	DATETIME,		
	@EndDate	DATETIME	
AS
BEGIN
	
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	---------------------------------------------------------
	-- Follow-Ups and Routine Consults - By Day
	-- name = consults-by-day
	---------------------------------------------------------

	Set @EndDate = (CONVERT(varchar(20), @EndDate, 23) + ' 23:59:59') 

	DECLARE @ucd_stroke_alert INT 
	SELECT @ucd_stroke_alert = ucd_key FROM ucl_data WHERE ucd_title = 'Stroke Alert' AND ucd_ucl_key = 11

	;WITH 
	Dates AS (

		SELECT  TOP (DATEDIFF(DAY, @StartDate, @EndDate) + 1)
				ThisDate = DATEADD(DAY, ROW_NUMBER() OVER(ORDER BY a.object_id) - 1, @StartDate)
		FROM    sys.all_objects a
				CROSS JOIN sys.all_objects b
	),
	CTE AS 
	(
		SELECT 
			CONVERT(VARCHAR(128), fac_key) AS FacilityID, 
			fac_name AS FacilityName, 
			ISNULL(fac_md_staff_reference_source_id, '') AS MDStaffReferenceSourceID,
			ISNULL(fac_md_staff_source_name, '') AS MDStaffReferenceSourceName,
			cas_ctp_key, 
			CAST(dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) AS DATE) AS CaseDate
			
		FROM facility 
		INNER JOIN [case] ON cas_fac_key = fac_key
		WHERE 
			cas_ctp_key = @ucd_stroke_alert
			AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
			AND fac_is_active = 1
			AND cas_is_active = 1
			AND cas_cst_key in (17,18,19,20,140)

	)
	SELECT 
			--CONVERT(VARCHAR, Dates.ThisDate, 23) AS 'Date',
			Dates.ThisDate AS 'Date', 
			ISNULL(FacilityID, '') AS FacilityID, 
			ISNULL(FacilityName, '') AS FacilityName, 
			ISNULL(MDStaffReferenceSourceID, '') AS MDStaffReferenceSourceID, 
			ISNULL(MDStaffReferenceSourceName, '') AS MDStaffReferenceSourceName, 
			SUM(CASE WHEN cas_ctp_key IS NULL THEN 0 ELSE 1 END) AS 'StrokeAlerts'
	FROM Dates
	LEFT OUTER JOIN CTE ON Dates.ThisDate = CTE.CaseDate
	GROUP BY Dates.ThisDate, FacilityID, FacilityName, MDStaffReferenceSourceID, MDStaffReferenceSourceName
	ORDER BY Dates.ThisDate, FacilityID, FacilityName, MDStaffReferenceSourceID, MDStaffReferenceSourceName

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

END