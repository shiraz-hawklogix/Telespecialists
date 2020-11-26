create PROCEDURE [dbo].[usp_dashboard_completed_cases]
@filter VARCHAR(50) = '',
@Physicians varchar(MAX) = null,
 @Take INT = NULL,
 @Skip INT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	IF OBJECT_ID(N'tempdb..#temp') IS NOT NULL BEGIN DROP TABLE #temp END
DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	SET @filter = LOWER(@filter)

	SELECT @startDate = CASE WHEN @filter = 'last 3 months' THEN  (@currentDate_EST - 90)
							 WHEN @filter = 'last 2 months' THEN  (@currentDate_EST - 60)
							 WHEN @filter = 'last month' THEN (@currentDate_EST - 30)
							 END 

SELECT cas_key
,cas_case_number
,users.FirstName+' '+users.LastName AS QPS_Name
,facilities.fac_name AS FacilityName
,CONVERT(varchar,cas_billing_date_of_consult,101) AS billing_date_of_consult
,DATEDIFF(mi, cas_metric_door_time, cas_metric_needle_time) AS arivaltoneedle
,cas_created_date
,cas_response_case_qps_reviewed
,cas_billing_date_of_consult
into #temp
from [dbo].[case]
INNER JOIN facility AS facilities ON facilities.fac_key = cas_fac_key
INNER JOIN AspNetUsers AS users ON users.Id = facilities.qps_number
WHERE cas_created_date >= @startDate 
AND (ISNULL(@Physicians,'') = '' OR facilities.qps_number IN (select val from dbo.SplitData(@Physicians, ','))) 
AND cas_metric_tpa_consult = 1 
AND cas_is_active = 1 
AND cas_cst_key = 20 
AND cas_response_case_qps_reviewed = 1
ORDER BY cas_billing_date_of_consult DESC

select cas_key AS CaseKey,cas_case_number AS TC_CaseNumber,FacilityName,QPS_Name,billing_date_of_consult AS DateOfConsult
,CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END as TotalRecords
from #temp
where arivaltoneedle > 45
order by cas_billing_date_of_consult desc

OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS  
 FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY
end

