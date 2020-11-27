CREATE procedure [dbo].[sp_recent_used_diagnosis_code]      
@UserId varchar(128)      
AS      
BEGIN      
      
select distinct top 50  cas_key as Id, cas_billing_diagnosis  as icd_code     
from [case]       
where cas_phy_key = @UserId       
and cas_billing_diagnosis is not null and cas_billing_diagnosis != ''      
and (cas_created_date >= '2020-10-08 03:32:46.823' or cas_modified_date >= '2020-10-08 03:32:46.823')    
order by 1 desc      
END