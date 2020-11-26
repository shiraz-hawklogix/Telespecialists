CREATE procedure [dbo].[sp_get_casescount_by_casetype]
@Date DateTime = NULL
as
begin
--declare @date datetime = '11/17/2020 3:54:53 AM'
IF OBJECT_ID(N'tempdb..#temp') IS NOT NULL BEGIN DROP TABLE #temp END
IF OBJECT_ID(N'tempdb..#stroke') IS NOT NULL BEGIN DROP TABLE #stroke END
IF OBJECT_ID(N'tempdb..#pysicianblast') IS NOT NULL BEGIN DROP TABLE #pysicianblast END
IF OBJECT_ID(N'tempdb..#navigatorblast') IS NOT NULL BEGIN DROP TABLE #navigatorblast END
IF OBJECT_ID(N'tempdb..#stat') IS NOT NULL BEGIN DROP TABLE #stat END


select cas_created_date as casedate,cas_ctp_key,cas_billing_physician_blast,cas_is_nav_blast
into #temp from [dbo].[case]
where convert(date,cas_created_date) = convert(date,@date) and cas_cst_key != 140


select CAST(casedate as DATE) as 'Date',CAST(DATEPART(Hour, casedate) as int) as 'Hour',COUNT(*) as 'StrokeCount'
into #stroke from #temp
where cas_ctp_key = 9
GROUP BY CAST(casedate as DATE), DATEPART(Hour, casedate),cas_ctp_key
ORDER BY CAST(casedate as DATE) ASC

select CAST(casedate as DATE) as 'Date',CAST(DATEPART(Hour, casedate) as int) as 'Hour',COUNT(*) as 'PhysicianBlastCount'
into #pysicianblast from #temp
where cas_billing_physician_blast = 1
GROUP BY CAST(casedate as DATE), DATEPART(Hour, casedate),cas_ctp_key
ORDER BY CAST(casedate as DATE) ASC

select CAST(casedate as DATE) as 'Date',CAST(DATEPART(Hour, casedate) as int) as 'Hour',COUNT(*) as 'NavigatorBlastCount'
into #navigatorblast from #temp
where cas_is_nav_blast = 1
GROUP BY CAST(casedate as DATE), DATEPART(Hour, casedate),cas_ctp_key
ORDER BY CAST(casedate as DATE) ASC


select CAST(casedate as DATE) as 'Date',CAST(DATEPART(Hour, casedate) as int) as 'Hour',COUNT(*) as 'StatCount'
into #stat from #temp
where cas_ctp_key = 10
GROUP BY CAST(casedate as DATE), DATEPART(Hour, casedate),cas_ctp_key
ORDER BY CAST(casedate as DATE) ASC

select distinct #stroke.Date,#stroke.Hour
,ISNULL(StrokeCount,0) Count
,ISNULL(phyblas.PhysicianBlastCount,0) PhysicianBlastCount
,ISNULL(navblas.NavigatorBlastCount,0) NavigatorBlastCount
,ISNULL(stat.StatCount,0) STATCount
from #stroke
LEFT join #pysicianblast phyblas on phyblas.Date = #stroke.Date AND phyblas.Hour = #stroke.Hour
LEFT join #navigatorblast navblas on navblas.Date = #stroke.Date AND navblas.Hour = #stroke.Hour
LEFT join #stat stat on stat.Date = #stroke.Date AND stat.Hour = #stroke.Hour

end
