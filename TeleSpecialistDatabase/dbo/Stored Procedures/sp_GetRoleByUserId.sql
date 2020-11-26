


CREATE PROCEDURE [dbo].[sp_GetRoleByUserId]
	-- Add the parameters for the stored procedure here
	@Id as nvarchar(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  SELECT anur.UserId, anur.RoleId, anr.Name
  FROM [dbo].[AspNetUserRoles] anur
  JOIN AspNetRoles anr on anr.Id = anur.RoleId
  WHERE anur.UserId = @Id
END
