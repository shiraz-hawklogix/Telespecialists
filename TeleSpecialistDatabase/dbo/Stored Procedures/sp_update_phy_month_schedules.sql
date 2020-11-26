   
create procedure [dbo].[sp_update_phy_month_schedules]          
@month int,  
@year int  
AS          
BEGIN      
  
Update user_schedule  
set uss_is_publish = 1  
where MONTH(uss_date) = @month  
AND YEAR(uss_date) = @year  
 
END