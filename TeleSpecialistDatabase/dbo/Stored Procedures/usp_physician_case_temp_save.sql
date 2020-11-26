-- =============================================
-- Author:		Adnan K
-- Create date: March 9, 2020	
-- Description:	Purpose of the store procedure is to create/update the record in physician_case_temp table against the guid 
--				passed from case create page, to keep track of selected physician on create case page before the case is saved

-- =============================================

CREATE PROCEDURE [dbo].[usp_physician_case_temp_save]
(
	@pct_guid							uniqueidentifier,
	@pct_cst_key						int,
	@pct_phy_key						nvarchar(128),
	@pct_saved_by						nvarchar(128),
	@pct_ctp_key						int
	
)
AS
BEGIN

SET NOCOUNT ON;

BEGIN TRANSACTION [Tran1]
BEGIN TRY

	DECLARE @CurrentDateEST DATETIME
	SELECT @CurrentDateEST = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', GetUTCDATE())

	DELETE FROM physician_case_temp where pct_key = @pct_guid
	IF (ISNULL(@pct_phy_key,'') <> '' AND  @pct_ctp_key = 9) 
	INSERT INTO physician_case_temp(pct_key, pct_phy_key,pct_cst_key,pct_created_date,pct_created_by)
								values(@pct_guid, @pct_phy_key,@pct_cst_key, @CurrentDateEST, @pct_saved_by)

	select 'True' as success

	COMMIT TRANSACTION [Tran1]    

END TRY

BEGIN CATCH

	ROLLBACK TRANSACTION [Tran1]

END CATCH  
END