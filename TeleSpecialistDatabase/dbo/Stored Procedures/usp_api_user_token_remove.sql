  
-- =============================================    
-- Author:  <Adnan K.>    
-- Create date: <2020-June-01>    
-- Description: < SP for saving auth token, used by api >    
-- =============================================    
CREATE PROCEDURE [dbo].[usp_api_user_token_remove]    
 -- Add the parameters for the stored procedure here    
 @UserId    NVARCHAR(128),    
 @Token    nvarchar(max),    
 @deviceType nvarchar(50)    
AS    
BEGIN    
delete from token where tok_phy_key = @UserId AND  tok_phy_token = @Token AND tok_device_type = @deviceType 
return 'true'
END