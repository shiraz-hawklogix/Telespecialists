
CREATE procedure [dbo].[usp_UserPresence_Graph_Report]

@starttime datetime = null,
 @endtime datetime = null,
 @status varchar(Max) = null
as
begin
IF OBJECT_ID(N'tempdb..#tempUserPR') IS NOT NULL BEGIN   DROP TABLE #tempUserPR  END 

SELECT Id,(convert(varchar,@starttime,101) + ' ( ' + convert(varchar(5),uss_time_from_calc,114) + ' - ' +  convert(varchar(5),uss_time_to_calc,114) + ' ) ') as CreatedDate   ,Physician, StatusName, DATEDIFF(SECOND, pDataDate, psl_created_date) as diff, Convert(char(8),pDataDate,114) as StartTime
into #tempUserPR
FROM    (
      SELECT *,LAG(psl_created_date) OVER (ORDER BY psl_created_date) pDataDate,LAG(phs_name) OVER (ORDER BY psl_created_date) StatusName FROM (
	  SELECT  uss_time_from_calc,uss_time_to_calc,phs_name,psl_created_date,psl_user_key as Id, FirstName + ' ' + LastName as Physician 
        FROM    physician_status_log
		join AspNetUsers on Id = psl_user_key
		join user_schedule on uss_user_id = psl_user_key
		join physician_status on psl_phs_key = phs_key
		where  CONVERT(date,psl_created_date) >=  convert(date,@starttime) and  convert(date,psl_created_date) <= convert(date,@endtime)  and psl_user_key = @status
		and psl_created_date >= uss_time_from_calc
		and psl_created_date <= uss_time_to_calc
		and uss_date =  @starttime 

		union 

		select uss_time_from_calc,uss_time_to_calc,'Z-TIME',uss_time_to_calc,uss_user_id, FirstName + ' ' + LastName as Physician
		from user_schedule 
		join AspNetUsers on Id = uss_user_id
		where uss_date =  @starttime   and uss_user_id in( select val FROM dbo.SplitData(@status,','))
) as tmp
      ) q 
WHERE   pDataDate IS NOT NULL and StatusName is not null 
order by Physician

select * from #tempUserPR
end
