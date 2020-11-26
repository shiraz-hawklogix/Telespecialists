CREATE PROCEDURE [dbo].[usp_api_case_reject]
	-- Add the parameters for the stored procedure here
	@Id	  INT,
	@Physician	varchar(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @cah_action  VARCHAR(50)
	DECLARE @cah_key INT = NULL
	DECLARE @cah_phy_key	VARCHAR(128)
	DECLARE @cas_cst_key INT
	DECLARE @cah_request_sent_time DATETIME
	DECLARE @Message varchar(200)
	DECLARE @CurrentDateTimeEST DATETIME
	DECLARE @CurrentDateTimeUTC DATETIME = GETUTCDate()
	DECLARE @Action  varchar(20)

	SELECT @CurrentDateTimeEST = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', @CurrentDateTimeUTC)

	SELECT TOP 1 @cah_key = cah_key,
				 @cah_action = cah_action,
				 @cah_phy_key = @cah_phy_key,
				 @cah_request_sent_time = cah_request_sent_time			
	FROM  case_assign_history 
	
	WHERE cah_cas_key = @Id 
	ORDER BY cah_key DESC

	SELECT 
				 @cas_cst_key = cas_cst_key				
				
	FROM
	[case]	
	
	WHERe
	cas_key = @Id
	

	IF (@cas_cst_key in (19,20,140) OR @Physician <> @cah_phy_key OR @cah_key  IS NULL)  BEGIN
	SET @Message = 'Case has been already assigned to another physician'
	END
	ELSE BEGIN
	--IF (@cah_action <> 'WaitingForAction' ) BEGIN  -- Commented code added by husnain for testing can be open if needed
	--	SET @Message = 'Request Timeout'
	--END
	--ELSE  BEGIN
	
	IF (DATEDIFF(Second, @cah_request_sent_time, @CurrentDateTimeEST) > 120) BEGIN -- default value  120 (2 mints) husnain changed it just for test
	SET @Action = 'Expired'	
	END
	ELSE BEGIN
	SET @Action = 'Rejected'
	END 
	UPDATE case_assign_history 
		   SET cah_action_time = @CurrentDateTimeEST,
			   cah_action_time_utc = @CurrentDateTimeUTC,
			   cah_modified_by = @Physician,
			   cah_modified_date = @CurrentDateTimeEST,
			   cah_action = @Action
	WHERE
			cah_key = @cah_key

	UPDATE [case]
	SET 
	cas_phy_key = null,
	cas_cst_key = 17,  -- Open Status
	cas_modified_by = @Physician,
	cas_modified_date = @CurrentDateTimeEST
	
	WHERE
	cas_key = @Id

	SET @Message = 'Success';

	END
	--END

	SELECT @Message

END