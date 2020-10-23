-- =============================================
-- Author:		<Adnan K>
-- Create date: <2020-Feb-21>
-- Description:	<Optimize Case Dashboard Page Query>
-- =============================================
CREATE PROCEDURE [dbo].[usp_case_dashboard] 
	-- Add the parameters for the stored procedure here	
	@Take INT = NULL,
	@Skip INT = NULL,
	@SortDir varchar(5) = 'ASC',
	@SortType varchar(100) = NULL,
	@Physician varchar(128) = NULL,
	@SearchText varchar(50) = NULL,
	@CaseNumber bigint = NULL,
	@CaseStatus varchar(300) = NULL,
	@CaseType varchar(300) = NULL,
	@CaseTypeOpr varchar(5) = NULL,
	@FacilityIds varchar(Max) = NULL,
	@VisitType varchar(20) = NULL,
	@Flagged INT = NULL,
	@StartDate DateTime = NULL,
	@EndDate DateTime = NULL, 
	@DateFilter varchar(20) = NULL,
	@IsEAlert INT = NULL,
	@PatientName varchar(50) = NULL,
	@UserInitialFilter varchar(1000) = NULL,
	@ResponseTimeSec	INT = NULL,
	@ResponseTimeSecOpr	varchar(5) = NULL,
	@StartToStampSec INT = NULL,
	@StartToStampSecOpr	varchar(5) = NULL, 
	@StartToAcceptSec INT = NULL,
	@StartToAcceptSecOpr	varchar(5) = NULL,
	@CallerType varchar(max) = NULL,
	@CallerSource  varchar(max) = NULL,
	@TPAFilter	varchar(max) = NULL,
	@BillingCodeFilter varchar(max) = NULL
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @CaseNumberSearchText				BIGINT =  TRY_PARSE(@SearchText as bigint)
	DECLARE @StrokeAlert						INT		= 9
	DECLARE @DirectCallType						INT		= 1		

	IF (@CaseNumberSearchText IS NOT NULL) 
	BEGIN
		SET @SearchText  = NULL
	END

