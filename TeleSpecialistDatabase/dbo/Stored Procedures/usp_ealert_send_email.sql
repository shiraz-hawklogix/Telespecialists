-- =============================================
-- Author:		<Amir.J>
-- Create date: <2020-Apr-07>
-- Description:	<responsible to get eAlert cases which call is not sent to five9, we use this SP to send an email of those cases>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ealert_send_email]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	 select 
		isnull(cas_case_number,'')  case_number,
		isnull(cas_callback,'') cas_callback,
		isnull(ucd_title,'') case_type,
		isnull(fac_name,'') facility,
		isnull(cas_callback_extension,'')  callback_extension,
		isnull(cas_cart,'') cart,
		isnull(cas_key,'') cas_key,
		isnull(wcl_error_description,'') error_description,
	    wcl_reprocessed_date  

	 from web2campaign_log cam_log(NOLOCK)

	 inner join [case](NOLOCK)   on cam_log.wcl_cas_key=cas_key
	 inner join ucl_data(NOLOCK) on cas_ctp_key=ucd_key 
	 inner join facility(NOLOCK) on facility.fac_key=cas_fac_key  

	 where wcl_error_mail_date is null  and ucd_ucl_key=11  and wcl_reprocessed_date is not null and 
	 [wcl_error_code] <> '0'
    
 
END