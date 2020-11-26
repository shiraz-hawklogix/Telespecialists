
-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-13>
-- Description:	<TeleCARE API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_stroke_alert_summary]
	-- Add the parameters for the stored procedure here
	@StartDate DATETIME, 
	@EndDate DATETIME
AS
BEGIN

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	---------------------------------------------------------
	-- Stroke Alerts - Summary
	-- name = stroke-alert-summary
	---------------------------------------------------------

	Set @EndDate = (CONVERT(varchar(20), @EndDate, 23) + ' 23:59:59') 

	SELECT 
			CONVERT(VARCHAR(128), fac_key) AS FacilityID, 
			fac_name AS FacilityName, 
			ISNULL(fac_md_staff_reference_source_id, '') AS MDStaffReferenceSourceID,
			ISNULL(fac_md_staff_source_name, '') AS MDStaffReferenceSourceName,
			'StrokeAlerts' = 
			(
				SELECT COUNT(*) FROM [case] (NOLOCK)
				WHERE 
						cas_fac_key = facility.fac_key
						AND cas_ctp_key = (SELECT ucd_key FROM ucl_data WHERE ucd_title = 'Stroke Alert' AND ucd_ucl_key = 11)
						AND dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) BETWEEN @StartDate AND @EndDate
						
						AND cas_is_active = 1
						AND cas_cst_key in (17,18,19,20,140)
			)

	FROM facility 
	Where fac_is_active = 1
	ORDER BY fac_name


	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END