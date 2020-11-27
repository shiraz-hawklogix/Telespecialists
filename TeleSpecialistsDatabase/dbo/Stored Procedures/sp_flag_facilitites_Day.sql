
-- Author:  <Muhammad Bilal>                
-- Create date: <08/27/2020>                
-- Description: <return the facility list with less than 3 doctors sceduled on any time on a day>                
-- =============================================                
              
            
CREATE procedure [dbo].[sp_flag_facilitites_Day]                
@start_date bigint              
AS                
BEGIN               
              
--declare @start_date varchar(100) set @start_date = GETDATE() - 1              
              
IF OBJECT_ID(N'tempdb..#tempSchDay') IS NOT NULL BEGIN   DROP TABLE #tempSchDay  END                
select * into #tempSchDay 
from view_user_schedule                
Where DayNumber = @start_date  

select *  from #tempSchDay
       
              
              
DECLARE @min_start_time decimal; SET @min_start_time = (SELECT MIN(TIMEFROM) from #tempSchDay)              
DECLARE @max_end_time decimal; SET @max_end_time =  (SELECT MAX(TIMETO) from #tempSchDay)              
DECLARE @min_start_time_increase decimal SET @min_start_time_increase =  @min_start_time + 100;              
declare @result TABLE ( [date] decimal , [Physcian_count] Int)              
              
WHILE ( @min_start_time < @max_end_time )              
BEGIN              
              
insert into @result              
  SELECT distinct @start_date [date], COUNT(t.uss_user_id) [Physcian_count]              
             from #tempSchDay t                              
             RIGHT JOIN view_facility_physician fp on fp.fap_user_key = t.uss_user_id                             
			 and t.TIMEFROM <= @min_start_time AND t.TIMETO >= @min_start_time_increase              
			 GROUP BY fac_name                
			 HAVING COUNT(t.uss_user_id) < 3  
				
				 if((select count([date]) from @result) > 0)
  BEGIN
	BREAK
  END
								

  SET @min_start_time =  @min_start_time + 100             
  SET @min_start_time_increase = @min_start_time + 100 
  

END              
              
select * from @result              
              
END    
    
