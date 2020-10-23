-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetUserFullName]
(
	@userId nvarchar(50)
)
RETURNS nvarchar(MAX)
AS
BEGIN
	DECLARE @userName nvarchar(MAX) = '';

	SELECT @userName = ISNULL(usr.FirstName, '') + CASE WHEN usr.FirstName IS NOT NULL THEN ' ' ELSE '' END + ISNULL(usr.LastName, '') FROM AspNetUsers usr WHERE usr.Id = @userId 

	RETURN @userName

END
