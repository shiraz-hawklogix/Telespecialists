-- =============================================
CREATE PROCEDURE [dbo].[usp_get_cwh_data]

	-- Add the parameters for the stored procedure here

	@StartDateForAll		datetime,
	@edateForAll			datetime

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	----- CONSTANTS ---


	DECLARE @pos					int
	Declare @len					int
	Declare @VALUE					VARCHAR(128)
	DECLARE @result                 TABLE ([fac_key]  nvarchar(max)  , [fac_name]  nvarchar(max),  [month_wise_cwh] nvarchar(max),[Date] date)
	DECLARE @facilitiesId           varchar(MAX)
	SET @pos						= 0
	SET @len						= 0
	SET @facilitiesId				= (Select STRING_AGG(Convert(varchar(MAX), fac_key), ',') from facility )
	

	
WHILE CHARINDEX(',', @facilitiesId, @pos+1) > 0
BEGIN
    set @len = CHARINDEX(',', @facilitiesId, @pos+1) - @pos
    set @VALUE = SUBSTRING(@facilitiesId, @pos, @len)
	DECLARE @StartDate				datetime = @StartDateForAll
	DECLARE @edate					datetime = @edateForAll
	SET @StartDate				  = @StartDateForAll
	SET  @edate					  = @edateForAll

WHILE @StartDate  < @edate
BEGIN
    
DECLARE @startdates  datetime
set @startdates = @StartDate

DECLARE @edates  datetime 
set @edates  =  EOMONTH(@startdates)
  Insert into @result      
SELECT *  FROM 
(SELECT DISTINCT fat.fac_key, fat.fac_name
,
--ISNULL(ROUND(cast((cast((select COUNT(*) 
--	FROM [dbo].[case] where cas_response_ts_notification >=  @StartDateForAll
--	AND cas_response_ts_notification <= @edateForAll and cas_ctp_key = 9
--    AND cas_cst_key = 20 and cas_is_active = 1 and convert(varchar(128),cas_fac_key) IN (@VALUE)) as float)    /
--	cast(NULLIF((select count(*)
--    FROM [dbo].[case] 
--    WHERE cas_response_ts_notification >= @StartDateForAll and cas_response_ts_notification <= @edateForAll and cas_ctp_key = 9
--    AND cas_cst_key = 20 and cas_is_active = 1 ),0) as float)) as float),4), 0) 
--	as Total_CWH,
	ISNULL(ROUND(cast((cast((select COUNT(*)
    FROM [dbo].[case] where cas_response_ts_notification >=  @startdates 
    AND cas_response_ts_notification <=@edates  and cas_ctp_key = 9
    AND cas_cst_key = 20 and cas_is_active = 1 
    AND convert(varchar(128),cas_fac_key) IN (@VALUE)) as float)/
	 cast(NULLIF((select count(*)
    FROM [dbo].[case] where cas_response_ts_notification >= @startdates 
    AND cas_response_ts_notification <=  @edates and cas_ctp_key = 9
    AND cas_cst_key = 20 and cas_is_active = 1 ),0) as float)) as float),4), 0) 
    AS month_wise_cwh,   (@startdates) as date

     FROM [dbo].[facility] fat 
    left join [dbo].[case] cas ON cas.cas_fac_key= fat.fac_key

    WHERE convert(varchar(128),fat.fac_key) IN (@VALUE)
   --AND cas.cas_response_ts_notification >= @StartDate and cas.cas_response_ts_notification <=@edate
   --and cas.cas_ctp_key = 9
   --AND cas.cas_cst_key = 20 and 
	--cas.cas_is_active = 1 
    GROUP By   fat.fac_name , cas.cas_fac_key,fat.fac_key
	) As result
     SET @StartDate = DATEADD(month,1,@StartDate)
END

  set @pos = CHARINDEX(',', @facilitiesId, @pos+@len) +1

END

SELECT * FROM @result order by fac_name
END