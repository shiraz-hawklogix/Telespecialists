
-- =============================================
-- Author:		<Adnan K.>
-- Create date: <2020-June-01>
-- Description:	< SP for saving auth token, used by api >
-- =============================================
CREATE PROCEDURE [dbo].[usp_api_user_token]
	-- Add the parameters for the stored procedure here
	@UserId				NVARCHAR(128),
	@Token				nvarchar(max),
	@deviceType	nvarchar(50)
AS
BEGIN
	IF NOT Exists (Select tok_key FROM  token  (NOLOCK)  where tok_phy_key = @UserId AND tok_phy_token = @Token) BEGIN
		INSERT INTO token(tok_phy_key, tok_phy_token,  tok_device_type) 
		Values(@UserId, @Token, @deviceType);
		select 'success' as result;
		
	END 
	ELSE BEGIN 
		select 'token already exists';
	END
	
END