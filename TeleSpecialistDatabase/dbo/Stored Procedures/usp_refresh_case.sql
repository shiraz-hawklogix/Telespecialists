

-- ============================================= 
CREATE PROCEDURE [dbo].[usp_refresh_case] 
AS
BEGIN
	SET NOCOUNT ON;

UPDATE [dbo].[For_Case]
SET refresh_requried = 1;
END
