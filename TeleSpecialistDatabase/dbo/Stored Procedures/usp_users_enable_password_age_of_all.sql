-- =============================================
-- Author:		<Qasim,a>
-- Create date: <13,June,2019>
-- Description:	<To update all the user password age filed>
-- =============================================
CREATE PROCEDURE [dbo].[usp_users_enable_password_age_of_all]
AS
BEGIN
 	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	SET NOCOUNT ON;
	 
	--Update AspNetUsers set EnablePasswordAge = 1
END