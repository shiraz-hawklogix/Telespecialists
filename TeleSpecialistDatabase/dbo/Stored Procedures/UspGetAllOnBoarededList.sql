﻿Create PROCEDURE [dbo].[UspGetAllOnBoarededList]
AS
BEGIN
select 
fp.fap_key,
fp.fap_fac_key,
fp.fap_user_key,
fp.fap_is_on_boarded
,CONCAT(ap.FirstName,' ' , ap.LastName) as Physician_Name
FROM [dbo].[facility_physician] fp             
join dbo.AspNetUsers ap
on CONVERT(nvarchar(50),fp.fap_user_key) =ap.Id
where fap_is_on_boarded = 1  and fap_is_active=1 
END