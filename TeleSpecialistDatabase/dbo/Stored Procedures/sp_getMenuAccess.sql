-- =============================================
-- Author:		<Awais>
-- Create date: <28-09-2020>
-- Description:	<Display Menu items for access>
-- =============================================
CREATE PROCEDURE [dbo].[sp_getMenuAccess]
	-- Add the parameters for the stored procedure here
	@Id as nvarchar(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT com.com_page_title,com.com_key,cac.cac_key, com.com_page_name, com.com_module_name, com.com_parentcomponentid, ISNULL(cac.cac_isAllowed, 0) as cac_isAllowed, cac.cac_roleid
FROM components com
LEFT JOIN component_access cac on com.com_key = cac.cac_com_key and (cac.cac_roleid = @Id OR cac.cac_roleid is null)
WHERE com_status = 1
--  and com.com_parentcomponentid is null
END
