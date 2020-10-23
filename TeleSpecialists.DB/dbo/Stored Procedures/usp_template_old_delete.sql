-- =============================================
-- Author:		<Adnan K.>
-- Create date: <2020-April-13>
-- Description:	<To Delete 90 Days Older templates >
-- =============================================
 
CREATE PROCEDURE [dbo].[usp_template_old_delete]
AS
BEGIN
	 
	SET NOCOUNT ON;
	DECLARE @CurrentDateTimeEST DATETIME;
	SELECT @CurrentDateTimeEST = dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier('Eastern Standard Time', GETUTCDate())



	SELECT cgt_cas_key, cgt_ent_key  into #case_generated_template
	FROM
	(
	SELECT cgt_cas_key, cgt_ent_key, cgt_modified_date, ROW_NUMBER() OVER (
      PARTITION BY cgt_cas_key
      ORDER BY cgt_key desc
   ) row_num
	FROM 
	case_generated_template  
	where DATEDIFF(Day,cgt_modified_date,@CurrentDateTimeEST) > 90
	) as temp
	where
	temp.row_num = 1
	
	
	SELECT cas_key into #deleteTemplates
		
	FROM
	(
	SELECT --top 1000
	cas_key, cas_case_number, cgt_ent_key,
	
		  CASE WHEN TemplateEntityType is null THEN  
		   CASE WHEN facility_contract.fct_service_calc like '%Neuro%' AND  cas_metric_tpa_consult = 1 
				THEN 4

				WHEN facility_contract.fct_service_calc like '%Stroke%' AND  cas_metric_tpa_consult = 1 
				THEN  3
				WHEN facility_contract.fct_service_calc like '%Neuro%' AND  cas_metric_tpa_consult = 0 
				THEN 	5

				WHEN facility_contract.fct_service_calc like '%Stroke%' AND  cas_metric_tpa_consult = 0 
				THEN   6
				ELSE NULL

		   END 
		   ELSE TemplateEntityType 
		   END AS TemplateType
		   
	FROM 
	[CASE] 
	INNER JOIN facility on cas_fac_key = facility.fac_key
	INNER JOIN facility_contract ON facility.fac_key = facility_contract.fct_key
	INNER JOIN #case_generated_template on cas_key = cgt_cas_key
	

	WHERE
	
	TemplateEntityType is not null
	or 
	(
	cas_ctp_key = 9 -- Stroke Alert
	and fac_not_templated_used = 1
	and facility_contract.fct_key is not null
	
	)
	) as TempCase
	WHERE
	TempCase.TemplateType = TempCase.cgt_ent_key
	
	
  update [case] 
   set [case].cas_template_deleted_date = @CurrentDateTimeEST
   FROM [case]
   INNER JOIN #deleteTemplates  on  [case].cas_key = #deleteTemplates.cas_key 

  Delete t  from case_generated_template as t  
   INNER JOIN #deleteTemplates  on  cgt_cas_key = #deleteTemplates.cas_key 

  DROP TABLE #deleteTemplates
  DROP TABLE #case_generated_template
  
   
END