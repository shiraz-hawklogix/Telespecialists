  
-- =============================================  
-- Author:  <Adnan K.>  
-- Create date: <2020-May-06>  
-- Description: <Used in Telecare API >  
-- =============================================  
CREATE PROCEDURE [dbo].[usp_api_cases_by_phy]  
 -- Add the parameters for the stored procedure here  
 @Take INT = 50,  
 @Skip INT = 0,  
 @Physician varchar(128) = NULL,  
 @CaseType varchar(300) = NULL,  
 @StartDate DateTime = NULL,  
 @EndDate DateTime = NULL  
AS  
BEGIN  
 SET NOCOUNT ON -- added to prevent extra result sets from  
  
 ;with cteUCLdata AS  
(  
 SELECT ucd_key, ucd_title, ucd_ucl_key  
 FROM ucl_data  
 WHERE ucd_ucl_key IN (10,11,12)  
)  
  
  
 Select  cas_key,   
  cas_created_date,  
         cas_phy_key,  
  (Physician.FirstName + ' ' + Physician.LastName) as phy_name,  
           
        cas_case_number,   
       cas_ctp_key,  
        CASE WHEN ([CaseTypeUCL].[ucd_key] IS NOT NULL) THEN [CaseTypeUCL].[ucd_title] ELSE N'' END AS ctp_name,    
  cas_fac_key,        
        [facility].[fac_name] AS fac_name,     
    
  cas_cst_key,  
  CASE WHEN ([CaseStatusUCL].[ucd_key] IS NOT NULL) THEN [CaseStatusUCL].[ucd_title] ELSE N'' END AS cst_name,  
  CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END  as totalRecords  
      
        FROM     [dbo].[case] AS [CaseTable]  (NOLOCK)   
  
    
  LEFT OUTER JOIN cteUCLdata AS [CaseStatusUCL]  (NOLOCK)  ON (cas_cst_key = [CaseStatusUCL].[ucd_key]) --AND ([CaseStatusUCL].[ucd_ucl_key] = 12)  
        LEFT OUTER JOIN cteUCLdata AS [CaseTypeUCL]  (NOLOCK)  ON (cas_ctp_key = [CaseTypeUCL].[ucd_key]) --AND ([CaseTypeUCL].[ucd_ucl_key] = 11)  
        
  LEFT OUTER JOIN AspNetUsers as Physician  (NOLOCK)  ON cas_phy_key = Physician.Id  
  INNER JOIN [dbo].[facility]   (NOLOCK)  ON cas_fac_key = [facility].[fac_key]  
  
   WHERE   
  cas_is_active = 1    
  AND (ISNULL(@Physician,'') = '' OR cas_phy_key  = @Physician)  
  AND   
  --(@CaseType IS NULL OR cas_ctp_key = @CaseType)  
  cas_ctp_key IN (select val FROM dbo.SplitData(@CaseType, ','))  
  
  AND (ISNULL(@StartDate,'') = '' OR CONVERT(date, cas_created_date)  >= CONVERT(date,  @StartDate))  
  AND (ISNULL(@EndDate,'') = '' OR CONVERT(date, cas_created_date)  <= CONVERT(date,  @EndDate))  
  
  ORDER BY cas_key desc  
  OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS  
  FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY  
  
END