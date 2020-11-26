-- =============================================
-- Author:		<Adnan K>
-- Create date: <2020-March-12>
-- Description:	<Optimize Case Listing Page Query>
-- =============================================
-- Exec [dbo].[usp_GetAllPhysiciansByFacility] 
CREATE PROCEDURE [dbo].[usp_case_listing_export] 
	-- Add the parameters for the stored procedure here	

	@Physician varchar(128) = NULL,
	@SortDir varchar(5) = 'ASC',
	@SortType varchar(100) = NULL,
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
	@TPAFilter	varchar(max) = NULL,
	@PatientName varchar(50) = NULL,
	@UserInitialFilter varchar(1000) = NULL,
	@DummyParam varchar(5) = NULL -- Dummy parameter added to handle comman in GetAllSqlParams function in code
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @CaseNumberSearchText				BIGINT =  TRY_PARSE(@SearchText as bigint)	
	--declare @users nvarchar(max) = '''a''  or cas_history_physician_initial like %AH%' 

	IF (@CaseNumberSearchText IS NOT NULL) 
	BEGIN
		SET @SearchText  = NULL
	END

;with cte_PhysicianSelection as 
(		
	select  distinct cah_cas_key from case_assign_history
	where
	cah_phy_key in (select val from dbo.SplitData(@UserInitialFilter, ','))	
),
cteUCLdata AS
(
	SELECT ucd_key, ucd_title, ucd_ucl_key
	FROM ucl_data
	WHERE ucd_ucl_key IN (10,11,12,18,23,24,25)
)



Select  cas_key, 
		cas_case_number,
		cas_created_date,		
		fac_name as FacityName,
		(Physician.FirstName + ' ' + Physician.LastName ) as AssignedPhysician,
		fac_timezone,
		cas_cart,
		cas_callback,
		cas_callback_extension,
		cas_patient,
		cas_billing_dob,
		cas_caller,
		cas_metric_door_time,
		cas_identification_number,
		cas_last_4_of_ssn,
		cas_referring_physician,
		cas_pulled_from_rounding,
		cas_is_nav_blast,
		cas_notes,
		cas_eta,
		cas_metric_lastwell_date,
		cas_metric_stamp_time,
		cas_patient_type,
		cas_response_first_atempt,
		cas_metric_video_start_time,
		cas_metric_symptoms,
		cas_metric_assesment_time,
		ISNULL(cas_metric_last_seen_normal,0) as cas_metric_last_seen_normal,
		ISNULL(cas_metric_has_hemorrhgic_history,0) as cas_metric_has_hemorrhgic_history,
		ISNULL(cas_metric_has_recent_anticoagulants,0) as cas_metric_has_recent_anticoagulants,
		ISNULL(cas_metric_has_major_surgery_history,0) as cas_metric_has_major_surgery_history,
		ISNULL(cas_metric_has_stroke_history,0) as cas_metric_has_stroke_history,
		cas_metric_tpa_verbal_order_time,
		cas_metric_tpa_consult,
		[LoginDelayUCL].ucd_title as  Reason_for_Login_Delay,
		cas_metric_notes,
		dbo.UDF_ConvertUtcToLocalByTimezoneIdentifier(fac_timezone,cas_metric_pa_ordertime) as tPA_CPOE_Order_Time,
		cas_metric_needle_time,
		cas_metric_weight,
		cas_metric_weight_unit,
		 cas_metric_total_dose,
		 cas_metric_bolus,
		 cas_metric_infusion,
		 cas_metric_discard_quantity,
		 cas_metric_video_end_time,
		[TpaDelayUCL].ucd_title as  Reason_for_tpa_Login_Delay,
		[IdentificationTypeUCL].ucd_title as Identification_Type,
		[NonTpaReasonsTypeUCL].ucd_title as non_tpa_reason,
		cas_billing_tpa_delay_notes,
		cas_metric_ct_head_has_no_acture_hemorrhage,
		cas_metric_ct_head_is_reviewed,
		cas_metric_ct_head_is_not_reviewed,
		cas_metric_is_neuro_interventional,
		cas_metric_discussed_with_neurointerventionalist,
		cas_metric_physician_notified_of_thrombolytics,
		cas_metric_physician_recommented_consult_neurointerventionalist,		
		cas_billing_date_of_consult,
		cas_billing_diagnosis,
		cas_billing_notes,
		cas_billing_visit_type,
		cas_follow_up_date,
		cas_billing_physician_blast,
		cas_response_date_consult,
		cas_response_ts_notification,
		cas_response_time_physician,
		ISNULL(cas_response_sa_ts_md,0) as cas_response_sa_ts_md,
		ISNULL(cas_navigator_concurrent_alerts,0) as cas_navigator_concurrent_alerts,
		ISNULL(cas_physician_concurrent_alerts,0) as cas_physician_concurrent_alerts,
		ISNULL(cas_response_miscommunication,0) as cas_response_miscommunication,
		ISNULL(cas_response_technical_issues,0) as cas_response_technical_issues,
		ISNULL(cas_response_tpa_to_minute,0) as cas_response_tpa_to_minute,
		ISNULL(cas_response_door_to_needle,0) as cas_response_door_to_needle,
		cas_metric_stamp_time_est,
		cas_metric_needle_time_est,
		cas_metric_pa_ordertime_est,
		cas_metric_pa_ordertime,
		cas_response_reviewer,
		cas_response_case_research,
		ISNULL(cas_response_nav_to_ts,0) as cas_response_nav_to_ts,
		ISNULL(cas_response_pulled_rounding,0) as cas_response_pulled_rounding,
		cas_phy_technical_issue_date,
		ISNULL(cas_response_physician_blast,0) as cas_response_physician_blast,
		ISNULL(cas_response_review_initiated,0) as cas_response_review_initiated,
		cas_response_case_number,
		CASE WHEN ([BillingCodeUCL].[ucd_key] IS NOT NULL) THEN [BillingCodeUCL].[ucd_title] ELSE N'' END AS Billing_Code,
		CASE WHEN ([CaseTypeUCL].[ucd_key] IS NOT NULL) THEN [CaseTypeUCL].[ucd_title] ELSE N'' END AS CaseType,
		CASE WHEN ([CaseStatusUCL].[ucd_key] IS NOT NULL) THEN [CaseStatusUCL].[ucd_title] ELSE N'' END AS [Status]					

        FROM     [dbo].[case] AS [CaseTable]  (NOLOCK) 

		
		LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK)  ON (cas_cst_key = [CaseStatusUCL].[ucd_key]) --AND ([CaseStatusUCL].[ucd_ucl_key] = 12)
        LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]  (NOLOCK)  ON (cas_ctp_key = [CaseTypeUCL].[ucd_key]) --AND ([CaseTypeUCL].[ucd_ucl_key] = 11)
        LEFT OUTER JOIN cteUCLdata AS [BillingCodeUCL]  (NOLOCK)  ON (cas_billing_bic_key = [BillingCodeUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 10)
		LEFT OUTER JOIN cteUCLdata AS [LoginDelayUCL]  (NOLOCK)  ON (cas_metric_tpaDelay_key = [LoginDelayUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 18)
		LEFT OUTER JOIN cteUCLdata AS [IdentificationTypeUCL]  (NOLOCK)  ON (cas_identification_type = [IdentificationTypeUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 25)
		LEFT OUTER JOIN cteUCLdata AS [NonTpaReasonsTypeUCL]  (NOLOCK)  ON (cas_metric_non_tpa_reason_key = [NonTpaReasonsTypeUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 24)
		LEFT OUTER JOIN cteUCLdata AS [TpaDelayUCL]  (NOLOCK)  ON (cas_metric_tpaDelay_key = [TpaDelayUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 23)
		LEFT OUTER JOIN AspNetUsers as Physician  (NOLOCK)  ON cas_phy_key = Physician.Id
        LEFT OUTER JOIN cte_PhysicianSelection   ON cas_key = cte_PhysicianSelection.cah_cas_key		
		INNER JOIN [dbo].[facility]   (NOLOCK)  ON cas_fac_key = [facility].[fac_key]
        WHERE 
		cas_is_active = 1 	
		AND (ISNULL(@Physician,'') = '' OR cas_phy_key  = @Physician)
		AND (ISNULL(@CaseStatus,'') = '' OR cas_cst_key IN (select val FROM dbo.SplitData(@CaseStatus, ',')))
		AND ( 
		ISNULL(@CaseType, '') = '' 
		OR  ( 

			(ISNULL(@CaseTypeOpr, '')  = 'neq'  AND cas_ctp_key NOT IN (select val FROM dbo.SplitData(@CaseType, ',')))
		OR (ISNULL(@CaseTypeOpr, '') = 'eq' AND  cas_ctp_key  IN (select val FROM dbo.SplitData(@CaseType, ',')))
	--	OR 1 = 1
			)
		)
		AND (ISNULL(@VisitType,'') = '' OR cas_billing_visit_type = @VisitType )						
		AND (@Flagged IS NULL  OR cas_is_flagged = @Flagged )
		AND (@IsEAlert IS NULL  OR cas_is_ealert = @IsEAlert )	
		AND (ISNULL(@TPAFilter,'') = '' OR cas_metric_tpa_consult IN (select val FROM dbo.SplitData(@TPAFilter, ',')))										
		AND (ISNULL(@StartDate,'') = '' 
								OR (@DateFilter IN ('Today', 'Yesterday','SpecificDate') AND  CONVERT(date, cas_created_date)  = CONVERT(date,  @StartDate))
								OR (@DateFilter NOT IN ('Today', 'Yesterday','SpecificDate','DateRange') AND  cas_created_date >= @StartDate)
								OR (@DateFilter  IN ('DateRange') AND  CONVERT(date, cas_created_date)  >= CONVERT(date,  @StartDate))
			)
		AND (ISNULL(@EndDate,'') = '' OR (Convert(date,cas_created_date) <= CONVERT(date,  @EndDate)))
	    AND ((ISNULL(@PatientName,'') = '') OR (cas_patient like Concat('%',@PatientName,'%')))		 
		AND (ISNULL(@FacilityIds,'') = '' OR (cas_fac_key IN (select val FROM dbo.SplitData(@FacilityIds,','))))
		AND (ISNULL(@CaseNumber,'') = '' OR (cas_case_number = @CaseNumber))
		AND (ISNULL(@UserInitialFilter,'') = '' OR   cte_PhysicianSelection.cah_cas_key is not null)
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
        when @SortType = 'date_of_consult' then cas_billing_date_of_consult
        end DESC,		
	     case
        when @SortDir <> 'ASC' then ''
        when @SortType = 'date_of_consult' then cas_billing_date_of_consult
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
        end ASC
	
	

END