-- =============================================
-- Author:		<Adan K.>
-- Create date: <2020-May-08>
-- Description:	<Used in Telecare API >
-- =============================================
CREATE PROCEDURE [dbo].[usp_api_case_accept]
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
	DECLARE @ctp_name VARCHAR(50)
	DECLARE @status_key	INT
	

	SELECT @CurrentDateTimeEST = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', @CurrentDateTimeUTC)

	SELECT TOP 1 @cah_key = cah_key,
				 @cah_action = cah_action,
				 @cah_phy_key = @cah_phy_key,
				 @cah_request_sent_time = cah_request_sent_time			
	FROM  case_assign_history 
	
	WHERE cah_cas_key = @Id 
	ORDER BY cah_key DESC

	SELECT 
				 @cas_cst_key = cas_cst_key,				
				 @ctp_name = ucd_title
	FROM
	[case]	
	INNER JOIN dbo.ucl_data on cas_ctp_key = ucd_key
	WHERe
	cas_key = @Id
	

	IF (@cas_cst_key <> 18 OR @Physician <> @cah_phy_key OR @cah_key  IS NULL)  BEGIN
	SET @Message = 'Case has been already assigned to another physician'
	END
	ELSE BEGIN
	
	IF (@cah_action = 'Expired' OR DATEDIFF(Second, @cah_request_sent_time, @CurrentDateTimeEST) > 1200000) BEGIN -- default value  120 (2 mints) husnain changed it just for test
		SET @Message = 'Request Timeout'
	END
	ELSE  BEGIN
	
	UPDATE case_assign_history 
		   SET cah_action_time = @CurrentDateTimeEST,
			   cah_action_time_utc = @CurrentDateTimeUTC,
			   cah_modified_by = @Physician,
			   cah_modified_date = @CurrentDateTimeEST,
			   cah_action = 'Expired'
	WHERE
			cah_key = @cah_key

	UPDATE [case]
	SET 
	cas_phy_key = @Physician,
	cas_cst_key = 19,  -- Accepted Status Key
	cas_history_physician_initial = dbo.FormatPhysiciansInitial(@Id),
	cas_modified_by = @Physician,
	cas_modified_date = @CurrentDateTimeEST,
	cas_response_time_physician = @CurrentDateTimeUTC,
	cas_status_assign_date = @CurrentDateTimeEST,
	cas_physician_assign_date = @CurrentDateTimeEST
	WHERE
	cas_key = @Id

	
	
	SELECT @status_key = phs_key from physician_status WHERE phs_name = @ctp_name


	UPDATE AspNetUsers 
	SET 
	status_change_cas_key = @Id,
	status_change_date = @CurrentDateTimeEST,
	status_change_date_forAll = @CurrentDateTimeEST,
	status_key = @status_key
	WHERE
	Id = @Physician

	INSERT INTO 
	physician_status_log(psl_cas_key,
						psl_created_date,
						psl_created_by,
						psl_user_key,
						psl_phs_key,
						psl_start_date,
						psl_comments,
						psl_status_name
						)
	Values(@Id,
		   @CurrentDateTimeEST,
		   @Physician,
		   @Physician,
		   @status_key,
		   @CurrentDateTimeEST,
		   'Case Accepted by physician itself',
		   @ctp_name)
	
	INSERT INTO 
	case_assign_history(
	 cah_cas_key, 
     cah_phy_key,
     cah_action,
     cah_created_date,
     cah_created_date_utc,
     cah_created_by,
     cah_is_active,
     cah_request_sent_time,
     cah_action_time,
     cah_is_manuall_assign,
     cah_action_time_utc,
     cah_request_sent_time_utc
	)
	Values(@Id, 
		   @Physician,
		   'Accepted',
		   @CurrentDateTimeEST,
		   @CurrentDateTimeUTC,
		   @Physician,
		   1,
		   @CurrentDateTimeEST,
		   @CurrentDateTimeEST,
		   1,
		   @CurrentDateTimeUTC,
		   @CurrentDateTimeUTC
		   )

	SET @Message = 'Success';
	


	END
	END

	SELECT @Message

END
