
create PROCEDURE [dbo].[UspGetCWHData3]
@StartDate datetime = null,
@edate datetime = null
AS
BEGIN
select
cwh_fac_id,
cwh_fac_name,
cwh_totalcwh,
cwh_month_wise_cwh,
cwh_date
FROM [dbo].[cwh_data]
where cwh_date >= @StartDate and cwh_date <= @edate
end