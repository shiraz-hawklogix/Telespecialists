

-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-21>
-- Description:	<Used in Telecare API >
-- =============================================
CREATE  PROCEDURE [dbo].[usp_api_physician_credentials]
	-- Add the parameters for the stored procedure here
	@NPI					VARCHAR(10),
	@ReferenceSourceName	VARCHAR(500),
	@FacilityName			VARCHAR(500),
	@Onboarded				BIT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT 
		CONVERT(VARCHAR(128), AspNetUsers.Id) AS 'UserID',
		UserName,
		FirstName, 
		LastName,
		NPINumber AS 'NPI', 
		IsActive AS 'Active',
		CONVERT(VARCHAR(128), fac_key) AS 'FacilityID',
		fac_name AS 'FacilityName',
		ISNULL(fac_md_staff_source_name, '') AS 'MDStaffReferenceSourceName',
		ISNULL(fac_md_staff_reference_source_id, '') AS 'MDStaffReferenceSourceID',
		fap_start_date AS 'MDStaffStartDate',
		fap_end_date AS 'MDStaffEndDate',
		fap_is_on_boarded AS 'Onboarded',
		fap_is_override AS 'Override'

	FROM AspNetUsers 
	INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
	INNER JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId
	INNER JOIN facility_physician ON facility_physician.fap_user_key = AspNetUsers.Id
	INNER JOIN facility ON facility.fac_key = facility_physician.fap_fac_key

	WHERE IsActive = 1
	AND AspNetRoles.Name IN ('Physician', 'Partner Physician')
	AND (ISNULL(@NPI, '') = '' OR NPINumber = @NPI )
	AND (ISNULL(@ReferenceSourceName, '') = '' OR fac_md_staff_source_name = @ReferenceSourceName )
	AND (ISNULL(@FacilityName, '') = '' OR fac_name = @FacilityName )
	AND (ISNULL(@Onboarded, 1) = 1 OR fap_is_on_boarded = @Onboarded )


	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END