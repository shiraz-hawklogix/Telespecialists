
  
  
  
 CREATE PROCEDURE [dbo].[usp_cas_number_dropdown]   
  @PhysicianKey    varchar(128)  
 AS  
 BEGIN  
  SET NOCOUNT ON;  
  
  DECLARE @DateEst DATETIME = DATEADD(day, -1, CONVERT(DATETIME,GETDATE() AT TIME ZONE 'Eastern Standard Time'))  
    
  BEGIN  
   
   Select   
   c.cas_key,  
   CONCAT('#',c.cas_case_number,' - ', c.cas_patient, ' - ' ,f.fac_name) as CaseNumFac  
   from [dbo].[case] c  
   Join facility f   
   on f.fac_key = c.cas_fac_key  
   where cas_phy_key = @PhysicianKey AND cas_created_date >=  @DateEst  
   Order By 1 DESC  
  
  
  END  
  
 END  
