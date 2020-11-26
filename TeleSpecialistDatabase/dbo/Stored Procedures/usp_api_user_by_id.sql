-- =============================================
-- Author:		<Adnan K.>
-- Create date: <2020-May-06>
-- Description:	<Used in Telecare API >
-- =============================================
CREATE PROCEDURE [dbo].[usp_api_user_by_id]
	-- Add the parameters for the stored procedure here
	@Id varchar(128)
AS
BEGIN
 SET NOCOUNT ON -- added to prevent extra result sets from

	SELECT
	Id,
	UserName,
	FirstName,
	LastName,
	Email,
	PhoneNumber,
	EnableFive9,
	MobilePhone,
	NPINumber,
	UserInitial,
	Gender,
	AddressBlock,
	IsActive,
	CaseReviewer,
	status_key, 
	status_change_date,
	CredentialCount,
	CredentialIndex,
	ContractDate,
	status_change_cas_key,
	status_change_date_forAll,
	IsStrokeAlert,
	NHAlert,
	User_Image
	FROM
	AspNetUsers
	WHERE
	Id = @Id

END
