﻿



-- ============================================= 
CREATE PROCEDURE [dbo].[usp_get_aspnetusers]
AS
BEGIN
	SET NOCOUNT ON;
Select 
	   anu.Id
      ,anu.[UserName]
	  ,CONCAT(anu.FirstName, ' ', anu.LastName) as [FirstName]
      ,anu.[LastName]
      ,anu.[Email]
      ,anu.[EmailConfirmed]
      ,anu.[PasswordHash]
      ,anu.[SecurityStamp]
      ,anu.[PhoneNumber]
      ,anu.[PhoneNumberConfirmed]
      ,anu.[TwoFactorEnabled]
      ,anu.[LockoutEndDateUtc]
      ,anu.[LockoutEnabled]
      ,anu.[AccessFailedCount]
      ,anu.[EnableFive9]
      ,anu.[MobilePhone]
      ,anu.[NPINumber]
      ,anu.[UserInitial]
      ,anu.[Gender]
      ,anu.[AddressBlock]
      ,anu.[IsActive]
      ,anu.[CreatedBy]
      ,anu.[CreatedByName]
      ,anu.[CreatedDate]
      ,anu.[ModifiedByName]
      ,anu.[ModifiedBy]
      ,anu.[ModifiedDate]
      ,anu.[CaseReviewer]
      ,anu.[status_key]
      ,anu.[status_change_date]
      ,anu.[CredentialCount]
      ,anu.[CredentialIndex]
      ,anu.[APISecretKey]
      ,anu.[APIPassword]
      ,anu.[IsEEG]
      ,anu.[RequirePasswordReset]
      ,anu.[PasswordExpirationDate]
      ,anu.[ContractDate]
      ,anu.[status_change_cas_key]
      ,anu.[IsDeleted]
      ,anu.[status_change_date_forAll]
      ,anu.[IsStrokeAlert]
      ,anu.[NHAlert]
      ,anu.[IsDisable]
      ,anu.[State_key]
      ,anu.[Zip]
      ,anu.[User_Image]
      ,anu.[IsSleep]
      ,anu.[Address_line1]
      ,anu.[Address_line2]
      ,anu.[City] 
	   ,anu.[IsTwoFactVerified]
      ,anu.[TwoFactVerifyCode]
      ,anu.[CodeExpiryTime]
FROM    [dbo].[AspNetUsers] anu

		ORDER BY FirstName ASC 
END
