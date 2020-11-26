         
CREATE procedure [dbo].[sp_flag_facilitites_Day]                
@start_date bigint              
AS                
BEGIN               
              
--declare @start_date varchar(100) set @start_date = GETDATE() - 1              
              
IF OBJECT_ID(N'tempdb..#tempSchDistinctDay') IS NOT NULL BEGIN   DROP TABLE #tempSchDistinctDay  END  
IF OBJECT_ID(N'tempdb..#tempSchDay') IS NOT NULL BEGIN   DROP TABLE #tempSchDay  END 

select * into #tempSchDay 
from view_user_schedule                
Where DayNumber = @start_date

--insert into #tempSchDay values('','',1,@start_date,CONVERT(VARCHAR,@start_date) + '0000',CONVERT(VARCHAR,@start_date) + '0100')
insert into #tempSchDay values('','',1,@start_date,CONVERT(VARCHAR,@start_date) + '2300',CONVERT(VARCHAR,@start_date) + '2400')
insert into #tempSchDay values('','',1,@start_date,CONVERT(VARCHAR,@start_date) + '2200',CONVERT(VARCHAR,@start_date) + '2300')
insert into #tempSchDay values('','',1,@start_date,CONVERT(VARCHAR,@start_date) + '2100',CONVERT(VARCHAR,@start_date) + '2200')
insert into #tempSchDay values('','',1,@start_date,CONVERT(VARCHAR,@start_date) + '2000',CONVERT(VARCHAR,@start_date) + '2100')

--select SUBSTRING(CONVERT(VARCHAR,TIMETO),0,8),*  from #tempSchDay

select ROW_NUMBER() OVER (ORDER BY timefrom) row_num,timefrom,timeto,COUNT(uss_user_id) as TotalUsers
into #tempSchDistinctDay
from #tempSchDay
group by timefrom,timeto  
order by 2 desc

declare @row_number_min int; set @row_number_min = (SELECT MIN(row_num) from #tempSchDistinctDay)
declare @row_number_max int; set @row_number_max = (SELECT MAX(row_num) from #tempSchDistinctDay)
                            
DECLARE @min_start_time decimal; SET @min_start_time = (SELECT timefrom from #tempSchDistinctDay where row_num = @row_number_min)
DECLARE @min_start_time_increase decimal SET @min_start_time_increase =  @min_start_time + 100;   
                        
declare @result TABLE ( [date] decimal , [Physcian_count] Int)              
              
WHILE ( @row_number_min <= @row_number_max )              
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

  set @row_number_min = @row_number_min + 1;
  SET @min_start_time = (SELECT timefrom from #tempSchDistinctDay where row_num = @row_number_min) 
  SET @min_start_time_increase =  @min_start_time + 100                   

END              
              
select * from @result              
              
END  