--;with cte_PhysicianSelection as 
--(		
--	select  distinct cah_cas_key from case_assign_history
--	where
--	cah_phy_key in (select val from dbo.SplitData(@UserInitialFilter, ','))	
--),
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
		ELSE ''	END as callerSource,
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
		dbo.FormatSeconds(cts_callback_to_tsnotification) as CallBackResponseTime,

		CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END  as totalRecords

     
        FROM     [dbo].[case] AS [CaseTable]  (NOLOCK) 

		
		LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK) ON (cas_cst_key = [CaseStatusUCL].[ucd_key]) --AND ([CaseStatusUCL].[ucd_ucl_key] = 12)
        LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]   (NOLOCK)ON (cas_ctp_key = [CaseTypeUCL].[ucd_key]) --AND ([CaseTypeUCL].[ucd_ucl_key] = 11)
        LEFT OUTER JOIN cteUCLdata AS [BillingCodeUCL]  (NOLOCK)ON (cas_billing_bic_key = [BillingCodeUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 10)
		LEFT OUTER JOIN cteUCLdata AS [CallerSourceUCL]   (NOLOCK)ON (cas_caller_source_key = [CallerSourceUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 10)
		LEFT OUTER JOIN AspNetUsers as Physician   (NOLOCK) ON cas_phy_key = Physician.Id
        -- LEFT OUTER JOIN cte_PhysicianSelection      ON cas_key = cte_PhysicianSelection.cah_cas_key
		LEFT OUTER JOIN case_timestamp  (NOLOCK) ON cas_key = cts_cas_key
		INNER JOIN [dbo].[facility]  (NOLOCK)  ON cas_fac_key = [facility].[fac_key]
        WHERE 
		cas_is_active = 1 	
		AND (cas_ctp_key IN (9,13,10)) -- StrokeAlert, StateEEG, StatConsult
		AND (ISNULL(@Physician,'') = '' OR cas_phy_key  = @Physician)
		AND (ISNULL(@CaseStatus,'') = '' OR cas_cst_key IN (select val FROM dbo.SplitData(@CaseStatus, ',')))
		AND (ISNULL(@CallerType,'') = '' OR cas_call_type IN (select val FROM dbo.SplitData(@CallerType, ',')))	
		AND (ISNULL(@CallerSource,'') = '' OR cas_caller_source_key IN (select val FROM dbo.SplitData(@CallerSource, ',')))			
		AND (ISNULL(@TPAFilter,'') = '' OR cas_metric_tpa_consult IN (select val FROM dbo.SplitData(@TPAFilter, ',')))			
		AND ( 
		ISNULL(@CaseType, '') = '' 
		OR  ( 

			(ISNULL(@CaseTypeOpr, '')  = 'neq'  AND cas_ctp_key NOT IN (select val FROM dbo.SplitData(@CaseType, ',')))
		OR (ISNULL(@CaseTypeOpr, '') = 'eq' AND  cas_ctp_key  IN (select val FROM dbo.SplitData(@CaseType, ',')))
	--	OR 1 = 1
			)
		)
		AND (ISNULL(@VisitType,'') = '' OR cas_billing_visit_type = @VisitType )						
		AND (@Flagged IS NULL  OR cas_is_flagged_dashboard = @Flagged )
		AND (@IsEAlert IS NULL  OR cas_is_ealert = @IsEAlert )										
		AND (ISNULL(@StartDate,'') = '' 
								OR (@DateFilter IN ('Today', 'Yesterday','SpecificDate') AND  CONVERT(date, cas_created_date)  = CONVERT(date,  @StartDate))
								OR (@DateFilter NOT IN ('Today', 'Yesterday','SpecificDate','DateRange') AND  cas_created_date >= @StartDate)
								OR (@DateFilter  IN ('DateRange') AND  CONVERT(date, cas_created_date)  >= CONVERT(date,  @StartDate))
			)
		AND (ISNULL(@EndDate,'') = '' OR (Convert(date,cas_created_date) <= CONVERT(date,  @EndDate)))
	    AND ((ISNULL(@PatientName,'') = '') OR (cas_patient like Concat('%',@PatientName,'%')))		 
		AND (ISNULL(@FacilityIds,'') = '' OR (cas_fac_key IN (select val FROM dbo.SplitData(@FacilityIds,','))))
		AND (ISNULL(@CaseNumber,'') = '' OR (cas_case_number = @CaseNumber))
		AND (ISNULL(@UserInitialFilter,'') = '' OR   (cas_phy_key IN (select val from dbo.SplitData(@UserInitialFilter, ','))))
		AND (@CaseNumberSearchText IS NULL OR cas_case_number = @CaseNumberSearchText)	
				
		AND (ISNULL(@SearchText,'') = ''   				
			OR (cas_cart = @SearchText)
			OR (cas_callback like Concat('%',@SearchText,'%'))
			OR (cas_patient like Concat('%',@SearchText,'%'))
			

			OR (fac_name like Concat('%',@SearchText,'%'))
		    OR (cas_created_by_name like Concat('%',@SearchText,'%'))
			OR (CaseStatusUCL.ucd_title like Concat('%',@SearchText,'%'))
			OR (CaseTypeUCL.ucd_title like Concat('%',@SearchText,'%'))
			OR (BillingCodeUCL.ucd_title like Concat('%',@SearchText,'%'))
			OR (Physician.FirstName like Concat('%',@SearchText,'%'))
			OR (Physician.LastName like Concat('%',@SearchText,'%'))
			
			)

		AND (
				ISNULL(@ResponseTimeSecOpr, '') = '' 
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'eq'  AND cts_response_sec  = @ResponseTimeSec)
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'gt'  AND cts_response_sec  > @ResponseTimeSec)				
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'lt'  AND cts_response_sec  < @ResponseTimeSec)
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'neq'  AND cts_response_sec  <> @ResponseTimeSec)
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'LessThanOrEqual'  AND cts_response_sec  <= @ResponseTimeSec)
				OR ( ISNULL(@ResponseTimeSecOpr, '')  = 'GreaterThanOrEqual'  AND cts_response_sec  >= @ResponseTimeSec)																								
		    )

	AND (
				ISNULL(@StartToStampSecOpr, '') = '' 
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'eq'  AND cts_start_to_stamp_sec  = @StartToStampSec)
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'gt'  AND cts_start_to_stamp_sec  > @StartToStampSec)				
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'lt'  AND cts_start_to_stamp_sec  < @StartToStampSec)
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'neq'  AND cts_start_to_stamp_sec  <> @StartToStampSec)
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'LessThanOrEqual'  AND cts_start_to_stamp_sec  <= @StartToStampSec)
				OR ( ISNULL(@StartToStampSecOpr, '')  = 'GreaterThanOrEqual'  AND cts_start_to_stamp_sec  >= @StartToStampSec)																								
		)
		AND (
				ISNULL(@StartToAcceptSecOpr, '') = '' 
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'eq'  AND cts_start_to_accept_sec  = @StartToAcceptSec)
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'gt'  AND cts_start_to_accept_sec  > @StartToAcceptSec)				
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'lt'  AND cts_start_to_accept_sec  < @StartToAcceptSec)
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'neq'  AND cts_start_to_accept_sec  <> @StartToAcceptSec)
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'LessThanOrEqual'  AND cts_start_to_accept_sec  <= @StartToAcceptSec)
				OR ( ISNULL(@StartToAcceptSecOpr, '')  = 'GreaterThanOrEqual'  AND cts_start_to_accept_sec  >= @StartToAcceptSec)																								
		)
		

					
		
		ORDER BY 
		case when @SortType is null  then cas_key        
        end DESC,	

		case when @SortDir <> 'DESC' then 0
        when @SortType = 'cas_key' then cas_key
        end DESC,		
	     case
        when @SortDir <> 'ASC' then 0
        when @SortType = 'cas_key' then cas_key
        end ASC,

		case when @SortDir <> 'DESC' then 0
        when @SortType = 'cas_case_number' then cas_case_number
        end DESC,		
	     case
        when @SortDir <> 'ASC' then 0
        when @SortType = 'cas_case_number' then cas_case_number
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_response_ts_notification' then cas_response_ts_notification
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_response_ts_notification' then cas_response_ts_notification
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_metric_stamp_time_est' then cas_metric_stamp_time_est
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_metric_stamp_time_est' then cas_metric_stamp_time_est
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_billing_date_of_consult' then cas_billing_date_of_consult
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_billing_date_of_consult' then cas_billing_date_of_consult
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_patient' then cas_patient
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_patient' then cas_patient
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'phy_name' then cas_history_physician_initial
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'phy_name' then cas_history_physician_initial
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cst_name' then [CaseStatusUCL].[ucd_title]
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cst_name' then [CaseStatusUCL].[ucd_title]
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'ctp_name' then [CaseTypeUCL].[ucd_title]
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'ctp_name' then [CaseTypeUCL].[ucd_title]
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'Navigator' then cas_created_by_name
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'Navigator' then cas_created_by_name
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_billing_visit_type' then cas_billing_visit_type
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_billing_visit_type' then cas_billing_visit_type
        end ASC,

		case when @SortDir <> 'DESC' then 0
        when @SortType = 'IsPhysicianBlast' then cas_billing_physician_blast
        end DESC,		
	     case
        when @SortDir <> 'ASC' then 0
        when @SortType = 'IsPhysicianBlast' then cas_billing_physician_blast
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'BillingCode' then [BillingCodeUCL].[ucd_title]
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'BillingCode' then [BillingCodeUCL].[ucd_title]
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'cas_callback' then cas_callback
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'cas_callback' then cas_callback
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'StartToStamp' then cts_start_to_stamp_sec
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'StartToStamp' then cts_start_to_stamp_sec
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'StartToAccept' then cts_start_to_accept_sec
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'StartToAccept' then cts_start_to_accept_sec
        end ASC,

		case when @SortDir <> 'DESC' then ''
        when @SortType = 'ResponseTime' then cts_response_sec
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'ResponseTime' then cts_response_sec
        end ASC,
		
		case when @SortDir <> 'DESC' then ''
        when @SortType = 'CallBackResponseTime' then cts_callback_to_tsnotification
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'CallBackResponseTime' then cts_callback_to_tsnotification
        end ASC

 
		OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS
	FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY

END
