-- =============================================
-- Author:		<Atta H.>
-- Create date: <2019-Feb-21>
-- Description:	<Return data for graph>
-- =============================================
CREATE PROCEDURE [dbo].[usp_dashboard_stats]
	-- Add the parameters for the stored procedure here
	@filter VARCHAR(50) = ''
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--start_time = cas_response_ts_notification;
	--stamp_time = cas_metric_stamp_time_est;
	--acceptance_time = cas_response_time_physician
	--first_login = cas_response_first_atempt

	DECLARE @StartToStampTime bigint
	DECLARE @StartToAcceptanceTime bigint
	DECLARE @StartToFirstLoginTime bigint

	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	
	SET @filter = LOWER(@filter)

	SELECT @startDate = CASE WHEN @filter = 'week' THEN  (@currentDate_EST - 7)
							 WHEN @filter = 'month' THEN  (@currentDate_EST - 30)
							 WHEN @filter = 'year' THEN (@currentDate_EST - 364)
							 WHEN @filter = 'today' THEN  @currentDate_EST							 
						END 
  
	set @StartToStampTime =  (select AVg(isnull(dbo.DiffSeconds(cas_metric_stamp_time_est, cas_response_ts_notification),0)) as totalSeconds from
   [case] (NOLOCK) where cas_created_date >= @startDate )
    
  set @StartToAcceptanceTime  =( select AVg(isnull( dbo.DiffSeconds(cas_response_ts_notification,cas_response_time_physician ),0)) as totalSeconds from
   [case] (NOLOCK) where cas_created_date >= @startDate )

	set @StartToFirstLoginTime =( select AVg(isnull(dbo.DiffSeconds(cas_response_ts_notification,cas_response_first_atempt),0)) as totalSeconds from
   [case] (NOLOCK) where cas_created_date >= @startDate)
	
 

	SELECT 
	TotalCases = (SELECT COUNT(*)  FROM [case] (NOLOCK) WHERE cas_created_date >= @startDate) ,
	dbo.FormatSeconds(  @StartToStampTime ) AverageStartToStamp, 
	dbo.FormatSeconds( @StartToAcceptanceTime ) AverageStartToAcceptance,
	dbo.FormatSeconds( @StartToFirstLoginTime ) AverageStartToFirstLogin,
	TotalStrokeAlert = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Stroke Alert' AND cas_created_date >= @startDate),
	TotalRoutineConsultsNew = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Routine Consult- New' AND cas_created_date >= @startDate),
	TotalRoutineConsultsFollowup = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Routine Consult-Follow-Up' AND cas_created_date >= @startDate),
	TotalOthers = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title NOT IN ('Stroke Alert', 'Routine Consult-Follow-Up', 'Routine Consult- New') AND cas_created_date >= @startDate), 	
	AverageConsultTime= dbo.FormatSeconds(@StartToStampTime+@StartToAcceptanceTime+@StartToFirstLoginTime)
	 	 
	 
	
END


