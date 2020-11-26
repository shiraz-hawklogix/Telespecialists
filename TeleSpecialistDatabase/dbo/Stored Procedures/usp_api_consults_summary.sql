
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-13>
-- Description:	<Telecare API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_consults_summary]
	-- Add the parameters for the stored procedure here
	@StartDate	DATETIME,		
	@EndDate	DATETIME	
AS
BEGIN
	---------------------------------------------------------
-- Follow-Ups and Routine Consults - Summary
-- name = consults-summary
---------------------------------------------------------

	Set @EndDate = (CONVERT(varchar(20), @EndDate, 23) + ' 23:59:59') 
	
	DECLARE @ucd_routine_consult INT 
	SELECT @ucd_routine_consult = ucd_key FROM ucl_data WHERE ucd_title = 'Routine Consult' AND ucd_ucl_key = 11


	;WITH CTE AS 
	(
		SELECT 
			CONVERT(VARCHAR(128), fac_key) AS FacilityID, 
			fac_name AS FacilityName, 
			ISNULL(fac_md_staff_reference_source_id, '') AS MDStaffReferenceSourceID,
			ISNULL(fac_md_staff_source_name, '') AS MDStaffReferenceSourceName,
			'FollowUps' = (
								SELECT COUNT(*) FROM [case] (NOLOCK)
								WHERE  cas_billing_visit_type = 'Follow-Up' 
								AND cas_fac_key = facility.fac_key
								AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
							),
			'RoutineConsults' = (
									SELECT COUNT(*) FROM [case] (NOLOCK)
									WHERE (
										cas_billing_visit_type = 'Routine Consult' 
										OR 
										cas_ctp_key = @ucd_routine_consult
									) 
									AND cas_fac_key = facility.fac_key
									AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
								)

		FROM facility (NOLOCK)
	)
	SELECT *, CTE.FollowUps+CTE.RoutineConsults AS 'Total' FROM CTE
END