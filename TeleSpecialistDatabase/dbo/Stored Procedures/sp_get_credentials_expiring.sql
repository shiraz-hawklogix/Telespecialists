Create procedure [dbo].[sp_get_credentials_expiring]


@Physicians varchar(MAX) = null,
@Facilities varchar(MAX) = null,
@Take INT = NULL,  
@Skip INT = NULL

as
begin
IF OBJECT_ID(N'tempdb..#temp') IS NOT NULL BEGIN DROP TABLE #temp END

DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)

select fap_key
,AspNetUsers.FirstName+' '+AspNetUsers.LastName as PhysicianName
,facility.fac_name as FacilityName
,DATEDIFF(d,@startDate,ISNULL(fap_end_date,0)) as Days
,CONVERT(varchar,fap_end_date,101) as EndDate
,fap_end_date
into #temp
from [dbo].[facility_physician]
inner join AspNetUsers on AspNetUsers.Id = facility_physician.fap_user_key
inner join facility on facility.fac_key = facility_physician.fap_fac_key
where (ISNULL(@Physicians,'') = '' OR fap_user_key IN (select val from dbo.SplitData(@Physicians, ','))) and (ISNULL(@Facilities,'') = '' OR CONVERT(varchar(128),fap_fac_key) IN (select val from dbo.SplitData(@Facilities, ',')))

select fap_key as Fac_Key,PhysicianName,FacilityName,Days,EndDate,fap_end_date
,CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END as totalRecords
from #temp
where Days > 0 and Days < 60
order by fap_end_date

OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS  
 FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY

end