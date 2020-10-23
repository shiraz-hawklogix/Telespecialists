-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-May-21>
-- Description:	<Used in Telecare API >
-- =============================================
CREATE PROCEDURE [dbo].[usp_api_physician_license]
	-- Add the parameters for the stored procedure here
	@NPI				VARCHAR(10),
	@State				VARCHAR(10),
	@LicenseType		VARCHAR(150),
	@Valid				BIT	
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
		phl_issued_date AS 'IssuedDate',
		phl_expired_date AS 'ExpiredDate',
		phl_license_number AS 'LicenseNumber',
		lt.ucd_title AS 'LicenseType',
		ls.ucd_title AS 'State',
		CAST(CASE 
			WHEN phl_issued_date IS NULL THEN 0 -- not valid
			WHEN phl_issued_date <= GETDATE() AND phl_expired_date IS NULL THEN 1 -- valid
			WHEN GETDATE() BETWEEN phl_issued_date AND phl_expired_date THEN 1 -- valid
			ELSE 0 
		END AS BIT) AS isValid

	FROM AspNetUsers 
	INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = AspNetUsers.Id
	INNER JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId
	INNER JOIN physician_license ON physician_license.phl_user_key = AspNetUsers.Id
	INNER JOIN dbo.ucl_data AS ls ON phl_license_state = ls.ucd_key
	INNER JOIN dbo.ucl_data AS lt ON lt.ucd_unique_id = phl_license_type 
	INNER JOIN ucl ON ucl.ucl_key = lt.ucd_ucl_key AND ucl_type = 'LicenseType'
	
	--AND lt.ucd_ucl_key = (SELECT ucl_key FROM ucl WHERE ucl_type = 'LicenseType')

	WHERE IsActive = 1 -- Active Physician
	AND AspNetRoles.Name IN ('Physician', 'Partner Physician')
	AND (ISNULL(@NPI, '') = '' OR NPINumber = @NPI )
	AND (ISNULL(@State, '') = '' OR ls.ucd_title = @State )
	AND (ISNULL(@LicenseType, '') = '' OR lt.ucd_title = @LicenseType )
	AND (@Valid = 
			(CASE 
				WHEN phl_issued_date IS NULL THEN 0 -- not valid
				WHEN phl_issued_date <= GETDATE() AND phl_expired_date IS NULL THEN 1 -- valid
				WHEN GETDATE() BETWEEN phl_issued_date AND phl_expired_date THEN 1 -- valid
				ELSE 0 
			END)
	)
	

	SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
END
