
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-13>
-- Description:	<Telecare API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_consults_by_day]
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

	DECLARE @ucd_routine_consult INT 
	SELECT @ucd_routine_consult = ucd_key FROM ucl_data WHERE ucd_title = 'Routine Consult' AND ucd_ucl_key = 11

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
			fac_key AS 'FacilityID',
			fac_name AS 'FacilityName',
			fac_md_staff_source_name AS 'MDStaffReferenceSourceName',
			fac_md_staff_reference_source_id AS 'MDStaffReferenceSourceID',
			CAST(dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) AS DATE) AS CaseDate,
			'FollowUps' = CASE WHEN cas_billing_visit_type = 'Follow-Up' THEN 1 ELSE 0 END,
			
			--(
			--					SELECT COUNT(*) FROM [case] (NOLOCK)
			--					WHERE  cas_billing_visit_type = 'Follow-Up' 
			--					AND cas_fac_key = facility.fac_key
			--					AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
			--				),
			'RoutineConsults' = CASE WHEN cas_billing_visit_type = 'Routine Consult' OR cas_ctp_key = @ucd_routine_consult THEN 1 ELSE 0 END
			
			--(
			--						SELECT COUNT(*) FROM [case] (NOLOCK)
			--						WHERE (
			--							cas_billing_visit_type = 'Routine Consult' 
			--							OR cas_ctp_key = @ucd_routine_consult
			--						) 
			--						AND cas_fac_key = facility.fac_key
			--						AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
			--					)
		FROM facility
		INNER JOIN [case] ON cas_fac_key = fac_key
		WHERE 
			
			dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate

	)
	SELECT 
			--CONVERT(VARCHAR, Dates.ThisDate, 23) AS 'Date', 
			Dates.ThisDate AS 'Date', 
			ISNULL(MDStaffReferenceSourceID, '') AS MDStaffReferenceSourceID,
			ISNULL(MDStaffReferenceSourceName, '') AS MDStaffReferenceSourceName,
			ISNULL(CONVERT(VARCHAR(128), CTE.FacilityID), '') AS FacilityID, 
			ISNULL(CTE.FacilityName, '') AS FacilityName, 
			SUM(COALESCE(FollowUps, 0)) AS 'FollowUps',
			SUM(COALESCE(RoutineConsults, 0)) AS 'RoutineConsults',
			SUM(COALESCE(FollowUps, 0)) + SUM(COALESCE(RoutineConsults, 0)) AS Total
	FROM Dates
	LEFT OUTER JOIN CTE ON Dates.ThisDate = CTE.CaseDate
	GROUP BY Dates.ThisDate, ISNULL(CONVERT(VARCHAR(128), CTE.FacilityID), ''), ISNULL(CTE.FacilityName, ''), MDStaffReferenceSourceID, MDStaffReferenceSourceName
	ORDER BY Dates.ThisDate, ISNULL(CONVERT(VARCHAR(128), CTE.FacilityID), ''), ISNULL(CTE.FacilityName, ''), MDStaffReferenceSourceID, MDStaffReferenceSourceName

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
	
END