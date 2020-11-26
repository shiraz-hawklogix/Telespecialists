CREATE PROCEDURE [dbo].[system_log_insert]  
(  
 @log_service_type      VARCHAR(200),  
 @log_status        varchar(50),  
 @log_error        varchar(MAX),  
 @out bigint output  
)  
AS  
BEGIN  
DECLARE @sql NVARCHAR(MAX)  
  
INSERT INTO [system_log]  
           ([log_service_type]  
           ,[log_status]  
           ,[log_time]  
           ,[log_error]  
           )  
     VALUES  
           (@log_service_type ,@log_status, CONVERT(DATETIME,GETDATE() AT TIME ZONE 'UTC' AT TIME ZONE 'Eastern Standard Time'), @log_error )  
  
SELECT @out = @@IDENTITY  
  
RETURN  
  
END