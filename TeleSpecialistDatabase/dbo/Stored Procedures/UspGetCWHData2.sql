
create PROCEDURE [dbo].[UspGetCWHData2]
 @StartDate datetime = null,
 @edate datetime = null
AS
BEGIN
select 
cas_fac_key,
cas_ctp_key,
cas_billing_bic_key,
cas_patient_type,
cas_response_ts_notification
FROM [dbo].[case]
where    cas_ctp_key =9 and cas_cst_key = 20 and cas_is_active = 1 and cas_response_ts_notification >= @StartDate and cas_response_ts_notification <= @edate 
end