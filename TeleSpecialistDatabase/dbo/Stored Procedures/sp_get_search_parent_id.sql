CREATE procedure sp_get_search_parent_id    
@value varchar(max),  
@table_name varchar(max) = '',  
@linked_code_id varchar(max) = ''   
AS     
begin    
if(@table_name = 'Billing_Cal')  
Begin  
select Id from icd10_billing_codes_calcualtor     
where cod_parent_id is null and @value  in ( select val FROM dbo.SplitData(cod_name,'|'))    
end  
else if (@table_name = 'Billing_Cal_Child')  
BEGIN  
select Id,cod_name,cod_class_name,cod_sort_order from icd10_billing_codes_calcualtor     
where @linked_code_id  in ( select val FROM dbo.SplitData(cod_linked_id,','))    
AND cod_parent_id = CONVERT(INT,@value)  
AND cod_is_active = 1  
END  
else  
begin  
select code_id from diagnosis_codes     
where diag_cat_parent_id is null and @value  in ( select val FROM dbo.SplitData(icd_code_title,'|'))    
end  
END