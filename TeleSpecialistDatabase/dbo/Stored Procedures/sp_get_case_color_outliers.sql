

CREATE procedure [dbo].[sp_get_case_color_outliers]
@casekey varchar(max)
--declare @casekey varchar(max) set @casekey = '189142,189134,189126,189119'
as
begin
IF OBJECT_ID(N'tempdb..#temp') IS NOT NULL BEGIN DROP TABLE #temp END
IF OBJECT_ID(N'tempdb..#temp1') IS NOT NULL BEGIN DROP TABLE #temp1 END
IF OBJECT_ID(N'tempdb..#tempfinal') IS NOT NULL BEGIN DROP TABLE #tempfinal END
IF OBJECT_ID(N'tempdb..#temp3') IS NOT NULL BEGIN DROP TABLE #temp3 END
IF OBJECT_ID(N'tempdb..#casetemp') IS NOT NULL BEGIN DROP TABLE #casetemp END
IF OBJECT_ID(N'tempdb..#tempResult') IS NOT NULL BEGIN DROP TABLE #tempResult END
IF OBJECT_ID(N'tempdb..#finalUserStatus') IS NOT NULL BEGIN DROP TABLE #finalUserStatus END


select cas.cas_key , asp.UserInitial
into #casetemp
from
[case] cas
join AspNetUsers asp on cas.cas_phy_key = asp.Id
and cas.cas_key in (select convert(int,TRIM(value)) from string_split(@casekey,','))


SELECT A.cas_key,  
     Split.a.value('.', 'VARCHAR(100)') AS PhysicianInitials,cas_created_date,ROW_NUMBER() OVER (PARTITION BY cas_key order by cas_key) as RowPartitionNumber   
 into #temp FROM  (SELECT cas_key,  
         CAST ('<M>' + REPLACE(cas_history_physician_initial, '/', '</M><M>') + '</M>' AS XML) AS PhysicianInitials,cas_created_date  
     FROM  [dbo].[case]) AS A CROSS APPLY PhysicianInitials.nodes ('/M') AS Split(a) 
	 --where cas_key IN (@casekey)
	 where Convert(varchar,cas_key) in (select convert(int,TRIM(value)) from string_split(@casekey,','))

select #temp.cas_key,RowPartitionNumber,Id,UserInitial,cas_created_date 
into #temp1 
from AspNetUsers as users
join #temp on #temp.PhysicianInitials = users.UserInitial
order by #temp.cas_key,RowPartitionNumber asc

--select * from #temp1

--select #temp1.cas_key,RowPartitionNumber,psl_phs_key,#temp1.UserInitial,psl_created_date
--into #tempfinal 
--from physician_status_log as phy_log
--join #temp1 on #temp1.Id = phy_log.psl_user_key and phy_log.psl_created_date <= #temp1.cas_created_date  
--where phy_log.psl_user_key = #temp1.Id 
--order by #temp1.cas_key,RowPartitionNumber asc

SELECT t1.Id, t1.cas_key,t1.UserInitial,
coalesce(MAX(t2.psl_key),0) As Prev_psl_key
into #temp3
FROM #temp1 t1
JOIN physician_status_log t2 ON t2.psl_user_key = t1.Id and t2.psl_created_date < t1.cas_created_date
GROUP BY  t1.Id, t1.cas_key,t1.UserInitial

select t3.cas_key,RowPartitionNumber,t3.Id,t3.UserInitial,t3.Prev_psl_key
into #tempfinal 
from #temp1 t1
join #temp3 t3 on t1.cas_key = t3.cas_key AND t1.Id = t3.Id
order by t1.cas_key,RowPartitionNumber asc


select t1.cas_key,t1.RowPartitionNumber,t1.UserInitial UserInitial,phs.phs_name psl_status_name,phs.phs_color_code psl_status_color,psl.psl_key 
into #tempResult 
from 
#tempfinal t1
join physician_status_log psl on psl.psl_key = t1.Prev_psl_key
join physician_status phs on phs.phs_key = psl.psl_phs_key
order by t1.cas_key,t1.RowPartitionNumber


SELECT distinct cas_key, STUFF((SELECT '/ ' + CAST(UserInitial AS VARCHAR(50)) [text()]
FROM #tempResult a
WHERE cas_key = t.cas_key
order by cas_key,RowPartitionNumber
FOR XML PATH(''), TYPE)
.value('.','NVARCHAR(MAX)'),1,2,' ') UserInitial,
STUFF((SELECT '/ ' + CAST(psl_status_name AS VARCHAR(50)) [text()]
FROM #tempResult a
WHERE cas_key = t.cas_key
order by cas_key,RowPartitionNumber
FOR XML PATH(''), TYPE)
.value('.','NVARCHAR(MAX)'),1,2,' ') psl_status_name,
STUFF((SELECT '/ ' + CAST(psl_status_color AS VARCHAR(50)) [text()]
FROM #tempResult a
WHERE cas_key = t.cas_key
order by cas_key,RowPartitionNumber
FOR XML PATH(''), TYPE)
.value('.','NVARCHAR(MAX)'),1,2,' ') psl_status_color
into #finalUserStatus
FROM #tempResult t

select
cas.cas_key as psl_cas_key,log.psl_status_color,log.psl_status_name,log.UserInitial
from #casetemp cas
left join #finalUserStatus log on cas.cas_key = log.cas_key

end
