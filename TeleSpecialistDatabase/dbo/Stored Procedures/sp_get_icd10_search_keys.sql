create procedure sp_get_icd10_search_keys  
AS  
BEGIN  
  
select id,name from icd1_blling_codes_search_keys where is_active = 1  
  
END