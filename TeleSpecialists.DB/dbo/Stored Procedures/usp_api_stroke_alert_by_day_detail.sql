-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-24>
-- Description:	<Telecare API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_stroke_alert_by_day_detail]
	-- Add the parameters for the stored procedure here
	@CaseDate	DATE 
AS
BEGIN
	
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	---------------------------------------------------------
	-- Follow-Ups and Routine Consults - By Day
	-- name = consults-by-day
	---------------------------------------------------------

	DECLARE @ucd_stroke_alert INT 
	SELECT @ucd_stroke_alert = ucd_key FROM ucl_data WHERE ucd_title = 'Stroke Alert' AND ucd_ucl_key = 11

	
	SELECT 
			CONVERT(VARCHAR(128), fac_key) AS FacilityID, 
			fac_name AS FacilityName, 
			ISNULL(fac_md_staff_reference_source_id, '') AS MDStaffReferenceSourceID,
			ISNULL(fac_md_staff_source_name, '') AS MDStaffReferenceSourceName,
			dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) AS 'Date'
			
		FROM facility 
		INNER JOIN [case] ON cas_fac_key = fac_key
		WHERE 
			cas_ctp_key = @ucd_stroke_alert
			AND CAST(dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', cas_response_ts_notification) AS DATE) = @CaseDate
			AND fac_is_active = 1
			AND cas_is_active = 1
			AND cas_cst_key in (17,18,19,20,140)

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

END
