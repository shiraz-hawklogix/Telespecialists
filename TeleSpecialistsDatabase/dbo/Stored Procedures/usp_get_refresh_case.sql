

-- ============================================= 
CREATE PROCEDURE [dbo].[usp_get_refresh_case] 
AS
BEGIN
	SET NOCOUNT ON;
Select  Top 1 cf.*
FROM     [dbo].[For_Case]  cf 
       
END
