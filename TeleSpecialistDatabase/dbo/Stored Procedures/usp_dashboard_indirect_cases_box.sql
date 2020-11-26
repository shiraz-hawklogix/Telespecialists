CREATE PROCEDURE [dbo].[usp_dashboard_indirect_cases_box]
	@filter VARCHAR(50) = ''
	
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @currentDate_EST DATETIME = [dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier]('Eastern Standard Time', GETUTCDATE())
	DECLARE @startDate DATE = CAST(@currentDate_EST AS DATE)
	DECLARE @DirectCallType INT  = 1
	SET @filter = LOWER(@filter)

	SELECT @startDate = CASE WHEN @filter = 'last 3 months' THEN  (@currentDate_EST - 90)
							 WHEN @filter = 'last 2 months' THEN  (@currentDate_EST - 60)
							 WHEN @filter = 'last month' THEN (@currentDate_EST - 30)
						END 

;with cteUCLdata AS  
(  
 SELECT ucd_key, ucd_title, ucd_ucl_key  
 FROM ucl_data  
 WHERE ucd_ucl_key = 35 
)

	SELECT cas_key AS CasKey, cas_case_number AS CaseNumber, facility.fac_name AS Facility
	,CASE WHEN cas_call_type <> @DirectCallType THEN   
    CASE WHEN CallerSourceUCL.ucd_title = 'other' AND cas_caller_source_text <> '' THEN cas_caller_source_text ELSE CallerSourceUCL.ucd_title END   
  ELSE '' END AS CallerSource
	FROM [case]
	INNER JOIN facility ON facility.fac_key = [case].cas_fac_key
	LEFT OUTER JOIN cteUCLdata AS [CallerSourceUCL]   (NOLOCK)ON (cas_caller_source_key = [CallerSourceUCL].[ucd_key])
	WHERE cas_cst_key != 140 AND cas_call_type = 2 AND cas_created_date >= @startDate
END
