-- =============================================
-- Author:		<Adnan K>
-- Create date: <2020-Feb-12>
-- Description:	<Optimize Case Listing Page Query>
-- =============================================
-- Exec [dbo].[usp_GetAllPhysiciansByFacility] 
CREATE PROCEDURE [dbo].[usp_case_listing] 
	-- Add the parameters for the stored procedure here	
	@Take INT = NULL,
	@Skip INT = NULL,
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
	@UserInitialFilter varchar(1000) = NULL
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
	WHERE ucd_ucl_key IN (10,11,12)
)



Select  cas_key, 
		cas_created_date,
        [dbo].[FormatDateTime](cas_status_assign_date,1) as cas_status_assign_date,         
        cas_cart, 
		cas_history_physician_initial as phy_name,
        cas_cst_key, 
        cas_case_number, 
        cas_is_flagged, 
		cas_is_ealert,
		[dbo].[FormatDateTime]([dbo].[UDF_ConvertUtcToLocalByTimezoneIdentifier](N'Eastern Standard Time', cas_response_ts_notification), 1) AS cas_response_ts_notification, 
        [dbo].[FormatDateTime](cas_metric_stamp_time_est, 1) as cas_metric_stamp_time_est, 
        [dbo].[FormatDateTime](cas_billing_date_of_consult,1) as date_of_consult,        
        CASE WHEN ([CaseTypeUCL].[ucd_key] IS NOT NULL) THEN [CaseTypeUCL].[ucd_title] ELSE N'' END AS ctp_name,        
        [facility].[fac_name] AS fac_name, 
		cas_patient,
		CASE WHEN ([CaseStatusUCL].[ucd_key] IS NOT NULL) THEN [CaseStatusUCL].[ucd_title] ELSE N'' END AS cst_name, 
		cas_created_by_name As Navigator, 
		cas_billing_visit_type,
		CASE WHEN (cas_billing_physician_blast = 1) THEN N'Yes' ELSE N'No' END AS IsPhysicianBlast, 
		CASE WHEN ([BillingCodeUCL].[ucd_key] IS NOT NULL) THEN [BillingCodeUCL].[ucd_title] ELSE N'' END AS BillingCode,	
		cas_callback	,

		CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END  as totalRecords

     
        FROM     [dbo].[case] AS [CaseTable]  (NOLOCK) 

		
		LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK)  ON (cas_cst_key = [CaseStatusUCL].[ucd_key]) --AND ([CaseStatusUCL].[ucd_ucl_key] = 12)
        LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]  (NOLOCK)  ON (cas_ctp_key = [CaseTypeUCL].[ucd_key]) --AND ([CaseTypeUCL].[ucd_ucl_key] = 11)
        LEFT OUTER JOIN cteUCLdata AS [BillingCodeUCL]  (NOLOCK)  ON (cas_billing_bic_key = [BillingCodeUCL].[ucd_key]) --AND ([BillingCodeUCL].[ucd_ucl_key] = 10)
		LEFT OUTER JOIN AspNetUsers as Physician  (NOLOCK)  ON cas_phy_key = Physician.Id
        -- LEFT OUTER JOIN cte_PhysicianSelection   ON cas_key = cte_PhysicianSelection.cah_cas_key
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
		
		

 
		OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS
	FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY

END