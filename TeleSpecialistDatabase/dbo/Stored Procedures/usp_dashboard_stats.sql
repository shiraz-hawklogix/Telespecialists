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

	DECLARE @StartToStampTime time
	DECLARE @StartToAcceptanceTime time
	DECLARE @StartToFirstLoginTime time

	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	
	SET @filter = LOWER(@filter)

	SELECT @startDate = CASE WHEN @filter = 'week' THEN  (@currentDate_EST - 7)
							 WHEN @filter = 'month' THEN  (@currentDate_EST - 30)
							 WHEN @filter = 'year' THEN (@currentDate_EST - 364)
							 WHEN @filter = 'today' THEN  @currentDate_EST
						END 

	/*
	SELECT 'Average Start to Stamp' AS Title, '00:02:39' AS TitleCount
	UNION
	SELECT 'Average Start to Acceptance' AS Title, '00:01:39' AS TitleCount
	UNION
	SELECT 'Average Start to First Login' AS Title, '00:01:39' AS TitleCount
	UNION
	SELECT 'Average Consult Time' AS Title, '00:22:39' AS TitleCount
	UNION
	SELECT 'Total Cases' AS Title, CAST(COUNT(*) AS VARCHAR(50))  AS TitleCount  FROM [case] (NOLOCK)
	UNION
	SELECT 'Total Stroke Alert' AS Title, CAST(COUNT(*) AS VARCHAR(50))  AS TitleCount FROM [case] (NOLOCK) INNER JOIN case_type ON case_type.ctp_key = [case].cas_ctp_key WHERE ctp_name = 'Stroke Alert'
	UNION
	SELECT 'Total Routine Consults' AS Title, CAST(COUNT(*) AS VARCHAR(50))  AS TitleCount FROM [case] (NOLOCK) INNER JOIN case_type ON case_type.ctp_key = [case].cas_ctp_key WHERE ctp_name = 'Routine Consult'
	UNION
	SELECT 'Total Others' AS Title , CAST(COUNT(*) AS VARCHAR(50))  AS TitleCount FROM [case] (NOLOCK) INNER JOIN case_type ON case_type.ctp_key = [case].cas_ctp_key WHERE ctp_name NOT IN ('Stroke Alert', 'Routine Consult')
	*/
	set @StartToStampTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_metric_stamp_time_est - cas_response_ts_notification) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate),'00:00:00')

	set @StartToAcceptanceTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_response_time_physician - cas_response_ts_notification) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate),'00:00:00')

	set @StartToFirstLoginTime = ISNULL((SELECT CONVERT(VARCHAR, ( CAST(CAST(AVG(CAST((cas_response_first_atempt - cas_response_ts_notification) AS FLOAT)) AS DATETIME) AS TIME(0)) ))
										FROM [case] (NOLOCK) 
										WHERE cas_created_date >= @startDate),'00:00:00')
	
	SELECT 
	TotalCases = (SELECT COUNT(*)  FROM [case] (NOLOCK) WHERE cas_created_date >= @startDate),
	TotalStrokeAlert = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Stroke Alert' AND cas_created_date >= @startDate),
	TotalRoutineConsultsNew = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Routine Consult- New' AND cas_created_date >= @startDate),
	TotalRoutineConsultsFollowup = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title = 'Routine Consult-Follow-Up' AND cas_created_date >= @startDate),
	TotalOthers = (SELECT COUNT(*)  FROM [case] (NOLOCK) INNER JOIN ucl_data ON ucl_data.ucd_key = [case].cas_ctp_key WHERE ucd_title NOT IN ('Stroke Alert', 'Routine Consult-Follow-Up', 'Routine Consult- New') AND cas_created_date >= @startDate),
	
	 CONVERT(varchar,@StartToStampTime, 108) AS AverageStartToStamp,
	 CONVERT(varchar,@StartToAcceptanceTime, 108) AS AverageStartToAcceptance,
	 CONVERT(varchar,@StartToFirstLoginTime, 108) AS AverageStartToFirstLogin, 
	
	AverageConsultTime = ISNULL ( (CONVERT(varchar, DATEADD(ms, ((datediff(second,0, @StartToStampTime) + datediff(second,0, @StartToAcceptanceTime) + datediff(second,0, @StartToFirstLoginTime)) / 3) * 1000, 0), 108)),
							   '00:00:00')
	
END