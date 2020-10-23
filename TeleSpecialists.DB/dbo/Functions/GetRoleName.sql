-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetRoleName]
(
	@roleId nvarchar(50)
)
RETURNS nvarchar(MAX)
AS
BEGIN
	DECLARE @roleName nvarchar(MAX);

	SELECT @roleName = rle.[Name] FROM AspNetRoles rle WHERE rle.Id = @roleId 

	RETURN @roleName

END
