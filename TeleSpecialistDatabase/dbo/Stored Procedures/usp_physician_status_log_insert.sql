-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_physician_status_log_insert]
 @psl_user_key nvarchar(128)
, @psl_phs_key int
, @psl_status_name varchar(50)
, @psl_created_date datetime
, @psl_created_by nvarchar(128)
, @psl_start_date datetime
AS
BEGIN
	 
	SET NOCOUNT ON;
INSERT INTO [dbo].[physician_status_log]
           ([psl_user_key]
           ,[psl_phs_key]
           ,[psl_status_name]
           ,[psl_created_date]
           ,[psl_created_by]
           ,[psl_start_date])
     VALUES
           (@psl_user_key,@psl_phs_key,@psl_status_name,@psl_created_date,@psl_created_by,@psl_start_date )
 


END