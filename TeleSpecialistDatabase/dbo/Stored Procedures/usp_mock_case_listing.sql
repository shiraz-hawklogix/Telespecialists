    
-- =============================================      
-- Author:  <Muhammad Bilal>      
-- Create date: <2020-Apr-18>      
-- Description: <Optimize PAC Case Listing Query>      
-- =============================================       
CREATE PROCEDURE [dbo].[usp_mock_case_listing]       
 -- Add the parameters for the stored procedure here       
 @mCaseStatusEnumId INT = NULL,      
 @mCaseTypeEnumId INT = NULL,      
 @Take INT = NULL,      
 @Skip INT = NULL,      
 @Physician varchar(128) = NULL,      
 @SearchText varchar(50) = NULL,      
 @CaseStatus varchar(300) = NULL,      
 @CaseType varchar(300) = NULL,      
 @CaseTypeOpr varchar(5) = NULL,      
 @FacilityIds varchar(128) = NULL,      
 @StartDate DateTime = NULL,      
 @EndDate DateTime = NULL,       
 @DateFilter varchar(20) = NULL,      
 @PatientName varchar(50) = NULL,      
 @UserInitialFilter varchar(1000) = NULL      
AS      
BEGIN      
 SET NOCOUNT ON;      
with cteUCLdata AS      
(      
 SELECT ucd_key, ucd_title, ucd_ucl_key      
 FROM ucl_data      
 WHERE ucd_ucl_key IN (@mCaseStatusEnumId,@mCaseTypeEnumId)      
)      
Select  mcas_key,      
  mcas_ctp_key,      
  mcas_fac_key,      
  facility.fac_name AS facilityname,      
  CONCAT(Physician.FirstName,' ', Physician.LastName) AS PhysicianName,      
  mcas_cst_key,      
  mcas_patient,       
  mcas_callback,              
        [dbo].[FormatDateTime](mcas_created_date,1) as mcas_created_date,               
        mcas_phy_key,      
  CASE WHEN ([CaseTypeUCL].[ucd_key] IS NOT NULL) THEN [CaseTypeUCL].[ucd_title] ELSE N'' END AS casetype,              
  CASE WHEN ([CaseStatusUCL].[ucd_key] IS NOT NULL) THEN [CaseStatusUCL].[ucd_title] ELSE N'' END AS CaseStatus,           
  CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END  as totalRecords      
  FROM         
  [dbo].[mock_case] AS [CaseTable]  (NOLOCK)       
  LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK)  ON (mcas_cst_key = [CaseStatusUCL].[ucd_key])      
  LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]  (NOLOCK)  ON (mcas_ctp_key = [CaseTypeUCL].[ucd_key])      
  LEFT OUTER JOIN AspNetUsers as Physician  (NOLOCK)  ON mcas_phy_key = Physician.Id      
  INNER JOIN [dbo].[facility]   (NOLOCK)  ON mcas_fac_key = [facility].[fac_key]      
        WHERE       
   (ISNULL(@Physician,'') = '' OR mcas_phy_key  = @Physician)      
  AND (ISNULL(@CaseStatus,'') = '' OR mcas_cst_key IN (select val FROM dbo.SplitData(@CaseStatus, ',')))      
  AND (       
  ISNULL(@CaseType, '') = ''       
  OR  (       
   (ISNULL(@CaseTypeOpr, '')  = 'neq'  AND mcas_ctp_key NOT IN (select val FROM dbo.SplitData(@CaseType, ',')))      
  OR (ISNULL(@CaseTypeOpr, '') = 'eq' AND  mcas_ctp_key  IN (select val FROM dbo.SplitData(@CaseType, ',')))      
   )      
  )                
  AND (ISNULL(@StartDate,'') = ''       
        OR (@DateFilter IN ('Today', 'Yesterday','SpecificDate') AND  CONVERT(date, mcas_created_date)  = CONVERT(date,  @StartDate))      
        OR (@DateFilter NOT IN ('Today', 'Yesterday','SpecificDate','DateRange') AND  mcas_created_date >= @StartDate)      
        OR (@DateFilter  IN ('DateRange') AND  CONVERT(date, mcas_created_date)  >= CONVERT(date,  @StartDate))      
   )      
  AND (ISNULL(@EndDate,'') = '' OR (Convert(date,mcas_created_date) <= CONVERT(date,  @EndDate)))      
     AND ((ISNULL(@PatientName,'') = '') OR (mcas_patient like Concat('%',@PatientName,'%')))         
  AND (ISNULL(@FacilityIds,'') = '' OR (mcas_fac_key IN (select val FROM dbo.SplitData(@FacilityIds,','))))       
  AND (ISNULL(@UserInitialFilter,'') = '' OR   (mcas_phy_key IN (select val from dbo.SplitData(@UserInitialFilter, ','))))       
  AND (ISNULL(@SearchText,'') = ''             
          
   OR (mcas_callback like Concat('%',@SearchText,'%'))      
   OR (mcas_patient like Concat('%',@SearchText,'%'))        
   OR (fac_name like Concat('%',@SearchText,'%'))      
      --OR (pac_created_by_name like Concat('%',@SearchText,'%'))      
   OR (CaseStatusUCL.ucd_title like Concat('%',@SearchText,'%'))      
   OR (CaseTypeUCL.ucd_title like Concat('%',@SearchText,'%'))      
          
   OR (Physician.FirstName like Concat('%',@SearchText,'%'))      
   OR (Physician.LastName like Concat('%',@SearchText,'%'))      
         
   )      
           
  ORDER BY mcas_created_date DESC        
  OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS      
  FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY      
END