CREATE PROCEDURE [dbo].[usp_dashboard_stats_new]
	@filter VARCHAR(50) = ''
	
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DoorToNeedleTime time
	DECLARE @LogonToNeedleTime time
	DECLARE @StartToFirstLoginTime time

	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	DECLARE @startDateTime DATETIME = CAST(@currentDate_EST AS DATETIME)

	SET @filter = LOWER(@filter)

	if(@filter != 'today from midnight' AND @filter != 'today from 700EST' AND @filter != 'today from 1900EST')
	BEGIN
		SELECT @startDate = CASE WHEN @filter = 'week' THEN  (@currentDate_EST - 7)
							 WHEN @filter = 'month' THEN  (@currentDate_EST - 30)
							 WHEN @filter = 'quarter' THEN (@currentDate_EST - 90)
							 --WHEN @filter = 'today' THEN  @currentDate_EST
							 END
							 set @DoorToNeedleTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_metric_needle_time - cas_metric_door_time) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate AND cas_metric_tpa_consult = 1 AND cas_metric_wakeup_stroke = 0),'00:00:00')

	set @LogonToNeedleTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_metric_needle_time - cas_response_first_atempt) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate),'00:00:00')

	set @StartToFirstLoginTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_response_first_atempt - cas_response_ts_notification) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate),'00:00:00')
	
	SELECT 
	TotalCases = (SELECT COUNT(*)  FROM [case] (NOLOCK) WHERE cas_created_date >= @startDate),
	TotalStrokeAlert = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_ctp_key WHERE cas_ctp_key = 9 AND cas_created_date >= @startDate),
	TotalStatConsults = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_ctp_key WHERE cas_ctp_key = 10 AND cas_created_date >= @startDate),
	TotalNavigatorBlasts = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_is_nav_blast WHERE cas_is_nav_blast = 1 AND cas_created_date >= @startDate),
	TotalPhysicianBlasts = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_billing_physician_blast WHERE cas_billing_physician_blast = 1 AND cas_created_date >= @startDate),
	
	 CONVERT(varchar,@DoorToNeedleTime, 108) AS AverageDoorToNeedle,
	 CONVERT(varchar,@LogonToNeedleTime, 108) AS AverageLogonToNeedle,
	 CONVERT(varchar,@StartToFirstLoginTime, 108) AS AverageStartToFirstLogin

	END
	ELSE
	BEGIN
		SELECT @startDateTime  = CASE WHEN @filter = 'today from midnight' THEN  DATEADD(day, DATEDIFF(day, 0, @startDate), '00:00:00')
							 WHEN @filter = 'today from 700EST' THEN  DATEADD(day, DATEDIFF(day, 0, @startDate), '07:00:00')
							 WHEN @filter = 'today from 1900EST' THEN DATEADD(day, DATEDIFF(day, 0, @startDate), '19:00:00')
							 END

		set @DoorToNeedleTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_metric_needle_time - cas_metric_door_time) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDateTime AND cas_metric_tpa_consult = 1 AND cas_metric_wakeup_stroke = 0),'00:00:00')

	set @LogonToNeedleTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_metric_needle_time - cas_response_first_atempt) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDateTime),'00:00:00')

	set @StartToFirstLoginTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_response_first_atempt - cas_response_ts_notification) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDateTime),'00:00:00')
	
	SELECT 
	TotalCases = (SELECT COUNT(*)  FROM [case] (NOLOCK) WHERE cas_created_date >= @startDateTime),
	TotalStrokeAlert = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_ctp_key WHERE cas_ctp_key = 9 AND cas_created_date >= @startDateTime),
	TotalStatConsults = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_ctp_key WHERE cas_ctp_key = 10 AND cas_created_date >= @startDateTime),
	TotalNavigatorBlasts = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_is_nav_blast WHERE cas_is_nav_blast = 1 AND cas_created_date >= @startDateTime),
	TotalPhysicianBlasts = (SELECT COUNT(*)  FROM [case] (NOLOCK) cas_billing_physician_blast WHERE cas_billing_physician_blast = 1 AND cas_created_date >= @startDateTime),
	
	 CONVERT(varchar,@DoorToNeedleTime, 108) AS AverageDoorToNeedle,
	 CONVERT(varchar,@LogonToNeedleTime, 108) AS AverageLogonToNeedle,
	 CONVERT(varchar,@StartToFirstLoginTime, 108) AS AverageStartToFirstLogin

	END

END
