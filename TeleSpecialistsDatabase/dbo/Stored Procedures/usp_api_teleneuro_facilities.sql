

-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-13>
-- Description:	<TeleCARE API>
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_teleneuro_facilities]
	-- Add the parameters for the stored procedure here
	@Active INT = 1
AS
BEGIN

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
	---------------------------------------------------------
	--  TeleNeuro Facilities
	-- name = teleneuro-facilities
	-- Description: Facilities where TeleNeuro is checked + current date should be between contract start and end
	---------------------------------------------------------

	SELECT 
		CONVERT(VARCHAR(128), fac_key) AS 'FacilityID',
		fac_name AS 'FacilityName',
		ISNULL(fac_md_staff_reference_source_id, '') AS MDStaffReferenceSourceID,
		ISNULL(fac_md_staff_source_name, '') AS MDStaffReferenceSourceName,
		fct_service_calc AS 'Services',
		--CONVERT(VARCHAR, fct_start_date, 120) AS 'ContractStartDate', 
		--CONVERT(VARCHAR, fct_end_date, 120) AS 'ContractEndDate' 

		fct_start_date AS 'ContractStartDate', 
		fct_end_date AS 'ContractEndDate' 

	FROM facility (NOLOCK)
	INNER JOIN dbo.facility_contract ON facility_contract.fct_key = facility.fac_key

	WHERE fct_service_calc LIKE '%TeleNeuro%'
	AND fac_is_active = @Active

	ORDER BY fac_name
END