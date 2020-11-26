
CREATE PROCEDURE [dbo].[UspGetRCIData]
 @StartDate datetime = NULL,
 @edate datetime = NULL
AS
BEGIN
select 
fap_fac_key,
fap_user_key,
fap_is_on_boarded,
fap_key,
fap_created_date,
fap_end_date,
fap_start_date,
fap_onboarded_date
FROM [dbo].[facility_physician]
where  fap_is_active = 1 and fap_is_on_boarded = 1 
end