
CREATE procedure [dbo].[sp_get_status_snapshot]
@Date DateTime = NULL,
@Time DateTime = NULL
as
begin
IF OBJECT_ID(N'tempdb..#temp') IS NOT NULL BEGIN DROP TABLE #temp END
IF OBJECT_ID(N'tempdb..#temp1') IS NOT NULL BEGIN DROP TABLE #temp1 END

select psl_key,psl_user_key,users.FirstName+' '+users.LastName as physician_name,PhysicianId,statuss.phs_name as physician_status,psl_created_date,
ROW_NUMBER() OVER (PARTITION BY psl_user_key order by psl_created_date desc) as rownumber
into #temp
from [dbo].[physician_status_log]
join [dbo].[AspNetUsers] users on users.Id = physician_status_log.psl_user_key
join [dbo].[physician_status] statuss on statuss.phs_key = physician_status_log.psl_phs_key
where convert(varchar, psl_created_date, 101) = convert(varchar, @Date, 101)
and convert(varchar, psl_created_date, 114) < convert(varchar, @Time, 114)
order by psl_created_date desc

select psl_key,physician_name,PhysicianId,physician_status,psl_created_date into #temp1 from #temp
where rownumber = 1

select psl_key,physician_name,PhysicianId, physician_status,psl_created_date from #temp1

end
