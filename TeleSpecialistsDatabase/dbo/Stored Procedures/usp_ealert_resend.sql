-- =============================================
-- Author:		<Amir J.>
-- Create date: <2020-April-1>
-- Description:	<Get EAlert cases that are in error code:TC-Error>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ealert_resend]
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @CurrentDateTimeEST DATETIME;
	SELECT @CurrentDateTimeEST = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', GETUTCDate())

	SELECT  aps_five9_domain ,aps_five9_number_to_dial,aps_five9_list, aps_eAlert_retry_limt  FROM [dbo].[application_setting]
    
	 select 
	 ISNULL(cas_case_number,'') 
	 case_number,
	 ISNULL(cas_callback,'') cas_callback,
	 ISNULL(ucd_title,'') case_type,
	 ISNULL(fac_name,'') facility,
	 ISNULL(cas_callback_extension,'')  callback_extension,
	 ISNULL(cas_cart,'') cart,ISNULL(cas_key,'') cas_key,
	 ISNULL(wcl_request_retry_count,0) AS  wcl_request_retry_count
	 from web2campaign_log cam_log
	 inner join [case]   on cam_log.wcl_cas_key=cas_key
	 inner join  ucl_data on cas_ctp_key=ucd_key 
	 inner join facility on facility.fac_key=cas_fac_key  
	 where 
	 (ISNULL(cam_log.wcl_error_code,'0') <> '0'  or 
	  ( 
		dbo.DiffSeconds( cam_log.wcl_created_date,@CurrentDateTimeEST) < 10 AND wcl_raw_result is null
	  ) -- end of or bracket
	 )
	 and ucd_ucl_key=11 and wcl_reprocessed_date is   null
	 and dbo.DiffMinutes(cam_log.wcl_created_date,@CurrentDateTimeEST) < 15
		 
END