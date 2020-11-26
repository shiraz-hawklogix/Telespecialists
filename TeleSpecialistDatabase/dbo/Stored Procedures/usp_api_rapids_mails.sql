-- =============================================  
-- Author:  <Author,,Name>  
-- Create date: <Create Date,,>  
-- Description: <Description,,>  
-- =============================================  
CREATE PROCEDURE usp_api_rapids_mails  
 -- Add the parameters for the stored procedure here  
 @Take INT = 50,  
 @Skip INT = 0  
AS  
BEGIN  
 select [rpd_key]  
      ,[rpd_uid]  
      ,[rpd_date]  
      ,[rpd_from]  
      ,[rpd_to]  
      ,[rpd_subject]  
      ,[rpd_body]  
      ,[rpd_attachments]  
      ,[rpd_attachment_html]  
      ,[rpd_logs]  
      ,[rpd_is_read]  
      ,[rpd_created_by]  
      ,[rpd_created_date]   
   ,CASE WHEN @Take is not null THEN count(*) over() ELSE 0 END  as totalRecords  
  
   from rapids_mailbox  
   --where   
   ORDER BY [rpd_key] desc  
   OFFSET (CASE WHEN @Skip IS NULL THEN 0 ELSE @Skip END) ROWS  
   FETCH NEXT (CASE WHEN (@Take IS NULL OR @Take = 0) THEN 1000000000 ELSE @Take END) ROWS ONLY  
  
END  