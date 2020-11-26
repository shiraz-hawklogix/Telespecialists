CREATE procedure [dbo].[sp_pending_cases_notification]  
@TimeFrom datetime,
@TimeTo datetime
  
AS  
BEGIN  
--declare @TimeFrom datetime set @TimeFrom = '10/28/2020 4:57:35 PM'
--declare @TimeTo datetime  set @TimeTo = '10/28/2020 5:03:35 PM'
  
IF OBJECT_ID(N'tempdb..#tempPCase') IS NOT NULL BEGIN DROP TABLE #tempPCase END   
IF OBJECT_ID(N'tempdb..#tempPCaseCount') IS NOT NULL BEGIN DROP TABLE #tempPCaseCount END   
  
select distinct uss_user_id,uss_time_from_calc,uss_time_to_calc   
into #tempPCase from user_schedule  
where uss_time_to_calc between  @TimeFrom and @TimeTo  

  
select cas_phy_key,cas_created_date,asp.UserInitial as phy_name,cas_cst_key,ucl.ucd_title
--,Substring(Convert(varchar,cast(t.uss_time_from_calc as time)),0,6) + ' - ' + SUBSTRING(Convert(varchar,CAST(t.uss_time_to_calc as time)),0,6) as Shift_Time   
into #tempPCaseCount  
from [case] c  
join AspNetUsers asp on c.cas_phy_key = asp.Id  
join ucl_data ucl on c.cas_cst_key = ucl.ucd_key   
join #tempPCase t on t.uss_user_id = c.cas_phy_key  
--where cas_created_date between DATEADD(DAY,-3,t.uss_time_from_calc) and t.uss_time_to_calc 
where cas_created_date between t.uss_time_from_calc and t.uss_time_to_calc 
--where cas_created_date <= t.uss_time_to_calc 
and cas_cst_key in (17,18,19)  
and cas_phy_key in (select distinct uss_user_id from #tempPCase)  
  
select phy_name,count(phy_name) as pendingCasesCount  
from #tempPCaseCount  
group by phy_name  
order by 1 desc  
  
END