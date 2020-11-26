create PROCEDURE [dbo].[usp_mock_case_number_get]    
AS    
BEGIN    
    
SET NOCOUNT ON;    
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;    
  
update  telecare_counters  
set counter_value = counter_value + 1  
where counter_text = 'mock_case_number'  
  
  
select max(counter_value) as value from telecare_counters  where counter_text = 'mock_case_number'  
--Select (ISNULL(Max(cas_case_number),99999999) + 1) as value FROM [case]  
--WHERE cas_case_number is not null    
  
    
-- SET TRANSACTION ISOLATION LEVEL READ COMMITTED;    
    
END