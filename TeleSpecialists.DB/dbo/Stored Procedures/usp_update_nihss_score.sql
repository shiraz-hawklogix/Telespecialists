-- =============================================
-- Author:		Adnan K
-- Create date: August 29, 2019
-- Description:	Moved code to Sp in order to handle optimistic concurrency exceptions of entity framewokr on nihss score updating
-- =============================================
CREATE PROCEDURE [dbo].[usp_update_nihss_score] 
	-- Add the parameters for the stored procedure here
	--@userPasswordExpirationDays int,
	@cas_key INT,
	--@entityTypeKey INT, 
	@createdById nvarchar(128),
	@createdByName varchar(300),
	@selectedNIHSOptions varchar(2000)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	
	SET NOCOUNT ON;

	BEGIN TRANSACTION [Tran1]
	BEGIN TRY
	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE());

	
	DELETE FROM nih_stroke_scale_answer WHERE nsa_cas_key = @cas_key --AND nsa_ent_key =  @entityTypeKey;

	INSERT INTO [dbo].[nih_stroke_scale_answer]
           ([nsa_cas_key]
           ,[nsa_nss_key]
           ,[nsa_created_by]
           ,[nsa_created_date]
           ,[nsa_created_by_name]
           -- ,[nsa_ent_key]
		   )
     
	 SELECT 
		@cas_key as nsa_cas_key,
		val as nsa_nss_key,
		@createdById as nsa_created_by,
		@currentDate_EST as nsa_created_date,
		@createdByName as nsa_created_by_name
		--@entityTypeKey as nsa_ent_key
	  FROM
	dbo.SplitData(@selectedNIHSOptions,',');

   COMMIT TRANSACTION [Tran1]    

    END TRY

	  BEGIN CATCH

      ROLLBACK TRANSACTION [Tran1]

  END CATCH  
    	
END
