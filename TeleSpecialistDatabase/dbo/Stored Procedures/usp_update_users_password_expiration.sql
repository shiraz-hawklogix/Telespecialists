-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_update_users_password_expiration] 
	-- Add the parameters for the stored procedure here
	--@userPasswordExpirationDays int,
	@userPasswordExpirationDate datetime = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	 
	 IF @userPasswordExpirationDate is not null
	 begin
		update AspNetUsers set PasswordExpirationDate = @userPasswordExpirationDate
		-- where PasswordExpirationDate is null
		-- Temporary commenting where clause. will be fix proparly by Qasim soon.
	 end
	 Else If @userPasswordExpirationDate is null
	 begin
		update AspNetUsers set PasswordExpirationDate = @userPasswordExpirationDate
	 end
END