CREATE PROCEDURE [dbo].[sp_send_text_notification_operation_outliers]       
 @case_start_date_time datetime = null,    
 @case_end_date_time datetime = null    
AS      
BEGIN      
       
 SET NOCOUNT ON;      
      
 DECLARE @StrokeAlert  INT  = 9      
 DECLARE @DirectCallType INT  = 1        
      
;with cteUCLdata AS      
(      
 SELECT ucd_key, ucd_title, ucd_ucl_key      
 FROM ucl_data      
 WHERE ucd_ucl_key IN (10,11,12,35)      
)      
          
Select  cas_key,       
        [dbo].[FormatDateTime](cas_status_assign_date,1) as cas_status_assign_date,                      
        cas_cst_key,       
        cas_case_number,       
        cas_is_flagged_dashboard as cas_is_flagged,      
  [dbo].[FormatDateTime]([dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier](N'Eastern Standard Time', cas_response_ts_notification), 1) AS cas_response_ts_notification,       
        [dbo].[FormatDateTime](cas_metric_stamp_time_est, 1) as cas_metric_stamp_time_est,       
  CASE WHEN cas_ctp_key = @StrokeAlert THEN       
     CASE WHEN cas_call_type = @DirectCallType THEN 'Direct' WHEN cas_call_type IS NULL THEN '' ELSE 'Indirect' END        
     ELSE  '' END as callType,        
  CASE WHEN cas_call_type <> @DirectCallType THEN       
    CASE WHEN CallerSourceUCL.ucd_title = 'other' AND cas_caller_source_text <> '' THEN cas_caller_source_text ELSE CallerSourceUCL.ucd_title END       
  ELSE '' END as callerSource,      
  CASE WHEN ([CaseTypeUCL].[ucd_key] IS NOT NULL) THEN [CaseTypeUCL].[ucd_title] ELSE N'' END AS ctp_name,              
  [facility].[fac_name] AS fac_name,       
  cas_patient,      
  cas_history_physician_initial as phy_name,       
  cas_is_ealert,                   
  CASE WHEN ([CaseStatusUCL].[ucd_key] IS NOT NULL) THEN [CaseStatusUCL].[ucd_title] ELSE '' END AS cst_name,         
  CASE WHEN cas_metric_tpa_consult = 1 THEN 'Yes' ELSE 'No' END AS TPACandidate,      
  cas_created_by_name As Navigator,       
  dbo.FormatSeconds(cts_start_to_stamp_sec) as StartToStamp,      
  dbo.FormatSeconds(cts_start_to_accept_sec) as StartToAccept,       
  dbo.FormatSeconds(cts_response_sec) as ResponseTime,      
  dbo.FormatSeconds(cts_callback_to_tsnotification) as CallBackResponseTime,ISNULL(cas_created_date,'') cas_created_date, ISNULL(cas_modified_date,'') cas_modified_date       
  FROM [dbo].[case] AS [CaseTable]  (NOLOCK)       
      
  LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK) ON (cas_cst_key = [CaseStatusUCL].[ucd_key])     
  LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]   (NOLOCK)ON (cas_ctp_key = [CaseTypeUCL].[ucd_key])     
  LEFT OUTER JOIN cteUCLdata AS [BillingCodeUCL]  (NOLOCK)ON (cas_billing_bic_key = [BillingCodeUCL].[ucd_key])    
  LEFT OUTER JOIN cteUCLdata AS [CallerSourceUCL]   (NOLOCK)ON (cas_caller_source_key = [CallerSourceUCL].[ucd_key])    
  LEFT OUTER JOIN AspNetUsers as Physician   (NOLOCK) ON cas_phy_key = Physician.Id      
  LEFT OUTER JOIN case_timestamp  (NOLOCK) ON cas_key = cts_cas_key      
  INNER JOIN [dbo].[facility]  (NOLOCK)  ON cas_fac_key = [facility].[fac_key]      
  WHERE  cas_is_active = 1        
  AND (cas_ctp_key IN (9,13,10))    
  AND ((cas_created_date between @case_start_date_time and @case_end_date_time) OR (cas_modified_date IS NOT NULL AND cas_modified_date between @case_start_date_time and @case_end_date_time))      
END
