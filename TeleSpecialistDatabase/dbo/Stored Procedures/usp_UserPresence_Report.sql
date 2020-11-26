
CREATE procedure [dbo].[usp_UserPresence_Report]

 @starttime datetime = null,
 @endtime datetime = null,
 @status varchar(Max) = null,
 @reportType varchar(Max) = ''

as
begin 

IF OBJECT_ID(N'tempdb..#tempStart') IS NOT NULL BEGIN   DROP TABLE #tempStart  END
IF OBJECT_ID(N'tempdb..#tempDailyFinal') IS NOT NULL BEGIN   DROP TABLE #tempDailyFinal  END
IF OBJECT_ID(N'tempdb..#tempUserPR') IS NOT NULL BEGIN   DROP TABLE #tempUserPR  END 
IF OBJECT_ID(N'tempdb..#tempUserPRFinal') IS NOT NULL BEGIN   DROP TABLE #tempUserPRFinal  END
IF OBJECT_ID(N'tempdb..#tempFinal') IS NOT NULL BEGIN   DROP TABLE #tempFinal  END


if(@reportType = 'daily')
begin

SELECT Id,convert(varchar,@starttime,101) as [date],(convert(varchar,@starttime,101) + ' ( ' + convert(varchar(5),uss_time_from_calc,114) + ' - ' +  convert(varchar(5),uss_time_to_calc,114) + ' ) ') as CreatedDate   ,Physician,PhysicianId, StatusName, DATEDIFF(SECOND, pDataDate, psl_created_date) as diff
into #tempUserPR
FROM    (
      SELECT *,LAG(psl_created_date) OVER (ORDER BY psl_created_date) pDataDate,LAG(phs_name) OVER (ORDER BY psl_created_date) StatusName FROM (
	  SELECT  uss_time_from_calc,uss_time_to_calc,phs_name,psl_created_date,psl_user_key as Id, FirstName + ' ' + LastName as Physician , PhysicianId
        FROM    physician_status_log
		join AspNetUsers on Id = psl_user_key
		join user_schedule on uss_user_id = psl_user_key
		join physician_status on psl_phs_key = phs_key
		where  CONVERT(date,psl_created_date) >=  convert(date,@starttime) and  convert(date,psl_created_date) <= convert(date,@endtime)  and psl_user_key = @status
		and psl_created_date >= uss_time_from_calc
		and psl_created_date <= uss_time_to_calc
		and uss_date =  @starttime 

		union 

		select uss_time_from_calc,uss_time_to_calc,'Z-TIME',uss_time_to_calc,uss_user_id, FirstName + ' ' + LastName as Physician, PhysicianId
		from user_schedule 
		join AspNetUsers on Id = uss_user_id
		where uss_date =  @starttime   and uss_user_id = @status
) as tmp
      ) q 
WHERE   pDataDate IS NOT NULL and StatusName is not null 
order by Physician

SELECT 
Id,
[date],
CreatedDate,
Physician,
PhysicianId,
ISNULL([Available],0) as Available,
ISNULL([TPA],0) as TPA,
ISNULL([Stroke Alert],0) as StrokeAlert, 
ISNULL([Rounding],0) as Rounding,
ISNULL([STAT Consult],0) as STATConsult,
ISNULL([Break],0) as [Break],
ISNULL([Post SA Workup],0) as PostSAWorkup,
ISNULL([Rounding prep],0) as RoundingPrep,
ISNULL([Post Stat Workup],0) as PostStatWorkup
into #tempDailyFinal
FROM   
(
    SELECT  Id,
	[date],
	CreatedDate,
	Physician,
	PhysicianId,
        StatusName,
		diff
    FROM 
       #tempUserPR
) t 
PIVOT(
    sum(diff)
	FOR StatusName IN (
        [Available], 
        [TPA],
		[Stroke Alert],
		[Rounding],
		[STAT Consult],
		[Break]
		,[Post SA Workup]
		,[Rounding Prep]
		,[Post Stat Workup])       
) AS pivot_table ;

select Id,[date],Physician,PhysicianId, CreatedDate,sum(Available) Available,sum(TPA) TPA,sum(StrokeAlert) StrokeAlert,
sum(Rounding) Rounding,sum(STATConsult) STATConsult,sum([Break]) [Break],sum(PostSAWorkup) PostSAWorkup,sum(RoundingPrep) RoundingPrep,sum(PostStatWorkup) PostStatWorkup
from #tempDailyFinal 
group by Id,Physician,PhysicianId, CreatedDate,[date]
order by CreatedDate
 

 end
-- else
-- begin
-- SELECT Id,cast(psl_created_date as date) as CreatedDate ,Physician, StatusName, DATEDIFF(MINUTE, pDataDate, psl_created_date) as diff
--  into #tempUserPRFinal
--FROM    (
--        SELECT  psl_status_name,psl_created_date,psl_user_key as Id, FirstName + ' ' + LastName as Physician,
--                LAG(psl_created_date) OVER (ORDER BY psl_created_date) pDataDate,
--				 LAG(psl_status_name) OVER (ORDER BY psl_created_date) StatusName
--        FROM    physician_status_log
--		join AspNetUsers on Id = psl_user_key
--		where  psl_created_date >=  @starttime and  psl_created_date <= @endtime  and psl_user_key = @status
--        ) q 
--WHERE   pDataDate IS NOT NULL and StatusName is not null 
--order by Physician

--SELECT 
--Id,
--CreatedDate,
--Physician,
--ISNULL([Available],0) as Available,
--ISNULL([TPA],0) as TPA,
--ISNULL([Stroke Alert],0) as StrokeAlert, 
--ISNULL([Rounding],0) as Rounding,
--ISNULL([STAT Consult],0) as STATConsult,
--ISNULL([Break],0) as [Break]
--into #tempFinal
--FROM   
--(
--    SELECT  Id,
--	CreatedDate,
--	Physician,
--        StatusName,
--		diff
--    FROM 
--       #tempUserPRFinal
--) t 
--PIVOT(
--    sum(diff)
--	FOR StatusName IN (
--        [Available], 
--        [TPA],
--		[Stroke Alert],
--		[Rounding],
--		[STAT Consult],
--		[Break])       
--) AS pivot_table ;


--select Physician, DATENAME(month, CreatedDate) as CreatedDate,sum(Available) Available,sum(TPA) TPA,sum(StrokeAlert) StrokeAlert,
--sum(Rounding) Rounding,sum(STATConsult) STATConsult,sum([Break]) [Break]
--from #tempFinal 
--group by Physician,DATENAME(month, CreatedDate)
--order by max(CreatedDate)
 
-- end
end
