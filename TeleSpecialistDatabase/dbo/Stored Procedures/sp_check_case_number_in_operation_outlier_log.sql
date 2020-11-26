CREATE procedure sp_check_case_number_in_operation_outlier_log  
@cas_case_number varchar(100),  
@cas_case_color varchar(50),  
@cas_case_type varchar(50)  
  
AS  
BEGIN  
if exists (select * from OperationOutlierNotificationLog where cas_case_number = @cas_case_number AND cas_case_color =  @cas_case_color AND cas_case_type = @cas_case_type)  
 BEGIN  
  select  CAST(1 AS BIT);  
 END  
else  
 BEGIN  
  select CAST(0 AS BIT);  
 END  
END